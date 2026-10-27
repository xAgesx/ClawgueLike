using UnityEngine;

// Echo: when it touches a coin that has moved during this turn, it copies that coin's
// value gain and gives it to a random other coin. The gain is measured from the last
// turn boundary, so a sleeping coin's growth or a snowball's packed value can echo.
[CreateAssetMenu(fileName = "EchoCoin", menuName = "Data/Effects/SpecialCoins/Echo")]
public class EchoEffect : CoinEffect {
    [Tooltip("The echoed gain is at least this much, so a moved-but-unchanged coin still echoes something.")]
    [SerializeField] private int minEcho = 1;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (target == null) return;
        if (!target.HasMovedSinceTurnStart) return;

        int gain = Mathf.Max(minEcho, target.ValueGainedThisTurn);

        CoinInstance recipient = PickRandomCoin(target);
        if (recipient != null) recipient.AddBaseValue(gain);
    }

    private CoinInstance PickRandomCoin(CoinInstance exclude) {
        CoinInstance[] all = FindObjectsByType<CoinInstance>(FindObjectsInactive.Exclude);
        int count = 0;
        foreach (CoinInstance coin in all) {
            if (coin != null && coin != exclude) count++;
        }
        if (count == 0) return null;

        int pick = Random.Range(0, count);
        foreach (CoinInstance coin in all) {
            if (coin == null || coin == exclude) continue;
            if (pick == 0) return coin;
            pick--;
        }
        return null;
    }
}
