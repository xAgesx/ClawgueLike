using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

// Runtime half of the Vampire. It rides on the coin itself so the beam follows it
// around, and owns every piece of state the effect asset is forbidden to keep: the
// current victim, the sip timers and how many coins it has already emptied. Created by
// VampireEffect at a turn boundary and left running from then on.
//
// The shop freezes time, so the timers simply stop advancing while it is open and the
// siphon picks up exactly where it left off once the next turn starts.
public class VampireDrain : MonoBehaviour {
    private enum State { Seeking, Draining, Pausing, Done }

    private VampireEffect effect;
    private CoinInstance self;

    // Reused every frame so seeking does not allocate. Colliders are skipped as targets,
    // so the beam never latches onto a trigger volume.
    private readonly Collider[] hits = new Collider[128];
    private readonly List<CoinInstance> candidates = new List<CoinInstance>();

    private LineRenderer beam;
    private Material beamMaterial;
    private CoinInstance target;

    // A second, wider line underneath the core, drawn additively, fakes a soft glow.
    private LineRenderer glow;
    private Material glowMaterial;

    private const int BeamPoints = 14;
    private readonly Vector3[] beamPoints = new Vector3[BeamPoints];

    // Look-and-feel constants. pulseTime drives the breathing width/brightness, sipFlash
    // spikes on every sip and decays so the beam visibly "bites" each time it feeds.
    private const float PulseSpeed = 4.5f;
    private const float WaveSpeed = 7f;
    private const float WaveCycles = 1.6f;

    private float pulseTime;
    private float sipFlash;

    private State state = State.Seeking;
    private float sipTimer;
    private float timeOnTarget;
    private float pauseTimer;
    private int drained;

    public void Configure(VampireEffect vampire, CoinInstance owner) {
        effect = vampire;
        self = owner;
        BuildBeam();
    }

    // The value a victim is drained down to before the Vampire moves on. Zero means it
    // drinks a coin completely dry (a worthless husk); clamping at zero keeps a mis-set
    // asset from asking for a negative target.
    private int Floor => effect != null ? Mathf.Max(0, effect.MinTargetValue) : 0;

    // Pooled coins come back with a clean slate, exactly like every other effect.
    private void OnEnable() {
        target = null;
        state = State.Seeking;
        sipTimer = 0f;
        timeOnTarget = 0f;
        pauseTimer = 0f;
        drained = 0;
        HideBeam();
    }

    private void Update() {
        if (effect == null || self == null) return;
        if (state == State.Done) return;

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        switch (state) {
            case State.Seeking:
                target = FindTarget();
                if (target != null) {
                    sipTimer = 0f;
                    timeOnTarget = 0f;
                    state = State.Draining;
                }
                break;

            case State.Draining:
                if (target == null) {
                    // Someone collected the victim out from under us. It was not emptied,
                    // so it does not count against the quota.
                    BeginPause();
                } else {
                    Drain(dt);
                }
                break;

            case State.Pausing:
                pauseTimer -= dt;
                if (pauseTimer <= 0f) state = State.Seeking;
                break;
        }

        UpdateBeam(dt);
    }

    private void Drain(float dt) {
        timeOnTarget += dt;
        sipTimer += dt;

        // interval guarded against zero so a mis-set asset cannot spin here forever.
        float interval = Mathf.Max(0.01f, effect.DrainInterval);
        while (sipTimer >= interval && state == State.Draining) {
            sipTimer -= interval;
            Sip();
        }

        if (target != null && target.BaseValue <= Floor) {
            target = null;
            drained++;

            if (drained >= effect.MaxTargets) Finish();
            else BeginPause();
        }
    }

    private void Sip() {
        if (target == null) return;

        int reserve = Floor;
        int available = target.BaseValue - reserve;
        if (available <= 0) return;

        // Drain first, then take only what the victim could actually spare, so asking
        // for more than it holds can never conjure value out of thin air.
        int amount = Mathf.Min(SipAmount(), available);
        target.DrainBaseValue(amount);
        self.AddBaseValue(amount);

        // Kick the beam so the sip reads as a bite.
        sipFlash = 1f;
    }

    // The longer this beam has been on the same victim, the more each sip takes.
    private int SipAmount() {
        int ramp = effect.RampSecondsPerSip > 0f
            ? Mathf.FloorToInt(timeOnTarget / effect.RampSecondsPerSip)
            : 0;

        return Mathf.Max(1, effect.DrainPerSip + ramp);
    }

    // Everything in reach is gathered first and only then picked from, so each coin has
    // an even chance - grabbing the first hit would always favour whichever collider
    // happened to come back first, which is not exactly "random".
    private CoinInstance FindTarget() {
        int count = Physics.OverlapSphereNonAlloc(transform.position, effect.SearchRadius, hits, ~0, QueryTriggerInteraction.Ignore);

        candidates.Clear();
        for (int i = 0; i < count; i++) {
            CoinInstance coin = hits[i].GetComponentInParent<CoinInstance>();
            if (coin == null || coin == self) continue;
            if (coin.BaseValue <= Floor) continue;
            if (candidates.Contains(coin)) continue;
            candidates.Add(coin);
        }

        if (candidates.Count == 0) return null;
        return candidates[Random.Range(0, candidates.Count)];
    }

    private void BeginPause() {
        target = null;
        state = State.Pausing;
        pauseTimer = Mathf.Max(0f, effect.PauseBetweenTargets);
        HideBeam();
    }

    private void Finish() {
        target = null;
        state = State.Done;
        HideBeam();

        // Same "spent" wash every other capped coin gets.
        self.SetTint(effect.SpentTint, effect.SpentTintStrength);
    }

