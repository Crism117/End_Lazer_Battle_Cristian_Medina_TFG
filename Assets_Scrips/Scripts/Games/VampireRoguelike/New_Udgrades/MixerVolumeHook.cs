using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

// ============================================================
// MIXER VOLUME HOOK
// Se añade al GameObject del sliderVolumen del MenuPrincipal.
// Aplica el valor del slider al MainMixer (en dB), en paralelo
// a tu AjustesManager (que sigue funcionando con AudioListener).
//
// Ventaja: no toca tu AjustesManager y el efecto persiste en
// la escena del Roguelite (el Mixer es un asset compartido).
// ============================================================
[RequireComponent(typeof(Slider))]
public class MixerVolumeHook : MonoBehaviour
{
    [Header("─── REFERENCIA ──")]
    [Tooltip("Si lo dejas vacío, se busca en el propio GameObject")]
    public Slider sliderVolumen;

    [Header("─── AUDIO MIXER ──")]
    [Tooltip("Mismo MainMixer que se usa en la escena del Roguelite")]
    public AudioMixer mainMixer;

    [Tooltip("Nombre del parámetro Master expuesto")]
    public string masterParam = "MasterVolume";

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    void Awake()
    {
        if (sliderVolumen == null) sliderVolumen = GetComponent<Slider>();
    }

    void Start()
    {
        if (sliderVolumen == null)
        {
            Debug.LogWarning("[MixerVolumeHook] No hay Slider asignado.");
            return;
        }

        if (mainMixer == null)
        {
            Debug.LogWarning("[MixerVolumeHook] MainMixer no asignado.");
            return;
        }

        // Aplicar valor inicial
        ApplyToMixer(sliderVolumen.value);

        // Suscribirse a cambios del slider
        sliderVolumen.onValueChanged.AddListener(ApplyToMixer);
    }

    void OnDestroy()
    {
        if (sliderVolumen != null)
            sliderVolumen.onValueChanged.RemoveListener(ApplyToMixer);
    }

    void ApplyToMixer(float linear)
    {
        if (mainMixer == null) return;

        // Conversión slider (0..1) → dB (-80 a 0)
        float dB = linear > 0.001f ? Mathf.Log10(linear) * 20f : -80f;
        mainMixer.SetFloat(masterParam, dB);

        if (debugMode)
            Debug.Log($"[MixerVolumeHook] Slider: {linear:F2} → {dB:F1} dB");
    }
}
