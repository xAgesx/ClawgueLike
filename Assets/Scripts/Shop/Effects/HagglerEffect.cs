using UnityEngine;

// Haggler: cheaper prices, but they creep up with every reroll made this round.
[CreateAssetMenu(fileName = "Haggler", menuName = "Data/Effects/Relics/Haggler")]
public class HagglerEffect : RelicEffect {
    [SerializeField] private float priceMultiplier = 0.8f;
    [SerializeField] private float rerollCreep = 0.05f;

    public override float ShopPriceMultiplier() => priceMultiplier;
    public override float ShopPriceRerollCreep() => rerollCreep;
}
