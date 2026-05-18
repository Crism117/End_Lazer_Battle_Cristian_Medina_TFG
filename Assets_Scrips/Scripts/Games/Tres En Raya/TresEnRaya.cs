using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class TresEnRaya : MonoBehaviour
{
    [Header("===== CASILLAS =====")]
    public Button[] casillas;
    public Image[] imagenesCasillas;

    [Header("===== FICHAS =====")]
    public Sprite spriteX;
    public Sprite spriteO;

    [Header("===== MARCADOR =====")]
    public Image imgPuntosRed;
    public Image imgPuntosBlue;
    public Sprite[] spritesNumerosRed;
    public Sprite[] spritesNumerosBlue;

    [Header("===== TEXTOS =====")]
    public TextMeshProUGUI textoRonda;

    [Header("===== MENÚ INTERNO =====")]
    public GameObject panelMenuInterno;
    public Button btnPvP;
    public Button btnPvIA;
    public Button btnVolverMenuPrincipal;
    public Button btnSalirPartida;

    [Header("===== PANEL JUEGO =====")]
    public GameObject panelJuego;

    [Header("===== FIN DE PARTIDA =====")]
    public GameObject panelVictoriaRed;
    public GameObject panelVictoriaBlue;
    public Button[] botonesVolverAJugar;
    public Button[] botonesIrAlMiniMenu;

    // ══════════════════════════════════════════════
    // AUDIO
    // ══════════════════════════════════════════════
    [Header("===== AUDIO SOURCES =====")]
    public AudioSource sfxSource;
    public AudioSource musicSource;
    public AudioSource victoryMusicSource;

    [Header("===== SFX CLIPS =====")]
    public AudioClip clipColocarX;
    public AudioClip clipColocarO;
    public AudioClip clipGanar;
    public AudioClip clipEmpate;
    public AudioClip clipClick;

    [Header("===== MÚSICA =====")]
    public AudioClip musicaFondo;
    public AudioClip musicaVictoriaRed;
    public AudioClip musicaVictoriaBlue;

    [Header("===== VOLÚMENES =====")]
    [Range(0f, 1f)] public float volumenSFX = 1f;
    [Range(0f, 1f)] public float volumenMusica = 0.6f;
    [Range(0f, 1f)] public float volumenVictoria = 0.9f;

    // ══════════════════════════════════════════════
    // ANIMACIONES DOTWEEN
    // ══════════════════════════════════════════════
    [Header("===== ANIMACIONES =====")]
    public float fichaAnimDuration = 0.25f;
    public float fichaPeakScale = 1.25f;
    public float fichaStartRotation = -25f;

    // ══════════════════════════════════════════════
    // VARIABLES INTERNAS
    // ══════════════════════════════════════════════
    private int[] tablero = new int[9];
    private bool turnoRed = true;
    private bool modoIA = false;
    private int rondaActual = 1;
    private int victoriasRed = 0;
    private int victoriasBlue = 0;
    private int rondasParaGanar = 3;
    private int profundidadIA = 4;
    private bool iaAgresiva = false;
    private bool esperandoIA = false;
    private bool juegoTerminado = false;

    // FIX: colores explícitos — nunca heredar color anterior
    private Color colorVerde = new Color(0f, 1f, 0f, 1f);
    private Color colorBlanco = new Color(1f, 1f, 1f, 1f);
    private Color colorInvisible = new Color(1f, 1f, 1f, 0f); // blanco transparente

    private int[,] combinaciones = new int[,]
    {
        {0,1,2},{3,4,5},{6,7,8},
        {0,3,6},{1,4,7},{2,5,8},
        {0,4,8},{2,4,6}
    };

    // ══════════════════════════════════════════════
    void Awake()
    {
        // FIX: Detener TODA música de escenas anteriores
        DetenerMusicaDeOtrasEscenas();

        HacerTransparente(panelMenuInterno);
        HacerTransparente(panelJuego);
        if (panelMenuInterno != null && panelMenuInterno.transform.parent != null)
            HacerTransparente(panelMenuInterno.transform.parent.gameObject);
    }

    // ══════════════════════════════════════════════
    // FIX: Detiene cualquier AudioSource activo
    // que venga de escenas anteriores (menú principal, etc.)
    // ══════════════════════════════════════════════
    void DetenerMusicaDeOtrasEscenas()
    {
        // Busca TODOS los AudioSources activos en la escena
        // y para los que no son propios de este GameObject
        AudioSource[] todosLosSources = FindObjectsOfType<AudioSource>();
        foreach (var source in todosLosSources)
        {
            // Solo detiene los que no son de este mismo objeto
            if (source.gameObject != gameObject && source.isPlaying)
            {
                source.Stop();
            }
        }
    }

    void HacerTransparente(GameObject obj)
    {
        if (obj == null) return;
        var img = obj.GetComponent<Image>();
        if (img != null) { Color c = img.color; c.a = 0f; img.color = c; }
    }

    void Start()
    {
        // ── FIX: Aplicar TODOS los ajustes guardados ──────────────
        AplicarAjustesGuardados();

        // Iniciar música de fondo de esta escena
        IniciarMusicaFondo();

        for (int i = 0; i < casillas.Length; i++)
        {
            int idx = i;
            casillas[i].onClick.AddListener(() => ClickCasilla(idx));
            casillas[i].transition = Selectable.Transition.None;
        }

        LimpiarCasillas();

        btnPvP.onClick.AddListener(ElegirPvP);
        btnPvIA.onClick.AddListener(ElegirPvIA);
        btnVolverMenuPrincipal.onClick.AddListener(VolverAlMenuPrincipal);
        if (btnSalirPartida != null)
            btnSalirPartida.onClick.AddListener(SalirPartida);

        foreach (Button btn in botonesVolverAJugar)
            if (btn != null) btn.onClick.AddListener(VolverAJugar);

        foreach (Button btn in botonesIrAlMiniMenu)
            if (btn != null) btn.onClick.AddListener(IrAlMiniMenu);

        panelMenuInterno.SetActive(true);
        panelJuego.SetActive(false);
        OcultarPanelesDeVictoria();
        DesactivarCasillas();
    }

    // ══════════════════════════════════════════════
    // FIX: APLICAR TODOS LOS AJUSTES GUARDADOS
    // Lee exactamente lo que guardó AjustesManager
    // ══════════════════════════════════════════════
    void AplicarAjustesGuardados()
    {
        // Volumen maestro (guardado por AjustesManager via GuardarAjustes)
        float volumen = GuardarAjustes.CargarVolumen();
        AudioListener.volume = volumen;

        // Música: si está muteada, mutear el musicSource
        bool musicaActiva = GuardarAjustes.CargarMusica();
        if (musicSource != null)
            musicSource.mute = !musicaActiva;

        // Brillo (si tienes BrightnessOverlay en esta escena)
        // Se aplica automáticamente desde BrightnessOverlay.cs
    }

    // ══════════════════════════════════════════════
    // AUDIO
    // ══════════════════════════════════════════════

    void IniciarMusicaFondo()
    {
        if (musicSource == null || musicaFondo == null) return;
        musicSource.clip = musicaFondo;
        musicSource.loop = true;
        musicSource.volume = volumenMusica;

        // Aplicar mute guardado antes de reproducir
        bool musicaActiva = GuardarAjustes.CargarMusica();
        musicSource.mute = !musicaActiva;

        musicSource.Play();
    }

    void DetenerMusicaFondo()
    {
        if (musicSource != null && musicSource.isPlaying)
            musicSource.Stop();
    }

    void ReproducirVictoria(bool redGana)
    {
        DetenerMusicaFondo();
        if (victoryMusicSource == null) return;

        AudioClip clip = redGana ? musicaVictoriaRed : musicaVictoriaBlue;
        if (clip == null) return;

        victoryMusicSource.loop = false;
        victoryMusicSource.volume = volumenVictoria;
        victoryMusicSource.PlayOneShot(clip);
    }

    void ReproducirSFX(AudioClip clip)
    {
        if (sfxSource == null || clip == null) return;
        sfxSource.PlayOneShot(clip, volumenSFX);
    }

    void SonarClick() => ReproducirSFX(clipClick);
    void SonarColocarX() => ReproducirSFX(clipColocarX);
    void SonarColocarO() => ReproducirSFX(clipColocarO);
    void SonarGanar() => ReproducirSFX(clipGanar);
    void SonarEmpate() => ReproducirSFX(clipEmpate);

    // ══════════════════════════════════════════════
    // LIMPIEZA
    // ══════════════════════════════════════════════

    void LimpiarCasillas()
    {
        for (int i = 0; i < imagenesCasillas.Length; i++)
        {
            if (imagenesCasillas[i] == null) continue;

            // FIX: forzar color blanco transparente
            // Así nunca se hereda el verde del parpadeo anterior
            imagenesCasillas[i].sprite = spriteX;
            imagenesCasillas[i].color = colorInvisible; // blanco + alpha 0

            // Resetear escala y rotación de animaciones anteriores
            DOTween.Kill(imagenesCasillas[i].transform);
            imagenesCasillas[i].transform.localScale = Vector3.one;
            imagenesCasillas[i].transform.localRotation = Quaternion.identity;
        }
    }

    // ══════════════════════════════════════════════
    // MENÚ
    // ══════════════════════════════════════════════

    public void ElegirPvP()
    {
        SonarClick();
        modoIA = false;
        IniciarPartida();
    }

    public void ElegirPvIA()
    {
        SonarClick();
        modoIA = true;
        IniciarPartida();
    }

    public void VolverAlMenuPrincipal()
    {
        SonarClick();
        DetenerMusicaFondo();
        SceneManager.LoadScene("MenuPrincipal");
    }

    public void VolverAJugar()
    {
        SonarClick();
        if (victoryMusicSource != null && victoryMusicSource.isPlaying)
            victoryMusicSource.Stop();

        OcultarPanelesDeVictoria();
        IniciarMusicaFondo();
        IniciarPartida();
    }

    public void IrAlMiniMenu()
    {
        SonarClick();
        if (victoryMusicSource != null && victoryMusicSource.isPlaying)
            victoryMusicSource.Stop();

        OcultarPanelesDeVictoria();
        IniciarMusicaFondo();
        panelJuego.SetActive(false);
        panelMenuInterno.SetActive(true);
    }

    public void SalirPartida()
    {
        SonarClick();
        StopAllCoroutines();
        juegoTerminado = true;
        esperandoIA = false;

        if (victoryMusicSource != null && victoryMusicSource.isPlaying)
            victoryMusicSource.Stop();

        OcultarPanelesDeVictoria();
        IniciarMusicaFondo();
        panelJuego.SetActive(false);
        panelMenuInterno.SetActive(true);
    }

    // ══════════════════════════════════════════════
    // PARTIDA
    // ══════════════════════════════════════════════

    void IniciarPartida()
    {
        panelMenuInterno.SetActive(false);
        panelJuego.SetActive(true);
        OcultarPanelesDeVictoria();

        rondaActual = 1;
        victoriasRed = 0;
        victoriasBlue = 0;
        iaAgresiva = false;
        profundidadIA = 4;

        IniciarRonda();
    }

    void IniciarRonda()
    {
        for (int i = 0; i < 9; i++) tablero[i] = 0;

        LimpiarCasillas();
        turnoRed = true;
        juegoTerminado = false;
        esperandoIA = false;

        if (modoIA) AjustarDificultadIA();

        ActualizarMarcador();
        ActualizarUI();
        ActivarCasillas();
    }

    // ══════════════════════════════════════════════
    // GAMEPLAY
    // ══════════════════════════════════════════════

    void ClickCasilla(int idx)
    {
        if (tablero[idx] != 0 || juegoTerminado || esperandoIA) return;
        ColocarFicha(idx);
        if (VerificarFin()) return;
        turnoRed = !turnoRed;
        ActualizarUI();

        if (modoIA && !turnoRed && !juegoTerminado)
        {
            esperandoIA = true;
            StartCoroutine(TurnoIA());
        }
    }

    void ColocarFicha(int idx)
    {
        tablero[idx] = turnoRed ? 1 : 2;

        if (imagenesCasillas[idx] != null)
        {
            imagenesCasillas[idx].sprite = turnoRed ? spriteX : spriteO;

            // FIX PRINCIPAL: forzar SIEMPRE color blanco puro
            // Nunca usar el color anterior que puede ser verde
            imagenesCasillas[idx].color = colorBlanco;

            AnimarColocarFicha(imagenesCasillas[idx].transform);
        }

        if (turnoRed) SonarColocarX();
        else SonarColocarO();
    }

    void AnimarColocarFicha(Transform t)
    {
        DOTween.Kill(t);

        t.localScale = Vector3.zero;
        t.localRotation = Quaternion.Euler(0f, 0f, fichaStartRotation);

        var seq = DOTween.Sequence();
        seq.Append(t.DOScale(fichaPeakScale, fichaAnimDuration * 0.6f)
            .SetEase(Ease.OutBack));
        seq.Join(t.DOLocalRotate(Vector3.zero, fichaAnimDuration * 0.6f)
            .SetEase(Ease.OutCubic));
        seq.Append(t.DOScale(1f, fichaAnimDuration * 0.4f)
            .SetEase(Ease.InOutQuad));
    }

    // ══════════════════════════════════════════════
    // VERIFICAR FIN
    // ══════════════════════════════════════════════

    bool VerificarFin()
    {
        int linea = ObtenerLineaGanadora(tablero);

        if (linea >= 0)
        {
            juegoTerminado = true;
            if (turnoRed) victoriasRed++;
            else victoriasBlue++;

            ActualizarMarcador();
            SonarGanar();
            StartCoroutine(SecuenciaGanador(linea));
            return true;
        }

        if (TableroLleno(tablero))
        {
            juegoTerminado = true;
            SonarEmpate();
            StartCoroutine(SecuenciaEmpate());
            return true;
        }

        return false;
    }

    IEnumerator SecuenciaGanador(int linea)
    {
        DesactivarCasillas();
        yield return StartCoroutine(ParpadeoGanador(linea));
        yield return new WaitForSeconds(0.8f);

        if (victoriasRed >= rondasParaGanar || victoriasBlue >= rondasParaGanar)
        {
            MostrarFinPartida();
            yield break;
        }

        rondaActual++;
        IniciarRonda();
    }

    IEnumerator SecuenciaEmpate()
    {
        DesactivarCasillas();
        yield return new WaitForSeconds(1.5f);
        IniciarRonda();
    }

    IEnumerator ParpadeoGanador(int linea)
    {
        int a = combinaciones[linea, 0];
        int b = combinaciones[linea, 1];
        int c = combinaciones[linea, 2];

        // FIX: solo las 3 casillas ganadoras cambian a verde
        // Las demás NO se tocan
        for (int i = 0; i < 4; i++)
        {
            imagenesCasillas[a].color = colorVerde;
            imagenesCasillas[b].color = colorVerde;
            imagenesCasillas[c].color = colorVerde;
            yield return new WaitForSeconds(0.25f);

            // Volver a blanco (no transparente, la ficha sigue visible)
            imagenesCasillas[a].color = colorBlanco;
            imagenesCasillas[b].color = colorBlanco;
            imagenesCasillas[c].color = colorBlanco;
            yield return new WaitForSeconds(0.25f);
        }

        // Dejar en verde al terminar el parpadeo
        imagenesCasillas[a].color = colorVerde;
        imagenesCasillas[b].color = colorVerde;
        imagenesCasillas[c].color = colorVerde;
    }

    void MostrarFinPartida()
    {
        OcultarPanelesDeVictoria();

        if (victoriasRed >= rondasParaGanar)
        {
            if (panelVictoriaRed != null)
            {
                panelVictoriaRed.SetActive(true);
                AnimarPanelVictoria(panelVictoriaRed.transform);
            }
            ReproducirVictoria(redGana: true);
        }
        else
        {
            if (panelVictoriaBlue != null)
            {
                panelVictoriaBlue.SetActive(true);
                AnimarPanelVictoria(panelVictoriaBlue.transform);
            }
            ReproducirVictoria(redGana: false);
        }
    }

    void AnimarPanelVictoria(Transform t)
    {
        DOTween.Kill(t);
        t.localScale = Vector3.zero;
        t.DOScale(Vector3.one, 0.45f).SetEase(Ease.OutBack);
    }

    void OcultarPanelesDeVictoria()
    {
        if (panelVictoriaRed != null) panelVictoriaRed.SetActive(false);
        if (panelVictoriaBlue != null) panelVictoriaBlue.SetActive(false);
    }

    // ══════════════════════════════════════════════
    // UI
    // ══════════════════���═══════════════════════════

    void ActualizarMarcador()
    {
        if (imgPuntosRed != null && spritesNumerosRed != null)
        {
            int idx = Mathf.Clamp(victoriasRed, 0, spritesNumerosRed.Length - 1);
            imgPuntosRed.sprite = spritesNumerosRed[idx];
        }
        if (imgPuntosBlue != null && spritesNumerosBlue != null)
        {
            int idx = Mathf.Clamp(victoriasBlue, 0, spritesNumerosBlue.Length - 1);
            imgPuntosBlue.sprite = spritesNumerosBlue[idx];
        }
    }

    void ActualizarUI()
    {
        if (textoRonda != null)
            textoRonda.text = "RONDA " + rondaActual;
    }

    void ActivarCasillas() { foreach (var c in casillas) c.interactable = true; }
    void DesactivarCasillas() { foreach (var c in casillas) c.interactable = false; }

    // ══════════════════════════════════════════════
    // IA CON MINIMAX
    // ══════════════════════════════════════════════

    IEnumerator TurnoIA()
    {
        yield return new WaitForSeconds(0.6f);
        int mov = ObtenerMejorMovimiento();
        if (mov >= 0)
        {
            ColocarFicha(mov);
            if (!VerificarFin())
            {
                turnoRed = true;
                esperandoIA = false;
                ActualizarUI();
            }
        }
        esperandoIA = false;
    }

    int ObtenerMejorMovimiento()
    {
        int mejorPuntaje = int.MinValue;
        int mejorMov = -1;

        for (int i = 0; i < 9; i++)
        {
            if (tablero[i] != 0) continue;
            tablero[i] = 2;
            int puntaje = Minimax(tablero, 0, false, int.MinValue, int.MaxValue);
            tablero[i] = 0;
            if (!iaAgresiva && profundidadIA < 9)
                puntaje += Random.Range(-2, 2);
            if (puntaje > mejorPuntaje) { mejorPuntaje = puntaje; mejorMov = i; }
        }
        return mejorMov;
    }

    int Minimax(int[] tab, int prof, bool maximizando, int alfa, int beta)
    {
        int ganador = ObtenerGanador(tab);
        if (ganador == 2) return 10 - prof;
        if (ganador == 1) return prof - 10;
        if (TableroLleno(tab)) return 0;
        if (prof >= profundidadIA) return 0;

        if (maximizando)
        {
            int mejor = int.MinValue;
            for (int i = 0; i < 9; i++)
            {
                if (tab[i] != 0) continue;
                tab[i] = 2;
                int p = Minimax(tab, prof + 1, false, alfa, beta);
                tab[i] = 0;
                mejor = Mathf.Max(mejor, p);
                alfa = Mathf.Max(alfa, p);
                if (beta <= alfa) break;
            }
            return mejor;
        }
        else
        {
            int mejor = int.MaxValue;
            for (int i = 0; i < 9; i++)
            {
                if (tab[i] != 0) continue;
                tab[i] = 1;
                int p = Minimax(tab, prof + 1, true, alfa, beta);
                tab[i] = 0;
                mejor = Mathf.Min(mejor, p);
                beta = Mathf.Min(beta, p);
                if (beta <= alfa) break;
            }
            return mejor;
        }
    }

    int ObtenerGanador(int[] tab)
    {
        for (int i = 0; i < 8; i++)
        {
            int a = combinaciones[i, 0];
            int b = combinaciones[i, 1];
            int c = combinaciones[i, 2];
            if (tab[a] != 0 && tab[a] == tab[b] && tab[b] == tab[c])
                return tab[a];
        }
        return 0;
    }

    bool TableroLleno(int[] tab)
    {
        for (int i = 0; i < 9; i++) if (tab[i] == 0) return false;
        return true;
    }

    int ObtenerLineaGanadora(int[] tab)
    {
        for (int i = 0; i < 8; i++)
        {
            int a = combinaciones[i, 0];
            int b = combinaciones[i, 1];
            int c = combinaciones[i, 2];
            if (tab[a] != 0 && tab[a] == tab[b] && tab[b] == tab[c])
                return i;
        }
        return -1;
    }

    void AjustarDificultadIA()
    {
        if (victoriasRed >= 2) { iaAgresiva = true; profundidadIA = 9; }
        else if (victoriasRed >= 1) { iaAgresiva = false; profundidadIA = 6; }
        else { iaAgresiva = false; profundidadIA = 4; }
    }
}