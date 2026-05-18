using UnityEngine;
using UnityEngine.UI;

// ============================================================
// AJUSTES MANAGER
// - AudioSource de música: solo suena en el menú principal
// - AudioSource de SFX: para botones
// - El toggle de música mutea/desmutea en tiempo real
// - Al cambiar de escena la música para sola (no es DontDestroyOnLoad)
// - Guarda la configuración entre escenas con PlayerPrefs
// ============================================================
public class AjustesManager : MonoBehaviour
{
    [Header("===== PANTALLAS =====")]
    public GameObject optionsPanel;
    public GameObject menuPanel;

    [Header("===== SLIDERS =====")]
    public Slider sliderBrillo;
    public Slider sliderVolumen;

    [Header("===== TOGGLES =====")]
    public Toggle toggleMusica;
    public Toggle toggleCRT;

    [Header("===== BRILLO =====")]
    public Image panelOscuro;
    [Range(0.3f, 0.9f)]
    public float maxOscuridad = 0.7f;

    [Header("===== ICONOS BRILLO =====")]
    public Image imgIconoBrillo;
    public Sprite iconoSol;
    public Sprite iconoLuna;
    [Range(0f, 1f)]
    public float umbralBrillo = 0.5f;

    [Header("===== ICONOS VOLUMEN =====")]
    public Image imgIconoVolumen;
    public Sprite iconoBocina;
    public Sprite iconoBocinaX;

    // ══════════════════════════════════════════════════════════
    // AUDIO — Dos AudioSources directamente en AjustesManager
    // ══════════════════��═══════════════════════════════════════
    [Header("===== AUDIO =====")]
    [Tooltip("AudioSource para la música de fondo del menú (loop)")]
    public AudioSource musicaSource;

    [Tooltip("AudioSource para sonidos de botones (SFX)")]
    public AudioSource sfxSource;

    [Tooltip("Clip de música de fondo del menú")]
    public AudioClip clipMusica;

    [Tooltip("Clip de sonido de botones")]
    public AudioClip clipClick;

    [Header("===== CRT =====")]
    public CRTEffect crtEffect;
    public GameObject volumeObject;

    // ══════════════════════════════════════════════════════════
    void Start()
    {
        // Cargar todos los ajustes guardados
        float brillo = GuardarAjustes.CargarBrillo();
        float volumen = GuardarAjustes.CargarVolumen();
        bool musica = GuardarAjustes.CargarMusica();
        bool crt = GuardarAjustes.CargarCRT();

        // ── Brillo ──────────────────────────────────────────
        if (sliderBrillo != null)
        {
            sliderBrillo.onValueChanged.AddListener(CambiarBrillo);
            sliderBrillo.value = brillo;
        }
        CambiarBrillo(brillo);

        // ── Volumen ─────────────────────────────────────────
        if (sliderVolumen != null)
        {
            sliderVolumen.onValueChanged.AddListener(CambiarVolumen);
            sliderVolumen.value = volumen;
        }
        CambiarVolumen(volumen);

        // ── Música ──────────────────────────────────────────
        if (toggleMusica != null)
        {
            toggleMusica.onValueChanged.AddListener(CambiarMusica);
            toggleMusica.isOn = musica;
        }
        // Iniciar música SIN reproducir click
        IniciarMusica(musica);

        // ── CRT ─────────────────────────────────────────────
        if (toggleCRT != null)
        {
            toggleCRT.onValueChanged.AddListener(CambiarCRT);
            toggleCRT.isOn = crt;
        }
        AplicarCRT(crt);
    }

    // ══════════════════════════════════════════════════════════
    // MÚSICA
    // ══════════════════════════════════════════════════════════

    // Solo se llama en Start para arrancar la música
    void IniciarMusica(bool encendida)
    {
        if (musicaSource == null) return;

        // Asignar clip si no está asignado
        if (clipMusica != null && musicaSource.clip == null)
            musicaSource.clip = clipMusica;

        musicaSource.loop = true;
        musicaSource.mute = !encendida;

        // Reproducir si no estaba ya sonando
        if (!musicaSource.isPlaying)
            musicaSource.Play();
    }

    // Llamado por el Toggle cuando el usuario lo cambia
    public void CambiarMusica(bool encendida)
    {
        ReproducirClick();

        if (musicaSource != null)
        {
            musicaSource.mute = !encendida;

            // Si se activa y no está sonando, reanudar
            if (encendida && !musicaSource.isPlaying)
                musicaSource.Play();
        }

        // Guardar estado
        GuardarAjustes.GuardarMusica(encendida);
    }

    // ══════════════════════════════════════════════════════════
    // BRILLO
    // ══════════════════════════════════════════════════════════
    public void CambiarBrillo(float valor)
    {
        if (panelOscuro != null)
        {
            Color c = panelOscuro.color;
            c.a = (1f - valor) * maxOscuridad;
            panelOscuro.color = c;
        }

        if (imgIconoBrillo != null)
            imgIconoBrillo.sprite = valor >= umbralBrillo
                ? iconoSol : iconoLuna;

        GuardarAjustes.GuardarBrillo(valor);
    }

    // ══════════════════════════════════════════════════════════
    // VOLUMEN
    // ══════════════════════════════════════════════════════════
    public void CambiarVolumen(float valor)
    {
        AudioListener.volume = valor;

        if (imgIconoVolumen != null)
            imgIconoVolumen.sprite = valor > 0.01f
                ? iconoBocina : iconoBocinaX;

        GuardarAjustes.GuardarVolumen(valor);
    }

    // ══════════════════════════════════════════════════════════
    // CRT
    // ══════════════════════════════════════════════════════════
    public void CambiarCRT(bool encendido)
    {
        ReproducirClick();
        AplicarCRT(encendido);
        GuardarAjustes.GuardarCRT(encendido);
    }

    void AplicarCRT(bool encendido)
    {
        if (crtEffect != null) crtEffect.SetCRT(encendido);
        if (volumeObject != null) volumeObject.SetActive(encendido);
    }

    // ══════════════════════════════════════════════════════════
    // PANELES
    // ══════════════════════════════════════════════════════════
    public void AbrirAjustes()
    {
        ReproducirClick();
        if (optionsPanel != null) optionsPanel.SetActive(true);
        if (menuPanel != null) menuPanel.SetActive(false);
    }

    public void CerrarAjustes()
    {
        ReproducirClick();
        if (optionsPanel != null) optionsPanel.SetActive(false);
        if (menuPanel != null) menuPanel.SetActive(true);
    }

    // ══════════════════════════════════════════════════════════
    // SFX BOTONES
    // ══════════════════════════════════════════════════════════
    public void ReproducirClick()
    {
        if (sfxSource != null && clipClick != null)
            sfxSource.PlayOneShot(clipClick);
    }
}