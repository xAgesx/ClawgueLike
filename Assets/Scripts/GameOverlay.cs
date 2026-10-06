using System.Collections;
using TMPro;
using UnityEngine;

public class GameOverlay : MonoBehaviour {
    [Header("Currency")]
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text fragmentsText;

    [Header("Currency Animation")]
    [SerializeField] private TMP_Text animationCoinText;
    [SerializeField] private Color spendTextColor = new Color(1f, 0.35f, 0.35f, 1f);
    [SerializeField] private Color gainTextColor = new Color(0.4f, 1f, 0.45f, 1f);
    [SerializeField] private float coinCountSpeed = 30f;
    [SerializeField] private float coinCountMinDuration = 0.9f;
    [SerializeField] private float coinCountMaxDuration = 3f;
    [SerializeField] private float coinTextFadeIn = 0.25f;
    [SerializeField] private float coinTextHold = 1.5f;
    [SerializeField] private float coinTextFadeOut = 0.75f;
    [SerializeField] private float coinTextRiseDistance = 24f;

    [Header("Pulls")]
    [SerializeField] private TMP_Text pullsText;
    [SerializeField] private TMP_Text maxPullsText;

    [Header("Turn/Round")]
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text roundText;

    private float displayedCoins;
    private bool hasCoinBaseline;
    private Coroutine coinCountRoutine;
    private Coroutine coinTextRoutine;
    private Vector2 coinTextStartPos;
    private bool coinTextStartPosCached;

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
        if (coinCountRoutine != null) {
            StopCoroutine(coinCountRoutine);
            coinCountRoutine = null;
        }
        if (coinTextRoutine != null) {
            StopCoroutine(coinTextRoutine);
            coinTextRoutine = null;
        }
        HideCoinText();
        if (PanelManager.Instance != null) {
            PanelManager.Instance.OnCurrencyChanged -= OnCurrencyChanged;
            PanelManager.Instance.OnPullsChanged -= OnPullsChanged;
            PanelManager.Instance.OnTurnChanged -= OnTurnChanged;
            PanelManager.Instance.OnRoundChanged -= OnRoundChanged;
        }
    }

    private void OnCurrencyChanged(int coins, int fragments) {
        if (fragmentsText != null) fragmentsText.text = fragments.ToString();

        if (!hasCoinBaseline) {
            hasCoinBaseline = true;
            displayedCoins = coins;
            SetCoinsText(coins);
            return;
        }

        if (!Mathf.Approximately(coins, displayedCoins)) {
            StartCoinCount(coins);
        } else {
            displayedCoins = coins;
            SetCoinsText(coins);
        }
    }

    private void SetCoinsText(int coins) {
        if (coinsText != null) coinsText.text = coins.ToString();
    }

    private void StartCoinCount(int targetCoins) {
        if (coinCountRoutine != null) StopCoroutine(coinCountRoutine);
        coinCountRoutine = StartCoroutine(CountCoins(targetCoins));
    }

    private IEnumerator CountCoins(int targetCoins) {
        while (!gameObject.activeInHierarchy) yield return null;

        float start = displayedCoins;
        float amount = targetCoins - start;

        if (Mathf.Abs(amount) >= 1f) {
            PlayCoinText(amount);
        }

        float duration = Mathf.Clamp(
            Mathf.Abs(amount) / Mathf.Max(coinCountSpeed, 0.01f),
            coinCountMinDuration,
            coinCountMaxDuration);

        float elapsed = 0f;
        while (elapsed < duration) {
            elapsed += Time.unscaledDeltaTime;
            float t = 1f - Mathf.Pow(1f - Mathf.Clamp01(elapsed / duration), 2f);
            displayedCoins = Mathf.Lerp(start, targetCoins, t);
            SetCoinsText(Mathf.RoundToInt(displayedCoins));
            yield return null;
        }

        displayedCoins = targetCoins;
        SetCoinsText(targetCoins);
        coinCountRoutine = null;
    }

    private void PlayCoinText(float amount) {
        if (animationCoinText == null) return;
        int rounded = Mathf.RoundToInt(amount);
        if (rounded == 0) return;
        if (coinTextRoutine != null) StopCoroutine(coinTextRoutine);
        coinTextRoutine = StartCoroutine(CoinTextRoutine(rounded));
    }

    private IEnumerator CoinTextRoutine(int amount) {
        while (!gameObject.activeInHierarchy) yield return null;

        Color target = amount < 0 ? spendTextColor : gainTextColor;
        Color transparent = new Color(target.r, target.g, target.b, 0f);

        if (!coinTextStartPosCached) {
            coinTextStartPos = animationCoinText.rectTransform.anchoredPosition;
            coinTextStartPosCached = true;
        }
        Vector2 startPos = coinTextStartPos;

        animationCoinText.gameObject.SetActive(true);
        animationCoinText.text = amount < 0 ? $"-{-amount}" : $"+{amount}";
        animationCoinText.color = target;
        animationCoinText.rectTransform.anchoredPosition = startPos;

        yield return new WaitForSecondsRealtime(coinTextFadeIn);

        yield return new WaitForSecondsRealtime(coinTextHold);

        float elapsed = 0f;
        while (elapsed < coinTextFadeOut) {
            elapsed += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(elapsed / coinTextFadeOut);
            animationCoinText.color = Color.Lerp(target, transparent, k);
            animationCoinText.rectTransform.anchoredPosition = startPos + Vector2.up * (coinTextRiseDistance * k);
            yield return null;
        }

        animationCoinText.rectTransform.anchoredPosition = startPos;
        coinTextRoutine = null;
        HideCoinText();
    }

    private void HideCoinText() {
        if (animationCoinText == null) return;
        if (animationCoinText.gameObject == gameObject) return;
        if (animationCoinText.gameObject == coinsText?.gameObject) return;
        if (animationCoinText.gameObject == fragmentsText?.gameObject) return;
        animationCoinText.gameObject.SetActive(false);
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