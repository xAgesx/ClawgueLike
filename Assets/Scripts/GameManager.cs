using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance { get; private set; }

    [Header("Economy")]
    [SerializeField] private int startingCoins = 100;
    public int CoinsBalance { get; private set; }

    [Header("Pulls")]
    [SerializeField] private int maxPullsPerTurn = 3;
    [SerializeField] private int turnsPerRound = 3;
    public int CurrentPulls { get; private set; }
    public int CurrentTurn { get; private set; }
    public int CurrentRound { get; private set; }

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
        CurrentPulls = maxPullsPerTurn;
        CurrentTurn = 1;
        CurrentRound = 1;
    }

    public void AddCoins(int amount) {
        CoinsBalance += amount;
        Debug.Log($"[GameManager] Coins: {CoinsBalance}");
    }

    public bool SpendCoins(int amount) {
        if (CoinsBalance >= amount) {
            CoinsBalance -= amount;
            Debug.Log($"[GameManager] Spent {amount}, Balance: {CoinsBalance}");
            return true;
        }
        return false;
    }

    public void UsePull() {
        if (CurrentPulls > 0) {
            CurrentPulls--;
            Debug.Log($"[GameManager] Pull used. Remaining: {CurrentPulls}");
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
    }

    public bool CanAfford(int cost) => CoinsBalance >= cost;
}