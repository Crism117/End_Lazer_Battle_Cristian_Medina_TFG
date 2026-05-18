// ==========================================================
// BOTON HOVER EFFECT - Efecto visual para los botones
// ==========================================================
// Este script se pone en cada botón que tenga una imagen hija.
// Hace dos cosas:
//
// 1. HOVER (mouse encima): la imagen crece un poquito en alto
//    para que sepas que estás sobre el botón.
//
// 2. CLICK (presionar): la imagen se oscurece un momento
//    para que sepas que lo presionaste.
//
// ¿Cómo se usa?
// - Se Arrastra el script al botón (Btn_Play, Btn_Settings, etc.)
// - En el Inspector se asigna la imagen hija (Img_Play, etc.)
// ==========================================================

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BotonHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Imagen del botón (la hija)")]
    public Image imagenBoton;

    [Header("Configuración del Hover")]
    public float escalaHover = 1.1f;        // cuánto crece en alto
    public float velocidadAnimacion = 8f;    // qué tan rápido crece/encoge

    [Header("Configuración del Click")]
    public Color colorNormal = Color.white;
    public Color colorPresionado = new Color(0.6f, 0.6f, 0.6f, 1f); // gris oscuro

    // Control interno
    private Vector3 escalaOriginal;
    private Vector3 escalaObjetivo;
    private bool mouseEncima = false;

    void Start()
    {
        if (imagenBoton != null)
        {
            escalaOriginal = imagenBoton.transform.localScale;
            escalaObjetivo = escalaOriginal;
            imagenBoton.color = colorNormal;
        }
    }

    void Update()
    {
        // Mover suavemente la escala hacia el objetivo
        if (imagenBoton != null)
        {
            imagenBoton.transform.localScale = Vector3.Lerp(
                imagenBoton.transform.localScale,
                escalaObjetivo,
                Time.deltaTime * velocidadAnimacion
            );
        }
    }

    // ===== MOUSE ENCIMA → crece en alto =====
    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEncima = true;
        escalaObjetivo = new Vector3(escalaOriginal.x, escalaOriginal.y * escalaHover, escalaOriginal.z);
    }

    // ===== MOUSE SALE → vuelve a tamaño normal =====
    public void OnPointerExit(PointerEventData eventData)
    {
        mouseEncima = false;
        escalaObjetivo = escalaOriginal;
        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;
        }
    }

    // ===== CLICK (presionar) → se oscurece =====
    public void OnPointerDown(PointerEventData eventData)
    {
        if (imagenBoton != null)
        {
            imagenBoton.color = colorPresionado;
        }
    }

    // ===== SOLTAR CLICK → vuelve a color normal =====
    public void OnPointerUp(PointerEventData eventData)
    {
        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;
        }
    }

    // ===== PARA NAVEGACIÓN CON TECLADO =====
    // El GameManager puede llamar estas funciones
    public void SimularHover()
    {
        mouseEncima = true;
        escalaObjetivo = new Vector3(escalaOriginal.x, escalaOriginal.y * escalaHover, escalaOriginal.z);
    }

    public void SimularNormal()
    {
        mouseEncima = false;
        escalaObjetivo = escalaOriginal;
        if (imagenBoton != null)
        {
            imagenBoton.color = colorNormal;
        }
    }

    public void SimularClick()
    {
        if (imagenBoton != null)
        {
            imagenBoton.color = colorPresionado;
        }
    }
}
