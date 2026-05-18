using UnityEngine;
using DG.Tweening;

// ============================================================
// MAP ITEM PICKUP (v8)
// FIX: usa SoundManager.PlayItemPickup() en vez de PlayUIClick()
// (antes los items reproducían sonidos cruzados con armas).
// ============================================================
public enum MapItemType
{
    Shield, Collar, Lifesteal, Wolf, Boots, Magnet
}

[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class MapItemPickup : MonoBehaviour
{
    [Header("─── TIPO ──")]
    public MapItemType itemType = MapItemType.Shield;

    [Header("─── DURACIÓN EN MAPA ──")]
    public float despawnTime = 10f;

    [Header("─── DURACIÓN DEL EFECTO ──")]
    public float effectActiveDuration = 6f;

    [Header("─── ANIMACIÓN SPAWN ──")]
    public float blinkDuration = 0.6f;
    public int blinkCount = 4;

    [Header("─── ANIMACIÓN FLOTACIÓN ──")]
    public float floatHeight = 0.15f;
    public float floatPeriod = 1.2f;

    [Header("─── ICONO ──")]
    public Sprite itemIcon;

    SpriteRenderer _sr;
    bool _collected = false;
    Tween _floatTween, _blinkTween;
    Vector3 _basePos;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.freezeRotation = true;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        _basePos = transform.position;
        StartBlinkSpawn();
        DOVirtual.DelayedCall(blinkDuration, StartFloat);

        // ── SONIDO DE SPAWN DEL ITEM ──
        SoundManager.Instance?.PlayItemSpawn();

        if (despawnTime > 0 && itemType != MapItemType.Shield)
        {
            DOVirtual.DelayedCall(despawnTime, () =>
            {
                if (!_collected) DespawnAnimated();
            });
        }

        if (itemType != MapItemType.Shield && WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveEnded += OnWaveEnded;
    }

    void OnDestroy()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveEnded -= OnWaveEnded;
    }

    void OnWaveEnded(WaveData _)
    {
        if (!_collected) DespawnAnimated();
    }

    void StartBlinkSpawn()
    {
        if (_sr == null) return;
        _blinkTween = _sr.DOFade(1f, blinkDuration / (blinkCount * 2f))
            .From(0f)
            .SetLoops(blinkCount * 2, LoopType.Yoyo)
            .OnComplete(() => { if (_sr != null) _sr.color = new Color(_sr.color.r, _sr.color.g, _sr.color.b, 1f); });
    }

    void StartFloat()
    {
        if (transform == null) return;
        _floatTween = transform.DOMoveY(_basePos.y + floatHeight, floatPeriod * 0.5f)
            .SetEase(Ease.InOutSine)
            .SetLoops(-1, LoopType.Yoyo);
    }

    void DespawnAnimated()
    {
        _collected = true;
        _floatTween?.Kill();
        _blinkTween?.Kill();

        var seq = DOTween.Sequence();
        if (_sr != null) seq.Append(_sr.DOFade(0f, 0.4f));
        seq.Join(transform.DOScale(Vector3.zero, 0.4f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_collected) return;
        if (!other.CompareTag("Player")) return;
        Collect();
    }

    void Collect()
    {
        _collected = true;
        _floatTween?.Kill();
        _blinkTween?.Kill();

        ApplyEffect();

        if (itemType != MapItemType.Shield)
        {
            var iconMgr = ActiveEffectIconManager.Instance;
            Sprite iconToShow = itemIcon != null ? itemIcon : (_sr != null ? _sr.sprite : null);
            iconMgr?.ShowEffect(iconToShow, effectActiveDuration);
        }

        // ── SONIDO ESPECÍFICO DE PICKUP DE ITEM (no UI click) ──
        SoundManager.Instance?.PlayItemPickup();

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMoveY(transform.position.y + 0.6f, 0.3f).SetEase(Ease.OutQuad));
        if (_sr != null) seq.Join(_sr.DOFade(0f, 0.3f));
        seq.Join(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

    void ApplyEffect()
    {
        var ctrl = ItemEffectController.Instance;

        switch (itemType)
        {
            case MapItemType.Shield:
                PlayerShield.Instance?.AddShield();
                break;
            case MapItemType.Collar:
                ctrl?.ActivateCollar(effectActiveDuration);
                break;
            case MapItemType.Lifesteal:
                ctrl?.ActivateLifesteal(effectActiveDuration);
                break;
            case MapItemType.Wolf:
                ctrl?.ActivateWolf(effectActiveDuration);
                break;
            case MapItemType.Boots:
                ctrl?.ActivateBoots(effectActiveDuration);
                break;
            case MapItemType.Magnet:
                ctrl?.ActivateMagnet(effectActiveDuration);
                break;
        }
    }
}