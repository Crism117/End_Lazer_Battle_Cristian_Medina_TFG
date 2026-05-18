using UnityEngine;

// ============================================================
// MAP WALLS
// Crea 4 muros invisibles (arriba, abajo, izquierda, derecha)
// que forman una caja cerrada alrededor del mapa.
// Los arbustos son solo decoración encima de estos muros.
// Player y enemigos no pueden salir ni entrar por los bordes.
//
// SETUP:
// 1. Crea un GameObject vacío llamado "MapWalls"
// 2. Añade este script
// 3. Ajusta Width y Height según el tamaño de tu mapa
// ============================================================
public class MapWalls : MonoBehaviour
{
    [Header("Tamaño del mapa jugable")]
    [Tooltip("Ancho total del mapa (de muro izquierdo a muro derecho)")]
    public float width = 30f;

    [Tooltip("Alto total del mapa (de muro inferior a muro superior)")]
    public float height = 20f;

    [Tooltip("Grosor de cada muro (invisible, hazlo generoso: 1 o 2)")]
    public float wallThickness = 1f;

    [Header("Layer")]
    [Tooltip("Asigna la layer 'Wall' para que colisione con player y enemigos")]
    public string wallLayer = "Wall";

    void Awake()
    {
        // Crea los 4 muros automáticamente al iniciar
        CreateWall("Muro_Arriba",
            new Vector2(0f, height * 0.5f),
            new Vector2(width + wallThickness * 2f, wallThickness));

        CreateWall("Muro_Abajo",
            new Vector2(0f, -height * 0.5f),
            new Vector2(width + wallThickness * 2f, wallThickness));

        CreateWall("Muro_Izquierda",
            new Vector2(-width * 0.5f, 0f),
            new Vector2(wallThickness, height));

        CreateWall("Muro_Derecha",
            new Vector2(width * 0.5f, 0f),
            new Vector2(wallThickness, height));
    }

    void CreateWall(string wallName, Vector2 localPos, Vector2 size)
    {
        // Crea el GameObject hijo
        var go = new GameObject(wallName);
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;

        // Asigna la layer Wall
        int layer = LayerMask.NameToLayer(wallLayer);
        if (layer >= 0) go.layer = layer;

        // Rigidbody estático (necesario para colisionar con Dynamic)
        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;

        // BoxCollider2D del tamaño exacto del muro
        var col = go.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    // Muestra los muros en el editor como líneas rojas
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Arriba
        Gizmos.DrawWireCube(
            transform.position + new Vector3(0f, height * 0.5f, 0f),
            new Vector3(width, wallThickness, 0f));

        // Abajo
        Gizmos.DrawWireCube(
            transform.position + new Vector3(0f, -height * 0.5f, 0f),
            new Vector3(width, wallThickness, 0f));

        // Izquierda
        Gizmos.DrawWireCube(
            transform.position + new Vector3(-width * 0.5f, 0f, 0f),
            new Vector3(wallThickness, height, 0f));

        // Derecha
        Gizmos.DrawWireCube(
            transform.position + new Vector3(width * 0.5f, 0f, 0f),
            new Vector3(wallThickness, height, 0f));

        // Área interior (verde = zona jugable)
        Gizmos.color = new Color(0f, 1f, 0f, 0.1f);
        Gizmos.DrawCube(transform.position, new Vector3(width, height, 0f));
    }
}
