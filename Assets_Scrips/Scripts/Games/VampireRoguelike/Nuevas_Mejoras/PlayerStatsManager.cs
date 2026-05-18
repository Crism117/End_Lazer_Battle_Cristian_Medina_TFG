using UnityEngine;

// ============================================================
// PLAYER STATS MANAGER
// Calcula y expone los stats finales del jugador en tiempo real[cite: 38].
// Mantiene los valores de vida, velocidad, daño, etc., para que
// otros scripts (como la UI o el AutoLevelUp) puedan leerlos[cite: 38].
// ============================================================
public class PlayerStatsManager : MonoBehaviour
{
    // Hacemos que sea un Singleton (solo existe uno en todo el juego)[cite: 38]
    public static PlayerStatsManager Instance { get; private set; }

    // Stats finales calculados (variables de solo lectura para otros scripts)[cite: 38]
    public int MaxHP { get; private set; }
    public float MoveSpeed { get; private set; }
    public float AttackSpeed { get; private set; }    // % de mejora[cite: 38]
    public float AttackRange { get; private set; }
    public float CritChance { get; private set; }    // Probabilidad de 0.0 a 1.0[cite: 38]
    public float LifeSteal { get; private set; }
    public int Damage { get; private set; }    // Daño total sumado[cite: 38]
    public float Armor { get; private set; }
    public int Luck { get; private set; }

    void Awake()
    {
        // Asegura que solo exista un manager. Si hay otro, se destruye[cite: 38].
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        // FIX: Comentado porque borramos PlayerInventory en la limpieza anterior
        // if (PlayerInventory.Instance != null)
        //     PlayerInventory.Instance.OnInventoryChanged += Recalculate;

        // Calcula los stats por primera vez al iniciar[cite: 38]
        Recalculate();
    }

    void OnDestroy()
    {
        // FIX: Comentado porque borramos PlayerInventory
        // if (PlayerInventory.Instance != null)
        //     PlayerInventory.Instance.OnInventoryChanged -= Recalculate;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Recalcula todos los stats desde cero leyendo al jugador[cite: 38]
    // ─────────────────────────────────────────────────────────────────────
    public void Recalculate()
    {
        // Busca al jugador en la escena[cite: 38]
        var player = FindObjectOfType<PlayerControllerRogueLite>();
        if (player == null) return;

        // Empieza con los valores base que tiene el script del jugador[cite: 38]
        MaxHP = player.maxHealth;
        MoveSpeed = player.moveSpeed;
        CritChance = player.critChance;
        LifeSteal = player.lifeSteal;
        Armor = player.resistance * 100f; // Lo convierte a % visual (ej. 0.5 -> 50%)[cite: 38]
        AttackSpeed = 0f;
        AttackRange = 0f;
        Luck = 0;
        Damage = 0;

        // FIX: Comentado porque borramos PlayerInventory y su sistema de suma de daño
        // if (PlayerInventory.Instance != null)
        // {
        //     foreach (var w in PlayerInventory.Instance.weaponSlots)
        //         if (w != null) Damage += w.projectileDamage * w.shotCount;
        // }
    }
}
