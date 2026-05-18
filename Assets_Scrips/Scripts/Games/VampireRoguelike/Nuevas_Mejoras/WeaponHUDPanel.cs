using UnityEngine;
using System.Collections.Generic;

// ============================================================
// WEAPON HUD PANEL
// 3 slots, loadout balanceado al inicio
// ============================================================
public class WeaponHUDPanel : MonoBehaviour
{
    [Header("─── SLOTS (3 en el HUD) ──")]
    public WeaponSlotHUD[] slots = new WeaponSlotHUD[3];

    [Header("─── REFERENCIAS ──")]
    public WeaponSystemV2 weaponSystem;

    [Header("─── COMPORTAMIENTO ──")]
    public bool randomizeLoadoutOnStart = true;

    void Start()
    {
        if (weaponSystem == null)
            weaponSystem = FindObjectOfType<WeaponSystemV2>();

        if (weaponSystem == null)
        {
            Debug.LogError("[WeaponHUDPanel] WeaponSystemV2 no encontrado.");
            return;
        }

        weaponSystem.OnWeaponUpgraded += OnWeaponUpgraded;

        if (randomizeLoadoutOnStart)
            GenerateLoadout();
        else
            RefreshAllSlots();
    }

    void OnDestroy()
    {
        if (weaponSystem != null)
            weaponSystem.OnWeaponUpgraded -= OnWeaponUpgraded;
    }

    void GenerateLoadout()
    {
        if (WeaponLootTable.Instance == null)
        {
            Debug.LogWarning("[WeaponHUDPanel] WeaponLootTable no encontrado.");
            return;
        }

        weaponSystem.weapons.Clear();
        weaponSystem.weaponLevels.Clear();

        var picks = WeaponLootTable.Instance.RollLoadout(slots.Length);

        foreach (var pick in picks)
            weaponSystem.AddWeapon(pick);

        RefreshAllSlots();
    }

    void RefreshAllSlots()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] == null) continue;
            if (i < weaponSystem.weapons.Count)
            {
                slots[i].gameObject.SetActive(true);
                slots[i].Setup(i, weaponSystem.weapons[i], this);
            }
            else
            {
                slots[i].gameObject.SetActive(false);
            }
        }
    }

    public bool OnUpgradeRequested(int slotIndex)
    {
        return weaponSystem.TryUpgradeWeapon(slotIndex);
    }

    void OnWeaponUpgraded(int index)
    {
        if (index >= 0 && index < slots.Length && slots[index] != null)
            slots[index].Refresh();
    }
}