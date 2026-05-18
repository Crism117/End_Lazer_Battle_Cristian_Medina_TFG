using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Collections;

// ============================================================
// WAVE TRANSITION MANAGER
// Coordina TODA la transición entre oleadas SIN pausar el juego.
// - Muestra texto "Oleada Terminada" animado en el centro
// - Despawn animado de enemigos (parpadeo rojo + fade)
// - Curación progresiva del jugador (Lerp/DOTween)
// - Avance automático a la siguiente oleada
// ============================================================
public class WaveTransitionManager : MonoBehaviour
{
    public static WaveTransitionManager Instance { get; private set; }

    [Header("─── REFERENCIAS UI ──────────────────")]
    [Tooltip("TextMeshPro centro pantalla — 'Oleada Terminada'")]
    public TextMeshProUGUI waveEndText;

    [Header("─── REFERENCIAS DE ESCENA ───────────")]
    [Tooltip("Si está vacío, se busca con FindObjectOfType en Start")]
    public EnemySpawnerRogueLite enemySpawner;
    public PlayerControllerRogueLite player;

    [Header("─── TIEMPOS ──────────────────────────")]
    [Tooltip("Cuánto tiempo permanece visible el texto 'Oleada Terminada'")]
    [Range(0.5f, 4f)] public float textDisplayTime = 1.5f;

    [Tooltip("Cuánto tarda la curación progresiva del jugador")]
    [Range(0.5f, 4f)] public float healDuration = 1.5f;

    [Tooltip("Cuánto tarda el despawn animado de enemigos (parpadeo + fade)")]
    [Range(1f, 5f)] public float despawnDelay = 2.5f;

    [Tooltip("Espera total antes de iniciar la siguiente oleada")]
    [Range(1f, 6f)] public float nextWaveDelay = 3.0f;

    [Header("─── ANIMACIÓN TEXTO ──────────────────")]
    [Tooltip("Escala mínima desde la que aparece el texto (efecto pop)")]
    [Range(0.1f, 1f)] public float textStartScale = 0.3f;

    [Tooltip("Color del texto al aparecer")]
    public Color textColor = Color.white;

    [Header("─── DEBUG ─────────────────────────────")]
    public bool debugMode = false;

    // ── Estado interno ────────────────────────────────────────────────────
    bool _transitioning = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        // Auto-localizar referencias si no están asignadas
        if (enemySpawner == null) enemySpawner = FindObjectOfType<EnemySpawnerRogueLite>();
        if (player == null) player = FindObjectOfType<PlayerControllerRogueLite>();

        // Texto oculto al inicio
        if (waveEndText != null)
        {
            waveEndText.gameObject.SetActive(false);
            waveEndText.alpha = 0f;
        }

        // Suscribirse al fin de oleada del WaveSystem
        if (WaveSystemRogueLite.Instance != null)
        {
            WaveSystemRogueLite.Instance.OnWaveEnded += HandleWaveEnded;
        }
        else
        {
            Debug.LogError("[WaveTransition] WaveSystemRogueLite no encontrado.");
        }
    }

    void OnDestroy()
    {
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.OnWaveEnded -= HandleWaveEnded;
    }

    // ─────────────────────────────────────────────────────────────────────
    // ENTRY POINT — el WaveSystem dispara OnWaveEnded
    // ─────────────────────────────────────────────────────────────────────
    void HandleWaveEnded(WaveData wave)
    {
        if (_transitioning) return;
        StartCoroutine(TransitionRoutine(wave));
    }

    // ─────────────────────────────────────────────────────────────────────
    // RUTINA PRINCIPAL DE TRANSICIÓN
    // ─────────────────────────────────────────────────────────────────────
    IEnumerator TransitionRoutine(WaveData wave)
    {
        _transitioning = true;
        if (debugMode) Debug.Log($"[WaveTransition] Iniciando transición tras oleada {wave?.waveNumber}");

        // ── 1. MOSTRAR TEXTO "OLEADA TERMINADA" ──────────────────────────
        ShowWaveEndText();

        // ── 2. DESPAWN ANIMADO DE ENEMIGOS ───────────────────────────────
        if (enemySpawner != null)
            enemySpawner.DespawnAllEnemiesAnimated(despawnDelay);

        // ── 3. CURACIÓN PROGRESIVA DEL JUGADOR ───────────────────────────
        if (player != null)
            player.HealProgressive(healDuration);

        // ── 4. ESPERAR A QUE TERMINEN LAS ANIMACIONES ────────────────────
        yield return new WaitForSeconds(textDisplayTime);

        // ── 5. OCULTAR TEXTO ─────────────────────────────────────────────
        HideWaveEndText();

        // ── 6. ESPERAR EL TIEMPO RESTANTE ANTES DE LA SIGUIENTE OLEADA ───
        float remaining = Mathf.Max(0f, nextWaveDelay - textDisplayTime);
        yield return new WaitForSeconds(remaining);

        // ── 7. AVANZAR A LA SIGUIENTE OLEADA ─────────────────────────────
        if (WaveSystemRogueLite.Instance != null)
            WaveSystemRogueLite.Instance.AdvanceToNextWave();

        _transitioning = false;
        if (debugMode) Debug.Log("[WaveTransition] Transición completada.");
    }

    // ─────────────────────────────────────────────────────────────────────
    // ANIMACIONES DEL TEXTO (DOTween)
    // ─────────────────────────────────────────────────────────────────────
    void ShowWaveEndText()
    {
        if (waveEndText == null) return;

        waveEndText.gameObject.SetActive(true);
        waveEndText.text = "Oleada Terminada";
        waveEndText.color = new Color(textColor.r, textColor.g, textColor.b, 0f);
        waveEndText.transform.localScale = Vector3.one * textStartScale;

        // Fade in + pop scale en paralelo
        var seq = DOTween.Sequence();
        seq.Append(waveEndText.DOFade(1f, 0.35f));
        seq.Join(waveEndText.transform.DOScale(Vector3.one, 0.35f).SetEase(Ease.OutBack));

        // Pequeño "latido" del texto mientras se muestra
        seq.Append(waveEndText.transform.DOScale(Vector3.one * 1.08f, 0.4f).SetEase(Ease.InOutSine));
        seq.Append(waveEndText.transform.DOScale(Vector3.one, 0.4f).SetEase(Ease.InOutSine));
    }

    void HideWaveEndText()
    {
        if (waveEndText == null) return;

        // Fade out + shrink
        var seq = DOTween.Sequence();
        seq.Append(waveEndText.DOFade(0f, 0.3f));
        seq.Join(waveEndText.transform.DOScale(Vector3.one * textStartScale, 0.3f).SetEase(Ease.InBack));
        seq.OnComplete(() =>
        {
            if (waveEndText != null)
                waveEndText.gameObject.SetActive(false);
        });
    }
}
