using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

// ============================================================
// APLICAR AJUSTES EN ESCENA (v13)
// FIX iter 13: enfoque MUY SIMPLE:
//   - AudioListener.volume = volumen guardado
//   - soundManager.musicSource.mute = !musica
// ============================================================
public class AplicarAjustesEnEscena : MonoBehaviour
{
    [Header("─── SOUND MANAGER ──")]
    [Tooltip("CRÍTICO: drag el SoundManager de esta escena. " +
             "El script muteará directamente su musicSource.")]
    public SoundManager soundManager;

    [Header("─── BRILLO ──")]
    [Tooltip("Image semi-transparente que cubre la pantalla")]
    public Image panelOscuro;
    [Range(0.3f, 0.9f)]
    public float maxOscuridad = 0.7f;

    [Header("─── CRT ──")]
    public UniversalRendererData rendererData;
    public GameObject volumeObject;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Start()
    {
        AplicarAjustesGuardados();
    }

    /// <summary>
    /// Lee y aplica TODOS los ajustes guardados. Misma lógica
    /// que TresEnRaya.AplicarAjustesGuardados().
    /// </summary>
    public void AplicarAjustesGuardados()
    {
        // ── VOLUMEN MAESTRO ──
        // Afecta todo el audio (incluido el AudioMixer)
        float volumen = GuardarAjustes.CargarVolumen();
        AudioListener.volume = volumen;

        // ── MÚSICA ──
        // Mute directo al musicSource del SoundManager (igual que TresEnRaya)
        bool musicaActiva = GuardarAjustes.CargarMusica();
        if (soundManager == null) soundManager = SoundManager.Instance;
        if (soundManager != null && soundManager.musicSource != null)
        {
            soundManager.musicSource.mute = !musicaActiva;
            if (debugMode)
                Debug.Log($"[Ajustes] musicSource muteado: {!musicaActiva}");
        }
        else
        {
            if (debugMode)
                Debug.LogWarning("[Ajustes] SoundManager o musicSource no asignados.");
        }

        // ── BRILLO ──
        float brillo = GuardarAjustes.CargarBrillo();
        if (panelOscuro == null)
        {
            var go = GameObject.Find("PanelOscuro");
            if (go != null) panelOscuro = go.GetComponent<Image>();
        }
        if (panelOscuro != null)
        {
            Color c = panelOscuro.color;
            c.a = (1f - brillo) * maxOscuridad;
            panelOscuro.color = c;
        }

        // ── CRT ──
        bool crt = GuardarAjustes.CargarCRT();
        if (rendererData != null)
        {
            for (int i = 0; i < rendererData.rendererFeatures.Count; i++)
            {
                if (rendererData.rendererFeatures[i] != null)
                    rendererData.rendererFeatures[i].SetActive(crt);
            }
            rendererData.SetDirty();
        }
        if (volumeObject != null)
            volumeObject.SetActive(crt);

        if (debugMode)
            Debug.Log($"[Ajustes] APLICADOS — vol:{volumen:F2} musica:{musicaActiva} brillo:{brillo:F2} CRT:{crt}");
    }

    /// <summary>
    /// Permite reaplicar desde fuera.
    /// </summary>
    public void Reapply() => AplicarAjustesGuardados();
}