    private void UpdateBeam(float dt) {
        if (beam == null) return;

        if (state != State.Draining || target == null) {
            HideBeam();
            return;
        }

        pulseTime += dt;
        sipFlash = Mathf.MoveTowards(sipFlash, 0f, dt * 4f);

        // Breathing width + a punch on each sip.
        float pulse = 0.5f + 0.5f * Mathf.Sin(pulseTime * PulseSpeed);
        float width = (effect != null ? effect.BeamWidth : 0.04f)
            * Mathf.Lerp(0.55f, 1.15f, pulse)
            * (1f + sipFlash * 1.5f);

        Color baseColor = effect != null ? effect.BeamColor : Color.red;
        float brightness = 0.55f + 0.45f * pulse + sipFlash * 0.6f;

        beam.enabled = true;
        beam.startWidth = width;
        beam.endWidth = width * 0.55f;
        SetMaterialColor(beamMaterial, Brighten(baseColor, brightness));

        if (glow != null) {
            glow.enabled = true;
            glow.startWidth = width * 3.2f;
            glow.endWidth = width * 0.4f;

            Color glowColor = Brighten(baseColor, brightness * 0.8f);
            glowColor.a = Mathf.Clamp01(0.18f + 0.3f * pulse + sipFlash * 0.35f);
            SetMaterialColor(glowMaterial, glowColor);
        }

        BuildBeamPoints();
        beam.SetPositions(beamPoints);
        if (glow != null) glow.SetPositions(beamPoints);
    }

    // A sine wave running from the coin to its victim: pinned to zero at both ends so it
    // never detaches, free to ripple through the middle. Two perpendicular axes give it
    // a slight three-dimensional coil rather than a flat wobble.
    private void BuildBeamPoints() {
        Vector3 start = transform.position;
        Vector3 end = target.transform.position;
        Vector3 delta = end - start;
        float length = delta.magnitude;
        Vector3 dir = length > 0.0001f ? delta / length : Vector3.forward;

        Vector3 up = Mathf.Abs(Vector3.Dot(dir, Vector3.up)) > 0.95f ? Vector3.right : Vector3.up;
        Vector3 perp = Vector3.Cross(dir, up).normalized;
        Vector3 perp2 = Vector3.Cross(dir, perp).normalized;

        float amplitude = (effect != null ? effect.BeamWidth : 0.04f) * 1.1f * (1f + sipFlash * 0.8f);

        for (int i = 0; i < BeamPoints; i++) {
            float t = (float)i / (BeamPoints - 1);
            Vector3 point = Vector3.Lerp(start, end, t);

            float envelope = Mathf.Sin(t * Mathf.PI);
            float phase = t * WaveCycles * Mathf.PI * 2f - pulseTime * WaveSpeed;
            float wave = Mathf.Sin(phase);
            float wave2 = Mathf.Cos(phase) * 0.5f;

            point += (perp * wave + perp2 * wave2) * (envelope * amplitude);
            beamPoints[i] = point;
        }
    }

    private static Color Brighten(Color color, float amount) {
        return new Color(
            Mathf.Clamp01(color.r + amount * (1f - color.r)),
            Mathf.Clamp01(color.g + amount * (1f - color.g)),
            Mathf.Clamp01(color.b + amount * (1f - color.b)),
            color.a);
    }

    private static void SetMaterialColor(Material material, Color color) {
        if (material == null) return;
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
    }

    private void HideBeam() {
        if (beam != null) beam.enabled = false;
        if (glow != null) glow.enabled = false;
    }

    // Both lines hang off child objects rather than the coin itself, so the coin keeps
    // exactly one Renderer and CoinInstance can never accidentally recolour a line
    // instead of the coin. The glow is drawn additively so it reads as light, not paint.
    private void BuildBeam() {
        if (beam != null) return;

        Color beamColor = effect != null ? effect.BeamColor : Color.red;
        float width = effect != null ? effect.BeamWidth : 0.04f;

        beamMaterial = CreateBeamMaterial(beamColor, false);
        glowMaterial = CreateBeamMaterial(beamColor, true);

        beam = CreateLine("VampireBeam", width, beamMaterial);
        glow = CreateLine("VampireBeamGlow", width * 3.2f, glowMaterial);
    }

    private LineRenderer CreateLine(string name, float width, Material material) {
        GameObject holder = new GameObject(name);
        holder.transform.SetParent(transform, false);

        LineRenderer line = holder.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = BeamPoints;
        line.numCapVertices = 4;
        line.numCornerVertices = 2;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.startWidth = width;
        line.endWidth = width;
        line.startColor = Color.white;
        line.endColor = Color.white;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        if (material != null) line.sharedMaterial = material;
        line.enabled = false;
        return line;
    }

    // URP project, so Unlit is the shader that is actually present; the built-in
    // fallbacks are only there so this cannot come up magenta if that ever changes.
    // The glow material is flipped into additive transparency so overlapping parts of
    // the beam accumulate into a hot core.
    private static Material CreateBeamMaterial(Color color, bool additive) {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Transparent");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");
        if (shader == null) return null;

        Material material = new Material(shader) {
            name = additive ? "VampireBeamGlow (runtime)" : "VampireBeam (runtime)",
            hideFlags = HideFlags.HideAndDontSave
        };

        if (additive) {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 2f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)BlendMode.One);
            material.SetFloat("_ZWrite", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.renderQueue = (int)RenderQueue.Transparent;
        }

        SetMaterialColor(material, additive ? new Color(color.r, color.g, color.b, 0.35f) : color);
        return material;
    }

    private void OnDestroy() {
        if (beamMaterial != null) Destroy(beamMaterial);
        if (glowMaterial != null) Destroy(glowMaterial);
    }
}
