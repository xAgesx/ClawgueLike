using UnityEngine;

// Homing: at the first turn boundary it starts drifting toward the most valuable coin
// within reach, re-picking a target as coins move or get collected. The drift is a
// continuous acceleration rather than a hop, so it noses through the pile.
[CreateAssetMenu(fileName = "HomingCoin", menuName = "Data/Effects/SpecialCoins/Homing")]
public class HomingEffect : CoinEffect {
    [Tooltip("How far it can sense a target.")]
    [SerializeField] private float searchRadius = 4f;

    [Tooltip("Acceleration toward the target, in m/s^2.")]
    [SerializeField] private float speed = 1.2f;

    [Tooltip("Small upward component so it climbs over the pile instead of grinding into it.")]
    [SerializeField] private float upwardDrift = 0.15f;

    [Tooltip("Seconds between target re-evaluations.")]
    [SerializeField] private float retargetInterval = 0.5f;

    public float SearchRadius => searchRadius;
    public float Speed => speed;
    public float UpwardDrift => upwardDrift;
    public float RetargetInterval => retargetInterval;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        HomingDrift drift = coin.GetComponent<HomingDrift>();
        if (drift == null) drift = coin.gameObject.AddComponent<HomingDrift>();
        drift.Configure(this, coin);
    }
}
