using UnityEngine;

// PiggyBank: interest on the balance at round end, but only if the final turn was busy
// enough to earn it. Reads the delivered count straight off the RelicManager.
[CreateAssetMenu(fileName = "PiggyBank", menuName = "Data/Effects/Relics/PiggyBank")]
public class PiggyBankEffect : RelicEffect {
    [SerializeField, Range(0f, 1f)] private float interestRate = 0.1f;
    [SerializeField] private int minDeliveries = 5;

    public override void OnRoundEnd(GameManager game) {
        if (game == null) return;

        RelicManager manager = RelicManager.Instance;
        int delivered = manager != null ? manager.CoinsDeliveredThisTurn : 0;
        if (delivered < minDeliveries) return;

        int gain = Mathf.RoundToInt(game.CoinsBalance * interestRate);
        if (gain > 0) game.AddCoins(gain);
    }
}
