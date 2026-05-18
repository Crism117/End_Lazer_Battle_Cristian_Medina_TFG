using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

// ============================================================
// GAME MANAGER (v11)
// FIX iter 11:
// + Restart() ahora limpia Instance + DOTween.KillAll()
// + Sonidos victoria/derrota ANTES de pausar audio
// ============================================================
public class GameManagerRogueLite : MonoBehaviour
{
    public static GameManagerRogueLite Instance { get; private set; }

    [Header("Referencias")]
    public UISRogueLiteManager uiManager;
    public Transform playerTransform;

    int score, kills, playerXP;
    int levelUpsCount = 0;
    int playerLevel = 1;
    public int xpToNextLevel = 10;

    public int LevelPoints { get; private set; } = 0;

    public bool IsPaused { get; private set; }
    public int PlayerLevel => playerLevel;
    public bool HasGameplayStarted { get; private set; } = false;

    public event Action<int> OnXPChanged;
    public event Action<int> OnLevelUp;
    public event Action<int> OnLevelPointsChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        ResetStats();

        LevelUpSystem.Instance?.Reset();
        CoinSystem.Instance?.Reset();

        uiManager?.UpdateLevel(playerLevel);
        uiManager?.UpdateLevelPoints(LevelPoints);

        if (WaveSystemRogueLite.Instance != null)
        {
            WaveSystemRogueLite.Instance.OnWaveEnded += OnWaveEnded;
            WaveSystemRogueLite.Instance.OnAllWavesCompleted += OnVictory;
        }

        IsPaused = true;
    }

    void OnDestroy()
    {
        // FIX v11: limpiar singleton al destruir
        if (Instance == this) Instance = null;
    }

    public void StartGameplay()
    {
        if (HasGameplayStarted) return;
        HasGameplayStarted = true;

        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void ResetStats()
    {
        score = kills = playerXP = 0;
        levelUpsCount = 0;
        playerLevel = 1;
        xpToNextLevel = 10;
        LevelPoints = 0;
        uiManager?.UpdateScore(score);
        uiManager?.UpdateXPBar(playerXP, xpToNextLevel);
        uiManager?.UpdateKills(kills);
        uiManager?.UpdateLevel(playerLevel);
        uiManager?.UpdateLevelPoints(LevelPoints);
    }

    public void AddScore(int amount) { score += amount; uiManager?.UpdateScore(score); }
    public void AddKill() { kills++; uiManager?.UpdateKills(kills); }

    public void AddXP(int amount)
    {
        playerXP += amount;
        OnXPChanged?.Invoke(playerXP);
        uiManager?.UpdateXPBar(playerXP, xpToNextLevel);

        while (playerXP >= xpToNextLevel)
        {
            playerXP -= xpToNextLevel;
            playerLevel++;
            levelUpsCount++;
            xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.35f);
            LevelPoints++;
            OnLevelUp?.Invoke(playerLevel);
            OnLevelPointsChanged?.Invoke(LevelPoints);
            uiManager?.UpdateLevelPoints(LevelPoints);

            SoundManager.Instance?.PlayPlayerLevelUp();
        }
        uiManager?.UpdateXPBar(playerXP, xpToNextLevel);
    }

    public bool SpendLevelPoint()
    {
        if (LevelPoints <= 0) return false;
        LevelPoints--;
        OnLevelPointsChanged?.Invoke(LevelPoints);
        uiManager?.UpdateLevelPoints(LevelPoints);
        return true;
    }

    public void PauseGame()
    {
        IsPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    void OnWaveEnded(WaveData wave)
    {
        if (WaveTransitionManager.Instance == null)
            FindObjectOfType<EnemySpawnerRogueLite>()?.ClearAllEnemies();
    }

    public void ContinueToNextWave()
    {
        var player = FindObjectOfType<PlayerControllerRogueLite>();
        player?.HealProgressive();

        ResumeGame();
        WaveSystemRogueLite.Instance?.AdvanceToNextWave();
    }

    public int CalculateFinalScore()
    {
        int coins = CoinSystem.Instance != null ? CoinSystem.Instance.CurrentCoins : 0;
        return (kills + coins + levelUpsCount) * 2;
    }

    public void GameOver()
    {
        // 1) SONIDO PRIMERO
        SoundManager.Instance?.PlayDefeat();

        // 2) Calcular score
        int finalScore = CalculateFinalScore();
        score = finalScore;
        uiManager?.UpdateScore(score);

        // 3) Mostrar panel
        if (RogueliteMenuController.Instance != null)
            RogueliteMenuController.Instance.ShowGameOver(finalScore, victory: false);
        else
            uiManager?.ShowGameOver(finalScore, won: false);

        // 4) Pausar gameplay pero NO audio
        IsPaused = true;
        Time.timeScale = 0f;
    }

    void OnVictory()
    {
        // 1) SONIDO PRIMERO
        SoundManager.Instance?.PlayVictory();

        // 2) Calcular score
        int finalScore = CalculateFinalScore();
        score = finalScore;
        uiManager?.UpdateScore(score);

        // 3) Mostrar panel
        if (RogueliteMenuController.Instance != null)
            RogueliteMenuController.Instance.ShowGameOver(finalScore, victory: true);
        else
            uiManager?.ShowGameOver(finalScore, won: true);

        // 4) Pausar gameplay pero NO audio
        IsPaused = true;
        Time.timeScale = 0f;
    }

    /// <summary>
    /// FIX v11: Restart con limpieza explícita.
    /// </summary>
    public void Restart()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        DOTween.KillAll();
        Instance = null;
        Destroy(gameObject);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}