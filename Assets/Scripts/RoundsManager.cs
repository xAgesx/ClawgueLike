using UnityEngine;

public class RoundsManager : MonoBehaviour {
    public static RoundsManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private int itemsPerSpawn = 50;

    [Header("Costs")]
    [SerializeField] private int[] resupplyCosts = new int[20];
    [SerializeField] private float resupplyCostMultiplier = 1.5f;
    public bool IsTurnActive => isTurnActive;

    private bool isTurnActive;
    private bool isTransitioning;
    private int itemsSpawnedThisRound;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        GameManager.Instance.ResetGame();
        if (itemSpawner != null) {
            itemSpawner.SpawnNormalCoins();
        }
        BeginTurn();
    }

    private void OnEnable() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnShopClosed += OnShopClosed;
        }
    }

    private void OnDisable() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnShopClosed -= OnShopClosed;
        }
    }

    private void OnShopClosed() {
        if (isTransitioning) return;
        if (itemSpawner != null) {
            itemSpawner.SpawnInventoryCoins();
        }
    }

    public void OnPullUsed() {
        if (!isTurnActive) return;

        GameManager.Instance.UsePull();

        if (GameManager.Instance.CurrentPulls <= 0) {
            EndTurn();
        }
    }

    private void BeginTurn() {
        isTurnActive = true;
        isTransitioning = true;
        if (PanelManager.Instance != null) {
            PanelManager.Instance.ClosePanel(GameManager.Instance.shopPanelIndex);
            PanelManager.Instance.OpenPanel(GameManager.Instance.shopPanelIndex);
        }
        isTransitioning = false;
    }

    private void EndTurn() {
        isTurnActive = false;

        if (GameManager.Instance.CurrentTurn >= GameManager.Instance.TurnsPerRound) {
            EndRound();
        } else {
            GameManager.Instance.AdvanceTurn();
            BeginTurn();
        }
    }

    private void EndRound() {
        int currentRound = GameManager.Instance.CurrentRound;
        int cost;
        
        if (currentRound <= resupplyCosts.Length && resupplyCosts[currentRound - 1] > 0) {
            cost = resupplyCosts[currentRound - 1];
        } else {
            int lastIndex = Mathf.Min(currentRound - 2, resupplyCosts.Length - 1);
            int baseCost = lastIndex >= 0 ? resupplyCosts[lastIndex] : 20;
            cost = Mathf.RoundToInt(baseCost * Mathf.Pow(resupplyCostMultiplier, currentRound - resupplyCosts.Length - 1));
        }
        
        
        if (GameManager.Instance.SpendCoins(cost)) {
            StartCoroutine(EndRoundSequence());
        } else {
            GameOver();
        }
    }

    private void GameOver() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OpenPanel(GameManager.Instance.gameOverPanelIndex);
        }
    }

private System.Collections.IEnumerator EndRoundSequence() {
        isTransitioning = true;
        yield return new WaitForSecondsRealtime(GameManager.Instance.ShopOpenDelay);
        GameManager.Instance.OpenShopPanel();

        yield return new WaitForSecondsRealtime(GameManager.Instance.RefillDelay);
        if (itemSpawner != null) {
            itemSpawner.SpawnRoundAndInventoryCoins();
            itemsSpawnedThisRound += itemsPerSpawn;
        }
        GameManager.Instance.AdvanceTurn();
        BeginTurn();
        isTransitioning = false;
    }
}



