using UnityEngine;

// Glue: sticks fast to whatever coin it touches. Each new neighbour is welded on with
// a fixed joint, so the glue coin and everything stuck to it move as a single lump.
// oncePerTarget on the asset stops it welding to the same coin twice.
[CreateAssetMenu(fileName = "GlueCoin", menuName = "Data/Effects/SpecialCoins/Glue")]
public class GlueEffect : CoinEffect {
    public override void Apply(CoinInstance target, SpecialCoin source) {
        if (source == null || target == null) return;

        Rigidbody own = source.GetComponent<Rigidbody>();
        Rigidbody neighbour = target.GetComponent<Rigidbody>();
        if (own == null || neighbour == null) return;

        // A fresh joint each time, pinned to this neighbour. enableCollision stays off
        // so the two welded coins stop jostling against each other.
        FixedJoint joint = source.gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = neighbour;
        joint.autoConfigureConnectedAnchor = true;
    }
}
