using UnityEngine;

// Antigrav: opens a field at its own position at the start of a turn. For five
// seconds every coin nearby gets an upward acceleration equal to half of gravity,
// so the whole pile drifts and tumbles instead of sitting still.
[CreateAssetMenu(fileName = "AntigravCoin", menuName = "Data/Effects/SpecialCoins/Antigrav")]
public class AntigravEffect : CoinEffect {
    [Tooltip("How far the field reaches. Large enough to cover the machine's pile.")]
    [SerializeField] private float radius = 4f;

    [Tooltip("How long the field lasts, in seconds.")]
    [SerializeField] private float duration = 5f;

    [Tooltip("Upward acceleration applied to coins. 4.905 is exactly half of standard gravity.")]
    [SerializeField] private float lift = 4.905f;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        GameObject field = new GameObject("AntigravField");
        field.transform.position = coin.transform.position;

        AntigravField antigrav = field.AddComponent<AntigravField>();
        antigrav.Configure(radius, duration, lift);
    }
}
