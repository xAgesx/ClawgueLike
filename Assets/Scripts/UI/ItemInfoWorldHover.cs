using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

// Hovering a physical item sitting in the machine shows the same info panel the shop
// uses. Add this component to any GameObject in the scene - it points itself at the
// main camera and the "Items" layer unless you override those here.
public class ItemInfoWorldHover : MonoBehaviour {
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask itemsLayer;
    [SerializeField] private float maxDistance = 100f;

    private void Awake() {
        if (cam == null) cam = Camera.main;

        if (itemsLayer.value == 0) {
            itemsLayer = LayerMask.GetMask("Items");
            if (itemsLayer.value == 0) itemsLayer = LayerMask.GetMask("items");
        }
    }

    private void Update() {
        if (ItemInfoPanel.Instance == null || cam == null) return;

        // Shop or game over is up - the machine is not what you are reading right now.
        if (PanelManager.Instance != null && PanelManager.Instance.IsAnyPanelOpen) {
            ItemInfoPanel.Instance.HideWorld();
            return;
        }

        // UI owns the pointer whenever it is over a raycast target.
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) {
            ItemInfoPanel.Instance.HideWorld();
            return;
        }

        Mouse mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 screenPos = mouse.position.ReadValue();

        if (Physics.Raycast(cam.ScreenPointToRay(screenPos), out RaycastHit hit,
                maxDistance, itemsLayer, QueryTriggerInteraction.Collide)) {
            ItemPickup pickup = hit.collider.GetComponentInParent<ItemPickup>();
            ItemData data = pickup != null ? pickup.ItemData : null;

            if (data != null) {
                ItemInfoPanel.Instance.ShowWorld(data.itemName, (data as CoinData)?.description, screenPos);
                return;
            }
        }

        ItemInfoPanel.Instance.HideWorld();
    }
}
