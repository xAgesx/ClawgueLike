using UnityEngine;
using UnityEngine.EventSystems;

// Makes any UI object show the info panel on hover. PanelManager adds one of these to
// every shop slot automatically - drop it on anything else (inventory icons, buff rows,
// a relic list in the HUD) and call SetInfo whenever its data changes.
public class ItemInfoTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    private string displayName;
    private string description;

    public void SetInfo(string itemName, string itemDescription) {
        displayName = itemName;
        description = itemDescription;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (ItemInfoPanel.Instance == null) return;
        if (string.IsNullOrEmpty(displayName) && string.IsNullOrEmpty(description)) return;
        ItemInfoPanel.Instance.Show(displayName, description, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData) {
        if (ItemInfoPanel.Instance != null) ItemInfoPanel.Instance.Hide();
    }

    // Shop slots get deactivated when the panel closes, and no OnPointerExit fires for
    // them, so this is what stops a tooltip outliving the thing you were hovering.
    private void OnDisable() {
        if (ItemInfoPanel.Instance != null) ItemInfoPanel.Instance.Hide();
    }
}
