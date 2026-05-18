using UnityEngine;
using System.Collections.Generic;

// ============================================================
// LEVEL UP SYSTEM (v5)
// Catálogo de armas (WeaponDefinition).
// Las armas EPIC tienen probabilidad muy baja al inicio
// y suben de probabilidad al avanzar las oleadas.
// ============================================================
public class LevelUpSystem : MonoBehaviour
{
    public static LevelUpSystem Instance { get; private set; }

    [Header("Catálogo completo de armas (WeaponDefinition)")]
    [Tooltip("Las 10 armas del juego configuradas desde el Inspector")]
    public List<WeaponDefinition> weaponCatalog = new List<WeaponDefinition>();

    readonly HashSet<string> _unlockedIds = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public void Reset()
    {
        _unlockedIds.Clear();
        foreach (var w in weaponCatalog) w.unlocked = false;
    }

    public bool HasWeapon(string id) => _unlockedIds.Contains(id);

    public void UnlockWeapon(string id)
    {
        _unlockedIds.Add(id);
        var def = weaponCatalog.Find(w => w.id == id);
        if (def != null) def.unlocked = true;
    }

    public WeaponDefinition GetWeaponById(string id)
    {
        return weaponCatalog.Find(w => w.id == id);
    }

    // ── Selección aleatoria de armas según oleada ──
    public List<WeaponDefinition> PickRandomWeapons(int count)
    {
        int wave = WaveSystemRogueLite.Instance != null
            ? WaveSystemRogueLite.Instance.ActiveWave?.waveNumber ?? 1
            : 1;

        var candidates = new List<(WeaponDefinition def, int weight)>();
        foreach (var w in weaponCatalog)
        {
            if (w.unlocked) continue;
            int weight = GetWeightForWave(w.rarity, wave);
            if (weight > 0) candidates.Add((w, weight));
        }

        var result = new List<WeaponDefinition>();
        while (result.Count < count && candidates.Count > 0)
        {
            int totalWeight = 0;
            foreach (var c in candidates) totalWeight += c.weight;
            int roll = UnityEngine.Random.Range(0, totalWeight);
            int acc = 0;
            for (int i = 0; i < candidates.Count; i++)
            {
                acc += candidates[i].weight;
                if (roll < acc)
                {
                    result.Add(candidates[i].def);
                    candidates.RemoveAt(i);
                    break;
                }
            }
        }
        return result;
    }

    int GetWeightForWave(WeaponRarity rarity, int wave)
    {
        return rarity switch
        {
            WeaponRarity.Common => 100,
            WeaponRarity.Uncommon => wave >= 2 ? 60 : 0,
            WeaponRarity.Rare => wave >= 4 ? 30 : 0,
            WeaponRarity.Epic => wave >= 7 ? 8 : (wave >= 4 ? 2 : 0),
            WeaponRarity.Legendary => wave >= 9 ? 2 : 0,
            _ => 0,
        };
    }

    public bool TryBuyWeapon(WeaponDefinition def)
    {
        if (def == null || def.unlocked) return false;
        if (CoinSystem.Instance == null) return false;
        if (!CoinSystem.Instance.SpendCoins(def.cost)) return false;

        var ws = FindObjectOfType<WeaponSystemV2>();
        if (ws != null)
        {
            var copy = WeaponSystemV2.CloneConfig(def);
            ws.AddWeapon(copy);
        }

        UnlockWeapon(def.id);
        return true;
    }
}