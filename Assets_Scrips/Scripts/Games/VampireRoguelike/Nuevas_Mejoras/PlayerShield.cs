using UnityEngine;
using TMPro;
using DG.Tweening;

// ============================================================
// PLAYER SHIELD
// Sistema de escudo acumulativo.
// - Cada item Escudo recogido suma N cargas (configurable)
// - Cuando el jugador recibe daño: consume 1 carga ANTES de bajar HP
// - HUD: icono + contador
// ============================================================
public class PlayerShield : MonoBehaviour
{
    public static PlayerShield Instance { get; private set; }

    [Header("─── ESCUDO ──")]
    [Tooltip("Cargas que da CADA item de escudo recogido")]
    public int shieldPerItem = 2;

    [Header("─── HUD ──")]
    [Tooltip("Contenedor del HUD del escudo (oculto si 0 cargas)")]
    public GameObject shieldDisplay;

    [Tooltip("Texto contador del escudo: 'x3'")]
    public TextMeshProUGUI txtShieldCount;

    [Tooltip("Imagen del icono (puede animarse al consumir)")]
    public RectTransform shieldIconRect;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    int _shieldStacks = 0;
    public int ShieldStacks => _shieldStacks;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        UpdateHUD();
    }

    /// <summary>
    /// Añade cargas al escudo (llamado cuando se recoge un item Escudo).
    /// </summary>
    public void AddShield()
    {
        _shieldStacks += shieldPerItem;
        UpdateHUD();

        // Pequeña animación al añadir
        if (shieldIconRect != null)
            shieldIconRect.DOPunchScale(Vector3.one * 0.3f, 0.3f, 5, 0.5f);

        if (debugMode) Debug.Log($"[Shield] +{shieldPerItem} cargas. Total: {_shieldStacks}");
    }

    /// <summary>
    /// Llamado por PlayerControllerRogueLite.TakeDamage().
    /// Si hay cargas: consume una y devuelve true (bloquea el daño).
    /// Si no: devuelve false (el jugador recibe daño normal).
    /// </summary>
    public bool TryConsumeShield()
    {
        if (_shieldStacks <= 0) return false;

        _shieldStacks--;
        UpdateHUD();

        // Animación al consumir (shake del icono)
        if (shieldIconRect != null)
        {
            shieldIconRect.DOShakeRotation(0.3f, new Vector3(0f, 0f, 30f), 10);
            shieldIconRect.DOPunchScale(Vector3.one * 0.4f, 0.25f, 8, 0.7f);
        }

        if (debugMode) Debug.Log($"[Shield] Consumido. Quedan: {_shieldStacks}");
        return true;
    }

    void UpdateHUD()
    {
        if (txtShieldCount != null)
            txtShieldCount.text = $"x{_shieldStacks}";

        if (shieldDisplay != null)
            shieldDisplay.SetActive(_shieldStacks > 0);
    }

    public void Reset()
    {
        _shieldStacks = 0;
        UpdateHUD();
    }
}
