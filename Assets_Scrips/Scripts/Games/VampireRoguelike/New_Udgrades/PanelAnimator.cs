using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

// ============================================================
// PANEL ANIMATOR (helper estático)
// Animaciones suaves de paneles con DOTween.
// Funciona con Time.timeScale = 0 (usa SetUpdate(true)).
//
// Uso:
//   PanelAnimator.AnimateOpen(myPanel);
//   PanelAnimator.AnimateClose(myPanel, () => Debug.Log("Cerrado"));
// ============================================================
public static class PanelAnimator
{
    /// <summary>
    /// Activa el panel y lo abre con animación scale + fade.
    /// </summary>
    public static void AnimateOpen(GameObject panel, float duration = 0.4f, Action onComplete = null)
    {
        if (panel == null) return;

        panel.SetActive(true);

        // Escala desde 0 a 1 con bounce
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(Vector3.one, duration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true)
            .OnComplete(() => onComplete?.Invoke());

        // Fade del CanvasGroup si existe
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.DOFade(1f, duration * 0.6f).SetUpdate(true);
    }

    /// <summary>
    /// Cierra el panel con animación y lo desactiva al final.
    /// </summary>
    public static void AnimateClose(GameObject panel, float duration = 0.25f, Action onComplete = null)
    {
        if (panel == null) return;
        if (!panel.activeSelf) { onComplete?.Invoke(); return; }

        var seq = DOTween.Sequence().SetUpdate(true);
        seq.Append(panel.transform.DOScale(Vector3.zero, duration).SetEase(Ease.InBack));

        var cg = panel.GetComponent<CanvasGroup>();
        if (cg != null) seq.Join(cg.DOFade(0f, duration));

        seq.OnComplete(() =>
        {
            panel.SetActive(false);
            // Reset escala para próxima apertura
            panel.transform.localScale = Vector3.one;
            if (cg != null) cg.alpha = 1f;
            onComplete?.Invoke();
        });
    }
}
