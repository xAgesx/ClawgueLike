using UnityEngine;

// All In: chute coins pay far more, but the balance bleeds at the start of every turn.
[CreateAssetMenu(fileName = "AllIn", menuName = "Data/Effects/Relics/All In")]
public class AllInEffect : RelicEffect {
    [SerializeField] private float coinMultiplier = 1.5f;
    [SerializeField, Range(0f, 1f)] private float balanceDrainPerTurn = 0.1f;

    public override void ModifyCoinForCollection(CoinInstance coin) {
        if (coin != null) coin.MultiplyMultiplier(coinMultiplier);
    }

    public override void OnTurnStart(GameManager game) {
        if (game == null) return;

        int lose = Mathf.RoundToInt(game.CoinsBalance * balanceDrainPerTurn);
        if (lose > 0) game.SpendCoins(lose);
    }

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        int coinPct = Mathf.RoundToInt((coinMultiplier - 1f) * 100f);
        return $"+{coinPct}% chute";
    }
}
