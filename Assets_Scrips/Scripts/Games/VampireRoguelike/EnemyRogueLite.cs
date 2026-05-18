using UnityEngine;
using System.Collections;

// ============================================================
// ENEMY ROGUE LITE
// Persigue al jugador. Spawnea con parpadeo, parpadea rojo al recibir
// daño, suelta monedas físicas al morir. La cantidad de monedas
// escala con la oleada para evitar lag en oleadas tempranas.
//
// AÑADIDOS:
// - Escala base por grupo (Slime 2.6, Araña 2.7, Gárgola 3.0)
// - Variación aleatoria de tamaño por instancia (±scaleVariation)
// - Inmunidad durante el spawn via _isInvulnerable (sin cambios)
// ============================================================
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyRogueLite : MonoBehaviour
{
    public enum EnemyGroup { Gargola, Arana, Slime }

    [Header("Tipo de enemigo")]
    public EnemyGroup enemyGroup = EnemyGroup.Slime;

    [Header("Stats base")]
    public int maxHealth = 3;
    public float moveSpeed = 1.5f;
    public int contactDamage = 1;
    public int xpValue = 1;

    [Header("Spawn")]
    public float spawnInDuration = 1.0f;

    [Header("─── ESCALAS POR GRUPO ───────────────")]
    [Tooltip("Escala base del Slime")]
    public float scaleSlime = 2.6f;

    [Tooltip("Escala base de la Araña")]
    public float scaleArana = 2.7f;

    [Tooltip("Escala base de la Gárgola")]
    public float scaleGargola = 3.0f;

    [Tooltip("Variación aleatoria del tamaño por instancia (±este valor).\n" +
             "0.2 = un enemigo puede salir entre ×0.8 y ×1.2 de su escala base.\n" +
             "Cada spawn tendrá un tamaño ligeramente distinto.")]
    [Range(0f, 0.5f)]
    public float scaleVariation = 0.2f;

    // ─────────────────────────────────────────────────────────────────────
    public EnemyType CurrentType { get; private set; } = EnemyType.Basic;

    int _coinDropForThisEnemy = 1;

    int currentHealth;
    Transform target;
    Rigidbody2D rb;
    SpriteRenderer sr;
    ObjectPoolRogueLite pool;
    Collider2D col;

    float contactCooldown = 0.5f;
    float lastContactTime;

    Color _originalColor;
    bool _isSpawning = true;
    bool _isInvulnerable = true;

    // ─────────────────────────────────────────────────────────────────────

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    void OnEnable()
    {
        currentHealth = maxHealth;
        lastContactTime = -contactCooldown;
        target = GameManagerRogueLite.Instance?.playerTransform;

        ApplyRandomScale();
        StartCoroutine(SpawnInRoutine());
    }

    // Escala base del grupo con variación aleatoria leve
    void ApplyRandomScale()
    {
        float baseScale = enemyGroup switch
        {
            EnemyGroup.Slime => scaleSlime,
            EnemyGroup.Arana => scaleArana,
            EnemyGroup.Gargola => scaleGargola,
            _ => 1f,
        };

        // Multiplica por un valor entre (1 - variation) y (1 + variation)
        float mult = 1f + Random.Range(-scaleVariation, scaleVariation);
        transform.localScale = Vector3.one * (baseScale * mult);
    }

    void FixedUpdate()
    {
        if (GameManagerRogueLite.Instance != null &&
            GameManagerRogueLite.Instance.IsPaused)
        { rb.velocity = Vector2.zero; return; }

        if (_isSpawning) { rb.velocity = Vector2.zero; return; }

        if (target == null)
        {
            target = GameManagerRogueLite.Instance?.playerTransform;
            if (target == null) return;
        }

        Vector2 dir = ((Vector2)target.position -
                       (Vector2)transform.position).normalized;
        rb.velocity = dir * moveSpeed;
    }

    public void Initialize(ObjectPoolRogueLite returnPool, EnemyType type,
                           int hp, float speed, float scoreMultiplier)
    {
        pool = returnPool;
        CurrentType = type;

        var (finalHP, finalSpeed, finalDmg) =
            WaveSystemRogueLite.GetTypeStats(type, hp, speed);

        maxHealth = finalHP;
        currentHealth = finalHP;
        moveSpeed = finalSpeed;
        contactDamage = finalDmg;

        _coinDropForThisEnemy = CalculateCoinDrop();

        target = GameManagerRogueLite.Instance?.playerTransform;

        // Nueva escala aleatoria cada vez que se reutiliza del pool
        ApplyRandomScale();

        if (sr != null)
        {
            _originalColor = GetPhaseColor(enemyGroup);
            sr.color = _originalColor;
        }
    }

    int CalculateCoinDrop()
    {
        int wave = WaveSystemRogueLite.Instance?.ActiveWave?.waveNumber ?? 1;
        if (wave <= 3) return 1;
        if (wave <= 6) return Random.Range(5, 7);
        return Mathf.Min(10, wave);
    }

    Color GetPhaseColor(EnemyGroup group)
    {
        var phase = WaveSystemRogueLite.Instance?.ActiveWave?.phase
                    ?? GamePhase.Easy;
        return (group, phase) switch
        {
            (EnemyGroup.Slime, GamePhase.Easy) => new Color(0.5f, 1f, 0.5f),
            (EnemyGroup.Slime, GamePhase.Medium) => new Color(0.2f, 0.9f, 0.4f),
            (EnemyGroup.Slime, GamePhase.Aggressive) => new Color(0.0f, 0.7f, 0.2f),
            (EnemyGroup.Arana, GamePhase.Easy) => new Color(0.9f, 0.9f, 0.9f),
            (EnemyGroup.Arana, GamePhase.Medium) => new Color(0.8f, 0.7f, 0.5f),
            (EnemyGroup.Arana, GamePhase.Aggressive) => new Color(0.7f, 0.5f, 0.3f),
            (EnemyGroup.Gargola, GamePhase.Easy) => new Color(0.7f, 0.7f, 0.85f),
            (EnemyGroup.Gargola, GamePhase.Medium) => new Color(0.5f, 0.5f, 0.7f),
            (EnemyGroup.Gargola, GamePhase.Aggressive) => new Color(0.3f, 0.3f, 0.5f),
            _ => Color.white,
        };
    }

    IEnumerator SpawnInRoutine()
    {
        _isSpawning = true;
        _isInvulnerable = true;
        if (col != null) col.enabled = false;

        float elapsed = 0f;
        while (elapsed < spawnInDuration)
        {
            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.PingPong(elapsed * 6f, 0.7f) + 0.3f;
                sr.color = c;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (sr != null)
        {
            Color c = _originalColor; c.a = 1f;
            sr.color = c;
        }
        if (col != null) col.enabled = true;

        _isSpawning = false;
        _isInvulnerable = false;
    }

    public void TakeDamage(int dmg)
    {
        if (_isInvulnerable) return;

        bool isCrit = false;
        var player = GameManagerRogueLite.Instance?.playerTransform
                        ?.GetComponent<PlayerControllerRogueLite>();

        if (player != null && Random.value < player.critChance)
        {
            dmg *= 2;
            isCrit = true;
        }

        currentHealth -= dmg;
        FloatingDamageManager.Instance?.Show(transform.position, dmg, isCrit);

        // ── SONIDO de daño del enemigo ──
        var soundCfg = GetComponent<EnemySoundConfig>();
        soundCfg?.PlayHurt();

        // ── SONIDO de hit (genérico, agudo) ──
        SoundManager.Instance?.PlayHit();

        if (player != null && player.lifeSteal > 0f)
        {
            int healAmount = Mathf.Max(1,
                Mathf.RoundToInt(dmg * player.lifeSteal));
            player.Heal(healAmount);
        }

        if (currentHealth <= 0) Die();
        else StartCoroutine(HitFlashRoutine());
    }

    IEnumerator HitFlashRoutine()
    {
        if (sr == null) yield break;
        for (int i = 0; i < 2; i++)
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = _originalColor;
            yield return new WaitForSeconds(0.05f);
        }
    }

    void Die()
    {
        rb.velocity = Vector2.zero;

        int xp = xpValue;
        if (ItemSystem.Instance != null && ItemSystem.Instance.XPBonusStacks > 0)
        {
            float bonus = 1f + 0.25f * ItemSystem.Instance.XPBonusStacks;
            xp = Mathf.RoundToInt(xp * bonus);
        }

        GameManagerRogueLite.Instance?.AddXP(xp);
        GameManagerRogueLite.Instance?.AddKill();
        CoinSpawner.SpawnCoins(transform.position, _coinDropForThisEnemy);

        gameObject.SetActive(false);
        pool?.Return(gameObject);
    }

    // Daño por contacto — sistema original con OnCollisionStay2D
    void OnCollisionStay2D(Collision2D c)
    {
        if (_isInvulnerable) return;
        if (!c.collider.CompareTag("Player")) return;
        if (Time.time < lastContactTime + contactCooldown) return;
        lastContactTime = Time.time;
        c.collider.GetComponent<PlayerControllerRogueLite>()
                  ?.TakeDamage(contactDamage);
    }

    // ─────────────────────────────────────────────────────────────────────
    // DESPAWN ANIMADO (entre oleadas)
    // ─────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Llamado por EnemySpawnerRogueLite.DespawnAllEnemiesAnimated().
    /// El enemigo parpadea rojo N veces y luego se devuelve al pool.
    /// </summary>
    public void BeginDespawn(float duration = 2.5f, int blinkCount = 3)
    {
        _isInvulnerable = true;

        // Detener movimiento
        if (rb != null) rb.velocity = Vector2.zero;

        StartCoroutine(DespawnRoutine(duration, blinkCount));
    }

    IEnumerator DespawnRoutine(float duration, int blinkCount)
    {
        float blinkInterval = duration / (blinkCount * 2f);

        for (int i = 0; i < blinkCount; i++)
        {
            if (sr != null) sr.color = Color.red;
            yield return new WaitForSeconds(blinkInterval);
            if (sr != null) sr.color = _originalColor;
            yield return new WaitForSeconds(blinkInterval);
        }

        // Desvanecer hasta invisible
        if (sr != null)
        {
            float elapsed = 0f;
            float fadeDuration = 0.4f;
            Color c = sr.color;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                c.a = 1f - (elapsed / fadeDuration);
                if (sr != null) sr.color = c;
                yield return null;
            }
        }

        // Devolver al pool
        gameObject.SetActive(false);
        pool?.Return(gameObject);
    }
}