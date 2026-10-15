using UnityEngine;

// A "cursed relic": every relic is a boon paired with a curse. All of its numbers
// live here so a single asset fully describes one relic - the runtime logic lives in
// RelicManager, not on this shared asset.
[CreateAssetMenu(fileName = "RelicData", menuName = "Data/Items/Relics/Cursed Relic Data")]
public class RelicData : ItemData {
    [Header("Info")]
    public string relicName;
    public string description;
    public Sprite icon;
    public int basePrice;

    [Header("Coin delivery")]
    [Tooltip("Multiplies the multiplier of every coin collected in the chute (1 = no change).")]
    public float coinMultiplier = 1f;
    [Tooltip("Added to the baseValue of every coin collected in the chute.")]
    public int coinBaseBonus = 0;

    [Header("Claw / magnet")]
    public float magnetRadiusMultiplier = 1f;
    public float magnetForceMultiplier = 1f;
    [Tooltip("While the machine holds at least this many coins, crowdMagnetForceMultiplier also applies. 0 = disabled.")]
    public int crowdMagnetThreshold = 0;
    public float crowdMagnetForceMultiplier = 1f;

    [Header("Turns / pulls")]
    public int extraPulls = 0;
    [Tooltip("Fraction of the coin balance lost at the start of every turn (0.1 = 10%).")]
    public float balancePercentPerTurnStart = 0f;

    [Header("Rounds")]
    [Tooltip("Fraction of the coin balance gained as interest at the end of a round.")]
    public float roundEndInterest = 0f;
    [Tooltip("Interest is only paid if the final turn of the round delivered at least this many coins.")]
    public int interestMinDeliveries = 0;
    [Tooltip("Fraction of the coin balance lost at the end of every round (0.08 = 8%).")]
    public float balancePercentPerRoundEnd = 0f;

    [Header("Spawner")]
    public int extraNormalCoinsPerRound = 0;

    [Header("Shop")]
    public float shopPriceMultiplier = 1f;
    public int extraCoinSlots = 0;
    public int extraRelicSlots = 0;
    [Tooltip("Free rerolls granted at the start of each shop visit.")]
    public int freeRerolls = 0;
    [Tooltip("Multiplier on the cost of paid rerolls (2 = double).")]
    public float paidRerollCostMultiplier = 1f;
    [Tooltip("Fraction added to shop prices for each reroll already made this round (0.05 = +5%).")]
    public float shopPriceRerollCreep = 0f;

    [Header("Risk")]
    [Tooltip("Chance a delivered coin pays double (0.5 = 50%).")]
    public float gambleDoubleChance = 0f;
    [Tooltip("Chance a delivered coin pays nothing (0.5 = 50%). Applied after the double roll.")]
    public float gambleNothingChance = 0f;
}
