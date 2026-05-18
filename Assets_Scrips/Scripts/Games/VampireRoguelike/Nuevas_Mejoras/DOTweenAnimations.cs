using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

// ============================================================
// DOTween ANIMATIONS
// Colección de animaciones reutilizables para el juego.
// Copia los métodos que necesites o usa esta clase entera.
// ============================================================

// ─────────────────────────────────────────────────────────────
// 1. PANEL SHOP — Entrada y salida suave
// ─────────────────────────────────────────────────────────────
public class ShopPanelAnimator : MonoBehaviour
{
    [Header("Animación del panel")]
    [Tooltip("Duración de la entrada del panel")]
    public float openDuration  = 0.4f;

    [Tooltip("Duración de la salida del panel")]
    public float closeDuration = 0.25f;

    CanvasGroup _cg;
    RectTransform _rt;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _rt = GetComponent<RectTransform>();

        // Si no tiene CanvasGroup, lo añade automáticamente
        if (_cg == null) _cg = gameObject.AddComponent<CanvasGroup>();
    }

    // Llama a esto en lugar de gameObject.SetActive(true)
    public void AnimateOpen()
    {
        gameObject.SetActive(true);

        // Empieza invisible y pequeño
        _cg.alpha = 0f;
        _rt.localScale = Vector3.one * 0.85f;

        // Secuencia: aparece y crece al mismo tiempo
        var seq = DOTween.Sequence();
        seq.Join(_cg.DOFade(1f, openDuration).SetEase(Ease.OutCubic));
        seq.Join(_rt.DOScale(Vector3.one, openDuration).SetEase(Ease.OutBack));
        seq.SetUpdate(true); // ignora Time.timeScale = 0
    }

    // Llama a esto en lugar de gameObject.SetActive(false)
    public void AnimateClose(TweenCallback onComplete = null)
    {
        var seq = DOTween.Sequence();
        seq.Join(_cg.DOFade(0f, closeDuration).SetEase(Ease.InCubic));
        seq.Join(_rt.DOScale(Vector3.one * 0.9f, closeDuration).SetEase(Ease.InCubic));
        seq.SetUpdate(true);
        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }
}

// ─────────────────────────────────────────────────────────────
// 2. CAMBIO DE SUBPANEL — Transición entre Level Up y Tienda
// ─────────────────────────────────────────────────────────────
public class SubpanelTransition : MonoBehaviour
{
    [Header("Duración")]
    public float duration = 0.3f;

    // Llama a esto al pulsar "IR A TIENDA"
    public void SwitchTo(GameObject panelSale, GameObject panelEntra)
    {
        var rtSale  = panelSale.GetComponent<RectTransform>();
        var rtEntra = panelEntra.GetComponent<RectTransform>();

        // Panel que sale: desliza a la izquierda y se oculta
        var seqSale = DOTween.Sequence();
        seqSale.Append(rtSale.DOAnchorPosX(-200f, duration).SetEase(Ease.InCubic).SetRelative());
        seqSale.SetUpdate(true);
        seqSale.OnComplete(() => panelSale.SetActive(false));

        // Panel que entra: viene desde la derecha
        panelEntra.SetActive(true);
        rtEntra.anchoredPosition += new Vector2(200f, 0f);

        var seqEntra = DOTween.Sequence();
        seqEntra.Append(rtEntra.DOAnchorPosX(-200f, duration).SetEase(Ease.OutCubic).SetRelative());
        seqEntra.SetUpdate(true);
    }
}

// ─────────────────────────────────────────────────────────────
// 3. SHOP CARDS — Aparición en cascada al abrir la tienda
// ─────────────────────────────────────────────────────────────
public class ShopCardAnimator : MonoBehaviour
{
    // Llama a esto después de GenerarCards()
    // Pasa el array de las 3 cards y animarán en cascada
    public static void AnimateCards(GameObject[] cards)
    {
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i] == null) continue;

            var rt = cards[i].GetComponent<RectTransform>();
            var cg = cards[i].GetComponent<CanvasGroup>();
            if (cg == null) cg = cards[i].AddComponent<CanvasGroup>();

            // Estado inicial
            rt.localScale = Vector3.one * 0.7f;
            cg.alpha = 0f;

            float delay = i * 0.08f; // cada card 80ms después de la anterior

            var seq = DOTween.Sequence();
            seq.AppendInterval(delay);
            seq.Append(cg.DOFade(1f, 0.25f));
            seq.Join(rt.DOScale(Vector3.one, 0.25f).SetEase(Ease.OutBack));
            seq.SetUpdate(true);
        }
    }

    // Animación al comprar una card: encoge y desaparece
    public static void AnimateBuy(GameObject card, TweenCallback onComplete)
    {
        var rt = card.GetComponent<RectTransform>();
        var cg = card.GetComponent<CanvasGroup>();
        if (cg == null) cg = card.AddComponent<CanvasGroup>();

        var seq = DOTween.Sequence();
        seq.Append(rt.DOScale(1.15f, 0.1f).SetEase(Ease.OutQuad));
        seq.Append(rt.DOScale(0f, 0.2f).SetEase(Ease.InBack));
        seq.Join(cg.DOFade(0f, 0.2f));
        seq.SetUpdate(true);
        seq.OnComplete(onComplete);
    }
}

// ─────────────────────────────────────────────────────────────
// 4. NÚMEROS FLOTANTES DE DAÑO — Sube con rebote
// ─────────────────────────────────────────────────────────────
public class FloatingDamageAnimator : MonoBehaviour
{
    [Header("Animación del número")]
    public float riseDuration  = 0.6f;
    public float fadeDelay     = 0.3f;
    public float totalDuration = 0.9f;

    TextMeshPro _tmp;

    void Awake() => _tmp = GetComponent<TextMeshPro>();

