using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

// ============================================================
// HIGH SCORE PANEL
// Panel scrollable que muestra los mejores scores guardados.
// Se abre desde el botón del Start Menu.
// ============================================================
public class HighScorePanel : MonoBehaviour
{
    [Header("─── PREFAB Y CONTAINER ──")]
    [Tooltip("Prefab de cada fila (HighScoreEntryUI)")]
    public GameObject entryPrefab;

    [Tooltip("Content del ScrollView (con VerticalLayoutGroup)")]
    public Transform entriesContainer;

    [Header("─── BOTÓN CERRAR ──")]
    public Button btnClose;

    [Header("─── EMPTY STATE ──")]
    [Tooltip("Texto/objeto que se muestra si no hay scores")]
    public GameObject emptyStateObject;

    [Header("─── ANIMACIÓN ──")]
    public float openDuration = 0.3f;

    void Awake()
    {
        if (btnClose != null) btnClose.onClick.AddListener(Close);
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Abre el panel y refresca la lista.
    /// </summary>
    public void Open()
    {
        SoundManager.Instance?.PlayUIClick();
        gameObject.SetActive(true);
        Refresh();

        // Animación de entrada
        transform.localScale = Vector3.one * 0.8f;
        transform.DOScale(Vector3.one, openDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void Close()
    {
        SoundManager.Instance?.PlayUIClick();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Repuebla la lista con los scores actuales.
    /// </summary>
    public void Refresh()
    {
        // Limpiar
        if (entriesContainer != null)
        {
            for (int i = entriesContainer.childCount - 1; i >= 0; i--)
                Destroy(entriesContainer.GetChild(i).gameObject);
        }

        if (HighScoreManager.Instance == null)
        {
            ShowEmpty(true);
            return;
        }

        var scores = HighScoreManager.Instance.GetAllScores();

        if (scores.Count == 0)
        {
            ShowEmpty(true);
            return;
        }

        ShowEmpty(false);

        // Crear filas
        for (int i = 0; i < scores.Count; i++)
        {
            if (entryPrefab == null || entriesContainer == null) break;

            var go = Instantiate(entryPrefab, entriesContainer);
            var ui = go.GetComponent<HighScoreEntryUI>();
            if (ui != null)
                ui.SetData(i + 1, scores[i]);
        }
    }

    void ShowEmpty(bool show)
    {
        if (emptyStateObject != null) emptyStateObject.SetActive(show);
    }
}
