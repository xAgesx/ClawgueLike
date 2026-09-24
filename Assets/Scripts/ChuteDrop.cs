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

        if (data.itemType == ItemType.Coin) {
            turnCoins++;
            gm.AddCoins(1);
        } else if (data.itemType == ItemType.Fragment) {
            turnFragments++;
            gm.AddFragments(1);
        }

        Debug.Log($"[ChuteDrop] Collected: {data.itemName} ({data.itemType}) | Turn Total: {turnCoins}c / {turnFragments}f");

                Destroy(other.gameObject);
       }

    private bool IsInLayerMask(int layer) {
        return (itemLayers.value & (1 << layer)) != 0;
    }

    public void ResetTurnCounters() {
        turnCoins = 0;
        turnFragments = 0;
    }

}