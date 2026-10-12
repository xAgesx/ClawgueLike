using UnityEngine;

// Vampire: it never touches anything. It sits with a siphon beam permanently open,
// latching onto one random coin within reach and drinking it dry at a rate that climbs
// the longer it keeps feeding. When a victim bottoms out it pauses, picks a fresh one,
// and goes again - until it has emptied its quota, after which it goes grey for good.
//
// Contact does nothing, so TriggersOnContact is off on the asset. The live half of this
// effect is VampireDrain, which the coin gains at the first turn boundary and keeps
// from then on; this asset only supplies the numbers.
[CreateAssetMenu(fileName = "VampireCoin", menuName = "Data/Effects/SpecialCoins/Vampire")]
public class VampireEffect : CoinEffect {
    [Tooltip("How far the siphon can reach for a new victim.")]
    [SerializeField] private float searchRadius = 2f;

    [Tooltip("Seconds between sips.")]
    [SerializeField] private float drainInterval = 0.4f;

    [Tooltip("Value taken by the first sip on a victim. Every later sip takes more.")]
    [SerializeField] private int drainPerSip = 1;

    [Tooltip("Every this many seconds spent on the same victim, each sip takes one more. Lower means it accelerates harder.")]
    [SerializeField] private float rampSecondsPerSip = 0.8f;

    [Tooltip("A victim is never drained below this, so no coin is emptied out of existence.")]
    [SerializeField] private int minTargetValue = 1;

    [Tooltip("Seconds it waits after emptying a victim before it latches onto the next.")]
    [SerializeField] private float pauseBetweenTargets = 3f;

    [Tooltip("How many coins it may drain before it goes quiet for good.")]
    [SerializeField] private int maxTargets = 5;

    [Tooltip("Thickness of the drain beam.")]
    [SerializeField] private float beamWidth = 0.04f;

    [Tooltip("Colour of the drain beam.")]
    [SerializeField] private Color beamColor = new Color(0.8f, 0.05f, 0.15f, 1f);

    public float SearchRadius => searchRadius;
    public float DrainInterval => drainInterval;
    public int DrainPerSip => drainPerSip;
    public float RampSecondsPerSip => rampSecondsPerSip;
    public int MinTargetValue => minTargetValue;
    public float PauseBetweenTargets => pauseBetweenTargets;
    public int MaxTargets => maxTargets;
    public float BeamWidth => beamWidth;
    public Color BeamColor => beamColor;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        // The beam hangs around between turns, so a coin that already carries one must
        // not be handed a second every time a turn opens.
        if (coin.GetComponent<VampireDrain>() != null) return;

        coin.gameObject.AddComponent<VampireDrain>().Configure(this, coin);
    }
}
