using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;

// ============================================================
// ROGUELITE MENU CONTROLLER (v11)
// FIX iter 11:
// + OnClickReplay limpia explícitamente singletons antes de recargar
// + Mata todos los tweens de DOTween para evitar huérfanos
// + Resetea Time.timeScale y AudioListener.pause
// ============================================================
public class RogueliteMenuController : MonoBehaviour
{
    public static RogueliteMenuController Instance { get; private set; }

    [Header("─── PANELES ──")]
    public GameObject startMenu;
    public GameObject countdownPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;

    [Header("─── ELEMENTOS ──")]
    public RectTransform logoImage;
    public TextMeshProUGUI txtCountdown;
    public TextMeshProUGUI txtResult;
    public TextMeshProUGUI txtScore;

    [Header("─── HIGH SCORES ──")]
    public TextMeshProUGUI txtWaveSurvived;
    public TMP_InputField inputName;
    public Button btnSaveScore;
    public HighScorePanel highScorePanel;

    [Header("─── LOGO ANIMACIÓN ──")]
    public float logoFloatHeight = 30f;
    public float logoFloatPeriod = 1.5f;

    [Header("─── ESCENA ──")]
    public string mainMenuSceneName = "MenuPrincipal";

    [Header("─── COUNTDOWN ──")]
    public CountdownStartManager countdownManager;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    bool _gameStarted = false;
    int _lastFinalScore = 0;
    int _lastMaxWave = 0;
    bool _scoreSaved = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        if (startMenu != null) startMenu.SetActive(true);
        if (countdownPanel != null) countdownPanel.SetActive(false);
        if (pausePanel != null) pausePanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);

        if (logoImage != null)
        {
            Vector2 origPos = logoImage.anchoredPosition;
            logoImage.DOAnchorPosY(origPos.y + logoFloatHeight, logoFloatPeriod * 0.5f)
                .SetEase(Ease.InOutSine)
                .SetLoops(-1, LoopType.Yoyo)
                .SetUpdate(true);
        }

        if (btnSaveScore != null)
            btnSaveScore.onClick.AddListener(OnClickSaveScore);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && _gameStarted)
            TogglePause();
    }

    // ═══════════════════════════════════════════════════════════════════
    // START MENU
    // ═══════════════════════════════════════════════════════════════════
    public void OnClickPlay()
    {
        if (_gameStarted) return;
        _gameStarted = true;

        SoundManager.Instance?.PlayUIClick();
        if (startMenu != null) startMenu.SetActive(false);

        if (countdownManager != null)
            countdownManager.StartCountdown(OnCountdownFinished);
        else
            OnCountdownFinished();
    }

    public void OnClickShowHighScores()
    {
        SoundManager.Instance?.PlayUIClick();
        if (highScorePanel != null) highScorePanel.Open();
    }

    public void OnClickExitToMainMenu()
    {
        SoundManager.Instance?.PlayUIClick();
        PrepareSceneChange();
        SceneManager.LoadScene(mainMenuSceneName);
    }

    void OnCountdownFinished()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        if (GameManagerRogueLite.Instance != null)
        {
            if (!GameManagerRogueLite.Instance.HasGameplayStarted)
                GameManagerRogueLite.Instance.StartGameplay();
            else
                GameManagerRogueLite.Instance.ResumeGame();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // PAUSE
    // ═══════════════════════════════════════════════════════════════════
    public void ShowPauseMenu()
    {
        GameManagerRogueLite.Instance?.PauseGame();
        if (pausePanel != null)
            PanelAnimator.AnimateOpen(pausePanel);
    }

    public void OnClickResume()
    {
        SoundManager.Instance?.PlayUIClick();
        PanelAnimator.AnimateClose(pausePanel, 0.2f, () =>
        {
            GameManagerRogueLite.Instance?.ResumeGame();
        });
    }

    public void TogglePause()
    {
        if (!_gameStarted) return;
        if (gameOverPanel != null && gameOverPanel.activeSelf) return;

        if (pausePanel != null && pausePanel.activeSelf)
            OnClickResume();
        else
            ShowPauseMenu();
    }

    // ═══════════════════════════════════════════════════════════════════
    // GAME OVER
    // ═══════════════════════════════════════════════════════════════════
    public void ShowGameOver(int score, bool victory)
    {
        if (gameOverPanel == null) return;

        _lastFinalScore = score;
        _lastMaxWave = GetMaxWaveSurvived();
        _scoreSaved = false;

        if (txtResult != null)
        {
            txtResult.text = victory ? "VICTORIA" : "DERROTA";
            txtResult.color = victory ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
        }

        if (txtScore != null)
            txtScore.text = $"Score: {score}";

        if (txtWaveSurvived != null)
            txtWaveSurvived.text = $"Sobreviviste hasta la oleada {_lastMaxWave}";

        if (inputName != null)
        {
            inputName.text = "";
            inputName.interactable = true;
        }

        if (btnSaveScore != null)
            btnSaveScore.interactable = true;

        PanelAnimator.AnimateOpen(gameOverPanel, 0.5f);
    }

    int GetMaxWaveSurvived()
    {
        if (WaveSystemRogueLite.Instance == null) return 1;
        var current = WaveSystemRogueLite.Instance.ActiveWave;
        if (current != null) return current.waveNumber;
        return WaveSystemRogueLite.Instance.CurrentWaveIndex + 1;
    }

    public void OnClickSaveScore()
    {
        if (_scoreSaved) return;

        string name = inputName != null ? inputName.text.Trim() : "";

        if (string.IsNullOrEmpty(name))
        {
            SoundManager.Instance?.PlayDeniedBuzz();
            if (debugMode) Debug.Log("[Menu] Nombre vacío, no se guarda score.");
            return;
        }

        if (HighScoreManager.Instance != null)
        {
            HighScoreManager.Instance.AddScore(name, _lastFinalScore, _lastMaxWave);
            _scoreSaved = true;

            SoundManager.Instance?.PlaySaveConfirm();

            if (btnSaveScore != null) btnSaveScore.interactable = false;
            if (inputName != null) inputName.interactable = false;
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // REPLAY / VOLVER / SALIR (FIX v11)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// FIX v11: limpia explícitamente todo antes de recargar para evitar
    /// que los singletons huérfanos rompan la nueva partida.
    /// </summary>
    void PrepareSceneChange()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;

        // Matar todos los tweens activos
        DOTween.KillAll();

        // Limpiar singletons explícitamente
        Instance = null;
    }

    /// <summary>
    /// REJUGAR — recarga la escena con limpieza explícita.
    /// </summary>
    public void OnClickReplay()
    {
        SoundManager.Instance?.PlayUIClick();
        PrepareSceneChange();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// VOLVER — vuelve al StartMenu interno recargando la escena.
    /// </summary>
    public void OnClickReturnToStartMenu()
    {
        SoundManager.Instance?.PlayUIClick();
        PrepareSceneChange();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}