using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ItemPickup : MonoBehaviour {
    [SerializeField] private ItemData itemData;

    public ItemData ItemData => itemData;
}
