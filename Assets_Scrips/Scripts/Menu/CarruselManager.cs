// ==========================================================
// CARRUSEL MANAGER - Baraja con tarjetas en los extremos
// ==========================================================
// Las tarjetas del carrusel funcionan así:
//
// - La tarjeta seleccionada está en el CENTRO, grande y sólida
// - Las otras dos se van a los EXTREMOS izquierdo y derecho,
//   más chicas y translúcidas, solapándose con la del centro
// - Al cambiar de tarjeta, la que estaba en un extremo
//   pasa al centro con animación suave
//
// IMPORTANTE:
// - El script fuerza el color blanco con alpha 255 en todas
//   las imágenes para evitar bugs de opacidad
// - La transparencia se controla SOLO con CanvasGroup
// - Así nunca se queda una tarjeta más transparente que otra
// ==========================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CarruselManager : MonoBehaviour
{
    [Header("===== TARJETAS =====")]
    public RectTransform[] tarjetas;

    [Header("===== BOTONES =====")]
    public Button btnLeft;
    public Button btnRight;

    [Header("===== POSICIÓN =====")]
    public float posicionExtremo = 250f;

    [Header("===== ESCALA =====")]
    public float escalaFrente = 1f;
    public float escalaAtras = 0.85f;

    [Header("===== TRANSPARENCIA =====")]
    public float alphaFrente = 1f;
    public float alphaAtras = 0.35f;

    [Header("===== ANIMACIÓN =====")]
    public float velocidadAnimacion = 7f;

    // Control interno
    private int tarjetaActual = 0;
    private bool animando = false;
    private bool activo = false;

    // CanvasGroups
    private CanvasGroup[] canvasGroups;

    // Objetivos de animación
    private float[] escalasObjetivo;
    private float[] alphasObjetivo;
    private Vector2[] posicionesObjetivo;

    void Start()
    {
        if (btnLeft != null)
            btnLeft.onClick.AddListener(IrIzquierda);
        if (btnRight != null)
            btnRight.onClick.AddListener(IrDerecha);

        // Inicializar arrays
        escalasObjetivo = new float[tarjetas.Length];
        alphasObjetivo = new float[tarjetas.Length];
        posicionesObjetivo = new Vector2[tarjetas.Length];
        canvasGroups = new CanvasGroup[tarjetas.Length];

        for (int i = 0; i < tarjetas.Length; i++)
        {
            // Asegurar CanvasGroup
            canvasGroups[i] = tarjetas[i].GetComponent<CanvasGroup>();
            if (canvasGroups[i] == null)
                canvasGroups[i] = tarjetas[i].gameObject.AddComponent<CanvasGroup>();

            // FORZAR alpha 1 en el CanvasGroup antes de empezar
            canvasGroups[i].alpha = 1f;

            // FORZAR color blanco con alpha 255 en TODAS las imágenes
            // Esto arregla el bug donde la tarjeta 1 o 2 se veían
            // más transparentes porque tenían el alpha del color bajo
            Image img = tarjetas[i].GetComponent<Image>();
            if (img != null)
                img.color = new Color(1f, 1f, 1f, 1f);

            Image[] hijas = tarjetas[i].GetComponentsInChildren<Image>();
            for (int j = 0; j < hijas.Length; j++)
                hijas[j].color = new Color(1f, 1f, 1f, 1f);
        }

        tarjetaActual = 0;
        AplicarEstado(true);
    }

    void Update()
    {
        if (!activo) return;

        if (!animando)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                IrIzquierda();

            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                IrDerecha();
        }

        AnimarTarjetas();
    }

    void OnEnable()
    {
        activo = true;
        if (tarjetas != null && tarjetas.Length > 0 && canvasGroups != null)
        {
            tarjetaActual = 0;
            AplicarEstado(true);
        }
    }

    void OnDisable()
    {
        activo = false;
    }

    // ==================== NAVEGACIÓN ====================

    public void IrIzquierda()
    {
        if (animando) return;
        tarjetaActual--;
        if (tarjetaActual < 0) tarjetaActual = tarjetas.Length - 1;
        AplicarEstado(false);
    }

    public void IrDerecha()
    {
        if (animando) return;
        tarjetaActual++;
        if (tarjetaActual >= tarjetas.Length) tarjetaActual = 0;
        AplicarEstado(false);
    }

    // ==================== ESTADO ====================

    private void AplicarEstado(bool instantaneo)
    {
        // Contar cuántas tarjetas van a la izquierda y cuántas a la derecha
        int contadorIzquierda = 0;
        int contadorDerecha = 0;

        // Primero calcular posiciones para cada tarjeta
        for (int i = 0; i < tarjetas.Length; i++)
        {
            if (i == tarjetaActual)
            {
                // ===== TARJETA DEL CENTRO =====
                // Grande, sólida, centrada, encima de todas
                escalasObjetivo[i] = escalaFrente;
                alphasObjetivo[i] = alphaFrente;
                posicionesObjetivo[i] = Vector2.zero;

                // Se dibuja encima de todas (índice más alto)
                tarjetas[i].SetSiblingIndex(tarjetas.Length);
            }
            else
            {
                // ===== TARJETAS DE LOS EXTREMOS =====
                // Más chicas, translúcidas, a los lados
                escalasObjetivo[i] = escalaAtras;
                alphasObjetivo[i] = alphaAtras;

                // Determinar si va a la izquierda o derecha
                bool izquierda = EstaALaIzquierda(i, tarjetaActual, tarjetas.Length);

                if (izquierda)
                {
                    posicionesObjetivo[i] = new Vector2(-posicionExtremo, 0f);
                    contadorIzquierda++;
                }
                else
                {
                    posicionesObjetivo[i] = new Vector2(posicionExtremo, 0f);
                    contadorDerecha++;
                }

                // Se dibuja detrás (índice bajo)
                tarjetas[i].SetSiblingIndex(0);
            }

            // Todas las tarjetas dejan pasar clicks a sus hijos (botones)
            // Pero solo la del centro permite interactuar
            canvasGroups[i].blocksRaycasts = true;
            canvasGroups[i].interactable = (i == tarjetaActual);

            if (instantaneo)
            {
                tarjetas[i].anchoredPosition = posicionesObjetivo[i];
                tarjetas[i].localScale = Vector3.one * escalasObjetivo[i];
                canvasGroups[i].alpha = alphasObjetivo[i];
            }
        }

        if (!instantaneo)
            StartCoroutine(EsperarAnimacion());
    }

    // ==================== ANIMACIÓN ====================

    private void AnimarTarjetas()
    {
        float lerp = Time.deltaTime * velocidadAnimacion;

        for (int i = 0; i < tarjetas.Length; i++)
        {
            // Posición suave hacia el extremo o el centro
            tarjetas[i].anchoredPosition = Vector2.Lerp(
                tarjetas[i].anchoredPosition,
                posicionesObjetivo[i],
                lerp
            );

            // Escala suave
            float escalaActual = tarjetas[i].localScale.x;
            float nuevaEscala = Mathf.Lerp(escalaActual, escalasObjetivo[i], lerp);
            tarjetas[i].localScale = Vector3.one * nuevaEscala;

            // Transparencia suave
            // ESTO es lo que hace que cambien de opacidad
            // La del centro va a alphaFrente (1.0 = sólida)
            // Las de los lados van a alphaAtras (0.35 = translúcida)
            canvasGroups[i].alpha = Mathf.Lerp(
                canvasGroups[i].alpha,
                alphasObjetivo[i],
                lerp
            );
        }
    }

    private IEnumerator EsperarAnimacion()
    {
        animando = true;
        yield return new WaitForSeconds(0.4f);
        animando = false;
    }

    // ==================== UTILIDADES ====================

    private bool EstaALaIzquierda(int indice, int actual, int total)
    {
        int diff = indice - actual;
        if (diff > total / 2) diff -= total;
        if (diff < -total / 2) diff += total;
        return diff < 0;
    }

    public int GetTarjetaActual()
    {
        return tarjetaActual;
    }
}
