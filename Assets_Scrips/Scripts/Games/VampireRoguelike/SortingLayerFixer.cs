using UnityEngine;

// ============================================================
// SORTING LAYER FIXER
// Asegura que todos los sprites del juego (player, enemigos,
// proyectiles) estén en el Sorting Layer correcto para que
// el HUD del Canvas siempre se vea encima de ellos.
//
// SETUP:
//   Adjunta este script al GameObject "GameManager".
//   No necesita referencias: lo busca automáticamente.
//
// REGLA DE ORDEN:
//   Sprites del mundo  → Sorting Layer "Default", Order 0-3
//   HUD Canvas         → Sort Order 5  (siempre encima del mundo)
//   CRT Overlay        → Sort Order 100 (encima de todo)
// ============================================================
public class SortingLayerFixer : MonoBehaviour
{
    [Header("Orden de los sprites (deben ser menores que el Canvas)")]
    [Tooltip("Orden del player (debe ser menor que el Sort Order del Canvas = 5)")]
    public int playerSortOrder = 2;

    [Tooltip("Orden de los enemigos")]
    public int enemySortOrder = 1;

    [Tooltip("Orden de los proyectiles")]
    public int projectileSortOrder = 3;

    void Start()
    {
        FixPlayerSorting();
        FixEnemySorting();
        FixProjectileSorting();
    }

    void FixPlayerSorting()
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        // Aplica a todos los SpriteRenderers del player y sus hijos
        foreach (var sr in player.GetComponentsInChildren<SpriteRenderer>())
        {
            sr.sortingLayerName = "Default";
            sr.sortingOrder = playerSortOrder;
        }
        Debug.Log($"✅ Player sorting: Order {playerSortOrder}");
    }

    void FixEnemySorting()
    {
        // Aplica a todos los enemigos activos en la escena
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            foreach (var sr in enemy.GetComponentsInChildren<SpriteRenderer>())
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = enemySortOrder;
            }
        }
    }

    void FixProjectileSorting()
    {
        // Aplica a todos los proyectiles activos
        foreach (var proj in GameObject.FindGameObjectsWithTag("Projectile"))
        {
            var sr = proj.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sortingLayerName = "Default";
                sr.sortingOrder = projectileSortOrder;
            }
        }
    }
}
