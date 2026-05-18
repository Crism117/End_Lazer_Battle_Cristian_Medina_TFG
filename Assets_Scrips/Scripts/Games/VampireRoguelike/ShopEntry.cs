using Unity.VisualScripting;
using UnityEngine;

// ============================================================
// SHOP ENTRY
// Datos de una entrada de la tienda: o es arma o es item.
// Usa WeaponConfig (no WeaponDefinition) para ser coherente
// con LevelUpSystem y WeaponSystemV2.
// ============================================================
public class ShopEntry
{
    public WeaponConfig weapon;   // <- WeaponConfig, NO WeaponDefinition
    public ItemDefinition item;

    public string DisplayName => weapon != null
        ? weapon.displayName : item?.displayName ?? "";

    public string Description => weapon != null
        ? weapon.description : item?.description ?? "";

    public int Cost => weapon != null
        ? weapon.cost : item?.cost ?? 0;

    public Sprite Icon => weapon != null
        ? weapon.icon : item?.icon;
}
