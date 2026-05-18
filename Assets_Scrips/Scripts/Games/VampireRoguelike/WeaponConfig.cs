using UnityEngine;

// ============================================================
// WEAPON CONFIG v3
// Configuración completa de un arma. Todo asignable en Inspector.
// 10 niveles máximo.
// Pasiva FIJA por arma + sprite de proyectil propio + posición preferida.
// ============================================================
[System.Serializable]
public class WeaponConfig
{
    // ═══════════════════════════════════════════════════════════════════
    // IDENTIFICACIÓN
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── IDENTIFICACIÓN ──────────────────")]
    public string id;
    public string displayName;
    [Tooltip("Descripción mostrada en tooltip")]
    public string description;
    public WeaponType weaponType = WeaponType.Ranged;

    [Tooltip("Determina probabilidad de aparecer en la lotería")]
    public WeaponRarity rarity = WeaponRarity.Common;

    // ═══════════════════════════════════════════════════════════════════
    // VISUAL
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── VISUAL DEL ARMA ─────────────────")]
    public GameObject visualPrefab;

    [Range(0.1f, 3f)]
    public float spriteScale = 0.5f;

    [Tooltip("Sprite del icono mostrado en el HUD")]
    public Sprite icon;

    // ── POSICIÓN PREFERIDA EN EL JUGADOR (NUEVO) ─────────────────────────
    public enum WeaponPosition { Front, LeftFlank, RightFlank }

    [Header("─── POSICIÓN EN JUGADOR ────────────")]
    [Tooltip("Dónde se sitúa el arma en relación al jugador")]
    public WeaponPosition preferredPosition = WeaponPosition.Front;

    [Tooltip("Distancia desde el jugador (offset radial)")]
    [Range(0.1f, 3f)]
    public float positionDistance = 0.8f;

    [Tooltip("Rotación adicional del sprite (grados, ajuste fino)")]
    [Range(-180f, 180f)]
    public float spriteRotation = 0f;

    // ═══════════════════════════════════════════════════════════════════
    // PUNTO DE DISPARO
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── PUNTO DE DISPARO ────────────────")]
    public Vector2 projectileOffset = Vector2.zero;

    // ═══════════════════════════════════════════════════════════════════
    // PROYECTIL (NUEVO — sprites en lugar de partículas)
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── PROYECTIL ──────────────────────")]
    [Tooltip("Tipo de proyectil. Sprite = usa projectileSprite. LaserBeam = línea recta.")]
    public ProjectileType projectileType = ProjectileType.Sprite;

    [Tooltip("Sprite del proyectil (bala, bola oscura, etc.)")]
    public Sprite projectileSprite;

    [Tooltip("Escala del sprite del proyectil")]
    [Range(0.1f, 3f)]
    public float projectileScale = 1f;

    [Tooltip("Color tinte del proyectil")]
    public Color projectileColor = Color.white;

    // ═══════════════════════════════════════════════════════════════════
    // MODO DE DISPARO
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── MODO DE DISPARO ────────────────")]
    public FireMode fireMode = FireMode.Single;
    [Range(1, 10)] public int shotCount = 1;
    [Range(0f, 45f)] public float spreadAngle = 15f;
    public float burstInterval = 0.08f;

    // ═══════════════════════════════════════════════════════════════════
    // CADENCIA Y DAÑO
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── CADENCIA ───────────────────────")]
    [Range(0.05f, 5f)]
    public float fireRate = 0.5f;

    [Header("─── DAÑO ───────────────────────────")]
    public int projectileDamage = 1;
    public float projectileSpeed = 7f;

    // ═══════════════════════════════════════════════════════════════════
    // LÁSER (rayo directo)
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── SOLO LÁSER ─────────────────────")]
    [Range(0.05f, 1f)] public float laserWidth = 0.2f;
    public Color laserColor = new Color(1f, 0.3f, 0.3f, 1f);

    // ═══════════════════════════════════════════════════════════════════
    // MELEE
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── SOLO MELEE ─────────────────────")]
    [Range(0.5f, 5f)] public float meleeRadius = 1.8f;
    [Tooltip("Ángulo del swing (animación bate)")]
    [Range(10f, 90f)] public float swingAngle = 40f;
    [Tooltip("Duración del swing en segundos")]
    [Range(0.05f, 1f)] public float swingDuration = 0.2f;

    // ═══════════════════════════════════════════════════════════════════
    // RANGO DE ATAQUE
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── RANGO DE ATAQUE ─────────────────")]
    [Tooltip("Radio de detección/disparo de ESTA arma")]
    [Range(1f, 20f)]
    public float attackRadius = 6f;

    // ═══════════════════════════════════════════════════════════════════
    // PASIVA — UN SOLO EFECTO FIJO POR ARMA
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── PASIVA (FIJA) ───────────────────")]
    [Tooltip("Efecto pasivo del arma. Bleed/Burn/Stun/Poison")]
    public SpecialPower specialPower = SpecialPower.None;

