using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// ============================================================
// ENEMY SPAWNER ROGUE LITE
// Spawnea enemigos durante la oleada DENTRO de los muros.
// Si hay un MapBounds en la escena, lo usa para no spawnear fuera.
// Si no hay MapBounds, usa el método antiguo (radio alrededor del player).
// ============================================================
public class EnemySpawnerRogueLite : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Pools por tipo de enemigo")]
    public ObjectPoolRogueLite slimePool;
    public ObjectPoolRogueLite aranaPool;
    public ObjectPoolRogueLite gargolaPool;

    [Header("Spawn")]
    [Tooltip("Si no hay MapBounds, distancia desde el player donde aparecen")]
    public float spawnRadius = 10f;

    [Tooltip("Distancia mínima del player al que aparecen los enemigos (con MapBounds)")]
    public float minDistanceFromPlayer = 6f;

    readonly List<GameObject> _activeEnemies = new List<GameObject>();
    WaveData _currentWave;
    float _currentInterval;

    void Start()
    {
        if (player == null) player = GameManagerRogueLite.Instance?.playerTransform;

        if (WaveSystemRogueLite.Instance != null)
        {
            WaveSystemRogueLite.Instance.OnWaveStarted += OnWaveStarted;
            WaveSystemRogueLite.Instance.StartWaves();
        }

        StartCoroutine(SpawnLoop());
    }

    void OnDestroy()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveStarted -= OnWaveStarted;
    }

    void OnWaveStarted(WaveData wave)
    {
        _currentWave = wave;
        _currentInterval = wave.spawnInterval;
        ClearAllEnemies();
    }

    IEnumerator SpawnLoop()
    {
        yield return new WaitUntil(() =>
            WaveSystemRogueLite.Instance != null &&
            WaveSystemRogueLite.Instance.ActiveWave != null);

        _currentWave = WaveSystemRogueLite.Instance.ActiveWave;
        _currentInterval = _currentWave.spawnInterval;

        while (true)
        {
            if (WaveSystemRogueLite.Instance.GameFinished) yield break;

            bool paused = GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused;
            bool running = WaveSystemRogueLite.Instance.IsWaveRunning;

            if (!paused && running && _currentWave != null)
            {
                SpawnOne(_currentWave);
                _currentInterval = Mathf.Max(0.25f, _currentInterval * _currentWave.difficultyRamp);
            }

            yield return new WaitForSeconds(running ? _currentInterval : 0.2f);
        }
    }

    void SpawnOne(WaveData wave)
    {
        if (player == null) return;

        var pool = PickPool(wave.waveNumber);
        if (pool == null) return;

        var obj = pool.Get();
        if (obj == null) return;

        // ── ELEGIR POSICIÓN DE SPAWN ──────────────────────────────────
        Vector2 pos = GetSpawnPosition();

        obj.transform.position = pos;
        obj.SetActive(true);

        var enemy = obj.GetComponent<EnemyRogueLite>();
        if (enemy != null)
        {
            EnemyType type = WaveSystemRogueLite.Instance.PickEnemyType();
            enemy.Initialize(pool, type, wave.baseHP, wave.baseSpeed, wave.scoreMultiplier);
        }
        _activeEnemies.Add(obj);
    }

    // Posición de spawn: usa MapBounds si existe, si no usa el radio antiguo
    Vector2 GetSpawnPosition()
    {
        // Si hay MapBounds en la escena, spawneamos dentro
        if (MapBounds.Instance != null)
        {
            return MapBounds.Instance.GetSpawnPositionAwayFromPlayer(
                player.position, minDistanceFromPlayer);
        }

        // Fallback: radio alrededor del player (modo antiguo)
        return (Vector2)player.position + Random.insideUnitCircle.normalized * spawnRadius;
    }

    ObjectPoolRogueLite PickPool(int waveNumber)
    {
        if (waveNumber <= 3) return slimePool;
        if (waveNumber <= 6) return Random.value < 0.5f ? slimePool : aranaPool;
        float r = Random.value;
        if (r < 0.33f) return slimePool;
        if (r < 0.66f) return aranaPool;
        return gargolaPool;
    }

    ObjectPoolRogueLite GetPoolForGroup(EnemyRogueLite.EnemyGroup g) => g switch
    {
        EnemyRogueLite.EnemyGroup.Arana => aranaPool,
        EnemyRogueLite.EnemyGroup.Gargola => gargolaPool,
        _ => slimePool,
    };

    public void ClearAllEnemies()
    {
        foreach (var go in _activeEnemies)
        {
            if (go == null) continue;
            if (!go.activeInHierarchy) continue;
            go.SetActive(false);
            var e = go.GetComponent<EnemyRogueLite>();
            var pool = GetPoolForGroup(e?.enemyGroup ?? EnemyRogueLite.EnemyGroup.Slime);
            pool?.Return(go);
        }
        _activeEnemies.Clear();
    }

    // ─────────────────────────────────────────────────────────────────────
    // DESPAWN ANIMADO ENTRE OLEADAS (Fase 4)
    // Cada enemigo activo: queda inmóvil, parpadea rojo y se desvanece.
    // No los devuelve al pool inmediatamente — lo hace cada enemigo al
    // terminar su animación.
    // ─────────────────────────────────────────────────────────────────────
    /// <summary>
    /// Llamado por WaveTransitionManager al terminar una oleada.
    /// </summary>
    /// <param name="duration">Duración total de la animación de cada enemigo</param>
    public void DespawnAllEnemiesAnimated(float duration = 2.5f)
    {
        foreach (var go in _activeEnemies)
        {
            if (go == null || !go.activeInHierarchy) continue;
            var e = go.GetComponent<EnemyRogueLite>();
            if (e != null) e.BeginDespawn(duration, blinkCount: 3);
        }

        // La lista se limpia: cada enemigo se devuelve al pool por su cuenta
        _activeEnemies.Clear();
    }
}