using UnityEngine;
using System.Collections.Generic;

// ============================================================
// WEAPON LOOT TABLE
// Sortea armas balanceadas: 2R+1M o 2M+1R con probabilidad por rareza.
// ============================================================
public class WeaponLootTable : MonoBehaviour
{
    public static WeaponLootTable Instance { get; private set; }

    [Header("─── ARSENAL DEL JUEGO (10 armas) ──")]
    public List<WeaponDefinition> allWeapons = new List<WeaponDefinition>();

    [Header("─── PESO POR RAREZA ──")]
    public int weightCommon = 50;
    public int weightUncommon = 30;
    public int weightRare = 15;
    public int weightEpic = 4;
    public int weightLegendary = 1;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    public List<WeaponDefinition> RollLoadout(int count = 3)
    {
        var result = new List<WeaponDefinition>();

        if (allWeapons == null || allWeapons.Count == 0)
        {
            Debug.LogError("[LootTable] No hay armas en el arsenal.");
            return result;
        }

        bool moreRanged = UnityEngine.Random.value < 0.5f;
        int rangedCount = moreRanged ? 2 : 1;
        int meleeCount = moreRanged ? 1 : 2;

        if (debugMode)
            Debug.Log($"[LootTable] {rangedCount} ranged + {meleeCount} melee");

        var rangedPool = FilterByType(WeaponType.Ranged);
        for (int i = 0; i < rangedCount && rangedPool.Count > 0; i++)
        {
            var pick = WeightedPick(rangedPool);
            if (pick != null)
            {
                result.Add(WeaponSystemV2.CloneConfig(pick));
                rangedPool.Remove(pick);
            }
        }

        var meleePool = FilterByType(WeaponType.Melee);
        for (int i = 0; i < meleeCount && meleePool.Count > 0; i++)
        {
            var pick = WeightedPick(meleePool);
            if (pick != null)
            {
                result.Add(WeaponSystemV2.CloneConfig(pick));
                meleePool.Remove(pick);
            }
        }

        while (result.Count < count && allWeapons.Count > 0)
        {
            var any = allWeapons[UnityEngine.Random.Range(0, allWeapons.Count)];
            result.Add(WeaponSystemV2.CloneConfig(any));
        }

        if (debugMode)
        {
            string names = "";
            foreach (var w in result) names += $"{w.displayName}({w.rarity}) ";
            Debug.Log($"[LootTable] Loadout: {names}");
        }

        return result;
    }

    List<WeaponDefinition> FilterByType(WeaponType type)
    {
        var list = new List<WeaponDefinition>();
        foreach (var w in allWeapons)
            if (w != null && w.weaponType == type)
                list.Add(w);
        return list;
    }

    WeaponDefinition WeightedPick(List<WeaponDefinition> pool)
    {
        if (pool.Count == 0) return null;

        int totalWeight = 0;
        foreach (var w in pool) totalWeight += GetWeightForRarity(w.rarity);
        if (totalWeight <= 0) return pool[UnityEngine.Random.Range(0, pool.Count)];

        int roll = UnityEngine.Random.Range(0, totalWeight);
        int accum = 0;
        foreach (var w in pool)
        {
            accum += GetWeightForRarity(w.rarity);
            if (roll < accum) return w;
        }
        return pool[pool.Count - 1];
    }

    int GetWeightForRarity(WeaponRarity rarity)
    {
        return rarity switch
        {
            WeaponRarity.Common => weightCommon,
            WeaponRarity.Uncommon => weightUncommon,
            WeaponRarity.Rare => weightRare,
            WeaponRarity.Epic => weightEpic,
            WeaponRarity.Legendary => weightLegendary,
            _ => weightCommon
        };
    }
}
