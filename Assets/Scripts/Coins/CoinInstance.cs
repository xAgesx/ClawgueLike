using System.Collections.Generic;
using UnityEngine;

// Runtime state for ONE physical coin: baseValue, multiplier and which effects have
// already landed on it. Lives on the prefab instance only - never on CoinData, which
// is shared by every coin spawned from that asset and survives editor play sessions.
public class CoinInstance : MonoBehaviour {
    private CoinData data;
    private int baseValue = 1;
    private float multiplier = 1f;
    private readonly HashSet<CoinEffect> appliedEffects = new HashSet<CoinEffect>();

    private Renderer tintRenderer;
    private Color originalColor;
    private bool hasOriginalColor;

    public CoinData Data => data;
    public int BaseValue => baseValue;
    public float Multiplier => multiplier;
    public int TotalValue => Mathf.Max(0, Mathf.RoundToInt(baseValue * multiplier));

    // Called by ItemSpawner the instant this coin leaves the pool.
    public void Initialize(CoinData coinData) {
        data = coinData;
        baseValue = coinData != null && coinData.baseValue > 0 ? coinData.baseValue : 1;
        multiplier = 1f;
        appliedEffects.Clear();
        ResetTint();
    }

    public void SetBaseValue(int value) {
        baseValue = Mathf.Max(1, value);
    }

    // DoubleDown uses this. Safe to call repeatedly because SpecialCoin only
    // applies an effect to a coin once, not once per frame of contact.
    public void MultiplyBaseValue(float factor) {
        baseValue = Mathf.Max(1, Mathf.RoundToInt(baseValue * factor));
    }

    public void SetMultiplier(float factor) {
        multiplier = factor;
    }

    public void MultiplyMultiplier(float factor) {
        multiplier *= factor;
    }

    public bool HasApplied(CoinEffect effect) {
        return effect != null && appliedEffects.Contains(effect);
    }

    public void MarkApplied(CoinEffect effect) {
        if (effect != null) appliedEffects.Add(effect);
    }

    public void SetTint(Color color) {
        Renderer rend = GetRenderer();
        if (rend == null) return;

        if (!hasOriginalColor) {
            originalColor = rend.material.color;
            hasOriginalColor = true;
        }

        rend.material.color = color;
    }

    public void ResetTint() {
        if (hasOriginalColor && tintRenderer != null) {
            tintRenderer.material.color = originalColor;
        }
    }

    private Renderer GetRenderer() {
        if (tintRenderer == null) tintRenderer = GetComponent<Renderer>();
        return tintRenderer;
    }

    // Pooled objects get flipped on before the spawner re-initialises them, so wipe
    // everything here as a safety net. Initialize() then supplies this round's data.
    private void OnEnable() {
        multiplier = 1f;
        appliedEffects.Clear();
        ResetTint();
    }
}
