using UnityEngine;

// ============================================================
// WEAPON ENUMS
// ============================================================
public enum WeaponType { Ranged, Melee }

public enum WeaponRarity { Common, Uncommon, Rare, Epic, Legendary }

public enum FireMode { Single, Burst, Spread }

public enum ProjectileType { LaserBeam, ParticleYellow, ParticleBlue, Sprite }

public enum SpecialPower
{
    None,
    Bleed, Burn, Poison, Stun,
    Pierce, Knockback, Slow, ChainExplosion,
    DoubleCoins, HighCrit, LifeStealHit, Fragmentation,
}

// ============================================================
// WEAPON DEFINITION (v6)
// + spriteScale per-arma
// + Growth stats per-arma (cada arma sube diferente)
// ============================================================
[System.Serializable]
public class WeaponDefinition
{
    [Header("Identificación")]
    public string id;
    public string displayName;
    [TextArea(2, 3)]
    public string description;

    [Header("Tipo y rareza")]
    public WeaponType weaponType = WeaponType.Ranged;
    public WeaponRarity rarity = WeaponRarity.Common;

    [Tooltip("Si es un gadget que orbita al jugador")]
    public bool isGadget = false;

    [Header("Costo en monedas")]
    public int cost = 100;

    [Header("Poder especial")]
    public SpecialPower specialPower = SpecialPower.None;

    [Header("Modo de disparo")]
    public FireMode fireMode = FireMode.Single;
    public float burstInterval = 0.08f;

    [Header("Sprites")]
    public GameObject visualPrefab;
    public Sprite icon;

    [Header("Prefab del proyectil")]
    public GameObject projectilePrefab;
    public GameObject gadgetPrefab;

    [Header("─── ESCALA Y POSICIÓN ──")]
    [Tooltip("Escala del sprite del arma (0 = usa default)")]
    [Range(0.1f, 3f)]
    public float spriteScale = 0.5f;

    [Tooltip("Punto de salida del proyectil (offset desde el sprite)")]
    public Vector2 firePointOffset = new Vector2(0.5f, 0f);

    [Header("─── STATS BASE (nivel 1) ──")]
    [Tooltip("Daño base. Empieza en 1-2 según el documento.")]
    public int projectileDamage = 1;

    [Tooltip("Cadencia: segundos entre disparos. Menos = más rápido")]
    public float fireRate = 0.5f;
    public float projectileSpeed = 6f;
    public int shotCount = 1;
    public float spreadAngle = 10f;
    public float range = 0f;

    [Header("─── GROWTH POR NIVEL (cada arma sube diferente) ──")]
    [Tooltip("Daño que se suma por nivel (escopeta sube más, revólver menos)")]
    public int damagePerLevel = 1;

    [Tooltip("% de mejora del fireRate por nivel (0.08 = 8% más rápido)")]
    [Range(0f, 0.3f)]
    public float fireRateImprovementPerLevel = 0.05f;

    [Tooltip("Solo RANGED: bonus de probabilidad de proc por nivel (0.02 = +2%)")]
    [Range(0f, 0.2f)]
    public float procChanceBoostPerLevel = 0.01f;

    [Header("─── PROBABILIDAD BASE DE PROC (Ranged) ──")]
    [Tooltip("Probabilidad de activar la pasiva al nivel 1 (0.05 = 5%)")]
    [Range(0f, 1f)]
    public float baseProcChance = 0.05f;

    [Header("─── MODIFICADORES ESPECIALES ──")]
    public float knockbackForce = 0f;
    public float aoeRadius = 0f;
    public int dotDamage = 0;
    public float effectDuration = 2f;

    [HideInInspector] public bool unlocked = false;

    /// <summary>
    /// Probabilidad de proc actual según el nivel.
    /// </summary>
    public float GetProcChanceAtLevel(int level)
    {
        return Mathf.Clamp01(baseProcChance + (level - 1) * procChanceBoostPerLevel);
    }

