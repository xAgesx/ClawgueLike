using UnityEngine;

public class ChuteDrop : MonoBehaviour {
    [SerializeField] private LayerMask itemLayers;
    [SerializeField] private ItemSpawner itemSpawner;

    private int turnCoins;
    private int turnFragments;

    

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
            collected = coin != null ? coin.TotalValue : FallbackCoinValue(data);
            turnCoins += collected;
            gm.AddCoins(collected);
        } else if (data.itemType == ItemType.Fragment) {
            turnFragments++;
            gm.AddFragments(1);
        }

        Debug.Log($"[ChuteDrop] Collected: {data.itemName} ({data.itemType}) | Value: {collected} | Turn Total: {turnCoins}c / {turnFragments}f");

                Destroy(other.gameObject);
       }

    // Only reachable if a coin prefab somehow missed the CoinInstance component.
    private static int FallbackCoinValue(ItemData data) {
        CoinData coinData = data as CoinData;
        return coinData != null && coinData.baseValue > 0 ? coinData.baseValue : 1;
    }

    private bool IsInLayerMask(int layer) {
        return (itemLayers.value & (1 << layer)) != 0;
    }

    public void ResetTurnCounters() {
        turnCoins = 0;
        turnFragments = 0;
    }

}