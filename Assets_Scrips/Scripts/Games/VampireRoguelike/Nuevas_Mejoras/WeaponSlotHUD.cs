using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using DG.Tweening;

// ============================================================
// WEAPON SLOT HUD — REDISEÑADO
// - BgPanel (fondo oscuro semitransparente)
// - Icono del arma centrado
// - Nivel ARRIBA (fuera de la celda)
// - Precio ABAJO (fuera) con icono de moneda
// - Botón invisible cubre todo el slot
// - Hover: ilumina con brillo + muestra info de pasiva
// - Click: sube nivel
// ============================================================
public class WeaponSlotHUD : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    [Header("─── REFERENCIAS UI ──")]
    public Image imgWeapon;
    public TextMeshProUGUI txtLevel;
    public Image highlightOverlay;     // overlay amarillo para hover
    public Image bgPanel;              // fondo oscuro

    [Header("─── PRECIO ──")]
    public TextMeshProUGUI txtPrice;
    public Image imgCoin;

    [Header("─── TOOLTIP PASIVA ──")]
    public GameObject passiveTooltip;
    public CanvasGroup tooltipCanvasGroup;
    public TextMeshProUGUI txtPassive;

    [Header("─── COLORES ──")]
    public Color maxLevelColor = new Color(1f, 0.84f, 0f);   // dorado
    public Color highlightColor = new Color(1f, 0.86f, 0.2f, 0.35f); // amarillo hover

    int _slotIndex;
    WeaponHUDPanel _panel;
    WeaponDefinition _config;
    Tween _tooltipTween;
    Tween _highlightTween;

    // ─────────────────────────────────────────────────────────────────────
    public void Setup(int slotIndex, WeaponDefinition config, WeaponHUDPanel panel)
    {
        _slotIndex = slotIndex;
        _config = config;
        _panel = panel;

        if (imgWeapon != null && config != null && config.icon != null)
        {
            imgWeapon.sprite = config.icon;
            imgWeapon.color = Color.white;
        }

        // Tooltip oculto al inicio
        if (passiveTooltip != null) passiveTooltip.SetActive(false);
        if (tooltipCanvasGroup != null) tooltipCanvasGroup.alpha = 0f;

        // Highlight oculto al inicio
        if (highlightOverlay != null)
        {
            highlightOverlay.gameObject.SetActive(true);
            var c = highlightColor; c.a = 0f;
            highlightOverlay.color = c;
        }

        Refresh();

        // Animación de entrada
        transform.localScale = Vector3.zero;
        transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack);
    }

    public void Refresh()
    {
        if (_config == null || _panel == null) return;

        int level = _panel.weaponSystem != null ? _panel.weaponSystem.GetWeaponLevel(_slotIndex) : 1;
        bool maxed = _panel.weaponSystem != null && _panel.weaponSystem.IsMaxLevel(_slotIndex);

        if (txtLevel != null)
        {
            txtLevel.text = $"Lv.{level}";
            txtLevel.color = maxed ? maxLevelColor : Color.white;
        }

        if (txtPrice != null)
        {
            if (maxed)
            {
                txtPrice.text = "MAX";
                if (imgCoin != null) imgCoin.gameObject.SetActive(false);
            }
            else
            {
                int cost = _panel.weaponSystem != null ? _panel.weaponSystem.GetUpgradeCost(_slotIndex) : 0;
                txtPrice.text = cost.ToString();
                if (imgCoin != null) imgCoin.gameObject.SetActive(true);
            }
        }

        // Texto de pasiva
        if (txtPassive != null)
            txtPassive.text = GetPassiveDescription(_config);
    }

    string GetPassiveDescription(WeaponDefinition w)
    {
        return w.specialPower switch
        {
            SpecialPower.Bleed => "Sangrado " + w.effectDuration + "s",
            SpecialPower.Burn => "Quemadura " + w.effectDuration + "s",
            SpecialPower.Poison => "Veneno + lentitud",
            SpecialPower.Stun => "Aturde " + w.effectDuration + "s",
            SpecialPower.Slow => "Ralentiza " + w.effectDuration + "s",
            SpecialPower.Knockback => "Empuja al enemigo",
            SpecialPower.Pierce => "Atraviesa enemigos",
            SpecialPower.HighCrit => "25% crítico ×3",
            SpecialPower.LifeStealHit => "Roba 1 HP por golpe",
            SpecialPower.Fragmentation => "Esquirlas al impactar",
            SpecialPower.ChainExplosion => "Explosión en cadena",
            SpecialPower.DoubleCoins => "Doble monedas al matar",
            _ => w.description ?? ""
        };
    }

    // ─────────────────────────────────────────────────────────────────────
    public void OnPointerClick(PointerEventData eventData)
    {
        if (_config == null || _panel == null) return;

        if (_panel.weaponSystem != null && _panel.weaponSystem.IsMaxLevel(_slotIndex))
        {
            transform.DOShakePosition(0.3f, new Vector3(5f, 0f, 0f), 15, 0f);
            return;
        }

        bool ok = _panel.OnUpgradeRequested(_slotIndex);

        if (ok)
        {
            transform.DOPunchScale(Vector3.one * 0.18f, 0.4f, 8, 0.5f);
            if (imgWeapon != null)
            {
                imgWeapon.DOColor(new Color(0.3f, 1f, 0.3f), 0.15f)
                         .OnComplete(() => imgWeapon.DOColor(Color.white, 0.2f));
            }
            Refresh();
        }
        else
        {
            transform.DOShakePosition(0.3f, new Vector3(8f, 0f, 0f), 18, 0f);
            if (imgWeapon != null)
            {
                imgWeapon.DOColor(Color.red, 0.15f)
                         .OnComplete(() => imgWeapon.DOColor(Color.white, 0.2f));
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Iluminar la celda (highlight amarillo)
        _highlightTween?.Kill();
        if (highlightOverlay != null)
            _highlightTween = highlightOverlay.DOFade(highlightColor.a, 0.15f);

        // Mostrar tooltip de pasiva
        if (passiveTooltip != null && tooltipCanvasGroup != null)
        {
            passiveTooltip.SetActive(true);
            _tooltipTween?.Kill();

            tooltipCanvasGroup.alpha = 0f;
            passiveTooltip.transform.localScale = Vector3.one * 0.7f;

            var seq = DOTween.Sequence();
            seq.Append(tooltipCanvasGroup.DOFade(1f, 0.2f));
            seq.Join(passiveTooltip.transform.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
            _tooltipTween = seq;
        }

        // Pequeño zoom en la celda
        transform.DOScale(Vector3.one * 1.06f, 0.15f).SetEase(Ease.OutQuad);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Apagar highlight
        _highlightTween?.Kill();
        if (highlightOverlay != null)
            _highlightTween = highlightOverlay.DOFade(0f, 0.2f);

        // Ocultar tooltip
        if (passiveTooltip != null && tooltipCanvasGroup != null)
        {
            _tooltipTween?.Kill();
            var seq = DOTween.Sequence();
            seq.Append(tooltipCanvasGroup.DOFade(0f, 0.15f));
            seq.OnComplete(() =>
            {
                if (passiveTooltip != null) passiveTooltip.SetActive(false);
            });
            _tooltipTween = seq;
        }

        // Restaurar tamaño
        transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.OutQuad);
    }

    public int SlotIndex => _slotIndex;
    public WeaponDefinition Config => _config;
}