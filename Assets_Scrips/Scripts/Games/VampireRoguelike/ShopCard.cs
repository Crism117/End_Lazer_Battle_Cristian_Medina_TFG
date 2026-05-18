using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using DG.Tweening;

// ============================================================
// SHOP CARD
// Card individual de la tienda. Muestra arma o item.
// Animaciones DOTween:
//   - Hover: sube y escala ligeramente
//   - Compra: gestionada desde ShopPanel
// ============================================================
public class ShopCard : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("─── ELEMENTOS ──────────────────────")]
    public Image imgIcon;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtDescription;
    public TextMeshProUGUI txtCost;
    public Button btnBuy;
    public GameObject soldOutOverlay;

    [Header("─── HOVER ──────────────────────────")]
    [Tooltip("Cuánto sube la card al hacer hover (píxeles)")]
    public float hoverLift = 8f;
    [Tooltip("Escala al hacer hover")]
    public float hoverScale = 1.05f;
    [Tooltip("Duración del hover")]
    public float hoverDuration = 0.15f;

    // ─────────────────────────────────────────────────────────
    ShopEntry _entry;
    Action<ShopEntry, ShopCard> _onBuy;
    RectTransform _rt;
    Vector2 _basePos;
    bool _sold;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _basePos = _rt.anchoredPosition;
    }

    public void SetEntry(ShopEntry entry, Action<ShopEntry, ShopCard> onBuy)
    {
        _entry = entry;
        _onBuy = onBuy;
        _sold = false;

        gameObject.SetActive(true);
        if (soldOutOverlay != null) soldOutOverlay.SetActive(false);

        if (imgIcon != null)
        {
            imgIcon.sprite = entry.Icon;
            imgIcon.enabled = entry.Icon != null;
        }

        if (txtName) txtName.text = entry.DisplayName;
        if (txtDescription) txtDescription.text = entry.Description;
        if (txtCost) txtCost.text = $"{entry.Cost} 💰";

        if (btnBuy != null)
        {
            btnBuy.onClick.RemoveAllListeners();
            btnBuy.onClick.AddListener(() => _onBuy?.Invoke(_entry, this));
        }
    }

    public void Refresh(int coins)
    {
        if (_entry == null || btnBuy == null || _sold) return;
        btnBuy.interactable = coins >= _entry.Cost;
    }

    public void Clear()
    {
        _sold = true;
        _entry = null;
        if (soldOutOverlay != null) soldOutOverlay.SetActive(true);
        if (btnBuy != null) btnBuy.interactable = false;
    }

    // ─── Hover ────────────────────────────────────────────────

    public void OnPointerEnter(PointerEventData _)
    {
        if (_sold) return;
        DOTween.Kill(_rt);
        _rt.DOAnchorPosY(_basePos.y + hoverLift, hoverDuration)
           .SetEase(Ease.OutQuad).SetUpdate(true);
        _rt.DOScale(hoverScale, hoverDuration)
           .SetEase(Ease.OutQuad).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData _)
    {
        DOTween.Kill(_rt);
        _rt.DOAnchorPosY(_basePos.y, hoverDuration)
           .SetEase(Ease.OutQuad).SetUpdate(true);
        _rt.DOScale(1f, hoverDuration)
           .SetEase(Ease.OutQuad).SetUpdate(true);
    }
}