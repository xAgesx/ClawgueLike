using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Item Pools")]
    [SerializeField] private List<ShopItemData> coinPool = new List<ShopItemData>();
    [SerializeField] private List<RelicData> relicPool = new List<RelicData>();
    [SerializeField] private List<CheatData> cheatPool = new List<CheatData>();

    [Header("Shop Configuration")]
    [SerializeField] private int baseCoinSlots = 4;
    [SerializeField] private int baseRelicSlots = 3;
    [SerializeField] private int baseCheatSlots = 3;
    [SerializeField] private int coinSlotUpgradeLevel = 0;
    [SerializeField] private int relicSlotUpgradeLevel = 0;
    [SerializeField] private int cheatSlotUpgradeLevel = 0;

    [Header("Price Multipliers (per round)")]
    [SerializeField] private float coinPriceMultiplier = 1.2f;
    [SerializeField] private float relicPriceMultiplier = 1.2f;
    [SerializeField] private float cheatPriceMultiplier = 1.2f;

    [Header("Reroll")]
    [SerializeField] private int baseRerollCost = 10;
    [SerializeField] private float rerollCostMultiplier = 1.5f;

    [Header("Upgrade Costs (per level)")]
    [SerializeField] private int[] coinSlotUpgradeCosts = { 50, 100, 200 };
    [SerializeField] private int[] relicSlotUpgradeCosts = { 50, 100, 200 };
    [SerializeField] private int[] cheatSlotUpgradeCosts = { 50, 100, 200 };

    // Runtime state
    private int currentRound = 1;
    private int currentRerollCost;
    private int currentCoinSlots;
    private int currentRelicSlots;
    private int currentCheatSlots;

    private List<ShopItemEntry> currentCoins = new List<ShopItemEntry>();
    private List<RelicEntry> currentRelics = new List<RelicEntry>();
    private List<CheatEntry> currentCheats = new List<CheatEntry>();

    private List<ShopItemData> purchasedCoinsThisSession = new List<ShopItemData>();
    private int rerollsThisSession = 0;

    public event Action OnShopRefreshed;
    public event Action OnRerollPerformed;
    public event Action OnItemPurchased;

    public void BuyCoinAtIndex(int index) => TryBuyCoin(index);
    public void BuyRelicAtIndex(int index) => BuyRelic(index);
    public void BuyCheatAtIndex(int index) => BuyCheat(index);

    public void RerollShop() => PerformReroll();
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeShop();
    }

    private void InitializeShop()
    {
        currentRound = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
        currentRerollCost = baseRerollCost;
        currentCoinSlots = baseCoinSlots + coinSlotUpgradeLevel;
        currentRelicSlots = baseRelicSlots + relicSlotUpgradeLevel;
        currentCheatSlots = baseCheatSlots + cheatSlotUpgradeLevel;

        RerollAll();
        UpdateShopUI();
    }

    public void OnRoundStarted(int round)
    {
        currentRound = round;
        currentRerollCost = baseRerollCost;
        rerollsThisSession = 0;
        purchasedCoinsThisSession.Clear();

        RerollAll();
    }

    public void OnTurnEnded()
    {
        RerollAll();
    }

    private void RerollAll()
    {
        RerollCoins();
        RerollRelics();
        RerollCheats();
        UpdateShopUI();
        OnShopRefreshed?.Invoke();
    }

    private void UpdateShopUI()
    {
        if (PanelManager.Instance != null)
        {
            PanelManager.Instance.UpdateShopCoins(currentCoins);
            PanelManager.Instance.UpdateRelics(currentRelics);
            PanelManager.Instance.UpdateCheats(currentCheats);
            PanelManager.Instance.UpdateRerollButton(currentRerollCost);
        }
    }

    private void RerollCoins()
    {
        currentCoins.Clear();
        var available = new List<ShopItemData>(coinPool);
        available.RemoveAll(item => purchasedCoinsThisSession.Contains(item));

        int count = Mathf.Min(currentCoinSlots, available.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var item = available[idx];
            available.RemoveAt(idx);

            int price = CalculatePrice(item.basePrice, coinPriceMultiplier);
            currentCoins.Add(new ShopItemEntry { itemData = item, price = price, isAvailable = true });
        }
    }

    private void RerollRelics()
    {
        currentRelics.Clear();
        var available = new List<RelicData>(relicPool);

        int count = Mathf.Min(currentRelicSlots, available.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var item = available[idx];
            available.RemoveAt(idx);

            int price = CalculatePrice(item.basePrice, relicPriceMultiplier);
            currentRelics.Add(new RelicEntry { relicData = item, price = price, isAvailable = true });
        }
    }

    private void RerollCheats()
    {
        currentCheats.Clear();
        var available = new List<CheatData>(cheatPool);

        int count = Mathf.Min(currentCheatSlots, available.Count);
        for (int i = 0; i < count; i++)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var item = available[idx];
            available.RemoveAt(idx);

            int price = CalculatePrice(item.basePrice, cheatPriceMultiplier);
            currentCheats.Add(new CheatEntry { cheatData = item, price = price, isAvailable = true });
        }
    }

    private int CalculatePrice(int basePrice, float roundMultiplier)
    {
        float price = basePrice * Mathf.Pow(roundMultiplier, GameManager.Instance != null ? GameManager.Instance.CurrentRound - 1 : 0);
        return Mathf.RoundToInt(price);
    }

    public int GetCurrentRerollCost() => currentRerollCost;

    public void PerformReroll()
    {
        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(currentRerollCost))
        {
            rerollsThisSession++;
            currentRerollCost = Mathf.RoundToInt(currentRerollCost * rerollCostMultiplier);
            RerollAll();
            UpdateShopUI();
            OnRerollPerformed?.Invoke();
        }
    }

    public bool TryBuyCoin(int index)
    {
        if (index < 0 || index >= currentCoins.Count) return false;
        var entry = currentCoins[index];
        if (!entry.isAvailable) return false;

        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(entry.price))
        {
            entry.isAvailable = false;
            purchasedCoinsThisSession.Add(entry.itemData);
            GameManager.Instance.AddItemToInventory(entry.itemData);
            UpdateShopUI();
            OnItemPurchased?.Invoke();
            return true;
        }
        return false;
    }

    public bool BuyRelic(int index)
    {
        if (index < 0 || index >= currentRelics.Count) return false;
        var entry = currentRelics[index];
        if (!entry.isAvailable) return false;

        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(entry.price))
        {
            entry.isAvailable = false;
            RerollSingleRelic(index);
            GameManager.Instance.AddItemToInventory(entry.relicData);
            UpdateShopUI();
            OnItemPurchased?.Invoke();
            return true;
        }
        return false;
    }

    public bool BuyCheat(int index)
    {
        if (index < 0 || index >= currentCheats.Count) return false;
        var entry = currentCheats[index];
        if (!entry.isAvailable) return false;

        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(entry.price))
        {
            entry.isAvailable = false;
            RerollSingleCheat(index);
            GameManager.Instance.AddItemToInventory(entry.cheatData);
            UpdateShopUI();
            OnItemPurchased?.Invoke();
            return true;
        }
        return false;
    }

    private void RerollSingleRelic(int index)
    {
        var available = new List<RelicData>(relicPool);
        available.RemoveAll(r => currentRelics.Exists(e => e.relicData == r));

        if (available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var item = available[idx];
            int price = CalculatePrice(item.basePrice, relicPriceMultiplier);
            currentRelics[index] = new RelicEntry { relicData = item, price = price, isAvailable = true };
        }
    }

    private void RerollSingleCheat(int index)
    {
        var available = new List<CheatData>(cheatPool);
        available.RemoveAll(c => currentCheats.Exists(e => e.cheatData == c));

        if (available.Count > 0)
        {
            int idx = UnityEngine.Random.Range(0, available.Count);
            var item = available[idx];
            int price = CalculatePrice(item.basePrice, cheatPriceMultiplier);
            currentCheats[index] = new CheatEntry { cheatData = item, price = price, isAvailable = true };
        }
    }

    // Slot upgrades
    public bool TryUpgradeCoinSlots()
    {
        if (coinSlotUpgradeLevel >= coinSlotUpgradeCosts.Length) return false;
        int cost = coinSlotUpgradeCosts[coinSlotUpgradeLevel];
        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
        {
            coinSlotUpgradeLevel++;
            currentCoinSlots = baseCoinSlots + coinSlotUpgradeLevel;
            return true;
        }
        return false;
    }

    public bool TryUpgradeRelicSlots()
    {
        if (relicSlotUpgradeLevel >= relicSlotUpgradeCosts.Length) return false;
        int cost = relicSlotUpgradeCosts[relicSlotUpgradeLevel];
        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
        {
            relicSlotUpgradeLevel++;
            currentRelicSlots = baseRelicSlots + relicSlotUpgradeLevel;
            return true;
        }
        return false;
    }

    public bool TryUpgradeCheatSlots()
    {
        if (cheatSlotUpgradeLevel >= cheatSlotUpgradeCosts.Length) return false;
        int cost = cheatSlotUpgradeCosts[cheatSlotUpgradeLevel];
        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
        {
            cheatSlotUpgradeLevel++;
            currentCheatSlots = baseCheatSlots + cheatSlotUpgradeLevel;
            return true;
        }
        return false;
    }

    public int GetCoinSlotUpgradeCost() => coinSlotUpgradeLevel < coinSlotUpgradeCosts.Length ? coinSlotUpgradeCosts[coinSlotUpgradeLevel] : -1;
    public int GetRelicSlotUpgradeCost() => relicSlotUpgradeLevel < relicSlotUpgradeCosts.Length ? relicSlotUpgradeCosts[relicSlotUpgradeLevel] : -1;
    public int GetCheatSlotUpgradeCost() => cheatSlotUpgradeLevel < cheatSlotUpgradeCosts.Length ? cheatSlotUpgradeCosts[cheatSlotUpgradeLevel] : -1;

    // Getters for UI
    public List<ShopItemEntry> GetCurrentCoins() => currentCoins;
    public List<RelicEntry> GetCurrentRelics() => currentRelics;
    public List<CheatEntry> GetCurrentCheats() => currentCheats;

    public int CurrentCoinSlots => currentCoinSlots;
    public int CurrentRelicSlots => currentRelicSlots;
    public int CurrentCheatSlots => currentCheatSlots;
    public int CoinSlotUpgradeLevel => coinSlotUpgradeLevel;
    public int RelicSlotUpgradeLevel => relicSlotUpgradeLevel;
    public int CheatSlotUpgradeLevel => cheatSlotUpgradeLevel;

    [Serializable]
    public class ShopItemEntry
    {
        public ShopItemData itemData;
        public int price;
        public bool isAvailable;
    }

    [Serializable]
    public class RelicEntry
    {
        public RelicData relicData;
        public int price;
        public bool isAvailable;
    }

    [Serializable]
    public class CheatEntry
    {
        public CheatData cheatData;
        public int price;
        public bool isAvailable;
    }

    public void OnShopClosed()
    {
        currentRerollCost = baseRerollCost;
        rerollsThisSession = 0;
    }
}