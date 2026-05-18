using UnityEngine;
using System.Collections.Generic;

// Tipo de efecto pasivo
public enum ItemEffect
{
    MoveSpeed,        // Botas / Capa
    MaxHP,            // Escudo protector
    Magnet,           // Imán
    LifeSteal,        // Dientes de vampiro
    AOERange,         // Binoculares
    CritChance,       // Amuleto del lobo feroz
    DoubleCoins,      // Collar avaricioso
}

// ============================================================
// ITEM DEFINITION
// Cada item es una pasiva que mejora un stat del jugador.
// Son apilables (stackable), se acumulan con cada compra
// hasta el maxStacks.
// ============================================================
[System.Serializable]
public class ItemDefinition
{
    [Header("Identificación")]
    public string id;
    public string displayName;
    [TextArea(2, 3)]
    public string description;

    [Header("Costo")]
    public int cost = 100;

    [Header("Apilamiento")]
    [Tooltip("Cuántas veces se puede comprar este item")]
    public int maxStacks = 10;

    [Header("Efecto")]
    public ItemEffect effect;

    [Tooltip("Cantidad de efecto que da CADA stack (cada compra)")]
    public float effectAmount = 0.1f;

    [Header("Sprite (opcional)")]
    public Sprite icon;
}

// ============================================================
// ITEM SYSTEM
// Gestiona todos los items que el jugador acumula.
// Los efectos se aplican automáticamente al jugador.
// ============================================================
public class ItemSystem : MonoBehaviour
{
    public static ItemSystem Instance { get; private set; }

    [Header("Catálogo de items")]
    public List<ItemDefinition> itemCatalog = new List<ItemDefinition>();

    // Cuántas copias tiene el jugador de cada item
    Dictionary<string, int> _stacks = new Dictionary<string, int>();

    // Propiedades de consulta para otros scripts
    public int MagnetStacks => GetStack("magnet");
    public bool HasDoubleCoins => GetStack("collar") > 0;
    public int AOERangeStacks => GetStack("binoculares");

    // Propiedades de compatibilidad con CoinPickup y EnemyRogueLite
    public bool HasMagnet => GetStack("magnet") > 0;      // CoinPickup línea 60
    public bool HasGuante => GetStack("collar") > 0;      // CoinPickup línea 75
    public int XPBonusStacks => GetStack("collar");          // EnemyRogueLite línea 223

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (itemCatalog.Count == 0) BuildDefaultCatalog();
    }

    public int GetStack(string id) => _stacks.TryGetValue(id, out int v) ? v : 0;

    public bool CanBuy(ItemDefinition def) =>
        def != null && GetStack(def.id) < def.maxStacks;

    public void Reset() { _stacks.Clear(); }

    /// <summary>
    /// Añade stacks de un item por ID (sin gastar monedas).
    /// Llamado por MapItemPickup cuando el jugador recoge un item del mapa.
    /// </summary>
    public void AddStack(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (!_stacks.ContainsKey(id)) _stacks[id] = 0;
        _stacks[id] += amount;
    }

    // Devuelve N items aleatorios disponibles para la tienda
    public List<ItemDefinition> PickRandomItems(int count)
    {
        var available = new List<ItemDefinition>();
        foreach (var item in itemCatalog)
            if (CanBuy(item)) available.Add(item);

        var result = new List<ItemDefinition>();
        while (result.Count < count && available.Count > 0)
        {
            int idx = Random.Range(0, available.Count);
            result.Add(available[idx]);
            available.RemoveAt(idx);
        }
        return result;
    }

    // Compra un item con monedas y aplica su efecto al jugador
    public bool TryBuyItem(ItemDefinition def)
    {
        if (def == null || !CanBuy(def)) return false;
        if (CoinSystem.Instance == null) return false;
        if (!CoinSystem.Instance.SpendCoins(def.cost)) return false;

        // Suma una copia
        if (!_stacks.ContainsKey(def.id)) _stacks[def.id] = 0;
        _stacks[def.id]++;

        ApplyEffect(def);
        return true;
    }

    void ApplyEffect(ItemDefinition def)
    {
        var player = FindObjectOfType<PlayerControllerRogueLite>();
        if (player == null) return;

        switch (def.effect)
        {
            case ItemEffect.MoveSpeed:
                player.moveSpeed += def.effectAmount;
                break;
            case ItemEffect.MaxHP:
                int hpAdd = Mathf.RoundToInt(def.effectAmount);
                player.maxHealth += hpAdd;
                player.Heal(hpAdd);
                break;
            case ItemEffect.LifeSteal:
                player.lifeSteal = Mathf.Min(1f, player.lifeSteal + def.effectAmount);
                break;
            case ItemEffect.CritChance:
                player.critChance = Mathf.Min(1f, player.critChance + def.effectAmount);
                break;
            // Magnet, DoubleCoins, AOERange se consultan vía propiedades
            case ItemEffect.Magnet:
            case ItemEffect.DoubleCoins:
            case ItemEffect.AOERange:
                break;
        }
    }

    // ── Catálogo por defecto: 8 items según tu lista ─────────────────────
    void BuildDefaultCatalog()
    {
        itemCatalog = new List<ItemDefinition>
        {
            new ItemDefinition
            {
                id="botas", displayName="Botas Ligeras",
                description="+0.4 velocidad de movimiento por stack.",
                cost=100, maxStacks=10, effect=ItemEffect.MoveSpeed, effectAmount=0.4f
            },
            new ItemDefinition
            {
                id="escudo", displayName="Escudo Protector",
                description="+5 HP máximo por stack. Hasta 10 stacks.",
                cost=150, maxStacks=10, effect=ItemEffect.MaxHP, effectAmount=5f
            },
            new ItemDefinition
            {
                id="magnet", displayName="Imán",
                description="Atrae monedas. Más stacks = más rango de atracción.",
                cost=200, maxStacks=5, effect=ItemEffect.Magnet, effectAmount=2f
            },
            new ItemDefinition
            {
                id="vampiro", displayName="Dientes de Vampiro",
                description="+5% probabilidad de robar HP al pegar.",
                cost=250, maxStacks=10, effect=ItemEffect.LifeSteal, effectAmount=0.05f
            },
            new ItemDefinition
            {
                id="capa", displayName="Capa",
                description="+1.0 velocidad de movimiento (efecto fuerte).",
                cost=400, maxStacks=2, effect=ItemEffect.MoveSpeed, effectAmount=1.0f
            },
            new ItemDefinition
            {
                id="binoculares", displayName="Binoculares",
                description="Aumenta el rango de daño en área y stats al subir nivel.",
                cost=300, maxStacks=5, effect=ItemEffect.AOERange, effectAmount=0.5f
            },
            new ItemDefinition
            {
                id="lobo", displayName="Amuleto del Lobo Feroz",
                description="+8% probabilidad de crítico por stack.",
                cost=280, maxStacks=10, effect=ItemEffect.CritChance, effectAmount=0.08f
            },
            new ItemDefinition
            {
                id="collar", displayName="Collar Avaricioso",
                description="Las monedas valen el doble (×2). No apilable.",
                cost=350, maxStacks=1, effect=ItemEffect.DoubleCoins, effectAmount=0f
            },
        };
    }
}
