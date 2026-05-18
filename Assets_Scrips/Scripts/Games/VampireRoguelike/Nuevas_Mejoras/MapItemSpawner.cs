using UnityEngine;
using System.Collections.Generic;

// ============================================================
// MAP ITEM SPAWNER (v7)
// Spawnea items según el número de oleada:
//   Oleadas 1-3 → 1 item máx
//   Oleadas 4-8 → 2 items máx
//   Oleadas 9-10 → 3 items máx
// ============================================================
public class MapItemSpawner : MonoBehaviour
{
    [Header("─── ITEM PREFABS ──")]
    public GameObject shieldPrefab;
    public GameObject collarPrefab;
    public GameObject lifestealPrefab;
    public GameObject wolfPrefab;
    public GameObject bootsPrefab;
    public GameObject magnetPrefab;

    [Header("─── ÁREA DE SPAWN ──")]
    public bool useCustomBounds = true;
    public Vector2 boundsCenter = Vector2.zero;
    public Vector2 boundsSize = new Vector2(15f, 10f);
    public float minDistanceFromPlayer = 3f;
    public float spawnAreaRadius = 8f;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;
    public Color boundsGizmoColor = new Color(0f, 1f, 0.4f, 0.3f);

    Transform _player;
    List<GameObject> _allPrefabs = new List<GameObject>();

    void Start()
    {
        var playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null) _player = playerGO.transform;

        if (shieldPrefab != null) _allPrefabs.Add(shieldPrefab);
        if (collarPrefab != null) _allPrefabs.Add(collarPrefab);
        if (lifestealPrefab != null) _allPrefabs.Add(lifestealPrefab);
        if (wolfPrefab != null) _allPrefabs.Add(wolfPrefab);
        if (bootsPrefab != null) _allPrefabs.Add(bootsPrefab);
        if (magnetPrefab != null) _allPrefabs.Add(magnetPrefab);

        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveStarted += SpawnItemsForWave;
    }

    void OnDestroy()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveStarted -= SpawnItemsForWave;
    }

    void SpawnItemsForWave(WaveData wave)
    {
        if (_allPrefabs.Count == 0) return;
        if (wave == null) return;

        int count = GetItemCountForWave(wave.waveNumber);

        for (int i = 0; i < count; i++)
        {
            var prefab = _allPrefabs[Random.Range(0, _allPrefabs.Count)];
            Vector2 spawnPos = GetRandomSpawnPosition();
            Instantiate(prefab, spawnPos, Quaternion.identity);

            if (debugMode)
                Debug.Log($"[MapItemSpawner] '{prefab.name}' en {spawnPos} (oleada {wave.waveNumber})");
        }
    }

    /// <summary>
    /// Devuelve la cantidad de items según número de oleada.
    /// </summary>
    int GetItemCountForWave(int waveNumber)
    {
        if (waveNumber >= 9) return 3;       // 9-10 → 3
        if (waveNumber >= 4) return 2;       // 4-8 → 2
        return 1;                             // 1-3 → 1
    }

    Vector2 GetRandomSpawnPosition()
    {
        Vector2 playerPos = _player != null ? (Vector2)_player.position : Vector2.zero;

        for (int attempt = 0; attempt < 12; attempt++)
        {
            Vector2 pos;
            if (useCustomBounds)
            {
                pos = boundsCenter + new Vector2(
                    Random.Range(-boundsSize.x * 0.5f, boundsSize.x * 0.5f),
                    Random.Range(-boundsSize.y * 0.5f, boundsSize.y * 0.5f));
            }
            else
            {
                pos = playerPos + Random.insideUnitCircle * spawnAreaRadius;
            }

            float dist = Vector2.Distance(pos, playerPos);
            if (dist >= minDistanceFromPlayer) return pos;
        }

        Vector2 dir = Random.insideUnitCircle.normalized;
        return playerPos + dir * minDistanceFromPlayer;
    }

    void OnDrawGizmos()
    {
        if (useCustomBounds)
        {
            Gizmos.color = boundsGizmoColor;
            Gizmos.DrawWireCube(boundsCenter, new Vector3(boundsSize.x, boundsSize.y, 0.1f));
        }
    }
}