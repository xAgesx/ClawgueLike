using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    [SerializeField] private int startingCoins = 100;
    [SerializeField] private int startingFragments = 0;
    public int CoinsBalance { get; private set; }
    public int FragmentsBalance { get; private set; }

    [Header("Pulls")]
    [SerializeField] private int maxPullsPerTurn = 3;
    [SerializeField] private int turnsPerRound = 3;
    public int CurrentPulls { get; private set; }
    public int CurrentTurn { get; private set; }
    public int CurrentRound { get; private set; }

    [Header("Shop")]
    [SerializeField] public int shopPanelIndex = 0;
    [SerializeField] public int gameOverPanelIndex = 1;
    [SerializeField] private float shopOpenDelay = 2f;
    [SerializeField] private float refillDelay = 1f;

    public float ShopOpenDelay => shopOpenDelay;
    public float RefillDelay => refillDelay;

    public int MaxPullsPerTurn => maxPullsPerTurn;
    public int TurnsPerRound => turnsPerRound;
    public bool IsTurnActive => CurrentPulls > 0;
    public bool IsRoundComplete => CurrentTurn >= turnsPerRound;

    [Header("Inventory")]
    [SerializeField] private List<InventoryItem> inventory = new List<InventoryItem>();

    public IReadOnlyList<InventoryItem> Inventory => inventory;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetGame();
    }

    private void Start() {
        NotifyAllUI();
    }

    public void ResetGame() {
        CoinsBalance = startingCoins;
        FragmentsBalance = startingFragments;
        CurrentPulls = maxPullsPerTurn;
        CurrentTurn = 1;
        CurrentRound = 1;
        inventory.Clear();
        NotifyAllUI();
    }

    public void AddCoins(int amount) {
        CoinsBalance += amount;
        PanelManager.Instance?.UpdateCurrencyDisplay(CoinsBalance, FragmentsBalance);
        Debug.Log($"[GameManager] Coins: {CoinsBalance}");
    }

    public void AddFragments(int amount) {
        FragmentsBalance += amount;
        PanelManager.Instance?.UpdateCurrencyDisplay(CoinsBalance, FragmentsBalance);
        Debug.Log($"[GameManager] Fragments: {FragmentsBalance}");
    }

    public bool SpendCoins(int amount) {
        if (CoinsBalance >= amount) {
            CoinsBalance -= amount;
            PanelManager.Instance?.UpdateCurrencyDisplay(CoinsBalance, FragmentsBalance);
            Debug.Log($"[GameManager] Spent {amount}, Balance: {CoinsBalance}");
            return true;
        }
        return false;
    }

    public bool SpendFragments(int amount) {
        if (FragmentsBalance >= amount) {
            FragmentsBalance -= amount;
            PanelManager.Instance?.UpdateCurrencyDisplay(CoinsBalance, FragmentsBalance);
            Debug.Log($"[GameManager] Spent {amount} fragments, Balance: {FragmentsBalance}");
            return true;
        }
        return false;
    }

    public void UsePull() {
        if (CurrentPulls > 0) {
            CurrentPulls--;
            PanelManager.Instance?.UpdatePullsDisplay(CurrentPulls);
        }
    }

    public void ResetPulls() {
        CurrentPulls = maxPullsPerTurn;
        PanelManager.Instance?.UpdatePullsDisplay(CurrentPulls);
    }

    public void AdvanceTurn() {
        CurrentTurn++;
        if (CurrentTurn > turnsPerRound) {
            CurrentTurn = 1;
            CurrentRound++;
            PanelManager.Instance?.UpdateRoundDisplay(CurrentRound);
        }
        ResetPulls();
        PanelManager.Instance?.UpdateTurnDisplay(CurrentTurn, TurnsPerRound);
        Debug.Log($"[GameManager] Turn {CurrentTurn} / Round {CurrentRound}");

        // Only open shop on turns 2+, not on first turn
        if (CurrentTurn > 1 && PanelManager.Instance != null) {
            PanelManager.Instance.ClosePanel(shopPanelIndex);
            PanelManager.Instance.OpenPanel(shopPanelIndex);
        }
    }

    public void OpenShopPanel() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.ClosePanel(shopPanelIndex);
            PanelManager.Instance.OpenPanel(shopPanelIndex);
        }
    }

    public void AddItemToInventory(ItemData itemData) {
        var existing = inventory.Find(i => i.itemData == itemData);
        if (existing != null) {
            existing.quantity++;
        } else {
            inventory.Add(new InventoryItem { itemData = itemData, quantity = 1 });
        }
        Debug.Log($"[GameManager] Added to inventory: {itemData.itemName} (qty: {GetItemQuantity(itemData)})");
    }

    public int GetItemQuantity(ItemData itemData) {
        var existing = inventory.Find(i => i.itemData == itemData);
        return existing != null ? existing.quantity : 0;
    }

    public bool HasItem(ItemData itemData, int quantity = 1) {
        return GetItemQuantity(itemData) >= quantity;
    }

    public bool SpendItem(ItemData itemData, int quantity = 1) {
        var existing = inventory.Find(i => i.itemData == itemData);
        if (existing != null && existing.quantity >= quantity) {
            existing.quantity -= quantity;
            if (existing.quantity <= 0) inventory.Remove(existing);
            return true;
        }
        return false;
    }

    private void NotifyAllUI() {
        PanelManager.Instance?.UpdateCurrencyDisplay(CoinsBalance, FragmentsBalance);
        PanelManager.Instance?.UpdatePullsDisplay(CurrentPulls);
        PanelManager.Instance?.UpdateTurnDisplay(CurrentTurn, TurnsPerRound);
        PanelManager.Instance?.UpdateRoundDisplay(CurrentRound);
    }

    public bool CanAfford(int cost) => CoinsBalance >= cost;
    public bool CanAffordFragments(int cost) => FragmentsBalance >= cost;
}

[System.Serializable]
public class InventoryItem {
    public ItemData itemData;
    public int quantity;
}