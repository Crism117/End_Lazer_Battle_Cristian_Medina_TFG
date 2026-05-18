using UnityEngine;
using DG.Tweening;

// ============================================================
// COIN PICKUP (v9)
// + StartBlinking() para limpieza al fin de oleada
// + Sigue siendo recogible mientras parpadea
// ============================================================
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
public class CoinPickup : MonoBehaviour
{
    [Header("─── VALOR ──")]
    public int amount = 1;

    [Header("─── ANIMACIÓN POP ──")]
    public float popHeight = 0.5f;
    public float popDuration = 0.3f;

    [Header("─── IMÁN (solo con item Imán) ──")]
    public float magnetRange = 3.5f;
    public float magnetSpeed = 8f;

    [Header("─── AUTO-DESPAWN ──")]
    public float despawnDistance = 25f;
    public float despawnCheckInterval = 2f;

    [Header("─── PARPADEO (limpieza fin oleada) ──")]
    public float blinkSpeed = 8f;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    Transform _player;
    float _nextDespawnCheck;
    bool _collected = false;
    bool _isBlinking = false;
    SpriteRenderer _sr;
    Tween _blinkTween;

    void Awake()
    {
        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.bodyType = RigidbodyType2D.Dynamic;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.4f;

        _sr = GetComponent<SpriteRenderer>();
        if (_sr == null) _sr = GetComponentInChildren<SpriteRenderer>();
    }

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;
        else _player = GameManagerRogueLite.Instance?.playerTransform;

        _nextDespawnCheck = Time.time + despawnCheckInterval;
    }

    void Update()
    {
        if (_collected) return;
        if (_player == null) return;
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused) return;

        float dist = Vector2.Distance(transform.position, _player.position);

        if (Time.time >= _nextDespawnCheck)
        {
            _nextDespawnCheck = Time.time + despawnCheckInterval;
            if (dist > despawnDistance) { Destroy(gameObject); return; }
        }

        if (HasMagnetItem() && dist < magnetRange && dist > 0.5f)
        {
            Vector2 dir = ((Vector2)_player.position - (Vector2)transform.position).normalized;
            transform.position += (Vector3)(dir * magnetSpeed * Time.deltaTime);
        }
    }

    bool HasMagnetItem()
    {
        return ItemSystem.Instance != null && ItemSystem.Instance.HasMagnet;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        StartCollectAnimation();
    }

    /// <summary>
    /// NUEVO v9: empieza el parpadeo y la moneda se destruye al cabo de duration.
    /// El jugador puede recogerla mientras parpadea.
    /// </summary>
    public void StartBlinking(float duration)
    {
        if (_isBlinking || _collected) return;
        _isBlinking = true;

        if (_sr != null)
        {
            _blinkTween = _sr.DOFade(0.2f, 1f / blinkSpeed)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }

        DOVirtual.DelayedCall(duration, () =>
        {
            if (this != null && !_collected) DespawnAnimated();
        });
    }

    void DespawnAnimated()
    {
        _collected = true;
        _blinkTween?.Kill();

        var seq = DOTween.Sequence();
        if (_sr != null) seq.Append(_sr.DOFade(0f, 0.3f));
        seq.Join(transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

    void StartCollectAnimation()
    {
        if (_collected) return;
        _collected = true;
        _blinkTween?.Kill();

        var col = GetComponent<CircleCollider2D>();
        if (col != null) col.enabled = false;
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null) { rb.velocity = Vector2.zero; rb.bodyType = RigidbodyType2D.Kinematic; }

        int finalAmount = amount;
        if (ItemSystem.Instance != null && ItemSystem.Instance.HasDoubleCoins)
            finalAmount *= 2;
        CoinSystem.Instance?.AddCoins(finalAmount);

        // ── SONIDO ──
        SoundManager.Instance?.PlayCoinPickup();

        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * popHeight;

        var seq = DOTween.Sequence();
        seq.Append(transform.DOMove(peakPos, popDuration * 0.7f).SetEase(Ease.OutQuad));
        if (_sr != null)
        {
            Color c = _sr.color; c.a = 1f; _sr.color = c;
            seq.Join(_sr.DOFade(0f, popDuration).SetEase(Ease.InQuad));
        }
        seq.Append(transform.DOScale(Vector3.zero, popDuration * 0.3f).SetEase(Ease.InQuad));
        seq.OnComplete(() => Destroy(gameObject));
    }

    void OnDisable()
    {
        _blinkTween?.Kill();
    }
}

public static class CoinSpawner
{
    public static GameObject coinPrefab;

    public static void SpawnCoins(Vector3 pos, int total)
    {
        if (coinPrefab == null || total <= 0) return;
        for (int i = 0; i < total; i++)
        {
            Vector3 offset = new Vector3(
                UnityEngine.Random.Range(-0.5f, 0.5f),
                UnityEngine.Random.Range(-0.5f, 0.5f), 0f);
            var go = Object.Instantiate(coinPrefab, pos + offset, Quaternion.identity);
            var cp = go.GetComponent<CoinPickup>();
            if (cp != null) cp.amount = 1;
        }
    }
}