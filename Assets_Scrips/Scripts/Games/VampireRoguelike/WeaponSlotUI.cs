using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

// ============================================================
// WEAPON SLOT UI
// Una celda del inventario de armas.
// Muestra el icono del arma y un botón X para quitarla.
// Clic derecho también quita el arma.
// Usa WeaponConfig (no WeaponDefinition).
// ============================================================
public class WeaponSlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Elementos")]
    public Image iconImage;
    public Button removeButton;    // Botón X esquina superior derecha
    public GameObject emptyOverlay;    // Oscurece el slot cuando está vacío

    Action _onRemove;

    public void SetWeapon(WeaponConfig def, Action onRemove)
    {
        _onRemove = onRemove;
        bool empty = def == null;

        if (iconImage != null)
        {
            iconImage.enabled = !empty;
            if (!empty && def.icon != null)
                iconImage.sprite = def.icon;
        }

        if (emptyOverlay != null)
            emptyOverlay.SetActive(empty);

        if (removeButton != null)
        {
            removeButton.gameObject.SetActive(!empty);
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(() => _onRemove?.Invoke());
        }
    }

    // Clic derecho también quita el arma
    public void OnPointerClick(PointerEventData e)
    {
        if (e.button == PointerEventData.InputButton.Right)
            _onRemove?.Invoke();
    }
}

