using UnityEngine;

public class ChuteDrop : MonoBehaviour {
    [SerializeField] private LayerMask itemLayers;
    [SerializeField] private ItemSpawner itemSpawner;

    // Turn totals are static so coin effects (Banker, Tally) can read them without
    // needing a reference to the chute trigger. Reset when a new turn begins.
    private static int turnCoins;
    private static int turnFragments;

    public static int CoinsCollectedThisTurn { get; private set; }
    public static int ValueCollectedThisTurn => turnCoins;

    private void OnTriggerEnter(Collider other) {
        if (!IsInLayerMask(other.gameObject.layer)) return;

        ItemPickup pickup = other.GetComponent<ItemPickup>();
        if (pickup == null) return;

        ItemData data = pickup.ItemData;
        if (data == null) return;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        CoinInstance coin = other.GetComponentInParent<CoinInstance>();
        int collected = 0;

        if (data.itemType == ItemType.Coin) {
            CoinsCollectedThisTurn++;

            // Relics adjust baseValue / multiplier before the value is read, so they
            // stack on top of whatever the coin's own effects did to it.
            RelicManager.Instance?.ModifyCoinForCollection(coin);

            // Announce the delivery first: any Greedy coin still in the machine grows
            // before this coin's payout is read.
            SpecialCoin.DispatchCoinCollected(coin);

            collected = coin != null ? coin.TotalValue : FallbackCoinValue(data);
            collected = SpecialCoin.ApplyCollectedPayout(coin, collected);

            turnCoins += collected;
            gm.AddCoins(collected);
            RelicManager.Instance?.NotifyCoinDelivered();
        } else if (data.itemType == ItemType.Fragment) {
            turnFragments++;
            gm.AddFragments(1);
        }

        Debug.Log($"[ChuteDrop] Collected: {data.itemName} ({data.itemType}) | Value: {collected} | Turn Total: {turnCoins}c / {turnFragments}f");

        // Route through the spawner so pooled coins are returned instead of leaked.
        ItemSpawner spawner = ItemSpawner.Instance;
        if (spawner != null) spawner.DespawnCoin(pickup.gameObject);
        else Destroy(pickup.gameObject);
    }

    // Only reachable if a coin prefab somehow missed the CoinInstance component.
    private static int FallbackCoinValue(ItemData data) {
        CoinData coinData = data as CoinData;
        return coinData != null && coinData.baseValue > 0 ? coinData.baseValue : 1;
    }

    private bool IsInLayerMask(int layer) {
        return (itemLayers.value & (1 << layer)) != 0;
    }

    // Called by RoundsManager as each new turn begins. Static counters would survive
    // a play session with domain reload disabled, so they are cleared on load too.
    public static void ResetTurnCounters() {
        turnCoins = 0;
        turnFragments = 0;
        CoinsCollectedThisTurn = 0;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        ResetTurnCounters();
    }

}