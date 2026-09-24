using TMPro;
using UnityEngine;

public class GameOverlay : MonoBehaviour {
    [Header("Currency")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text fragmentsText;

    [Header("Pulls")]
    [SerializeField] private TMP_Text pullsText;
    [SerializeField] private TMP_Text maxPullsText;

    [Header("Turn/Round")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text roundText;

    private void OnEnable() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnCurrencyChanged += OnCurrencyChanged;
            PanelManager.Instance.OnPullsChanged += OnPullsChanged;
            PanelManager.Instance.OnTurnChanged += OnTurnChanged;
            PanelManager.Instance.OnRoundChanged += OnRoundChanged;

            // Initial sync
            var gm = GameManager.Instance;
            if (gm != null) {
                OnCurrencyChanged(gm.CoinsBalance, gm.FragmentsBalance);
                OnPullsChanged(gm.CurrentPulls);
                OnTurnChanged(gm.CurrentTurn, gm.TurnsPerRound);
                OnRoundChanged(gm.CurrentRound);
            }
        }
    }

    private void OnDisable() {
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            PanelManager.Instance.OnPullsChanged -= OnPullsChanged;
            PanelManager.Instance.OnTurnChanged -= OnTurnChanged;
            PanelManager.Instance.OnRoundChanged -= OnRoundChanged;
        }
    }

    private void OnCurrencyChanged(int coins, int fragments) {
        if (coinsText != null) coinsText.text = coins.ToString();
        if (fragmentsText != null) fragmentsText.text = fragments.ToString();
    }

    private void OnPullsChanged(int pulls) {
        if (pullsText != null) pullsText.text = pulls.ToString();
        var gm = GameManager.Instance;
        if (maxPullsText != null && GameManager.Instance != null) {
            maxPullsText.text = $"/{GameManager.Instance.MaxPullsPerTurn}";
        }
    }

    private void OnTurnChanged(int currentTurn, int maxTurns) {
        if (turnText != null) turnText.text = $"{currentTurn}/{maxTurns}";
    }

    private void OnRoundChanged(int round) {
        if (roundText != null) roundText.text = round.ToString();
    }
}