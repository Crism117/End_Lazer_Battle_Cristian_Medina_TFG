using UnityEngine;

// ============================================================
// PROJECTILE BASE
// Proyectil f�sico que se mueve y aplica da�o + poder especial.
// Compatible con el nuevo WeaponConfig.
// ============================================================
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class ProjectileBase : MonoBehaviour
{
    [Header("Config en runtime (se asigna al spawnear)")]
    public int damage = 1;
    public float speed = 7f;
    public float maxLifeTime = 5f;

    // Poder especial que aplica al impactar
    public SpecialPower specialPower = SpecialPower.None;
    public int dotDamage = 1;
    public float dotDuration = 2f;
    public float knockbackForce = 0f;

    // Pool de retorno
    [HideInInspector] public ObjectPoolRogueLite pool;

    // ── PROBABILIDAD DE PROC ─────────────────────────────────────────────
    // Si > 0, la pasiva solo se aplicará con esa probabilidad al impactar.
    // Si == 0 (legacy), se aplica siempre (comportamiento antiguo).
    [HideInInspector] public float procChance = 0f;

    Rigidbody2D _rb;
    SpriteRenderer _sr;
    float _born;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();

        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnEnable()
    {
        _born = Time.time;
    }

    // Llamado por el WeaponSystem al spawnear este proyectil (legacy)
    public void Launch(Vector3 direction, int dmg, float spd,
                       SpecialPower power = SpecialPower.None,
                       int dot = 1, float dotDur = 2f, float kb = 0f,
                       ObjectPoolRogueLite returnPool = null)
    {
        damage = dmg;
        speed = spd;
        specialPower = power;
        dotDamage = dot;
        dotDuration = dotDur;
        knockbackForce = kb;
        pool = returnPool;
        procChance = 0f; // legacy = siempre proca

        _rb.velocity = direction.normalized * speed;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    /// <summary>
    /// Versión nueva con probabilidad de proc.
    /// Llamada desde WeaponSystemV2 para que la pasiva no se aplique siempre.
    /// </summary>
    public void LaunchWithProc(Vector3 direction, int dmg, float spd,
                               SpecialPower power, int dot, float dotDur, float kb,
                               ObjectPoolRogueLite returnPool, float chance)
    {
        Launch(direction, dmg, spd, power, dot, dotDur, kb, returnPool);
        procChance = chance;
    }

    void Update()
    {
        // Auto-destrucci�n por tiempo de vida
        if (Time.time - _born >= maxLifeTime)
            ReturnToPool();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyRogueLite>();
        if (enemy == null) return;

        // Aplica daño base (siempre)
        enemy.TakeDamage(damage);

        // Aplica poder especial — con probabilidad si procChance > 0
        bool shouldProc = procChance <= 0f || Random.value < procChance;
        if (shouldProc)
            ApplySpecialPower(enemy, other.gameObject);

        ReturnToPool();
    }

    void ApplySpecialPower(EnemyRogueLite enemy, GameObject enemyGO)
    {
        switch (specialPower)
        {
            case SpecialPower.Bleed:
                BleedEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(dotDuration * 2));
                break;

            case SpecialPower.Burn:
                BurnEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(dotDuration * 2));
                break;

            case SpecialPower.Stun:
                StartCoroutine(StunEnemy(enemy, dotDuration));
                break;

            case SpecialPower.Poison:
                PoisonEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(dotDuration * 2), 0.5f);
                break;

            case SpecialPower.Slow:
                StartCoroutine(SlowEnemy(enemy, dotDuration));
                break;

            case SpecialPower.Knockback:
                var rb = enemyGO.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    Vector2 dir = ((Vector2)enemyGO.transform.position -
                                   (Vector2)transform.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                break;

            case SpecialPower.LifeStealHit:
                // Gestionado en EnemyRogueLite.TakeDamage
                break;

            case SpecialPower.Fragmentation:
                SpawnFragments(enemyGO.transform.position);
                break;

            case SpecialPower.ChainExplosion:
                BurnEffect.Apply(enemyGO, dotDamage,
                                 Mathf.RoundToInt(dotDuration * 2));
                var nearby = Physics2D.OverlapCircleAll(
                    enemyGO.transform.position, 2.5f);
                foreach (var hit in nearby)
                    if (hit.CompareTag("Enemy") && hit.gameObject != enemyGO)
                        hit.GetComponent<EnemyRogueLite>()?.TakeDamage(damage / 2);
                break;
        }
    }

    System.Collections.IEnumerator SlowEnemy(EnemyRogueLite enemy, float dur)
    {
        float original = enemy.moveSpeed;
        enemy.moveSpeed *= 0.4f;
        yield return new WaitForSeconds(dur);
        if (enemy != null) enemy.moveSpeed = original;
    }

    System.Collections.IEnumerator StunEnemy(EnemyRogueLite enemy, float dur)
    {
        float original = enemy.moveSpeed;
        enemy.moveSpeed = 0f;
        yield return new WaitForSeconds(dur);
        if (enemy != null) enemy.moveSpeed = original;
    }

    void SpawnFragments(Vector3 pos)
    {
        // 4 esquirlas en diagonal
        float[] angles = { 45f, 135f, 225f, 315f };
        foreach (float a in angles)
        {
            Vector2 dir = new Vector2(
                Mathf.Cos(a * Mathf.Deg2Rad),
                Mathf.Sin(a * Mathf.Deg2Rad));
            // Spawn simple: clona este proyectil en miniatura
            var frag = Instantiate(gameObject, pos, Quaternion.identity);
            frag.transform.localScale = Vector3.one * 0.5f;
            var pb = frag.GetComponent<ProjectileBase>();
            if (pb != null)
            {
                pb.specialPower = SpecialPower.None;
                pb.Launch(dir, damage / 2, speed * 1.5f);
            }
        }
    }

    void ReturnToPool()
    {
        _rb.velocity = Vector2.zero;
        if (pool != null)
        {
            gameObject.SetActive(false);
            pool.Return(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
