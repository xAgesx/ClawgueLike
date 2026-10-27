using System.Collections.Generic;
using UnityEngine;

// A small planted marker that waits a number of turn boundaries and then releases a
// coin. Seed uses it for the delayed sprout and Caterpillar uses it for the cocoon.
// It deliberately has no collider or ItemPickup, so the pile and the chute ignore it.
public class DelayedCoinPod : MonoBehaviour {
    private static readonly List<DelayedCoinPod> active = new List<DelayedCoinPod>();

    private CoinData coin;
    private int value = 1;
    private int turnsLeft;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics() {
        active.Clear();
    }

    public static void Schedule(CoinData coin, Vector3 position, int value, int turns, Color tint) {
        if (coin == null) return;

        GameObject go = new GameObject($"Pod_{coin.itemName}");
        go.transform.position = position;

        DelayedCoinPod pod = go.AddComponent<DelayedCoinPod>();
        pod.coin = coin;
        pod.value = Mathf.Max(1, value);
        pod.turnsLeft = Mathf.Max(1, turns);

        pod.CreateMarker(tint);
        active.Add(pod);
    }

    // Called by RoundsManager after turn-start effects. A pod scheduled on turn N
    // sprouts on turn N+1.
    public static void TickAll() {
        for (int i = active.Count - 1; i >= 0; i--) {
            DelayedCoinPod pod = active[i];
            if (pod == null) {
                active.RemoveAt(i);
                continue;
            }

            pod.turnsLeft--;
            if (pod.turnsLeft <= 0) pod.Sprout();
        }
    }

    private void CreateMarker(Color tint) {
        GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visual.name = "Marker";
        visual.transform.SetParent(transform, false);
        visual.transform.localScale = Vector3.one * 0.14f;

        Collider markerCollider = visual.GetComponent<Collider>();
        if (markerCollider != null) Destroy(markerCollider);

        Renderer renderer = visual.GetComponent<Renderer>();
        if (renderer == null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return;

        Material material = new Material(shader);
        material.color = tint;
        renderer.material = material;
    }

    private void Sprout() {
        if (coin != null && ItemSpawner.Instance != null) {
            ItemSpawner.Instance.SpawnCoinAt(coin, transform.position, value);
        }

        active.Remove(this);
        Destroy(gameObject);
    }

    private void OnDestroy() {
        active.Remove(this);
    }
}
