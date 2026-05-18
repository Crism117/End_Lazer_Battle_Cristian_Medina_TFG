using UnityEngine;

// ============================================================
// COIN SPAWNER REGISTRAR
// Adjunta este script al GameObject "GameManager".
// Su única función es decirle al sistema cuál es el prefab
// de moneda que debe instanciar cuando muere un enemigo.
// ============================================================
public class CoinSpawnerRegistrar : MonoBehaviour
{
    [Tooltip("Arrastra aquí el prefab Coin desde la carpeta Prefabs")]
    public GameObject coinPrefab;

    void Awake()
    {
        // Registra el prefab en la clase estática CoinSpawner
        // para que cualquier enemigo pueda usarlo al morir
        CoinSpawner.coinPrefab = coinPrefab;
    }
}
