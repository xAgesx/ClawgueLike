using System.Collections.Generic;
using UnityEngine;

// Aggregates the modifiers of every owned cursed relic and exposes them as simple
// queries plus a handful of event callbacks. It owns all live state - the relic
// effects themselves are shared, stateless assets - and the systems that care
// (Magnet, ShopManager, RoundsManager, ChuteDrop, ItemSpawner) ask it for a number
// instead of reading anything themselves.
//
// The owned list lives on GameManager.relicInventory, so this manager never holds
// duplicate state. It creates itself on first use, so no scene wiring is required.
public class RelicManager : MonoBehaviour {
    private static RelicManager instance;

    public static RelicManager Instance {
        get {
            if (instance == null) {
                GameObject go = new GameObject("RelicManager");
                instance = go.AddComponent<RelicManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }

    // How many coins have been collected in the chute this turn. Reset at turn start
    // and read by relics whose payout depends on how busy the final turn was.
    private int coinsDeliveredThisTurn;
    public int CoinsDeliveredThisTurn => coinsDeliveredThisTurn;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        instance = null;
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    // Walks every effect on every owned relic. Yields nothing when there are no relics
    // or no GameManager yet, so callers never need a null check of their own.
    private static IEnumerable<RelicEffect> AllEffects() {
        GameManager gm = GameManager.Instance;
        if (gm == null) yield break;

        IReadOnlyList<RelicData> relics = gm.RelicInventory;
        if (relics == null) yield break;

        for (int i = 0; i < relics.Count; i++) {
            RelicData relic = relics[i];
            if (relic == null || relic.effects == null) continue;

            for (int j = 0; j < relic.effects.Count; j++) {
                RelicEffect effect = relic.effects[j];
                if (effect != null) yield return effect;
            }
        }
    }

    public int CountOwned() {
        GameManager gm = GameManager.Instance;
        if (gm == null) return 0;

        IReadOnlyList<RelicData> relics = gm.RelicInventory;
        if (relics == null) return 0;

        int n = 0;
        for (int i = 0; i < relics.Count; i++) {
            if (relics[i] != null) n++;
        }
        return n;
    }

    // ---- Delivery ---------------------------------------------------------

    // Called by ChuteDrop once per collected coin, before the value is read.
    public void ModifyCoinForCollection(CoinInstance coin) {
        if (coin == null) return;

        foreach (RelicEffect effect in AllEffects()) {
            effect.ModifyCoinForCollection(coin);
        }
    }

    public void NotifyCoinDelivered() {
        coinsDeliveredThisTurn++;
    }

    // ---- Claw / magnet ----------------------------------------------------

    public float MagnetRadiusMultiplier() {
        float m = 1f;
        foreach (RelicEffect effect in AllEffects()) {
            m *= effect.MagnetRadiusMultiplier();
        }
        return m;
    }

    public float MagnetForceMultiplier() {
        float m = 1f;
        int coinCount = CoinInstance.ActiveCount;
        foreach (RelicEffect effect in AllEffects()) {
            m *= effect.MagnetForceMultiplier(coinCount);
        }
        return m;
    }

    // ---- Turns / pulls ----------------------------------------------------

    public int ExtraPulls() {
        int n = 0;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.ExtraPulls();
        }
        return n;
    }

    public int ExtraNormalCoinsPerRound() {
        int n = 0;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.ExtraNormalCoinsPerRound();
        }
        return n;
    }

    // ---- Shop -------------------------------------------------------------

    public int ExtraCoinSlots() {
        int n = 0;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.ExtraCoinSlots();
        }
        return n;
    }

    public int ExtraRelicSlots() {
        int n = 0;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.ExtraRelicSlots();
        }
        return n;
    }

    public float ShopPriceMultiplier() {
        float m = 1f;
        foreach (RelicEffect effect in AllEffects()) {
            m *= effect.ShopPriceMultiplier();
        }
        return m;
    }

    public float ShopPriceRerollCreep() {
        float n = 0f;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.ShopPriceRerollCreep();
        }
        return n;
    }

    public int FreeRerolls() {
        int n = 0;
        foreach (RelicEffect effect in AllEffects()) {
            n += effect.FreeRerolls();
        }
        return n;
    }

    public float PaidRerollCostMultiplier() {
        float m = 1f;
        foreach (RelicEffect effect in AllEffects()) {
            m *= effect.PaidRerollCostMultiplier();
        }
        return m;
    }

    // Returns a compact status line for a specific relic (concatenates its effects'
    // non-empty GetStatusText). Used by the always-visible RelicsInventory UI.
    public string GetRelicStatus(RelicData relic) {
        if (relic == null || relic.effects == null || relic.effects.Count == 0) return string.Empty;

        System.Text.StringBuilder sb = null;
        GameManager gm = GameManager.Instance;
        foreach (RelicEffect effect in relic.effects) {
            if (effect == null) continue;
            string s = effect.GetStatusText(gm, this);
            if (string.IsNullOrEmpty(s)) continue;
            if (sb == null) sb = new System.Text.StringBuilder(s);
            else {
                sb.Append("  •  ");
                sb.Append(s);
            }
        }
        return sb != null ? sb.ToString() : string.Empty;
    }

    // ---- Turn / round boundaries ------------------------------------------

    // Called from RoundsManager when a turn's coins have landed. Drains are applied
    // after the counter is cleared so the deliver-count reflects the coming turn.
    public void OnTurnStart() {
        coinsDeliveredThisTurn = 0;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        foreach (RelicEffect effect in AllEffects()) {
            effect.OnTurnStart(gm);
        }
    }

    // Called at the end of a round, before the resupply cost is paid, so interest can
    // help cover it.
    public void OnRoundEnd() {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        foreach (RelicEffect effect in AllEffects()) {
            effect.OnRoundEnd(gm);
        }
    }
}