    // ============================================================
    // CATÁLOGO POR DEFECTO (10 armas balanceadas con stats bajos)
    // ============================================================
    public static System.Collections.Generic.List<WeaponDefinition> BuildDefaultCatalog()
    {
        var list = new System.Collections.Generic.List<WeaponDefinition>();

        // ── 5 ARMAS RANGED ──────────────────────────────────────────
        list.Add(new WeaponDefinition
        {
            id = "revolver",
            displayName = "Revólver",
            weaponType = WeaponType.Ranged,
            rarity = WeaponRarity.Common,
            description = "Disparo único preciso.",
            cost = 120,
            projectileDamage = 1,
            fireRate = 0.6f,
            projectileSpeed = 10f,
            shotCount = 1,
            fireMode = FireMode.Single,
            damagePerLevel = 2,
            fireRateImprovementPerLevel = 0.05f,
            procChanceBoostPerLevel = 0.02f,
            baseProcChance = 0.05f,
            specialPower = SpecialPower.Pierce,
            spriteScale = 0.5f
        });

        list.Add(new WeaponDefinition
        {
            id = "shotgun",
            displayName = "Escopeta",
            weaponType = WeaponType.Ranged,
            rarity = WeaponRarity.Common,
            description = "5 perdigones en abanico.",
            cost = 180,
            projectileDamage = 1,
            fireRate = 1.2f,
            projectileSpeed = 7f,
            shotCount = 5,
            fireMode = FireMode.Spread,
            spreadAngle = 25f,
            damagePerLevel = 4, // ESCOPETA SUBE MÁS DAÑO
            fireRateImprovementPerLevel = 0.05f,
            procChanceBoostPerLevel = 0.03f,
            baseProcChance = 0.10f,
            specialPower = SpecialPower.Knockback,
            knockbackForce = 4f,
            spriteScale = 0.5f
        });

        list.Add(new WeaponDefinition
        {
            id = "machinegun",
            displayName = "Metralleta",
            weaponType = WeaponType.Ranged,
            rarity = WeaponRarity.Rare,
            description = "Ráfagas de 3 balas directas.",
            cost = 280,
            projectileDamage = 1,
            fireRate = 0.6f,
            projectileSpeed = 12f,
            shotCount = 3,
            fireMode = FireMode.Burst,
            burstInterval = 0.08f,
            damagePerLevel = 2,
            fireRateImprovementPerLevel = 0.10f, // METRALLETA: MÁS CADENCIA
            procChanceBoostPerLevel = 0.01f,
            specialPower = SpecialPower.None,
            spriteScale = 0.4f
        });

        list.Add(new WeaponDefinition
        {
            id = "laser_pistol",
            displayName = "Pistola Láser",
            weaponType = WeaponType.Ranged,
            rarity = WeaponRarity.Epic,
            description = "Disparos de energía rápidos.",
            cost = 450,
            projectileDamage = 2,
            fireRate = 0.2f,
            projectileSpeed = 14f,
            shotCount = 1,
            fireMode = FireMode.Single,
            damagePerLevel = 3,
            fireRateImprovementPerLevel = 0.04f,
            procChanceBoostPerLevel = 0.04f,
            baseProcChance = 0.15f,
            specialPower = SpecialPower.Slow,
            effectDuration = 2f,
            spriteScale = 0.5f
        });

        list.Add(new WeaponDefinition
        {
            id = "laser_chaingun",
            displayName = "Láser Metralla",
            weaponType = WeaponType.Ranged,
            rarity = WeaponRarity.Epic,
            description = "Cañón de plasma con explosión en cadena.",
            cost = 550,
            projectileDamage = 2,
            fireRate = 2.0f,
            projectileSpeed = 8f,
            shotCount = 1,
            fireMode = FireMode.Single,
            damagePerLevel = 3,
            fireRateImprovementPerLevel = 0.06f,
            procChanceBoostPerLevel = 0.03f,
            specialPower = SpecialPower.ChainExplosion,
            aoeRadius = 3f,
            spriteScale = 0.6f
        });

        // ── 5 ARMAS MELEE ───────────────────────────────────────────
        list.Add(new WeaponDefinition
        {
            id = "knife",
            displayName = "Cuchillo",
            weaponType = WeaponType.Melee,
            rarity = WeaponRarity.Common,
            description = "Apuñala muy rápido.",
            cost = 100,
            projectileDamage = 1,
            fireRate = 0.15f,
            damagePerLevel = 1,
            fireRateImprovementPerLevel = 0.03f,
            specialPower = SpecialPower.LifeStealHit,
            spriteScale = 0.4f
        });

        list.Add(new WeaponDefinition
        {
            id = "bat",
            displayName = "Bate",
            weaponType = WeaponType.Melee,
            rarity = WeaponRarity.Common,
            description = "Golpes rápidos.",
            cost = 140,
            projectileDamage = 2,
            fireRate = 0.3f,
            damagePerLevel = 2,
            fireRateImprovementPerLevel = 0.04f,
            specialPower = SpecialPower.HighCrit,
            spriteScale = 0.5f
        });

        list.Add(new WeaponDefinition
        {
            id = "axe",
            displayName = "Hacha",
            weaponType = WeaponType.Melee,
            rarity = WeaponRarity.Common,
            description = "Provoca sangrado.",
            cost = 200,
            projectileDamage = 2,
            fireRate = 1.8f,
            damagePerLevel = 3,
            fireRateImprovementPerLevel = 0.05f,
            specialPower = SpecialPower.Bleed,
            dotDamage = 2,
            effectDuration = 3f,
            spriteScale = 0.6f
        });

        list.Add(new WeaponDefinition
        {
            id = "hammer",
            displayName = "Martillo",
            weaponType = WeaponType.Melee,
            rarity = WeaponRarity.Rare,
            description = "Empuja al enemigo.",
            cost = 320,
            projectileDamage = 2,
            fireRate = 2.5f,
            damagePerLevel = 5, // MARTILLO: DAÑO BRUTO
            fireRateImprovementPerLevel = 0.06f,
            specialPower = SpecialPower.Knockback,
            knockbackForce = 8f,
            spriteScale = 0.7f
        });

        list.Add(new WeaponDefinition
        {
            id = "chainsaw",
            displayName = "Motosierra",
            weaponType = WeaponType.Melee,
            rarity = WeaponRarity.Epic,
            description = "Daño continuo + duplica monedas.",
            cost = 500,
            projectileDamage = 1,
            fireRate = 0.1f,
            damagePerLevel = 1,
            fireRateImprovementPerLevel = 0.05f,
            specialPower = SpecialPower.DoubleCoins,
            isGadget = true,
            spriteScale = 0.5f
        });

        return list;
    }
}