    public void Play(int damage, bool isCrit)
    {
        // Color según crit
        _tmp.color = isCrit
            ? new Color(1f, 0.85f, 0f)   // amarillo crit
            : Color.white;

        // Tamaño según crit
        float scale = isCrit ? 1.4f : 1f;
        transform.localScale = Vector3.zero;

        // Posición aleatoria horizontal
        Vector3 targetPos = transform.position + new Vector3(
            Random.Range(-0.3f, 0.3f), 1.5f, 0f);

        var seq = DOTween.Sequence();

        // Aparece con rebote
        seq.Append(transform.DOScale(scale, 0.15f).SetEase(Ease.OutBack));

        // Sube flotando
        seq.Join(transform.DOMove(targetPos, riseDuration).SetEase(Ease.OutCubic));

        // Se desvanece al final
        seq.AppendInterval(fadeDelay);
        seq.Append(_tmp.DOFade(0f, 0.3f));

        // Se destruye al terminar
        seq.OnComplete(() => Destroy(gameObject));
    }
}

// ─────────────────────────────────────────────────────────────
// 5. BARRA DE HP — Parpadea en rojo al recibir daño
// ─────────────────────────────────────────────────────────────
public class HPBarAnimator : MonoBehaviour
{
    [Header("Referencia")]
    public Image hpBarFill;  // La imagen tipo Filled de la barra

    Color _normalColor = new Color(0.85f, 0.15f, 0.15f); // rojo normal
    Color _hitColor    = Color.white;                     // flash blanco al daño

    // Llama a esto cuando el jugador recibe daño
    public void AnimateDamage(float newFillAmount)
    {
        // Flash blanco instantáneo, luego vuelve al rojo
        hpBarFill.DOColor(_hitColor, 0.05f)
            .SetUpdate(true)
            .OnComplete(() =>
                hpBarFill.DOColor(_normalColor, 0.2f).SetUpdate(true));

        // La barra baja suavemente
        hpBarFill.DOFillAmount(newFillAmount, 0.25f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }

    // Llama a esto al curarse (sube suavemente en verde)
    public void AnimateHeal(float newFillAmount)
    {
        hpBarFill.DOColor(new Color(0.2f, 0.9f, 0.3f), 0.1f)
            .SetUpdate(true)
            .OnComplete(() =>
                hpBarFill.DOColor(_normalColor, 0.4f).SetUpdate(true));

        hpBarFill.DOFillAmount(newFillAmount, 0.4f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
    }
}

// ─────────────────────────────────────────────────────────────
// 6. MONEDAS DEL HUD — El contador salta al sumar monedas
// ─────────────────────────────────────────────────────────────
public class CoinsHUDAnimator : MonoBehaviour
{
    [Header("Texto del contador de monedas")]
    public TextMeshProUGUI txtCoins;

    int _displayedCoins = 0; // valor que se ve en pantalla

    // Llama a esto cuando cambian las monedas reales
    public void AnimateTo(int targetCoins)
    {
        // El número "rueda" hacia el nuevo valor en 0.5 segundos
        DOTween.To(
            () => _displayedCoins,
            val =>
            {
                _displayedCoins  = val;
                txtCoins.text = $"💰 {val}";
            },
            targetCoins,
            0.5f
        ).SetEase(Ease.OutCubic).SetUpdate(true);

        // La escala del texto da un pequeño bote
        txtCoins.transform
            .DOScale(1.3f, 0.1f)
            .SetUpdate(true)
            .OnComplete(() =>
                txtCoins.transform.DOScale(1f, 0.15f).SetUpdate(true));
    }
}

// ─────────────────────────────────────────────────────────────
// 7. OLEADA — Texto de inicio de oleada en el centro
// ─────────────────────────────────────────────────────────────
public class WaveStartBanner : MonoBehaviour
{
    [Header("Texto del anuncio de oleada")]
    public TextMeshProUGUI txtBanner;  // "OLEADA 3"

    [Tooltip("Cuánto tiempo se queda visible el texto")]
    public float stayDuration = 1.2f;

    // Llama a esto al comenzar cada oleada
    public void Show(int waveNumber)
    {
        if (txtBanner == null) return;

        txtBanner.text = $"OLEADA {waveNumber}";
        txtBanner.alpha = 0f;
        txtBanner.transform.localScale = Vector3.one * 0.5f;

        gameObject.SetActive(true);

        var seq = DOTween.Sequence();

        // Entra: crece y aparece
        seq.Append(txtBanner.DOFade(1f, 0.3f).SetEase(Ease.OutCubic));
        seq.Join(txtBanner.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack));

        // Se queda un momento
        seq.AppendInterval(stayDuration);

        // Sale: encoge y desaparece
        seq.Append(txtBanner.DOFade(0f, 0.3f).SetEase(Ease.InCubic));
        seq.Join(txtBanner.transform.DOScale(0.8f, 0.3f).SetEase(Ease.InCubic));

        seq.OnComplete(() => gameObject.SetActive(false));
    }
}

// ─────────────────────────────────────────────────────────────
// 8. ENEMIGOS — Sacudida (shake) al recibir golpe melee
// ─────────────────────────────────────────────────────────────
public class EnemyHitShake : MonoBehaviour
{
    // Llama a esto desde EnemyRogueLite.TakeDamage()
    // en vez de (o además de) el parpadeo rojo
    public void Shake(float strength = 0.15f, float duration = 0.2f)
    {
        transform.DOShakePosition(duration, strength, vibrato: 10, randomness: 90)
            .SetUpdate(false); // afectado por TimeScale normal
    }

    // Versión para golpe crítico: más fuerte
    public void ShakeCrit()
    {
        Shake(strength: 0.3f, duration: 0.25f);
    }
}
