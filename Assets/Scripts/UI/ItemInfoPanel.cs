using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// The hover tooltip itself. Drop this component anywhere in the scene (normally the
// Canvas), drag your panel object into infoPanel and point nameText / descriptionText
// at the two TMP fields inside it. ItemInfoTrigger finds it through Instance.
public class ItemInfoPanel : MonoBehaviour {
    public static ItemInfoPanel Instance { get; private set; }

    [Header("Wired Manually")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Placement")]
    [Tooltip("Pushed away from the cursor so the panel never hides what you hover.")]
    [SerializeField] private Vector2 offset = new Vector2(18f, -18f);
    [SerializeField] private Vector2 screenPadding = new Vector2(10f, 10f);

    private RectTransform panelRect;
    private RectTransform containerRect;
    private Camera uiCamera;
    private bool isShown;
    private bool shownByWorld;

    public bool IsShown => isShown;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Debug.LogWarning("[ItemInfoPanel] Duplicate ItemInfoPanel in the scene - keeping the first one.", this);
            enabled = false;
            return;
        }
        Instance = this;

        if (infoPanel == null) return;

        panelRect = infoPanel.GetComponent<RectTransform>();
        if (panelRect == null) {
            Debug.LogError("[ItemInfoPanel] infoPanel needs a RectTransform.", this);
            return;
        }
        containerRect = panelRect.parent as RectTransform;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindFirstObjectByType<Canvas>();
        uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;

        // A tooltip must never swallow the pointer - otherwise leaving the slot it is
        // covering becomes impossible and the panel flickers in and out.
        CanvasGroup group = infoPanel.GetComponent<CanvasGroup>();
        if (group == null) group = infoPanel.AddComponent<CanvasGroup>();
        group.blocksRaycasts = false;
        group.interactable = false;

        infoPanel.transform.SetAsLastSibling();
        infoPanel.SetActive(false);
    }

    private void OnDestroy() {
        if (Instance == this) Instance = null;
    }

    // Shop cards and the machine behind them both drive this panel. Tracking which one
    // owns it stops them fighting over the pointer as it moves between a card and a coin.
    public void Show(string itemName, string description, Vector2 screenPosition) {
        shownByWorld = false;
        ShowInternal(itemName, description, screenPosition);
    }

    public void ShowWorld(string itemName, string description, Vector2 screenPosition) {
        shownByWorld = true;
        ShowInternal(itemName, description, screenPosition);
    }

    public void Hide() {
        shownByWorld = false;
        HideInternal();
    }

    // Never steals a tooltip that a UI element is showing.
    public void HideWorld() {
        if (!shownByWorld) return;
        HideInternal();
    }

    private void ShowInternal(string itemName, string description, Vector2 screenPosition) {
        if (infoPanel == null || panelRect == null) return;

        if (nameText != null) nameText.text = itemName ?? string.Empty;
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;

        if (!infoPanel.activeSelf) infoPanel.SetActive(true);
        isShown = true;
        PlaceAt(screenPosition);
    }

    private void HideInternal() {
        isShown = false;
        if (infoPanel != null && infoPanel.activeSelf) infoPanel.SetActive(false);
    }

    private void Update() {
        if (!isShown) return;
        Mouse mouse = Mouse.current;
        if (mouse != null) PlaceAt(mouse.position.ReadValue());
    }

    private void PlaceAt(Vector2 screenPosition) {
        if (panelRect == null || containerRect == null) return;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect, screenPosition, uiCamera, out Vector2 local)) return;

        local += offset;

        // Keep the whole panel inside the canvas instead of letting it run off-screen.
        Rect area = containerRect.rect;
        Vector2 size = panelRect.rect.size;
        Vector2 pivotOffset = Vector2.Scale(size, panelRect.pivot);

        float minX = area.xMin + screenPadding.x + pivotOffset.x;
        float maxX = area.xMax - screenPadding.x - size.x + pivotOffset.x;
        float minY = area.yMin + screenPadding.y + pivotOffset.y;
        float maxY = area.yMax - screenPadding.y - size.y + pivotOffset.y;

        local.x = minX <= maxX ? Mathf.Clamp(local.x, minX, maxX) : (minX + maxX) * 0.5f;
        local.y = minY <= maxY ? Mathf.Clamp(local.y, minY, maxY) : (minY + maxY) * 0.5f;

        panelRect.localPosition = local;
    }
}
