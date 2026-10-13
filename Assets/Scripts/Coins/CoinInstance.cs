using System.Collections.Generic;
using UnityEngine;

// Runtime state for ONE physical coin: baseValue, multiplier and which effects have
// already landed on it. Lives on the prefab instance only - never on CoinData, which
// is shared by every coin spawned from that asset and survives editor play sessions.
public class CoinInstance : MonoBehaviour {
    private CoinData data;
    private int baseValue = 1;
    private float multiplier = 1f;
    private float sizeMultiplier = 1f;
    private float displayedSize = 1f;
    private readonly HashSet<CoinEffect> appliedEffects = new HashSet<CoinEffect>();

    private Renderer tintRenderer;
    private Color originalColor;
    private bool hasOriginalColor;

    private Vector3 baseScale;
    private bool hasBaseScale;

    [Tooltip("How fast the coin swells toward its target size, in size-multiples per second.")]
    [SerializeField] private float sizeGrowthRate = 0.4f;

    public CoinData Data => data;
    public int BaseValue => baseValue;
    public float Multiplier => multiplier;
    public float SizeMultiplier => sizeMultiplier;
    public int TotalValue => Mathf.Max(0, Mathf.RoundToInt(baseValue * multiplier));

    private void Awake() {
        baseScale = transform.localScale;
        hasBaseScale = true;
    }

    // Called by ItemSpawner the instant this coin leaves the pool.
    public void Initialize(CoinData coinData) {
        data = coinData;
        baseValue = coinData != null && coinData.baseValue > 0 ? coinData.baseValue : 1;
        multiplier = 1f;
        appliedEffects.Clear();
        ResetTint();
        ResetSize();
        ClearMotion();
    }

    // A reused coin still carries the velocity it had when it was collected, so it
    // would shoot straight back out of the pool. Coins without Poolable have nothing
    // else to wipe it, so the spawn path does it here.
    private void ClearMotion() {
        Rigidbody body = GetComponent<Rigidbody>();
        if (body == null) return;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.WakeUp();
    }

    public void SetBaseValue(int value) {
        baseValue = Mathf.Max(1, value);
    }

    // Used by effects that grow a coin in place instead of replacing its value
    // (a snowball packing on, a crystal ticking up).
    public void AddBaseValue(int amount) {
        baseValue = Mathf.Max(1, baseValue + amount);
    }

    // The one path that is allowed to push a coin below one, so the Vampire can drink
    // an ordinary value-1 coin dry and leave a worthless husk. Deliberately separate
    // from AddBaseValue, whose floor of one every growth effect relies on.
    public void DrainBaseValue(int amount) {
        baseValue = Mathf.Max(0, baseValue - Mathf.Max(0, amount));
    }

    // Infection uses this. Safe to call repeatedly because SpecialCoin only
    // applies an effect to a coin once, not once per frame of contact.
    public void MultiplyBaseValue(float factor) {
        baseValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * factor));
    }

    public void SetMultiplier(float factor) {
        multiplier = factor;
    }

    public void MultiplyMultiplier(float factor) {
        multiplier *= factor;
    }

    // Sets the size the coin is growing toward. The actual swell happens gradually in
    // Update so a snowball visibly fattens up over a beat instead of popping instantly.
    // Scaling the transform scales its collider with it, so a fattened snowball really
    // is a bigger obstacle. baseScale is the prefab's own size, captured in Awake
    // before anything ever inflates it.
    public void SetSizeMultiplier(float value) {
        sizeMultiplier = Mathf.Max(0.01f, value);
    }

    public void ResetSize() {
        sizeMultiplier = 1f;
        displayedSize = 1f;
        if (hasBaseScale) transform.localScale = baseScale;
    }

    private void Update() {
        if (!hasBaseScale || Mathf.Approximately(displayedSize, sizeMultiplier)) return;

        displayedSize = Mathf.MoveTowards(displayedSize, sizeMultiplier, sizeGrowthRate * Time.deltaTime);
        transform.localScale = baseScale * displayedSize;
    }

    public bool HasApplied(CoinEffect effect) {
        return effect != null && appliedEffects.Contains(effect);
    }

    public void MarkApplied(CoinEffect effect) {
        if (effect != null) appliedEffects.Add(effect);
    }

    // Blended over the coin's own colour rather than replacing it, so a spent coin
    // still reads as the coin it is: strength 1 is Infection stamping it red, the
    // spent wash uses less than that. originalColor is captured before anything tints,
    // so repeated tints all blend from the coin's real colour, not from each other.
    public void SetTint(Color color, float strength = 1f) {
        Renderer rend = GetRenderer();
        if (rend == null) return;

        if (!hasOriginalColor) {
            originalColor = rend.material.color;
            hasOriginalColor = true;
        }

        rend.material.color = Color.Lerp(originalColor, color, Mathf.Clamp01(strength));
    }

    public void ResetTint() {
        if (hasOriginalColor && tintRenderer != null) {
            tintRenderer.material.color = originalColor;
        }
    }

    private Renderer GetRenderer() {
        if (tintRenderer == null) tintRenderer = GetComponent<Renderer>();
        return tintRenderer;
    }

    // Pooled objects get flipped on before the spawner re-initialises them, so wipe
    // everything here as a safety net. Initialize() then supplies this round's data.
    private void OnEnable() {
        multiplier = 1f;
        appliedEffects.Clear();
        ResetTint();
        ResetSize();
    }
}
