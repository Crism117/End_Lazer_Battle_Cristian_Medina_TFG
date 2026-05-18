using UnityEngine;
using System.Collections.Generic;

public enum EnemyType { Basic, Tank, HighDamage, Fast }
public enum GamePhase { Easy, Medium, Aggressive }

[System.Serializable]
public class EnemyTypeWeight
{
    public EnemyType type;
    [Range(0, 100)] public int weight;
}

[System.Serializable]
public class WaveData
{
    [Header("Identificación")]
    public int waveNumber;
    public GamePhase phase;

    [Header("Spawn continuo durante la oleada")]
    public float spawnInterval = 1.5f;
    public float difficultyRamp = 0.97f;

    [Header("Stats de enemigos")]
    public int baseHP = 3;
    public float baseSpeed = 1.5f;
    public float scoreMultiplier = 1f;

    [Header("Tipos activos")]
    public List<EnemyTypeWeight> enemyWeights;

    // Fórmula de duración: Oleada 1 = 15s, cada siguiente +5s, Oleada 10 = 60s
    public float GetDuration() => 15f + (waveNumber - 1) * 5f;
}

// ============================================================
// WAVE SYSTEM
// - Timer variable por oleada (15s → 60s)
// - Al terminar el timer dispara OnWaveEnded (abre panel)
// - No avanza solo: espera al botón "Continuar"
// ============================================================
public class WaveSystemRogueLite : MonoBehaviour
{
    public static WaveSystemRogueLite Instance { get; private set; }

    [Header("Oleadas")]
    public List<WaveData> waves = new List<WaveData>();

    public int CurrentWaveIndex { get; private set; } = 0;
    public WaveData ActiveWave => CurrentWaveIndex < waves.Count ? waves[CurrentWaveIndex] : null;
    public bool GameFinished { get; private set; } = false;
    public bool IsWaveRunning { get; private set; } = false;

    float _waveTimer;
    int _lastWarningSecond = -1; // FIX: para que cada "pi" suene una sola vez

