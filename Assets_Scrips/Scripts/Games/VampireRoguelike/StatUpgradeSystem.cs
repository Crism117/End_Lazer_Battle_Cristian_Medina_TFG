using UnityEngine;
using System.Collections.Generic;

// ============================================================
// STAT UPGRADE SYSTEM
// Tres stats principales que el jugador mejora con puntos ⭐:
//   - HP máximo
//   - Velocidad de ataque
//   - Rango de ataque (sube AMBOS rangos: melee Y ranged)
//
// Cada stat tiene 10 niveles. Coste: 1 punto por nivel.
// ============================================================
public class StatUpgradeSystem : MonoBehaviour
{
    public static StatUpgradeSystem Instance { get; private set; }

    public enum StatType { Health, AttackSpeed, AttackRange }

    public const int MaxLevel = 10;
    public const int CostPerUpgrade = 1;

    // Cuánto sube cada stat por nivel
    [Header("HP Máximo")]
    public int hpPerLevel = 3;

    [Header("Velocidad de ataque")]
    [Tooltip("Multiplicador del fireRate (0.92 = 8% más rápido por nivel)")]
    [Range(0.7f, 0.99f)]
    public float attackSpeedMultiplier = 0.92f;

    [Header("Rango de ataque (afecta a AMBOS)")]
    [Tooltip("Cuánto sube el rango RANGED por nivel (más beneficio)")]
    public float rangedRangePerLevel = 1.0f;

    [Tooltip("Cuánto sube el rango MELEE por nivel (menos)")]
    public float meleeRangePerLevel = 0.3f;

    Dictionary<StatType, int> _levels = new Dictionary<StatType, int>
    {
        { StatType.Health,      0 },
        { StatType.AttackSpeed, 0 },
        { StatType.AttackRange, 0 },
    };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public int GetLevel(StatType s) => _levels[s];
    public bool IsMaxed(StatType s) => _levels[s] >= MaxLevel;

    public bool TryUpgrade(StatType stat)
    {
        if (IsMaxed(stat)) return false;
        if (GameManagerRogueLite.Instance == null) return false;
        if (!GameManagerRogueLite.Instance.SpendLevelPoint()) return false;

        _levels[stat]++;
        ApplyStatToPlayer(stat);
        return true;
    }

    void ApplyStatToPlayer(StatType stat)
    {
        var player = FindObjectOfType<PlayerControllerRogueLite>();
        if (player == null) return;

        switch (stat)
        {
            case StatType.Health:
                player.maxHealth += hpPerLevel;
                player.Heal(hpPerLevel);
                break;

            case StatType.AttackSpeed:
                // ⚠️ OBSOLETO en nuevo diseño
                // La velocidad de ataque ahora es POR ARMA (WeaponConfig.fireRate)
                // y se mejora con el botón "Upgrade" en cada arma (cuesta monedas, no puntos)
                Debug.LogWarning("[StatUpgradeSystem] AttackSpeed upgrade DESHABILITADO. Ver AutoLevelUpSystem para velocidad de movimiento.");
                break;

            case StatType.AttackRange:
                // ⚠️ OBSOLETO en nuevo diseño
                // El rango ahora es POR ARMA (WeaponConfig.attackRadius)
                // y se mejora automáticamente cuando upgradeas el arma
                Debug.LogWarning("[StatUpgradeSystem] AttackRange upgrade DESHABILITADO. El rango ahora se controla por arma.");
                break;
        }
    }

    public string GetStatName(StatType s) => s switch
    {
        StatType.Health => "HP Máximo",
        StatType.AttackSpeed => "Vel. Ataque",
        StatType.AttackRange => "Rango Ataque",
        _ => ""
    };

    public string GetStatEffect(StatType s) => s switch
    {
        StatType.Health => $"+{hpPerLevel} HP",
        StatType.AttackSpeed => $"-{(1 - attackSpeedMultiplier) * 100:F0}% tiempo",
        StatType.AttackRange => $"+{rangedRangePerLevel} ranged / +{meleeRangePerLevel} melee",
        _ => ""
    };

    public void Reset()
    {
        foreach (var k in new List<StatType>(_levels.Keys)) _levels[k] = 0;
    }
}