using UnityEngine;

// ============================================================
// GADGET CONTROLLER
// Este script va en el prefab de cada gadget (ej: motosierra).
// El gadget orbita alrededor del jugador y daña a los enemigos
// al tocarlos. Funciona al mismo tiempo que las armas normales.
// ============================================================
public class GadgetController : MonoBehaviour
{
    [Header("Órbita")]
    public float orbitRadius = 2f;   // Distancia al jugador
    public float orbitSpeed = 90f;  // Grados por segundo
    public int damage = 1;    // Daño al tocar un enemigo

    // Cooldown para no dañar al mismo enemigo 60 veces por segundo
    float damageCooldown = 0.5f;

    // Ángulo actual en la órbita
    float currentAngle = 0f;

    // Referencia al jugador (se asigna al spawnear)
    Transform playerTransform;

    // Inicializa el gadget con una referencia al jugador
    public void Initialize(Transform player, float startAngle = 0f)
    {
        playerTransform = player;
        currentAngle = startAngle;
    }

    void Update()
    {
        if (playerTransform == null) return;
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused) return;

        // Avanza el ángulo de órbita
        currentAngle += orbitSpeed * Time.deltaTime;

        // Calcula la posición en círculo alrededor del jugador
        float rad = currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
        transform.position = playerTransform.position + offset;
    }

    // Daña al enemigo al tocarlo
    void OnTriggerStay2D(Collider2D col)
    {
        if (!col.CompareTag("Enemy")) return;
        var enemy = col.GetComponent<EnemyRogueLite>();
        enemy?.TakeDamage(damage);
    }
}
