using UnityEngine;
using TMPro;

// ============================================================
// HIGH SCORE ENTRY UI
// Va en el prefab de cada fila. Recibe los datos y los muestra.
// ============================================================
public class HighScoreEntryUI : MonoBehaviour
{
    [Header("─── REFERENCIAS UI ──")]
    public TextMeshProUGUI txtRank;
    public TextMeshProUGUI txtName;
    public TextMeshProUGUI txtScore;
    public TextMeshProUGUI txtWave;

    public void SetData(int rank, HighScoreEntry entry)
    {
        if (entry == null) return;

        if (txtRank != null) txtRank.text = $"#{rank}";
        if (txtName != null) txtName.text = entry.playerName;
        if (txtScore != null) txtScore.text = entry.score.ToString();
        if (txtWave != null) txtWave.text = $"Oleada {entry.maxWave}";
    }
}
