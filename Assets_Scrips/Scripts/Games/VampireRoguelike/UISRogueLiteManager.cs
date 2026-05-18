using UnityEngine;
using UnityEngine.UI;
using TMPro;

// ============================================================
// UIS ROGUE LITE MANAGER
// Script único que controla TODO el HUD del juego:
//   - Portrait circular del jugador
//   - Barra de HP (roja, tipo Filled)
//   - Barra de XP (blanca, tipo Filled)
//   - Nivel, puntos, monedas, kills
//   - Texto de oleada (centro-arriba)
//   - Timer gigante debajo de la oleada
//   - Panel de Game Over / Victoria
// Adjunta este script al GameObject "HUD" dentro del Canvas.
// ============================================================
public class UISRogueLiteManager : MonoBehaviour
{
    // ── PORTRAIT ──────────────────────────────────────────────────────────
    [Header("Portrait")]
    [Tooltip("Imagen circular del personaje (esquina sup-izquierda)")]
    public Image portraitImage;

    // ── BARRA DE HP ───────────────────────────────────────────────────────
    [Header("Barra de HP")]
    [Tooltip("Image tipo Filled Horizontal — el relleno rojo que se mueve")]
    public Image hpBarFill;

    [Tooltip("Texto encima de la barra: 'HP:20/20'")]
    public TextMeshProUGUI hpText;

    // ── BARRA DE XP ───────────────────────────────────────────────────────
    [Header("Barra de XP")]
    [Tooltip("Image tipo Filled Horizontal — el relleno blanco que se mueve")]
    public Image xpBarFill;

    [Tooltip("Texto encima de la barra: 'XP:15/20'")]
    public TextMeshProUGUI xpText;

    // ── STATS LATERALES ───────────────────────────────────────────────────
    [Header("Stats laterales (esquina sup-izquierda)")]
    [Tooltip("Nivel del jugador: 'Lv-3'")]
    public TextMeshProUGUI levelText;

    [Tooltip("Puntos de nivel disponibles: '×5'")]
    public TextMeshProUGUI levelPointsText;

    [Tooltip("Monedas actuales: '610'")]
    public TextMeshProUGUI coinsText;

    [Tooltip("Enemigos eliminados: '119'")]
    public TextMeshProUGUI killsText;

    // ── OLEADA Y TIMER ────────────────────────────────────────────────────
    [Header("Centro superior — Oleada y Timer")]
    [Tooltip("Texto de la oleada: 'OLEADA-02'")]
    public TextMeshProUGUI waveText;

    [Tooltip("Número gigante del timer: '21'")]
    public TextMeshProUGUI timerText;

    // ── GAME OVER / VICTORIA ──────────────────────────────────────────────
    [Header("Panel Game Over")]
    public GameObject gameOverPanel;
    public TextMeshProUGUI resultTitleText; // "¡VICTORIA!" o "¡DERROTADO!"
    public TextMeshProUGUI finalScoreText;
    public Button restartButton;

    // ── Variables internas ─────────────────────────────────────────────────
    int _maxHP = 10;
    int _maxXP = 20;

    // ── Inicialización ─────────────────────────────────────────────────────
    void Start()
    {
        // Se registra en el GameManager para recibir las llamadas de Update
        var gm = GameManagerRogueLite.Instance;
        if (gm != null) gm.uiManager = this;

        // Oculta el panel de Game Over al iniciar
        gameOverPanel?.SetActive(false);

        // Escucha cuando cambian las monedas para actualizar el texto
        if (CoinSystem.Instance != null)
            CoinSystem.Instance.OnCoinsChanged += UpdateCoins;

        // Botón de reinicio del Game Over
        restartButton?.onClick.AddListener(() =>
            GameManagerRogueLite.Instance?.Restart());

        // Escucha subidas de nivel y cambios de XP
        if (gm != null)
        {
            gm.OnLevelUp += lvl => UpdateLevel(lvl);
            gm.OnXPChanged += xp => UpdateXPBar(xp, gm.xpToNextLevel);
        }

        // Valores iniciales en el HUD
        UpdateLevel(1);
        UpdateCoins(0);
        UpdateKills(0);
        UpdateLevelPoints(0);
    }

    // ── Update: timer en tiempo real ───────────────────────────────────────
    void Update()
    {
        if (WaveSystemRogueLite.Instance == null) return;

        float t = WaveSystemRogueLite.Instance.GetWaveTimeRemaining();

        // Timer gigante: muestra los segundos en 2 dígitos ("09", "21")
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(t).ToString("D2");
    }

    // ── HP ─────────────────────────────────────────────────────────────────
    // Actualiza la barra roja y el texto "HP:20/20"
    public void UpdateHP(int current, int max)
    {
        _maxHP = max;

        // fillAmount va de 0.0 (vacío) a 1.0 (lleno)
        if (hpBarFill != null)
            hpBarFill.fillAmount = max > 0 ? (float)current / max : 0f;

        if (hpText != null)
            hpText.text = $"HP:{current}/{max}";
    }

    // ── XP ─────────────────────────────────────────────────────────────────
    // Actualiza la barra blanca y el texto "XP:15/20"
    public void UpdateXPBar(int xp, int needed)
    {
        _maxXP = needed;

        if (xpBarFill != null)
            xpBarFill.fillAmount = needed > 0 ? (float)xp / needed : 0f;

        if (xpText != null)
            xpText.text = $"XP:{xp}/{needed}";
    }

    // ── Nivel ──────────────────────────────────────────────────────────────
    public void UpdateLevel(int lvl)
    {
        if (levelText != null)
            levelText.text = $"Lv-{lvl}";
    }

    // ── Puntos de nivel ────────────────────────────────────────────────────
    public void UpdateLevelPoints(int points)
    {
        if (levelPointsText != null)
            levelPointsText.text = $"×{points}";
    }

    // ── Monedas ────────────────────────────────────────────────────────────
    public void UpdateCoins(int coins)
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();
    }

    // ── Kills ──────────────────────────────────────────────────────────────
    public void UpdateKills(int kills)
    {
        if (killsText != null)
            killsText.text = kills.ToString();
    }

    // ── Oleada ─────────────────────────────────────────────────────────────
    // Actualiza el texto "OLEADA-05" y el multiplicador de fase
    public void UpdateWaveInfo(WaveData wave)
    {
        if (wave == null) return;

        if (waveText != null)
            waveText.text = $"OLEADA-{wave.waveNumber:D2}";
    }

    // ── Score (llamado internamente, sin texto visible por defecto) ─────────
    public void UpdateScore(int s) { /* Opcional: añade un scoreText si lo necesitas */ }

    // ── Game Over ──────────────────────────────────────────────────────────
    // won=true → Victoria, won=false → Derrotado
    public void ShowGameOver(int finalScore, bool won = false)
    {
        gameOverPanel?.SetActive(true);

        if (resultTitleText)
            resultTitleText.text = won ? "¡VICTORIA!" : "¡DERROTADO!";

        if (finalScoreText)
            finalScoreText.text = $"Score: {finalScore}";
    }

    // ── Pausa (llamado por GameManager) ────────────────────────────────────
    public void ShowPause(bool show) { /* Añade pausePanel si lo necesitas */ }
}