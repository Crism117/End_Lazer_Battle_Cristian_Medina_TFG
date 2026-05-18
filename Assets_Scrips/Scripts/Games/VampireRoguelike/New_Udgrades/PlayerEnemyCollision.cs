using UnityEngine;
using System.Collections;

// ============================================================
// PLAYER ENEMY COLLISION (v2)
// El jugador empuja a los enemigos al caminar hacia ellos.
// Soluciona el problema de ser aplastado contra la pared
// cuando hay muchos enemigos agrupados.
//
// LÓGICA:
// - El empuje va desde el jugador hacia el enemigo (no por input)
// - Cuanto más rápido va el jugador, más fuerte empuja
// - Si el jugador está quieto, el empuje es mínimo (no nulo)
// - El stun ya NO inmoviliza al jugador (causaba el problema)
// ============================================================
public class PlayerEnemyCollision : MonoBehaviour
{
    [Header("─── EMPUJE AL ENEMIGO ──")]
    [Tooltip("Fuerza base del empuje al enemigo")]
    public float pushForce = 5f;

    [Tooltip("Multiplicador extra cuando el jugador se mueve hacia el enemigo")]
    public float movingPushMultiplier = 1.8f;

    [Tooltip("Empuje mínimo garantizado aunque el jugador esté quieto")]
    public float minPushForce = 2f;

    [Tooltip("Cooldown entre empujes al mismo enemigo")]
    public float pushCooldown = 0.12f;

    [Header("─── SEPARACIÓN SUAVE ──")]
    [Tooltip("Fuerza de separación pasiva cuando hay muchos enemigos encima")]
    public float separationForce = 3f;

    [Tooltip("Radio de separación pasiva alrededor del jugador")]
    public float separationRadius = 0.8f;

    [Header("─── PROTECCIÓN PARED ──")]
    [Tooltip("Si está activo, aumenta el empuje cuando el jugador está cerca de una pared")]
    public bool wallProtection = true;

    [Tooltip("Distancia a la pared para activar el empuje extra")]
    public float wallDetectDistance = 0.6f;

    [Tooltip("Multiplicador del empuje cuando hay una pared cerca")]
    public float wallPushMultiplier = 2.5f;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    // ─────────────────────────────────────────────────────────
    Rigidbody2D _rb;
    PlayerControllerRogueLite _player;
    float _lastPushTime;

    // Para el IsStunned que usa PlayerControllerRogueLite
    public bool IsStunned => false; // Ya no stunneamos al jugador

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _player = GetComponent<PlayerControllerRogueLite>();
    }

    void FixedUpdate()
    {
        if (GameManagerRogueLite.Instance != null &&
            GameManagerRogueLite.Instance.IsPaused) return;

        // Separación pasiva: empuja suavemente a todos los enemigos
        // que estén muy pegados al jugador (radio pequeño)
        AplicarSeparacionPasiva();
    }

    // ── Empuje activo al colisionar ───────────────────────────
    void OnCollisionStay2D(Collision2D c)
    {
        if (!c.collider.CompareTag("Enemy")) return;
        if (Time.time < _lastPushTime + pushCooldown) return;

        var enemyRb = c.collider.GetComponent<Rigidbody2D>();
        if (enemyRb == null) return;

        // Dirección: desde el jugador hacia el enemigo
        Vector2 dir = ((Vector2)c.collider.transform.position -
                       (Vector2)transform.position).normalized;

        // Si la dirección es cero (superposición exacta), usar dirección aleatoria
        if (dir.sqrMagnitude < 0.01f)
            dir = Random.insideUnitCircle.normalized;

        // Calcular fuerza según velocidad del jugador
        float playerSpeed = _rb != null ? _rb.velocity.magnitude : 0f;
        float force = Mathf.Max(minPushForce,
                                pushForce + playerSpeed * movingPushMultiplier);

        // Bonus si hay una pared cerca (protección anti-aplastamiento)
        if (wallProtection && HayParedCerca())
        {
            force *= wallPushMultiplier;
            if (debugMode) Debug.Log("[Collision] Pared detectada — empuje extra");
        }

        // Aplicar empuje al enemigo
        enemyRb.AddForce(dir * force, ForceMode2D.Impulse);
        _lastPushTime = Time.time;

        if (debugMode)
            Debug.Log($"[Collision] Empuje a {c.collider.name} | Fuerza: {force:F1}");
    }

    // ── Separación pasiva (radio pequeño, fuerza suave) ───────
    // Evita que los enemigos se apilen encima del jugador
    void AplicarSeparacionPasiva()
    {
        var hits = Physics2D.OverlapCircleAll(
            transform.position, separationRadius);

        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;

            var enemyRb = hit.GetComponent<Rigidbody2D>();
            if (enemyRb == null) continue;

            Vector2 dir = ((Vector2)hit.transform.position -
                           (Vector2)transform.position).normalized;

            if (dir.sqrMagnitude < 0.01f)
                dir = Random.insideUnitCircle.normalized;

            // Fuerza suave continua — no impulso
            enemyRb.AddForce(dir * separationForce * Time.fixedDeltaTime,
                             ForceMode2D.Force);
        }
    }

    // ── Detecta si hay una pared cerca del jugador ────────────
    bool HayParedCerca()
    {
        // Lanza 4 rayos en cruz (arriba, abajo, izquierda, derecha)
        Vector2[] dirs = {
            Vector2.up, Vector2.down,
            Vector2.left, Vector2.right
        };

        foreach (var dir in dirs)
        {
            var hit = Physics2D.Raycast(
                transform.position, dir,
                wallDetectDistance,
                LayerMask.GetMask("Wall", "Default"));

            if (hit.collider != null &&
                !hit.collider.CompareTag("Enemy") &&
                !hit.collider.CompareTag("Player"))
            {
                return true;
            }
        }
        return false;
    }

    // Gizmos para visualizar los radios en el editor
    void OnDrawGizmosSelected()
    {
        // Radio de separación pasiva (amarillo)
        Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, separationRadius);

        // Radio de detección de paredes (rojo)
        if (wallProtection)
        {
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, wallDetectDistance);
        }
    }
}