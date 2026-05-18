using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

// ============================================================
// ENEMY EFFECT VFX (v6 — simplificado)
// Sin sprite sheets complicados:
//   - Tint del SpriteRenderer del enemigo (parpadeo de color)
//   - Múltiples instancias del sprite básico (gotas, llamas, burbujas)
// ============================================================
public enum EffectType { Burn, Poison, Bleed, Stun }

public class EnemyEffectVFX : MonoBehaviour
{
    [Header("─── PREFABS DE PARTÍCULAS ──")]
    [Tooltip("Sprite simple de gota de sangre (rojo)")]
    public GameObject bloodDropPrefab;
    [Tooltip("Sprite simple de llama (amarillo/naranja)")]
    public GameObject fireParticlePrefab;
    [Tooltip("Sprite simple de burbuja (morado)")]
    public GameObject bubblePrefab;

    [Header("─── COLORES TINT (parpadeo del enemigo) ──")]
    public Color bleedTintColor = new Color(0.78f, 0.08f, 0.12f); // rojo carmesí
    public Color burnTintColor = new Color(1f, 0.55f, 0.08f);     // amarillo-rojizo
    public Color poisonTintColor = new Color(0.63f, 0.08f, 0.78f); // morado
    public Color stunTintColor = new Color(0.31f, 0.31f, 0.27f);   // gris oscuro

    [Header("─── PARPADEO ──")]
    [Tooltip("Velocidad del parpadeo (más alto = más rápido)")]
    [Range(1f, 15f)] public float blinkSpeed = 5f;

    [Tooltip("Intensidad del tint (0 = nada, 1 = totalmente del color)")]
    [Range(0f, 1f)] public float tintIntensity = 0.7f;

    [Header("─── PARTÍCULAS ──")]
    [Tooltip("Partículas a spawnear cada vez")]
    [Range(1, 6)] public int particleCount = 3;

    [Tooltip("Cada cuánto spawnear partículas")]
    public float particleSpawnInterval = 0.4f;

    [Tooltip("Vida de cada partícula spawneada")]
    public float particleLifetime = 0.6f;

    [Tooltip("Distancia desde el centro del enemigo")]
    public float particleSpread = 0.4f;

    // ─── ESTADO ─────────────────────────────────────────────────────────
    SpriteRenderer _enemySr;
    Color _originalColor;
    Dictionary<EffectType, Coroutine> _activeBlinks = new Dictionary<EffectType, Coroutine>();
    Dictionary<EffectType, Coroutine> _activeParticles = new Dictionary<EffectType, Coroutine>();
    Dictionary<EffectType, Color> _activeTints = new Dictionary<EffectType, Color>();

    void Awake()
    {
        _enemySr = GetComponent<SpriteRenderer>();
        if (_enemySr == null) _enemySr = GetComponentInChildren<SpriteRenderer>();
        if (_enemySr != null) _originalColor = _enemySr.color;
    }

    /// <summary>
    /// Activa el VFX correspondiente. Si ya estaba activo, no hace nada.
    /// </summary>
    public void Show(EffectType type)
    {
        if (_activeBlinks.ContainsKey(type)) return;

        // Iniciar parpadeo del tint
        Color tint = GetTintForType(type);
        _activeTints[type] = tint;
        _activeBlinks[type] = StartCoroutine(BlinkRoutine(type));

        // Iniciar emisión de partículas (excepto Stun)
        GameObject particlePrefab = GetParticlePrefab(type);
        if (particlePrefab != null)
        {
            _activeParticles[type] = StartCoroutine(SpawnParticlesRoutine(particlePrefab));
        }
    }

    public void Hide(EffectType type)
    {
        if (_activeBlinks.ContainsKey(type))
        {
            if (_activeBlinks[type] != null) StopCoroutine(_activeBlinks[type]);
            _activeBlinks.Remove(type);
        }
        if (_activeParticles.ContainsKey(type))
        {
            if (_activeParticles[type] != null) StopCoroutine(_activeParticles[type]);
            _activeParticles.Remove(type);
        }
        _activeTints.Remove(type);

        // Si no quedan más efectos, restaurar color original
        if (_activeTints.Count == 0 && _enemySr != null)
            _enemySr.color = _originalColor;
    }

    public void HideAll()
    {
        foreach (var t in new List<EffectType>(_activeBlinks.Keys))
            Hide(t);
    }

    // ─────────────────────────────────────────────────────────────────────
    Color GetTintForType(EffectType type)
    {
        return type switch
        {
            EffectType.Bleed => bleedTintColor,
            EffectType.Burn => burnTintColor,
            EffectType.Poison => poisonTintColor,
            EffectType.Stun => stunTintColor,
            _ => Color.white
        };
    }

    GameObject GetParticlePrefab(EffectType type)
    {
        return type switch
        {
            EffectType.Bleed => bloodDropPrefab,
            EffectType.Burn => fireParticlePrefab,
            EffectType.Poison => bubblePrefab,
            EffectType.Stun => null, // sin partículas
            _ => null
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    // BLINK: parpadeo del tint del enemigo
    // ─────────────────────────────────────────────────────────────────────
    IEnumerator BlinkRoutine(EffectType type)
    {
        while (_enemySr != null)
        {
            // FIX: Sumamos los colores usando variables Color directamente para evitar el error de Vector4
            Color blendedTint = new Color(0, 0, 0, 0); // Empezamos en negro transparente
            float weight = 0f;

            foreach (var kvp in _activeTints)
            {
                blendedTint += kvp.Value; // Ahora sí sumamos Color con Color
                weight += 1f;
            }
            if (weight > 0f) blendedTint /= weight; // Promediamos

            // Parpadeo: alterna entre original y tint
            float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f; // 0..1
            Color current = Color.Lerp(_originalColor, blendedTint, t * tintIntensity);
            _enemySr.color = current;

            yield return null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // PARTÍCULAS: spawnea múltiples sprites con offsets aleatorios
    // ─────────────────────────────────────────────────────────────────────
    IEnumerator SpawnParticlesRoutine(GameObject particlePrefab)
    {
        while (true)
        {
            for (int i = 0; i < particleCount; i++)
            {
                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-particleSpread, particleSpread),
                    UnityEngine.Random.Range(-particleSpread, particleSpread),
                    0f);

                var p = Instantiate(particlePrefab, transform.position + offset, Quaternion.identity);

                // Animar la partícula: sube y desaparece
                var pSr = p.GetComponent<SpriteRenderer>();
                if (pSr != null) pSr.sortingOrder = 8;

                p.transform.DOMoveY(p.transform.position.y + 0.3f, particleLifetime).SetEase(Ease.OutQuad);
                if (pSr != null)
                    pSr.DOFade(0f, particleLifetime).OnComplete(() => Destroy(p));
                else
                    Destroy(p, particleLifetime);
            }

            yield return new WaitForSeconds(particleSpawnInterval);
        }
    }

    void OnDisable() => HideAll();
}