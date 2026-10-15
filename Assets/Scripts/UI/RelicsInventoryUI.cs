using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RelicsInventoryUI : MonoBehaviour {
    [System.Serializable]
    public class RelicSlot {
        [Tooltip("Card root to show/hide based on whether this slot has a relic.")]
        public GameObject root;
        public Image icon;
        [Tooltip("Compact status text showing the most important effect(s) for this relic.")]
        public TMP_Text statusText;
    }

    [SerializeField] private List<RelicSlot> slots = new List<RelicSlot>();

    private int lastCount;

    private void OnEnable() {
        lastCount = -1;
        Refresh();
    }

    private void Update() {
        // Refresh when the number of owned relics changes (cheap check). Also effects
        // like Haggler/Fence can change their status values between rerolls/turns.
        int count = GetOwnedCount();
        if (count != lastCount) {
            Refresh();
            lastCount = count;
            return;
        }

        // If count unchanged, still refresh periodically to keep dynamic status text
        // (rerolls this session, free rerolls remaining, crowd threshold etc.) up to date.
        Refresh();
    }

    public void Refresh() {
        GameManager gm = GameManager.Instance;
        RelicManager rm = RelicManager.Instance;

        IReadOnlyList<RelicData> relics = gm != null ? gm.RelicInventory : null;
        int owned = relics != null ? relics.Count : 0;

        for (int i = 0; i < slots.Count; i++) {
            var slot = slots[i];
            if (slot == null) continue;

            bool hasRelic = i < owned && relics[i] != null;
            SetActive(slot.root, hasRelic);
            if (!hasRelic) continue;

            RelicData data = relics[i];
            if (slot.icon != null && data.icon != null) slot.icon.sprite = data.icon;
            if (slot.statusText != null) {
                string status = rm != null ? rm.GetRelicStatus(data) : string.Empty;
                if (string.IsNullOrEmpty(status)) status = data.description ?? string.Empty;
                // Truncate to at most 3 words for compact display
                status = TruncateToWords(status, 3);
                slot.statusText.text = status;
            }
        }

        lastCount = owned;
    }

    private static void SetActive(GameObject root, bool active) {
        if (root != null && root.activeSelf != active) root.SetActive(active);
    }

    private static int GetOwnedCount() {
        GameManager gm = GameManager.Instance;
        if (gm == null) return 0;
        var list = gm.RelicInventory;
        if (list == null) return 0;
        return list.Count;
    }

    private static string TruncateToWords(string s, int maxWords) {
        if (string.IsNullOrEmpty(s)) return string.Empty;
        s = s.Trim();
        for (int i = 0; i < s.Length; i++) {
            char c = s[i];
            if (char.IsLetterOrDigit(c) || c == '+' || c == '-' || c == '%' || c == '(' || c == ')' || c == '=' || c == ':') continue;
            // replace separators with spaces
            if (!char.IsWhiteSpace(c)) s = s.Substring(0, i) + " " + s.Substring(i + 1);
        }
        string[] words = s.Split(new char[] { ' ', '\t' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (words.Length <= maxWords) return string.Join(" ", words);
        return string.Join(" ", words, 0, maxWords);
    }}
