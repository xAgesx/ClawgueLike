using UnityEngine;

// Census: on every turn boundary it counts the special coins still sitting in the
// machine and pays itself one point for each. Crowded, effect-heavy piles make it
// valuable; a lonely census coin does almost nothing.
[CreateAssetMenu(fileName = "CensusCoin", menuName = "Data/Effects/SpecialCoins/Census")]
public class CensusEffect : CoinEffect {
    [Tooltip("Value gained per special coin in the machine, including this one.")]
    [SerializeField] private int valuePerSpecial = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) { }

    public override void OnTurnStart(CoinInstance coin, SpecialCoin source) {
        if (coin == null) return;

        int specialCount = 0;
        foreach (CoinInstance other in FindObjectsByType<CoinInstance>(FindObjectsInactive.Exclude)) {
            if (other != null && other.Data != null && other.Data.IsSpecial) specialCount++;
        }

        coin.AddBaseValue(specialCount * valuePerSpecial);
    }
}
