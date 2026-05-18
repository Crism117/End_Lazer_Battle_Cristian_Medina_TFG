// ==========================================================
// GAME MANAGER - El jefe de todo el menú principal
// ==========================================================
// Controla la navegación entre pantallas:
// - Intro → Menú → MiniMenú / Opciones
//
// FUNCIONES:
// - Mouse: click en botones con hover y efecto presionado
// - Teclado: flechas arriba/abajo + Enter/Espacio
// - Fade suave entre pantallas
// - Logo con efecto gelatina
// - Videos a pantalla completa
// - CanvasGroup automático
// ==========================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("===== PANELES =====")]
    public GameObject introPanel;
    public GameObject menuPanel;
    public GameObject miniMenuPanel;
    public GameObject optionsPanel;

    [Header("===== INTRO =====")]
    public Image textoPulsarImg;
    public Image logoIntro;
    public float logoFadeDuration = 1.5f;

    [Header("===== BOTONES DEL MENÚ =====")]
    public Button btnPlay;
    public Button btnSettings;
    public Button btnExit;

    [Header("===== HOVER EFFECTS =====")]
    public BotonHoverEffect hoverPlay;
    public BotonHoverEffect hoverSettings;
    public BotonHoverEffect hoverExit;

    [Header("===== VIDEOS (Raw Images) =====")]
    public RawImage videoIntro;
    public RawImage videoMenu;
    public RawImage videoMini;
    public RawImage videoOptions;

    [Header("===== CONFIGURACIÓN =====")]
    public float fadeDuration = 0.5f;
    public float pulseSpeed = 1.5f;

    [Header("===== GELATINA DEL LOGO =====")]
    public float jellySpeed = 2f;
    public float jellyAmount = 0.03f;

    // Canvas Groups
    private CanvasGroup introCanvasGroup;
    private CanvasGroup menuCanvasGroup;
    private CanvasGroup miniMenuCanvasGroup;
    private CanvasGroup optionsCanvasGroup;

    // Control
    private bool enIntro = true;
    private bool enMenu = false;
    private bool transitando = false;

    // Navegación teclado
    private Button[] botonesMenu;
    private BotonHoverEffect[] hoversMenu;
    private int botonActual = 0;

    void Awake()
    {
        introCanvasGroup = AsegurarCanvasGroup(introPanel);
        menuCanvasGroup = AsegurarCanvasGroup(menuPanel);
        miniMenuCanvasGroup = AsegurarCanvasGroup(miniMenuPanel);
        optionsCanvasGroup = AsegurarCanvasGroup(optionsPanel);
    }

    void Start()
    {
        introPanel.SetActive(true);
        menuPanel.SetActive(false);
        miniMenuPanel.SetActive(false);
        optionsPanel.SetActive(false);

        introCanvasGroup.alpha = 1f;
        menuCanvasGroup.alpha = 0f;
        miniMenuCanvasGroup.alpha = 0f;
        optionsCanvasGroup.alpha = 0f;

        AjustarVideoFullScreen(videoIntro);
        AjustarVideoFullScreen(videoMenu);
        AjustarVideoFullScreen(videoMini);
        AjustarVideoFullScreen(videoOptions);

        botonesMenu = new Button[] { btnPlay, btnSettings, btnExit };
        hoversMenu = new BotonHoverEffect[] { hoverPlay, hoverSettings, hoverExit };

        if (logoIntro != null)
        {
            StartCoroutine(AnimarLogoFadeIn());
        }
    }

    void Update()
    {
        // ===== INTRO =====
        if (enIntro)
        {
            if (textoPulsarImg != null)
            {
                float alpha = Mathf.PingPong(Time.time * pulseSpeed, 1f);
                Color c = textoPulsarImg.color;
                c.a = alpha;
                textoPulsarImg.color = c;
            }

            if (logoIntro != null)
            {
                float scaleX = 1f + Mathf.Sin(Time.time * jellySpeed) * jellyAmount;
                float scaleY = 1f - Mathf.Sin(Time.time * jellySpeed) * jellyAmount;
                logoIntro.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }

            if (!transitando)
            {
                if (Input.GetMouseButtonDown(0) || Input.anyKeyDown)
                {
                    enIntro = false;
                    StartCoroutine(IrAlMenu());
                }
            }
        }

        // ===== MENÚ - TECLADO =====
        if (enMenu && !transitando)
        {
            if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
            {
                CambiarBoton(1);
            }
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
            {
                CambiarBoton(-1);
            }
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
            {
                PressBotonActual();
            }
        }
    }

    // ==================== NAVEGACIÓN TECLADO ====================

    private void CambiarBoton(int direccion)
    {
        // Quitar hover del actual
        if (hoversMenu[botonActual] != null)
            hoversMenu[botonActual].SimularNormal();

        botonActual += direccion;
        if (botonActual < 0) botonActual = botonesMenu.Length - 1;
        if (botonActual >= botonesMenu.Length) botonActual = 0;

        // Poner hover en el nuevo
        if (hoversMenu[botonActual] != null)
            hoversMenu[botonActual].SimularHover();
    }

    private void PressBotonActual()
    {
        // Efecto de click
        if (hoversMenu[botonActual] != null)
            hoversMenu[botonActual].SimularClick();

        // Ejecutar la acción del botón
        if (botonesMenu[botonActual] != null)
            botonesMenu[botonActual].onClick.Invoke();
    }

    private void SeleccionarPrimerBoton()
    {
        botonActual = 0;
        for (int i = 0; i < hoversMenu.Length; i++)
        {
            if (hoversMenu[i] != null)
                hoversMenu[i].SimularNormal();
        }
        if (hoversMenu[0] != null)
            hoversMenu[0].SimularHover();
    }

    // ==================== BOTONES ====================

    public void OnClickPlay()
    {
        Debug.Log(">>> JUGAR presionado");
        if (!transitando)
        {
            enMenu = false;
            StartCoroutine(CambiarPanel(menuPanel, menuCanvasGroup, miniMenuPanel, miniMenuCanvasGroup));
        }
    }

    public void OnClickSettings()
    {
        Debug.Log(">>> AJUSTES presionado");
        if (!transitando)
        {
            enMenu = false;
            StartCoroutine(CambiarPanel(menuPanel, menuCanvasGroup, optionsPanel, optionsCanvasGroup));
        }
    }

    public void OnClickExit()
    {
        Debug.Log(">>> SALIR presionado");
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    public void OnClickBack()
    {
        Debug.Log(">>> ATRÁS presionado");
        if (!transitando)
            StartCoroutine(IrAlMenu());
    }

    public void OnClickVolverMenu()
    {
        Debug.Log(">>> VOLVER AL MENÚ presionado");
        if (!transitando)
            StartCoroutine(IrAlMenu());
    }

    // ==================== TRANSICIONES ====================

    private IEnumerator IrAlMenu()
    {
        if (introPanel.activeSelf)
            yield return StartCoroutine(CambiarPanel(introPanel, introCanvasGroup, menuPanel, menuCanvasGroup));
        else if (miniMenuPanel.activeSelf)
            yield return StartCoroutine(CambiarPanel(miniMenuPanel, miniMenuCanvasGroup, menuPanel, menuCanvasGroup));
        else if (optionsPanel.activeSelf)
            yield return StartCoroutine(CambiarPanel(optionsPanel, optionsCanvasGroup, menuPanel, menuCanvasGroup));

        enMenu = true;
        SeleccionarPrimerBoton();
    }

    // ==================== ANIMACIÓN LOGO ====================

    private IEnumerator AnimarLogoFadeIn()
    {
        Color c = logoIntro.color;
        c.a = 0f;
        logoIntro.color = c;

        float timer = 0f;
        while (timer < logoFadeDuration)
        {
            timer += Time.deltaTime;
            c.a = timer / logoFadeDuration;
            logoIntro.color = c;
            yield return null;
        }

        c.a = 1f;
        logoIntro.color = c;
    }

    // ==================== FADE ====================

    private IEnumerator CambiarPanel(GameObject panelSalir, CanvasGroup cgSalir, GameObject panelEntrar, CanvasGroup cgEntrar)
    {
        transitando = true;

        panelEntrar.SetActive(true);
        cgEntrar.alpha = 0f;

        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = timer / fadeDuration;
            cgSalir.alpha = 1f - t;
            cgEntrar.alpha = t;
            yield return null;
        }

        cgSalir.alpha = 0f;
        cgEntrar.alpha = 1f;
        panelSalir.SetActive(false);

        transitando = false;
    }

    // ==================== UTILIDADES ====================

    private CanvasGroup AsegurarCanvasGroup(GameObject panel)
    {
        if (panel == null) return null;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    private void AjustarVideoFullScreen(RawImage video)
    {
        if (video == null) return;
        RectTransform rt = video.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }
}
