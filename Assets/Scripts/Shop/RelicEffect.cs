using UnityEngine;

// A relic effect is a data-only strategy: the asset is SHARED by every copy of the
// relic, so never store runtime state here (timers, counters, "already fired" flags) -
// it would leak between runs. RelicManager owns all live state and calls these hooks.
//
// Every hook has a neutral default, so a relic overrides only what it needs. That is
// what lets a single script describe both halves of a cursed relic: the boon on one
// hook and the curse on another.
public abstract class RelicEffect : ScriptableObject {
    // Called for each coin as it enters the chute, before its value is read. Use it to
    // fold the relic's payout into the coin itself.
    public virtual void ModifyCoinForCollection(CoinInstance coin) { }

    public virtual float MagnetRadiusMultiplier() => 1f;
    public virtual float MagnetForceMultiplier(int activeCoins) => 1f;

    public virtual int ExtraPulls() => 0;
    public virtual int ExtraNormalCoinsPerRound() => 0;

    public virtual int ExtraCoinSlots() => 0;
    public virtual int ExtraRelicSlots() => 0;
    public virtual float ShopPriceMultiplier() => 1f;
    public virtual float ShopPriceRerollCreep() => 0f;
    public virtual int FreeRerolls() => 0;
    public virtual float PaidRerollCostMultiplier() => 1f;

    // Turn / round boundaries. `game` is the live GameManager for balance changes.
    public virtual void OnTurnStart(GameManager game) { }
    public virtual void OnRoundEnd(GameManager game) { }
}
