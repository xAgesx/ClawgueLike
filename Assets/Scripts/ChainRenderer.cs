using UnityEngine;

[RequireComponent(typeof(VerletChain))]
[RequireComponent(typeof(LineRenderer))]
public class ChainRenderer : MonoBehaviour {
    [SerializeField] private float width = 0.02f;

    private VerletChain chain;
    private LineRenderer lineRenderer;

    private void Awake() {
        chain = GetComponent<VerletChain>();
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = 0;
        lineRenderer.startWidth = width;
        lineRenderer.endWidth = width;
        lineRenderer.useWorldSpace = true;
        lineRenderer.loop = false;
    }

    private void LateUpdate() {
        Vector3[] positions = chain.Positions;
        if (positions == null) return;

        lineRenderer.positionCount = positions.Length;
        lineRenderer.SetPositions(positions);
    }
}
