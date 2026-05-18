using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;

// ============================================================
// COUNTDOWN START MANAGER (v8 — FIX bug freeze)
// - Reproduce el audio robótico "3 2 1 GO" entero
// - Los números aparecen sincronizados con el audio
// - SIEMPRE usa Time.unscaledDeltaTime / WaitForSecondsRealtime
//   para no quedarse colgado si Time.timeScale = 0
// ============================================================
public class CountdownStartManager : MonoBehaviour
{
    [Header("─── REFERENCIAS UI ──")]
    public GameObject countdownPanel;
    public TextMeshProUGUI txtCountdown;

    [Header("─── DURACIONES (sincronizadas con audio) ──")]
    [Tooltip("Duración de cada número 3/2/1 en pantalla")]
    [Range(0.3f, 2f)] public float numberDuration = 1.0f;

    [Tooltip("Duración del 'GO!' en pantalla")]
    [Range(0.3f, 2f)] public float goDuration = 0.8f;

    [Header("─── COLORES ──")]
    public Color numberColor = Color.white;
    public Color goColor = new Color(0.3f, 1f, 0.3f);

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    Action _onComplete;
    bool _isRunning = false;

    /// <summary>
    /// Inicia el countdown. SIEMPRE funciona aunque Time.timeScale = 0.
    /// </summary>
    public void StartCountdown(Action onComplete)
    {
        if (_isRunning)
        {
            if (debugMode) Debug.LogWarning("[Countdown] Ya está corriendo.");
            return;
        }

        _onComplete = onComplete;
        StartCoroutine(CountdownRoutine());
    }

    IEnumerator CountdownRoutine()
    {
        _isRunning = true;

        if (countdownPanel != null) countdownPanel.SetActive(true);

        // ── REPRODUCIR AUDIO ROBÓTICO COMPLETO ─────────────────────────
        SoundManager.Instance?.PlayCountdown();

        if (debugMode) Debug.Log("[Countdown] Iniciado.");

        // ── 3, 2, 1 ────────────────────────────────────────────────────
        for (int i = 3; i >= 1; i--)
        {
            ShowText(i.ToString(), numberColor);
            yield return new WaitForSecondsRealtime(numberDuration);
        }

        // ── GO! ────────────────────────────────────────────────────────
        ShowText("GO!", goColor);
        yield return new WaitForSecondsRealtime(goDuration);

        // ── OCULTAR ────────────────────────────────────────────────────
        if (countdownPanel != null) countdownPanel.SetActive(false);

        _isRunning = false;

        if (debugMode) Debug.Log("[Countdown] Finalizado, llamando callback.");

        _onComplete?.Invoke();
    }

    void ShowText(string text, Color color)
    {
        if (txtCountdown == null) return;

        txtCountdown.text = text;
        txtCountdown.color = color;

        // Pop scale animation (con SetUpdate(true) para ignorar timeScale)
        txtCountdown.transform.localScale = Vector3.zero;
        txtCountdown.transform.DOScale(Vector3.one * 1.2f, 0.2f)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
        txtCountdown.transform.DOScale(Vector3.one, 0.15f)
            .SetEase(Ease.InQuad)
            .SetDelay(0.2f)
            .SetUpdate(true);

        // Fade in/out (también ignora timeScale)
        txtCountdown.alpha = 0f;
        txtCountdown.DOFade(1f, 0.15f).SetUpdate(true);
        txtCountdown.DOFade(0f, 0.3f).SetDelay(0.5f).SetUpdate(true);
    }
}
