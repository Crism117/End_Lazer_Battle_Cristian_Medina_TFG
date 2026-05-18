using UnityEngine;
using UnityEngine.Audio;

// ============================================================
// MIXER VOLUME BRIDGE
// Al iniciar la escena, lee el volumen guardado por
// AjustesManager (vía GuardarAjustes) y lo aplica al AudioMixer.
// Esto hace que el slider del MenuPrincipal afecte la escena del juego.
// ============================================================
public class MixerVolumeBridge : MonoBehaviour
{
    [Header("─── AUDIO MIXER ──")]
    [Tooltip("Mismo MainMixer que usa el AjustesManager del MenuPrincipal")]
    public AudioMixer mainMixer;

    [Header("─── PARÁMETROS EXPUESTOS ──")]
    [Tooltip("Nombre del parámetro Master expuesto en el Mixer")]
    public string masterParam = "MasterVolume";

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Start()
    {
        if (mainMixer == null)
        {
            Debug.LogWarning("[MixerVolumeBridge] MainMixer no asignado.");
            return;
        }

        float volume = LoadSavedVolume();
        SetMasterVolume(volume);

        if (debugMode)
            Debug.Log($"[MixerVolumeBridge] Volumen aplicado: {volume:F2}");
    }

    /// <summary>
    /// Convierte un slider (0..1) a dB y aplica al Mixer.
    /// </summary>
    public void SetMasterVolume(float linear)
    {
        if (mainMixer == null) return;
        // 0 = silencio (-80 dB), 1 = sin atenuación (0 dB)
        float dB = linear > 0.001f ? Mathf.Log10(linear) * 20f : -80f;
        mainMixer.SetFloat(masterParam, dB);
    }

    float LoadSavedVolume()
    {
        // Intenta usar tu clase GuardarAjustes (reflection-style)
        try
        {
            var type = System.Type.GetType("GuardarAjustes");
            if (type != null)
            {
                var method = type.GetMethod("CargarVolumen",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    object result = method.Invoke(null, null);
                    if (result is float v) return v;
                }
            }
        }
        catch { }

        // Fallback: PlayerPrefs directo
        return PlayerPrefs.GetFloat("Volumen", 1f);
    }
}
