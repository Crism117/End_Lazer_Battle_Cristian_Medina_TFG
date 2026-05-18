using UnityEngine;

// ============================================================
// MAP BOUNDS
// Define el área segura donde pueden spawnear los enemigos.
// Los enemigos nunca aparecerán fuera de los muros.
//
// USO:
// 1. Crea un GameObject vacío llamado "SpawnArea"
// 2. Añade BoxCollider2D (Is Trigger = true)
// 3. Ajusta el tamaño para que quepa DENTRO de los muros
//    (un poco más pequeño que el área jugable)
// 4. Añade este script
// 5. El EnemySpawner lo busca automáticamente
// ============================================================
[RequireComponent(typeof(BoxCollider2D))]
public class MapBounds : MonoBehaviour
{
    public static MapBounds Instance { get; private set; }

    [Header("Capa de muros (para evitar spawn dentro)")]
    [Tooltip("Selecciona la layer 'Wall' u 'Obstacle' que tengan los arbustos")]
    public LayerMask wallLayer;

    [Tooltip("Margen interior para no spawnear pegado a los muros")]
    public float spawnPadding = 0.5f;

    [Tooltip("Cuántas veces intenta encontrar una posición válida antes de rendirse")]
    public int maxRetries = 20;

    BoxCollider2D _col;

    void Awake()
    {
        Instance = this;
        _col = GetComponent<BoxCollider2D>();
        _col.isTrigger = true;
    }

    // Devuelve una posición aleatoria DENTRO de los muros
    // y libre de obstáculos (no encima de un arbusto)
    public Vector2 GetRandomSpawnPosition()
    {
        Bounds b = _col.bounds;

        // Reducimos los bounds con el padding para no spawnear pegado al muro
        float minX = b.min.x + spawnPadding;
        float maxX = b.max.x - spawnPadding;
        float minY = b.min.y + spawnPadding;
        float maxY = b.max.y - spawnPadding;

        // Intenta varias veces hasta encontrar un punto sin muro
        for (int i = 0; i < maxRetries; i++)
        {
            Vector2 candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

            // Comprueba que no haya muro en esa posición
            Collider2D hit = Physics2D.OverlapCircle(candidate, 0.3f, wallLayer);
            if (hit == null) return candidate;
        }

        // Si tras N intentos no encuentra hueco, devuelve el centro
        return _col.bounds.center;
    }

    // Devuelve una posición lejos del jugador pero dentro del mapa
    // Útil para que los enemigos no aparezcan justo encima del player
    public Vector2 GetSpawnPositionAwayFromPlayer(Vector3 playerPos, float minDistance = 6f)
    {
        for (int i = 0; i < maxRetries; i++)
        {
            Vector2 candidate = GetRandomSpawnPosition();
            if (Vector2.Distance(candidate, playerPos) >= minDistance)
                return candidate;
        }
        return GetRandomSpawnPosition();
    }

    // Visualización del área en el editor (rectángulo verde)
    void OnDrawGizmos()
    {
        var col = GetComponent<BoxCollider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(col.bounds.center, col.bounds.size);
    }
}
