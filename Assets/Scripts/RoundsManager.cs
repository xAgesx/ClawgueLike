using UnityEngine;

public class RoundsManager : MonoBehaviour {
    public static RoundsManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private ItemSpawner itemSpawner;
    [SerializeField] private int itemsPerSpawn = 50;

    [Header("Costs")]
    [SerializeField] private int resupplyCost = 20;
    public bool IsTurnActive => isTurnActive;

    private bool isTurnActive;
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
        BeginTurn();
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
        if (GameManager.Instance.SpendCoins(resupplyCost)) {
            StartCoroutine(EndRoundSequence());
        }
    }

private System.Collections.IEnumerator EndRoundSequence() {
        yield return new WaitForSecondsRealtime(GameManager.Instance.ShopOpenDelay);
        GameManager.Instance.OpenShopPanel();

        yield return new WaitForSecondsRealtime(GameManager.Instance.RefillDelay);
        if (itemSpawner != null) {
            itemSpawner.SpawnItems();
            itemsSpawnedThisRound += itemsPerSpawn;
        }
        GameManager.Instance.AdvanceTurn();
        BeginTurn();
    }
    }