    [Tooltip("Daño del DOT (Bleed/Burn/Poison)")]
    public int dotDamage = 1;

    [Tooltip("Duración del efecto (segundos)")]
    public float dotDuration = 2f;

    [Tooltip("Fuerza de Knockback (compat — no usado por defecto)")]
    public float knockbackForce = 0f;

    // ── PROBABILIDAD DE PROC (NUEVO) ─────────────────────────────────────
    [Header("─── PROBABILIDAD PASIVA ─────────────")]
    [Tooltip("Probabilidad de activar la pasiva al lvl 1 (ej: 0.01 = 1%)")]
    [Range(0f, 1f)]
    public float procChanceBase = 0.01f;

    [Tooltip("Cuánto sube la probabilidad por nivel (ej: 0.005 = +0.5% por lvl)")]
    [Range(0f, 0.2f)]
    public float procChancePerLevel = 0.005f;

    // ═══════════════════════════════════════════════════════════════════
    // UPGRADE
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── UPGRADE: COSTE ──────────────────")]
    [Tooltip("Precio del primer upgrade (lvl 1 → lvl 2)")]
    public int baseUpgradeCost = 50;

    [Tooltip("Multiplicador del coste por cada nivel: cost = base * mult^(level-1)")]
    [Range(1f, 3f)]
    public float upgradeCostMultiplier = 1.5f;

    [Header("─── UPGRADE: GROWTH POR NIVEL ───────")]
    [Tooltip("Daño adicional por nivel")]
    public int damageGrowthPerLevel = 1;

    [Tooltip("% de fireRate reducido por nivel (0.08 = 8% más rápido)")]
    [Range(0f, 0.5f)]
    public float fireRateGrowthPerLevel = 0.08f;

    [Tooltip("MELEE: aumento de meleeRadius por nivel (solo lvl 1-5)")]
    [Range(0f, 1f)]
    public float meleeRadiusGrowthPerLevel = 0.2f;

    [Tooltip("RANGED: aumento del attackRadius por nivel (todos los niveles)")]
    [Range(0f, 1f)]
    public float attackRadiusGrowthPerLevel = 0.3f;

    [Tooltip("Aumento del DOT por nivel")]
    public int dotGrowthPerLevel = 1;

    // ═══════════════════════════════════════════════════════════════════
    // ESTADO ACTUAL
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── ESTADO ACTUAL ──────────────────")]
    [Tooltip("Nivel actual (1-10)")]
    [Range(1, 10)]
    public int level = 1;

    public const int MAX_LEVEL = 10;

    [Tooltip("Coste antiguo, conservado por compatibilidad")]
    public int cost = 100;

    [HideInInspector] public bool unlocked = false;

    // ─────────────────────────────────────────────────────────────────────
    // HELPERS DE UPGRADE
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Coste actual para subir al siguiente nivel.
    /// Si está al máximo, devuelve -1.
    /// </summary>
    public int GetCurrentUpgradeCost()
    {
        if (level >= MAX_LEVEL) return -1;
        return Mathf.RoundToInt(baseUpgradeCost * Mathf.Pow(upgradeCostMultiplier, level - 1));
    }

    /// <summary>¿Está al nivel máximo?</summary>
    public bool IsMaxLevel() => level >= MAX_LEVEL;

    /// <summary>
    /// Aplica un upgrade al arma. Reglas:
    /// - Daño y fireRate suben siempre.
    /// - Melee: rango sube SOLO niveles 2-5 (cuando llegas al nivel 6+ ya no sube).
    /// - Ranged: rango sube en todos los niveles.
    /// - DOT damage sube siempre.
    /// </summary>
    public void ApplyUpgrade()
    {
        if (IsMaxLevel()) return;

        level++;

        // Stats que SIEMPRE suben
        projectileDamage += damageGrowthPerLevel;
        fireRate = Mathf.Max(0.05f, fireRate * (1f - fireRateGrowthPerLevel));
        dotDamage += dotGrowthPerLevel;

        // RANGO: depende del tipo de arma
        if (weaponType == WeaponType.Melee)
        {
            // Melee: solo sube rango hasta el nivel 5 (ahora el level vale 2-5)
            if (level <= 5)
                meleeRadius += meleeRadiusGrowthPerLevel;
            // A partir de nivel 6: NO sube rango
        }
        else
        {
            // Ranged: sube siempre
            attackRadius += attackRadiusGrowthPerLevel;
        }
    }

    /// <summary>
    /// Probabilidad actual de procar la pasiva, en función del nivel.
    /// </summary>
    public float GetCurrentProcChance()
    {
        return Mathf.Clamp01(procChanceBase + procChancePerLevel * (level - 1));
    }
}