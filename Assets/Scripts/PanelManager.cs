using System;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour {
    public static PanelManager Instance { get; private set; }

    public event Action<int, int> OnCurrencyChanged; // coins, fragments
    public event Action<int> OnPullsChanged;
    public event Action<int, int> OnTurnChanged; // currentTurn, maxTurns
    public event Action<int> OnRoundChanged;

    [System.Serializable]
    public class PanelEntry {
        public GameObject panel;
        public bool pauseGame = true;
    }

    [SerializeField] private List<PanelEntry> panels = new List<PanelEntry>();
    [SerializeField] private GameObject gameUIPanel;

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

        if (entry.panel.activeSelf) {
            openPanelCount--;
            UpdateGameUIPanel();
        }

        entry.panel.SetActive(false);

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