using UnityEngine;
using TMPro;

// ============================================================
// FLOATING DAMAGE MANAGER
// Encargado de crear los números flotantes en el mundo.
// Otros scripts lo llaman con: FloatingDamageManager.Instance.Show(...)
// ============================================================
public class FloatingDamageManager : MonoBehaviour
{
    public static FloatingDamageManager Instance { get; private set; }

    [Header("Prefab del texto flotante")]
    [Tooltip("Prefab con un TextMeshPro 3D y el script FloatingDamageText")]
    public GameObject floatingTextPrefab;

    [Header("Colores")]
    public Color normalDamageColor = Color.white;
    public Color critDamageColor = Color.yellow;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    // Crea un número flotante en una posición del mundo
    public void Show(Vector3 worldPos, int damage, bool isCrit = false)
    {
        if (floatingTextPrefab == null) return;

        // Pequeña variación aleatoria horizontal para que no se solapen
        Vector3 offset = new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f);

        var go = Instantiate(floatingTextPrefab, worldPos + offset, Quaternion.identity);
        var ft = go.GetComponent<FloatingDamageText>();
        ft?.Setup(damage, isCrit ? critDamageColor : normalDamageColor);
    }
}
