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
    [SerializeField] public int shopPanelIndex = 1;

    public int MaxPullsPerTurn => maxPullsPerTurn;
    public int TurnsPerRound => turnsPerRound;
    public bool IsTurnActive => CurrentPulls > 0;
    public bool IsRoundComplete => CurrentTurn >= turnsPerRound;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        ResetGame();
    }

    public void ResetGame() {
        CoinsBalance = startingCoins;
        FragmentsBalance = startingFragments;
        CurrentPulls = maxPullsPerTurn;
        CurrentTurn = 1;
        CurrentRound = 1;
    }

    public void AddCoins(int amount) {
        CoinsBalance += amount;
        Debug.Log($"[GameManager] Coins: {CoinsBalance}");
    }

    public void AddFragments(int amount) {
        FragmentsBalance += amount;
        Debug.Log($"[GameManager] Fragments: {FragmentsBalance}");
    }

    public bool SpendCoins(int amount) {
        if (CoinsBalance >= amount) {
            CoinsBalance -= amount;
            Debug.Log($"[GameManager] Spent {amount}, Balance: {CoinsBalance}");
            return true;
        }
        return false;
    }

    public bool SpendFragments(int amount) {
        if (FragmentsBalance >= amount) {
            FragmentsBalance -= amount;
            Debug.Log($"[GameManager] Spent {amount} fragments, Balance: {FragmentsBalance}");
            return true;
        }
        return false;
    }

    public void UsePull() {
        if (CurrentPulls > 0) {
            CurrentPulls--;
        }
    }

    public void ResetPulls() {
        CurrentPulls = maxPullsPerTurn;
    }

    public void AdvanceTurn() {
        CurrentTurn++;
        if (CurrentTurn > turnsPerRound) {
            CurrentTurn = 1;
            CurrentRound++;
        }
        ResetPulls();
        Debug.Log($"[GameManager] Turn {CurrentTurn} / Round {CurrentRound}");

        if (PanelManager.Instance != null) {
            StartCoroutine(OpenShopPanelDelayed());
        }
    }

    private System.Collections.IEnumerator OpenShopPanelDelayed() {
        yield return new WaitForSecondsRealtime(0.5f);
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OpenPanel(shopPanelIndex);
        }
    }

    public bool CanAfford(int cost) => CoinsBalance >= cost;
    public bool CanAffordFragments(int cost) => FragmentsBalance >= cost;
}