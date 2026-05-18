using UnityEngine;
using System;
using System.Collections.Generic;

// ============================================================
// HIGH SCORE MANAGER (singleton)
// Guarda y carga las puntuaciones más altas usando PlayerPrefs
// (formato JSON). Persiste entre sesiones.
// ============================================================

[Serializable]
public class HighScoreEntry
{
    public string playerName;
    public int score;
    public int maxWave;
    public string date;       // ISO format

    public HighScoreEntry() { }

    public HighScoreEntry(string name, int score, int wave)
    {
        this.playerName = name;
        this.score = score;
        this.maxWave = wave;
        this.date = DateTime.Now.ToString("yyyy-MM-dd");
    }
}

[Serializable]
class HighScoreData
{
    public List<HighScoreEntry> scores = new List<HighScoreEntry>();
}

public class HighScoreManager : MonoBehaviour
{
    public static HighScoreManager Instance { get; private set; }

    [Header("─── CONFIGURACIÓN ──")]
    [Tooltip("Máximo de scores guardados (más antiguos se eliminan)")]
    public int maxScoresSaved = 50;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    const string PREFS_KEY = "EndLazerBattle_HighScores";

    HighScoreData _data = new HighScoreData();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Load();
    }

    // ─────────────────────────────────────────────────────────────────────
    // API
    // ─────────────────────────────────────────────────────────────────────
    public void AddScore(string playerName, int score, int maxWave)
    {
        if (string.IsNullOrEmpty(playerName)) playerName = "???";

        var entry = new HighScoreEntry(playerName, score, maxWave);
        _data.scores.Add(entry);

        // Ordenar descendente por score
        _data.scores.Sort((a, b) => b.score.CompareTo(a.score));

        // Limitar a max
        if (_data.scores.Count > maxScoresSaved)
            _data.scores.RemoveRange(maxScoresSaved, _data.scores.Count - maxScoresSaved);

        Save();

        if (debugMode)
            Debug.Log($"[HighScores] Guardado: {playerName} - {score} (oleada {maxWave})");
    }

    public List<HighScoreEntry> GetAllScores()
    {
        return new List<HighScoreEntry>(_data.scores);
    }

    public List<HighScoreEntry> GetTopScores(int count)
    {
        var result = new List<HighScoreEntry>();
        for (int i = 0; i < Mathf.Min(count, _data.scores.Count); i++)
            result.Add(_data.scores[i]);
        return result;
    }

    public void ClearAll()
    {
        _data.scores.Clear();
        Save();
    }

    public bool HasScores() => _data.scores.Count > 0;

    // ─────────────────────────────────────────────────────────────────────
    // PERSISTENCIA
    // ─────────────────────────────────────────────────────────────────────
    void Save()
    {
        string json = JsonUtility.ToJson(_data);
        PlayerPrefs.SetString(PREFS_KEY, json);
        PlayerPrefs.Save();
    }

    void Load()
    {
        if (!PlayerPrefs.HasKey(PREFS_KEY))
        {
            _data = new HighScoreData();
            return;
        }

        string json = PlayerPrefs.GetString(PREFS_KEY);
        try
        {
            _data = JsonUtility.FromJson<HighScoreData>(json);
            if (_data == null) _data = new HighScoreData();
            if (_data.scores == null) _data.scores = new List<HighScoreEntry>();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[HighScores] Error cargando: {e.Message}. Reseteando.");
            _data = new HighScoreData();
        }

        if (debugMode)
            Debug.Log($"[HighScores] Cargados {_data.scores.Count} scores.");
    }
}
