using UnityEngine;

// ============================================================
// COIN DESPAWN MANAGER
// Al terminar cada oleada, hace que las monedas restantes
// parpadeen y se eliminen automáticamente (rendimiento).
// El jugador puede recogerlas mientras parpadean.
// ============================================================
public class CoinDespawnManager : MonoBehaviour
{
    [Header("─── CONFIGURACIÓN ──")]
    [Tooltip("Cuántos segundos parpadean antes de desaparecer")]
    public float blinkDuration = 3f;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Start()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveEnded += OnWaveEnded;
    }

    void OnDestroy()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveEnded -= OnWaveEnded;
    }

    void OnWaveEnded(WaveData wave)
    {
        var coins = FindObjectsOfType<CoinPickup>();

        if (debugMode)
            Debug.Log($"[CoinDespawn] Limpiando {coins.Length} monedas tras oleada {wave?.waveNumber}");

        foreach (var coin in coins)
        {
            if (coin != null) coin.StartBlinking(blinkDuration);
        }
    }
}
