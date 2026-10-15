using UnityEngine;

[CreateAssetMenu(fileName = "PiggyBank", menuName = "Data/Effects/Relics/PiggyBank")]
public class PiggyBankEffect : RelicEffect {
    [SerializeField, Range(0f, 1f)] private float interestRate = 0.1f;
    [SerializeField] private int minDeliveries = 5;
    [System.NonSerialized] public int totalEarned;

    public override void OnRoundEnd(GameManager game) {
        if (game == null) return;
        RelicManager m = RelicManager.Instance;
        int d = m != null ? m.CoinsDeliveredThisTurn : 0;
        if (d < minDeliveries) return;
        int g = Mathf.RoundToInt(game.CoinsBalance * interestRate);
        if (g > 0) {
            game.AddCoins(g);
            totalEarned += g;
        }
    }

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        return "+" + totalEarned + " so far";
    }
}
