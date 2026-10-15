using UnityEngine;

// Blood Pact: an extra pull every turn, paid for with a chunk of the balance each
// round end.
[CreateAssetMenu(fileName = "BloodPact", menuName = "Data/Effects/Relics/Blood Pact")]
public class BloodPactEffect : RelicEffect {
    [SerializeField] private int extraPulls = 1;
    [SerializeField, Range(0f, 1f)] private float balanceDrainPerRound = 0.08f;

    public override int ExtraPulls() => extraPulls;

    public override void OnRoundEnd(GameManager game) {
        if (game == null) return;

        int lose = Mathf.RoundToInt(game.CoinsBalance * balanceDrainPerRound);
        if (lose > 0) game.SpendCoins(lose);
    }
}