    // Eventos
    public System.Action<WaveData> OnWaveStarted;       // La oleada arrancó
    public System.Action<WaveData> OnWaveEnded;         // Timer llegó a 0
    public System.Action OnAllWavesCompleted;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
        if (waves.Count == 0) BuildDefaultWaves();
    }

    void Update()
    {
        if (!IsWaveRunning) return;
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused) return;

        _waveTimer -= Time.deltaTime;

        // ── ALERTA "PI PI PI" cuando faltan 5 segundos ──
        if (_waveTimer <= 5f && _waveTimer > 0f)
        {
            int currentSecond = Mathf.CeilToInt(_waveTimer);
            if (currentSecond != _lastWarningSecond && currentSecond <= 5 && currentSecond >= 1)
            {
                _lastWarningSecond = currentSecond;
                SoundManager.Instance?.PlayWaveWarning();
            }
        }

        if (_waveTimer <= 0f) EndCurrentWave();
    }

    // Inicia la primera oleada
    public void StartWaves()
    {
        CurrentWaveIndex = 0;
        GameFinished = false;
        BeginWave();
    }

    // Botón "Continuar" del panel entre oleadas llama a esto
    public void AdvanceToNextWave()
    {
        CurrentWaveIndex++;
        if (CurrentWaveIndex >= waves.Count)
        {
            GameFinished = true;
            OnAllWavesCompleted?.Invoke();
            return;
        }
        BeginWave();
    }

    void BeginWave()
    {
        IsWaveRunning = true;
        _waveTimer = ActiveWave.GetDuration();
        _lastWarningSecond = -1; // reset para próxima alerta

        // ── SONIDO INICIO OLEADA ──
        SoundManager.Instance?.PlayWaveStart();

        OnWaveStarted?.Invoke(ActiveWave);
        GameManagerRogueLite.Instance?.uiManager?.UpdateWaveInfo(ActiveWave);
    }

    void EndCurrentWave()
    {
        IsWaveRunning = false;
        _waveTimer = 0f;

        // ── CAMPANA FIN OLEADA ──
        SoundManager.Instance?.PlayWaveEndBell();

        OnWaveEnded?.Invoke(ActiveWave);
    }

    public float GetWaveTimeRemaining() => Mathf.Max(0f, _waveTimer);

    public EnemyType PickEnemyType()
    {
        var w = ActiveWave;
        if (w == null || w.enemyWeights == null || w.enemyWeights.Count == 0) return EnemyType.Basic;
        int total = 0;
        foreach (var ew in w.enemyWeights) total += ew.weight;
        int roll = Random.Range(0, total), acc = 0;
        foreach (var ew in w.enemyWeights) { acc += ew.weight; if (roll < acc) return ew.type; }
        return EnemyType.Basic;
    }

    public static (int hp, float speed, int damage) GetTypeStats(EnemyType t, int baseHP, float baseSpeed)
    {
        return t switch
        {
            EnemyType.Tank => (Mathf.RoundToInt(baseHP * 2.2f), baseSpeed * 0.65f, 1),
            EnemyType.HighDamage => (baseHP, baseSpeed, 3),
            EnemyType.Fast => (Mathf.Max(1, Mathf.RoundToInt(baseHP * 0.6f)), baseSpeed * 1.8f, 1),
            _ => (baseHP, baseSpeed, 1),
        };
    }

    void BuildDefaultWaves()
    {
        List<EnemyTypeWeight> W(params (EnemyType t, int w)[] items)
        {
            var list = new List<EnemyTypeWeight>();
            foreach (var (t, w) in items) list.Add(new EnemyTypeWeight { type = t, weight = w });
            return list;
        }

        // Oleadas 1-3: solo Slimes (Basic)
        waves.Add(new WaveData { waveNumber = 1, phase = GamePhase.Easy, spawnInterval = 2.0f, baseHP = 2, baseSpeed = 1.0f, scoreMultiplier = 3f, enemyWeights = W((EnemyType.Basic, 100)) });
        waves.Add(new WaveData { waveNumber = 2, phase = GamePhase.Easy, spawnInterval = 1.8f, baseHP = 2, baseSpeed = 1.1f, scoreMultiplier = 3f, enemyWeights = W((EnemyType.Basic, 100)) });
        waves.Add(new WaveData { waveNumber = 3, phase = GamePhase.Easy, spawnInterval = 1.6f, baseHP = 3, baseSpeed = 1.2f, scoreMultiplier = 3f, enemyWeights = W((EnemyType.Basic, 100)) });

        // Oleadas 4-6: Slimes + Arañas (Fast)
        waves.Add(new WaveData { waveNumber = 4, phase = GamePhase.Medium, spawnInterval = 1.4f, baseHP = 4, baseSpeed = 1.3f, scoreMultiplier = 1.5f, enemyWeights = W((EnemyType.Basic, 65), (EnemyType.Fast, 35)) });
        waves.Add(new WaveData { waveNumber = 5, phase = GamePhase.Medium, spawnInterval = 1.2f, baseHP = 5, baseSpeed = 1.4f, scoreMultiplier = 1.5f, enemyWeights = W((EnemyType.Basic, 55), (EnemyType.Fast, 45)) });
        waves.Add(new WaveData { waveNumber = 6, phase = GamePhase.Medium, spawnInterval = 1.0f, baseHP = 6, baseSpeed = 1.5f, scoreMultiplier = 1.5f, enemyWeights = W((EnemyType.Basic, 50), (EnemyType.Fast, 50)) });

        // Oleadas 7-10: Slimes + Arañas + Gárgolas (Tank)
        waves.Add(new WaveData { waveNumber = 7, phase = GamePhase.Aggressive, spawnInterval = 0.9f, baseHP = 8, baseSpeed = 1.7f, scoreMultiplier = 2f, enemyWeights = W((EnemyType.Basic, 40), (EnemyType.Fast, 35), (EnemyType.Tank, 25)) });
        waves.Add(new WaveData { waveNumber = 8, phase = GamePhase.Aggressive, spawnInterval = 0.8f, baseHP = 10, baseSpeed = 1.9f, scoreMultiplier = 2f, enemyWeights = W((EnemyType.Basic, 30), (EnemyType.Fast, 35), (EnemyType.Tank, 35)) });
        waves.Add(new WaveData { waveNumber = 9, phase = GamePhase.Aggressive, spawnInterval = 0.6f, baseHP = 13, baseSpeed = 2.2f, scoreMultiplier = 2f, enemyWeights = W((EnemyType.Basic, 25), (EnemyType.Fast, 35), (EnemyType.Tank, 40)) });
        waves.Add(new WaveData { waveNumber = 10, phase = GamePhase.Aggressive, spawnInterval = 0.4f, baseHP = 16, baseSpeed = 2.5f, scoreMultiplier = 2f, enemyWeights = W((EnemyType.Basic, 20), (EnemyType.Fast, 40), (EnemyType.Tank, 40)) });
    }
}