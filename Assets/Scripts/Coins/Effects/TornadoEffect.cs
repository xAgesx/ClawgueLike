using UnityEngine;

// Tornado: at the start of every turn it tears a whirlwind open where it is sitting
// and lets it throw every coin nearby around for a few seconds. Never fires on
// contact, so TriggersOnContact is off on the asset.
[CreateAssetMenu(fileName = "TornadoCoin", menuName = "Data/Effects/SpecialCoins/Tornado")]
public class TornadoEffect : CoinEffect {
    [Tooltip("How far the whirlwind reaches.")]
    [SerializeField] private float radius = 1.2f;

    [Tooltip("How long the whirlwind lasts.")]
    [SerializeField] private float duration = 5f;

    [Tooltip("Upward acceleration. Must beat gravity to actually lift coins off the pile.")]
    [SerializeField] private float lift = 12f;

    [Tooltip("How hard coins are swung around the centre.")]
    [SerializeField] private float swirl = 6f;

    [Tooltip("How hard coins are dragged toward the centre, so the pile spirals instead of spraying outward.")]
    [SerializeField] private float inward = 2f;

    [Tooltip("Speed ceiling for anything caught in the whirlwind, so coins swirl instead of being shot through the glass.")]
    [SerializeField] private float maxSpeed = 4f;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        GameObject field = new GameObject("TornadoField");
        field.transform.position = coin.transform.position;
        field.AddComponent<TornadoField>().Configure(radius, duration, lift, swirl, inward, maxSpeed, coin);
    }
}
