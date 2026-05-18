using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;
// FIX: Eliminé la librería del Nuevo Input System para usar el clásico

// ============================================================
// MENÚ INTERNO ROGUELITE
// Controla la pantalla de inicio, la cuenta atrás de 3 segundos
// y el menú de pausa del mini-juego Roguelite.
// ============================================================
public class MenuInternoRogueLite : MonoBehaviour
{
    [Header("===== PANELES =====")]
    [Tooltip("El panel gigante que tapa todo al entrar a la escena")]
    public GameObject panelMenuInterno;

    [Tooltip("El panel que muestra los números 3, 2, 1")]
    public GameObject panelContador;

    [Tooltip("El panel que sale cuando le das a Escape")]
    public GameObject panelPausa;

    [Header("===== TEXTOS =====")]
    [Tooltip("El texto gigante que cambiará de 3 a 1")]
    public TextMeshProUGUI textoContador;

    [Header("===== BOTONES DEL MENÚ INTERNO =====")]
    public Button btnJugar;
    public Button btnVolverMenuUniversal;

    [Header("===== BOTONES DE PAUSA =====")]
    public Button btnReanudar;
    public Button btnVolverMenuInterno;

    [Header("===== AUDIO (Opcional) =====")]
    public AudioSource sfxSource;
    public AudioClip clipClick;
    public AudioClip clipCuentaAtras; // Sonido tipo "bip" para los números
    public AudioClip clipYa;          // Sonido cuando dice "¡YA!"

    // Variable para saber si el juego ya empezó (y así poder pausar)
    private bool juegoIniciado = false;

    void Start()
    {
        // Conectamos los clics de los botones con sus funciones correspondientes
        btnJugar.onClick.AddListener(EmpezarCuentaAtras);
        btnVolverMenuUniversal.onClick.AddListener(IrMenuUniversal);

        btnReanudar.onClick.AddListener(ReanudarJuego);
        btnVolverMenuInterno.onClick.AddListener(ReiniciarNivelAlMenu);

        // Estado inicial: Mostramos el menú y congelamos el tiempo del juego
        panelMenuInterno.SetActive(true);
        panelContador.SetActive(false);
        panelPausa.SetActive(false);

        Time.timeScale = 0f; // El juego está "congelado" detrás del menú
        juegoIniciado = false;
    }

    void Update()
    {
        // FIX: Usamos el sistema de Input ANTIGUO para detectar la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Solo podemos pausar si el juego ya empezó y no estamos en otro menú
            if (juegoIniciado)
            {
                // Si la pausa está activa, reanudamos. Si no, pausamos.
                if (panelPausa.activeSelf)
                {
                    ReanudarJuego();
                }
                else
                {
                    PausarJuego();
                }
            }
        }
    }

    // ==================== FUNCIONES DE LOS BOTONES ====================

    // Se llama al pulsar "Jugar"
    void EmpezarCuentaAtras()
    {
        SonarClick();

        // Apagamos el menú interno y encendemos el cartel del contador
        panelMenuInterno.SetActive(false);
        panelContador.SetActive(true);

        // Iniciamos la corrutina (función en el tiempo) de la cuenta atrás
        StartCoroutine(RutinaContador());
    }

    // Se llama al pulsar "Volver al Menú Universal"
    void IrMenuUniversal()
    {
        SonarClick();
        Time.timeScale = 1f; // Descongelamos el tiempo por seguridad antes de salir
        SceneManager.LoadScene("MenuPrincipal"); // Pon el nombre exacto de tu escena principal
    }

    // Se llama al pulsar "Reanudar" en la pausa
    void ReanudarJuego()
    {
        SonarClick();
        panelPausa.SetActive(false); // Ocultamos la pausa
        Time.timeScale = 1f;         // El tiempo vuelve a correr normal
    }

    // Se llama al pulsar "Volver al panel interno" desde la pausa
    void ReiniciarNivelAlMenu()
    {
        SonarClick();
        Time.timeScale = 1f; // Descongelamos el tiempo
        // Recargamos la escena actual. 
        // Como en el Start() activamos el panel interno, volverá a verse como al inicio.
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Activa la pantalla de pausa
    void PausarJuego()
    {
        panelPausa.SetActive(true); // Mostramos el menú de pausa
        Time.timeScale = 0f;        // Congelamos todo (enemigos, proyectiles, etc.)
    }

    // ==================== RUTINA DE CUENTA ATRÁS ====================

    IEnumerator RutinaContador()
    {
        // IMPORTANTE: Usamos WaitForSecondsRealtime porque Time.timeScale está en 0

        textoContador.text = "3";
        if (sfxSource && clipCuentaAtras) sfxSource.PlayOneShot(clipCuentaAtras);
        yield return new WaitForSecondsRealtime(1f); // Espera 1 segundo real

        textoContador.text = "2";
        if (sfxSource && clipCuentaAtras) sfxSource.PlayOneShot(clipCuentaAtras);
        yield return new WaitForSecondsRealtime(1f);

        textoContador.text = "1";
        if (sfxSource && clipCuentaAtras) sfxSource.PlayOneShot(clipCuentaAtras);
        yield return new WaitForSecondsRealtime(1f);

        textoContador.text = "¡GO!";
        if (sfxSource && clipYa) sfxSource.PlayOneShot(clipYa);
        yield return new WaitForSecondsRealtime(0.5f); // Se muestra el GO medio segundo

        // Terminó la cuenta: Apagamos el panel y arrancamos el juego
        panelContador.SetActive(false);
        juegoIniciado = true;
        Time.timeScale = 1f; // ¡El juego empieza a moverse!
    }

    // Reproduce un sonido de interfaz simple
    void SonarClick()
    {
        if (sfxSource != null && clipClick != null)
        {
            // Reproduce el sonido incluso si el tiempo está congelado
            sfxSource.PlayOneShot(clipClick);
        }
    }
}
