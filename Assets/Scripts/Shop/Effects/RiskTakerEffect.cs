using UnityEngine;

// Risk Taker: every delivered coin is a coin flip - double or nothing.
[CreateAssetMenu(fileName = "RiskTaker", menuName = "Data/Effects/Relics/Risk Taker")]
public class RiskTakerEffect : RelicEffect {
    [SerializeField, Range(0f, 1f)] private float doubleChance = 0.5f;
    [SerializeField, Range(0f, 1f)] private float nothingChance = 0.5f;

    public override void ModifyCoinForCollection(CoinInstance coin) {
        if (coin == null) return;

        if (nothingChance > 0f && Random.value < nothingChance) {
            coin.MultiplyMultiplier(0f);
            return;
        }

        if (doubleChance > 0f && Random.value < doubleChance) {
            coin.MultiplyMultiplier(2f);
        }
    }

    public override string GetStatusText(GameManager gm, RelicManager rm) {
        int dbl = Mathf.RoundToInt(doubleChance * 100f);
        int zero = Mathf.RoundToInt(nothingChance * 100f);
        return $"Delivered coin: {dbl}% ×2, {zero}% ×0";
    }
}
