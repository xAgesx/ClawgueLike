using System.Collections.Generic;
using UnityEngine;

// Glue: only reacts when it is wedged against two or more coins at the same moment,
// and then welds them all into one coin worth the total. Resting against a single
// neighbour does nothing, so a lone pair never fuses into anything.
//
// The neighbours are gathered straight from the physics world rather than from the
// contact event, because the contact that triggers the fusion is only ever with the
// second coin - the first one bumped earlier and its event is long gone.
[CreateAssetMenu(fileName = "GlueCoin", menuName = "Data/Effects/SpecialCoins/Glue")]
public class GlueEffect : CoinEffect {
    [Tooltip("How close a coin has to be to count as stuck on.")]
    [SerializeField] private float fuseRadius = 0.28f;

    [Tooltip("How many neighbours have to be stuck to it before it fuses.")]
    [SerializeField] private int coinsRequired = 2;

    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (source == null || source.Self == null) return;

        Vector3 origin = source.transform.position;
        Collider[] hits = Physics.OverlapSphere(origin, fuseRadius, ~0, QueryTriggerInteraction.Ignore);

        List<CoinInstance> glued = new List<CoinInstance>();
        HashSet<CoinInstance> seen = new HashSet<CoinInstance>();

        foreach (Collider hit in hits) {
            CoinInstance coin = hit.GetComponentInParent<CoinInstance>();
            if (coin == null || coin == source.Self) continue;
            if (!seen.Add(coin)) continue;
            glued.Add(coin);
        }

        if (glued.Count < coinsRequired) return;

        int total = 0;
        foreach (CoinInstance coin in glued) {
            total += coin.TotalValue;

            // Destroy only lands at the end of the frame, so a second contact event in
            // the same physics step would still see these coins and fuse them twice.
            // Switching them off removes them from the world immediately.
            coin.gameObject.SetActive(false);
            Destroy(coin.gameObject);
        }

        source.Self.AddBaseValue(total);
    }
}
