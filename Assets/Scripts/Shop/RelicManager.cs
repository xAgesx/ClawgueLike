using System.Collections.Generic;
using UnityEngine;

// Aggregates the modifiers of every owned cursed relic and exposes them as simple
// queries plus a handful of event callbacks. It owns no gameplay logic itself - the
// systems that care (Magnet, ShopManager, RoundsManager, ChuteDrop, ItemSpawner) ask
// it for a number instead of reading their own serialized fields.
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

    private static readonly List<RelicData> Empty = new List<RelicData>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        instance = null;
    }

    private static IReadOnlyList<RelicData> Relics {
        get {
            GameManager gm = GameManager.Instance;
            return gm != null ? gm.RelicInventory : Empty;
        }
    }

    private void Awake() {
        if (instance != null && instance != this) {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public int CountOwned() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n++;
        }
        return n;
    }

    // ---- Delivery ---------------------------------------------------------

    // Called by ChuteDrop once per collected coin, before the value is read.
    public void ModifyCoinForCollection(CoinInstance coin) {
        if (coin == null) return;

        float multiplier = 1f;
        int baseBonus = 0;

        foreach (RelicData relic in Relics) {
            if (relic == null) continue;

            multiplier *= relic.coinMultiplier;
            baseBonus += relic.coinBaseBonus;

            if (relic.gambleNothingChance > 0f && Random.value < relic.gambleNothingChance) {
                multiplier = 0f;
            } else if (relic.gambleDoubleChance > 0f && Random.value < relic.gambleDoubleChance) {
                multiplier *= 2f;
            }
        }

        if (!Mathf.Approximately(multiplier, 1f)) coin.MultiplyMultiplier(multiplier);
        if (baseBonus != 0) coin.AddBaseValue(baseBonus);
    }

    public void NotifyCoinDelivered() {
        coinsDeliveredThisTurn++;
    }

    // ---- Claw / magnet ----------------------------------------------------

    public float MagnetRadiusMultiplier() {
        float m = 1f;
        foreach (RelicData r in Relics) {
            if (r != null) m *= r.magnetRadiusMultiplier;
        }
        return m;
    }

    public float MagnetForceMultiplier() {
        float m = 1f;
        int coinCount = CoinInstance.ActiveCount;

        foreach (RelicData r in Relics) {
            if (r == null) continue;
            m *= r.magnetForceMultiplier;
            if (r.crowdMagnetThreshold > 0 && coinCount >= r.crowdMagnetThreshold) {
                m *= r.crowdMagnetForceMultiplier;
            }
        }
        return m;
    }

    // ---- Turns / pulls ----------------------------------------------------

    public int ExtraPulls() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.extraPulls;
        }
        return n;
    }

    public int ExtraNormalCoinsPerRound() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.extraNormalCoinsPerRound;
        }
        return n;
    }

    // ---- Shop -------------------------------------------------------------

    public int ExtraCoinSlots() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.extraCoinSlots;
        }
        return n;
    }

    public int ExtraRelicSlots() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.extraRelicSlots;
        }
        return n;
    }

    public float ShopPriceMultiplier() {
        float m = 1f;
        foreach (RelicData r in Relics) {
            if (r != null) m *= r.shopPriceMultiplier;
        }
        return m;
    }

    public float ShopPriceRerollCreep() {
        float n = 0f;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.shopPriceRerollCreep;
        }
        return n;
    }

    public int FreeRerolls() {
        int n = 0;
        foreach (RelicData r in Relics) {
            if (r != null) n += r.freeRerolls;
        }
        return n;
    }

    public float PaidRerollCostMultiplier() {
        float m = 1f;
        foreach (RelicData r in Relics) {
            if (r != null) m *= r.paidRerollCostMultiplier;
        }
        return m;
    }

    // ---- Turn / round boundaries ------------------------------------------

    // Called from RoundsManager when a turn's coins have landed. Drains are applied
    // after the counter is cleared so the deliver-count reflects the coming turn.
    public void OnTurnStart() {
        coinsDeliveredThisTurn = 0;

        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        float drain = 0f;
        foreach (RelicData r in Relics) {
            if (r != null) drain += r.balancePercentPerTurnStart;
        }

        if (drain > 0f) {
            int lose = Mathf.RoundToInt(gm.CoinsBalance * drain);
            if (lose > 0) gm.SpendCoins(lose);
        }
    }

    // Called at the end of a round, before the resupply cost is paid, so interest can
    // help cover it.
    public void OnRoundEnd() {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        float interest = 0f;
        int minDeliveries = 0;
        float loss = 0f;

        foreach (RelicData r in Relics) {
            if (r == null) continue;

            if (r.roundEndInterest > 0f && coinsDeliveredThisTurn >= r.interestMinDeliveries) {
                interest += r.roundEndInterest;
                minDeliveries = Mathf.Max(minDeliveries, r.interestMinDeliveries);
            }
            loss += r.balancePercentPerRoundEnd;
        }

        if (interest > 0f) {
            int gain = Mathf.RoundToInt(gm.CoinsBalance * interest);
            if (gain > 0) gm.AddCoins(gain);
            Debug.Log($"[RelicManager] Round-end interest paid: {gain} (needed {minDeliveries} deliveries, had {coinsDeliveredThisTurn}).");
        }

        if (loss > 0f) {
            int lose = Mathf.RoundToInt(gm.CoinsBalance * loss);
            if (lose > 0) gm.SpendCoins(lose);
        }
    }
}
