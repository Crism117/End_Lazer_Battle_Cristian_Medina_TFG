using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections.Generic;

// ============================================================
// ACTIVE EFFECT ICON MANAGER
// Muestra un icono encima de la cabeza del jugador con un contador
// parpadeante mientras la habilidad está activa.
//
// Cómo usarlo:
//   ActiveEffectIconManager.Instance.ShowEffect(itemSprite, 6f);
// ============================================================
public class ActiveEffectIconManager : MonoBehaviour
{
    public static ActiveEffectIconManager Instance { get; private set; }

    [Header("─── PREFAB DEL ICONO ──")]
    [Tooltip("Prefab con SpriteRenderer + TextMeshPro 3D")]
    public GameObject effectIconPrefab;

    [Header("─── POSICIÓN ──")]
    [Tooltip("Altura sobre la cabeza del jugador")]
    public float iconOffsetY = 0.8f;

    [Tooltip("Espaciado horizontal si hay varios iconos")]
    public float spacing = 0.4f;

    [Header("─── ANIMACIÓN ──")]
    [Tooltip("Velocidad del parpadeo del icono")]
    [Range(1f, 10f)] public float blinkSpeed = 4f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    List<ActiveIcon> _icons = new List<ActiveIcon>();

    class ActiveIcon
    {
        public GameObject go;
        public SpriteRenderer sr;
        public TextMeshPro txt;
        public float remaining;
    }

    /// <summary>
    /// Muestra un icono activo con timer.
    /// </summary>
    public void ShowEffect(Sprite icon, float duration)
    {
        if (effectIconPrefab == null || icon == null)
        {
            Debug.LogWarning("[ActiveEffectIcon] Falta prefab o sprite.");
            return;
        }

        var go = Instantiate(effectIconPrefab, transform);
        go.transform.localPosition = new Vector3(0f, iconOffsetY, 0f);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) { sr.sprite = icon; sr.sortingOrder = 9; }

        var txt = go.GetComponentInChildren<TextMeshPro>();

        var ai = new ActiveIcon { go = go, sr = sr, txt = txt, remaining = duration };
        _icons.Add(ai);

        RepositionIcons();
    }

    void Update()
    {
        for (int i = _icons.Count - 1; i >= 0; i--)
        {
            var ai = _icons[i];
            if (ai.go == null) { _icons.RemoveAt(i); continue; }

            ai.remaining -= Time.deltaTime;

            if (ai.remaining <= 0f)
            {
                Destroy(ai.go);
                _icons.RemoveAt(i);
                RepositionIcons();
                continue;
            }

            // Texto contador
            if (ai.txt != null)
                ai.txt.text = Mathf.CeilToInt(ai.remaining).ToString();

            // Parpadeo
            if (ai.sr != null)
            {
                float t = (Mathf.Sin(Time.time * blinkSpeed) + 1f) * 0.5f;
                Color c = ai.sr.color;
                c.a = 0.6f + t * 0.4f;
                ai.sr.color = c;
            }
        }
    }

    void RepositionIcons()
    {
        // Distribuir iconos horizontalmente
        float totalWidth = (_icons.Count - 1) * spacing;
        float startX = -totalWidth * 0.5f;

        for (int i = 0; i < _icons.Count; i++)
        {
            if (_icons[i].go != null)
                _icons[i].go.transform.localPosition = new Vector3(startX + i * spacing, iconOffsetY, 0f);
        }
    }
}
