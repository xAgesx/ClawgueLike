using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PanelManager : MonoBehaviour {
    public static PanelManager Instance { get; private set; }

    public event Action<int, int> OnCurrencyChanged; // coins, fragments
    public event Action<int> OnPullsChanged;
    public event Action<int, int> OnTurnChanged; // currentTurn, maxTurns
    public event Action<int> OnRoundChanged;
    public event Action OnShopClosed;

    [System.Serializable]
    public class PanelEntry {
        public GameObject panel;
        public bool pauseGame = true;
    }

    [Serializable]
    public class ShopItemUISlot {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public GameObject unavailableOverlay;
        public Button buyButton;
    }

    [Serializable]
    public class ShopRelicUISlot {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public GameObject unavailableOverlay;
        public Button buyButton;
    }

    [Serializable]
    public class ShopCheatUISlot {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text priceText;
        public GameObject unavailableOverlay;
        public Button buyButton;
    }

    [Header("Panels")]
    [SerializeField] private List<PanelEntry> panels = new List<PanelEntry>();
    [SerializeField] private GameObject gameUIPanel;

    [Header("Shop UI - Coin Slots")]
    [SerializeField] private List<ShopItemUISlot> coinSlots = new List<ShopItemUISlot>();

    [Header("Shop UI - Relic Slots")]
    [SerializeField] private List<ShopRelicUISlot> relicSlots = new List<ShopRelicUISlot>();

    [Header("Shop UI - Cheat Slots")]
    [SerializeField] private List<ShopCheatUISlot> cheatSlots = new List<ShopCheatUISlot>();

    [Header("Reroll UI")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TMP_Text rerollButtonText;
    [SerializeField] private TMP_Text rerollCostText;

    private int openPanelCount = 0;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (gameUIPanel != null) {
            gameUIPanel.SetActive(true);
        }
    }

    public void OpenPanel(int index) {
        if (index < 0 || index >= panels.Count) return;

        var entry = panels[index];
        if (entry.panel == null) return;

        if (!entry.panel.activeSelf) {
            openPanelCount++;
            UpdateGameUIPanel();
        }

        entry.panel.SetActive(true);

        if (entry.pauseGame) {
            Time.timeScale = 0f;
        }
    }

    public void ClosePanel(int index) {
        if (index < 0 || index >= panels.Count) return;

        var entry = panels[index];
        if (entry.panel == null) return;

        bool wasShop = (index == GameManager.Instance?.shopPanelIndex);
        bool wasActive = entry.panel.activeSelf;

        if (wasActive) {
            openPanelCount--;
            UpdateGameUIPanel();
        }

        entry.panel.SetActive(false);

        if (wasShop && wasActive) {
            OnShopClosed?.Invoke();
        }

        if (entry.pauseGame) {
            if (openPanelCount == 0) {
                Time.timeScale = 1f;
            }
        }
    }

    public void TogglePanel(int index) {
        if (index < 0 || index >= panels.Count) return;

        var entry = panels[index];
        if (entry.panel == null) return;

        if (entry.panel.activeSelf) {
            ClosePanel(index);
        } else {
            OpenPanel(index);
        }
    }

    public void CloseAllPanels() {
        foreach (var entry in panels) {
            if (entry.panel != null) entry.panel.SetActive(false);
        }
        openPanelCount = 0;
        UpdateGameUIPanel();
        Time.timeScale = 1f;
    }

    public bool IsPanelOpen(int index) {
        if (index < 0 || index >= panels.Count) return false;
        return panels[index].panel != null && panels[index].panel.activeSelf;
    }

    // Shop UI Updates
    public void UpdateShopCoins(List<ShopManager.ShopItemEntry> coins) {
        for (int i = 0; i < coinSlots.Count; i++) {
            var slot = coinSlots[i];
            if (slot == null) continue;
            
            if (i < coins.Count) {
                var entry = coins[i];
                var data = entry.itemData;
                if (slot.icon != null) slot.icon.sprite = data.icon;
                if (slot.nameText != null) slot.nameText.text = data.itemName;
                if (slot.priceText != null) slot.priceText.text = entry.price.ToString();
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(!entry.isAvailable);
                if (slot.buyButton != null) slot.buyButton.interactable = entry.isAvailable;
            } else {
                if (slot.icon != null) slot.icon.sprite = null;
                if (slot.nameText != null) slot.nameText.text = "";
                if (slot.priceText != null) slot.priceText.text = "";
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(true);
                if (slot.buyButton != null) slot.buyButton.interactable = false;
            }
        }
    }

    public void UpdateRelics(List<ShopManager.RelicEntry> relics) {
        for (int i = 0; i < relicSlots.Count; i++) {
            var slot = relicSlots[i];
            if (slot == null) continue;
            
            if (i < relics.Count) {
                var entry = relics[i];
                var data = entry.relicData;
                if (slot.icon != null) slot.icon.sprite = data.icon;
                if (slot.nameText != null) slot.nameText.text = data.relicName;
                if (slot.priceText != null) slot.priceText.text = entry.price.ToString();
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(!entry.isAvailable);
                if (slot.buyButton != null) slot.buyButton.interactable = entry.isAvailable;
            } else {
                if (slot.icon != null) slot.icon.sprite = null;
                if (slot.nameText != null) slot.nameText.text = "";
                if (slot.priceText != null) slot.priceText.text = "";
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(true);
                if (slot.buyButton != null) slot.buyButton.interactable = false;
            }
        }
    }

    public void UpdateCheats(List<ShopManager.CheatEntry> cheats) {
        for (int i = 0; i < cheatSlots.Count; i++) {
            var slot = cheatSlots[i];
            if (slot == null) continue;
            
            if (i < cheats.Count) {
                var entry = cheats[i];
                var data = entry.cheatData;
                if (slot.icon != null) slot.icon.sprite = data.icon;
                if (slot.nameText != null) slot.nameText.text = data.cheatName;
                if (slot.priceText != null) slot.priceText.text = entry.price.ToString();
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(!entry.isAvailable);
                if (slot.buyButton != null) slot.buyButton.interactable = entry.isAvailable;
            } else {
                if (slot.icon != null) slot.icon.sprite = null;
                if (slot.nameText != null) slot.nameText.text = "";
                if (slot.priceText != null) slot.priceText.text = "";
                if (slot.unavailableOverlay != null) slot.unavailableOverlay.SetActive(true);
                if (slot.buyButton != null) slot.buyButton.interactable = false;
            }
        }
    }

    public void UpdateRerollButton(int cost) {
        if (rerollButtonText != null) {
            rerollButtonText.text = $"Reroll\n{cost}";
        }
        if (rerollCostText != null) {
            rerollCostText.text = cost.ToString();
        }
    }

    public void UpdateCurrencyDisplay(int coins, int fragments) {
        OnCurrencyChanged?.Invoke(coins, fragments);
    }

    public void UpdatePullsDisplay(int pulls) {
        OnPullsChanged?.Invoke(pulls);
    }

    public void UpdateTurnDisplay(int currentTurn, int maxTurns) {
        OnTurnChanged?.Invoke(currentTurn, maxTurns);
    }

    public void UpdateRoundDisplay(int round) {
        OnRoundChanged?.Invoke(round);
    }

    private void UpdateGameUIPanel() {
        if (gameUIPanel != null) {
            gameUIPanel.SetActive(openPanelCount == 0);
        }
    }
}