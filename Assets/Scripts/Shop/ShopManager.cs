using System;
using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Item Pools")]
    [SerializeField] private List<CoinData> coinPool = new List<CoinData>();
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

    private int rerollsThisSession = 0;
    private int freeRerollsRemaining = 0;

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

    private void OnDisable()
    {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnShopClosed -= OnShopClosed;
            PanelManager.Instance.OnShopOpened -= OnShopOpened;
        }
    }

    private void InitializeShop()
    {
        currentRound = GameManager.Instance != null ? GameManager.Instance.CurrentRound : 1;
        currentRerollCost = baseRerollCost;
        RecalculateSlots();
        freeRerollsRemaining = RelicManager.Instance != null ? RelicManager.Instance.FreeRerolls() : 0;

        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnShopClosed += OnShopClosed;
            PanelManager.Instance.OnShopOpened += OnShopOpened;
        }

        RerollAll();
        UpdateShopUI();
    }

    // Walking back up to the shop should present a fresh board, not whatever was
    // left sitting there from the last visit.
    private void OnShopOpened()
    {
        RecalculateSlots();
        freeRerollsRemaining = RelicManager.Instance != null ? RelicManager.Instance.FreeRerolls() : 0;
        RerollAll();
    }

    public void OnRoundStarted(int round)
    {
        currentRound = round;
        currentRerollCost = baseRerollCost;
        rerollsThisSession = 0;
        RecalculateSlots();
        freeRerollsRemaining = RelicManager.Instance != null ? RelicManager.Instance.FreeRerolls() : 0;

        RerollAll();
    }

    // Base slots plus shop upgrades plus whatever the owned relics add or remove.
    // Clamped at zero so a "one fewer relic slot" curse can never go negative.
    private void RecalculateSlots()
    {
        RelicManager relics = RelicManager.Instance;
        int bonusCoin = relics != null ? relics.ExtraCoinSlots() : 0;
        int bonusRelic = relics != null ? relics.ExtraRelicSlots() : 0;

        currentCoinSlots = Mathf.Max(0, baseCoinSlots + coinSlotUpgradeLevel + bonusCoin);
        currentRelicSlots = Mathf.Max(0, baseRelicSlots + relicSlotUpgradeLevel + bonusRelic);
        currentCheatSlots = Mathf.Max(0, baseCheatSlots + cheatSlotUpgradeLevel);
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

    // The paid/free reroll only re-shuffles the coin row - relics and cheats are stable
    // offers the player saves up for, so they stay put until the round rolls over.
    private void RerollCoinsOnly()
    {
        RerollCoins();
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
            PanelManager.Instance.UpdateRerollButton(GetRerollCostNow());
        }
    }

    private void RerollCoins()
    {
        currentCoins.Clear();

        // Whatever the player has already bought is not held back - a coin that was
        // picked up last turn can turn up again and be bought again.
        var items = PickSlots(coinPool, currentCoinSlots);

        foreach (CoinData item in items)
        {
            int price = CalculatePrice(item.basePrice, coinPriceMultiplier);
            currentCoins.Add(new ShopItemEntry { itemData = item, price = price, isAvailable = true });
        }
    }

    private void RerollRelics()
    {
        currentRelics.Clear();

        foreach (RelicData item in PickSlots(relicPool, currentRelicSlots))
        {
            int price = CalculatePrice(item.basePrice, relicPriceMultiplier);
            currentRelics.Add(new RelicEntry { relicData = item, price = price, isAvailable = true });
        }
    }

    private void RerollCheats()
    {
        currentCheats.Clear();

        foreach (CheatData item in PickSlots(cheatPool, currentCheatSlots))
        {
            int price = CalculatePrice(item.basePrice, cheatPriceMultiplier);
            currentCheats.Add(new CheatEntry { cheatData = item, price = price, isAvailable = true });
        }
    }

    // Picks `count` distinct items for a shop row. Null entries are dropped so one bad
    // reference cannot stall the loop, and with the pool smaller than the row the row
    // just comes out short.
    private static List<T> PickSlots<T>(List<T> pool, int count) where T : class
    {
        var results = new List<T>(count);
        if (pool == null || count <= 0) return results;

        var eligible = new List<T>(pool);
        eligible.RemoveAll(item => item == null);

        for (int i = 0; i < count && eligible.Count > 0; i++)
        {
            int idx = UnityEngine.Random.Range(0, eligible.Count);
            results.Add(eligible[idx]);
            eligible.RemoveAt(idx);
        }

        return results;
    }

    private int CalculatePrice(int basePrice, float roundMultiplier)
    {
        float price = basePrice * Mathf.Pow(roundMultiplier, GameManager.Instance != null ? GameManager.Instance.CurrentRound - 1 : 0);

        RelicManager relics = RelicManager.Instance;
        if (relics != null) {
            price *= relics.ShopPriceMultiplier();
            // Haggler-style creep: each reroll already made this round nudges prices up.
            price *= 1f + relics.ShopPriceRerollCreep() * rerollsThisSession;
        }

        return Mathf.RoundToInt(price);
    }

    public int GetCurrentRerollCost() => GetRerollCostNow();

    // Free rerolls (Fence) come first; once they are spent every paid reroll is scaled
    // by any relic multiplier. The visible cost always reflects this.
    private int GetRerollCostNow()
    {
        if (freeRerollsRemaining > 0) return 0;

        float mult = RelicManager.Instance != null ? RelicManager.Instance.PaidRerollCostMultiplier() : 1f;
        return Mathf.RoundToInt(currentRerollCost * mult);
    }

    public void PerformReroll()
    {
        if (freeRerollsRemaining > 0) {
            freeRerollsRemaining--;
            rerollsThisSession++;
            RerollCoinsOnly();
            UpdateShopUI();
            OnRerollPerformed?.Invoke();
            return;
        }

        int cost = GetRerollCostNow();
        if (GameManager.Instance != null && GameManager.Instance.SpendCoins(cost))
        {
            rerollsThisSession++;
            currentRerollCost = Mathf.RoundToInt(currentRerollCost * rerollCostMultiplier);
            RerollCoinsOnly();
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
            GameManager.Instance.AddRelicToInventory(entry.relicData);

            // A relic can change the number of slots (Overstock). If it did, rebuild
            // the whole board so the rows match; otherwise just refill the bought slot.
            int coinBefore = currentCoinSlots;
            int relicBefore = currentRelicSlots;
            RecalculateSlots();

            // A relic can hand out free rerolls the moment it is bought (Fence), so
            // refresh the count now instead of waiting for the next shop visit.
            freeRerollsRemaining = RelicManager.Instance != null ? RelicManager.Instance.FreeRerolls() : 0;

            if (currentCoinSlots != coinBefore || currentRelicSlots != relicBefore) {
                RerollAll();
            } else {
                RerollSingleRelic(index);
                UpdateShopUI();
            }

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
            RecalculateSlots();
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
            RecalculateSlots();
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
            RecalculateSlots();
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
        public CoinData itemData;
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
        UpdateShopUI();
    }
}