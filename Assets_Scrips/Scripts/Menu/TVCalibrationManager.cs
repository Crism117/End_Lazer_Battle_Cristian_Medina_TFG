// ==========================================================
// TV CALIBRATION MANAGER - SOLO controla el Overlay
// ==========================================================
// Este script SOLO hace una cosa:
// - Busca "OverlayTV" y lo estira a pantalla completa
//
// NO toca el ContenedorJuego
// NO toca el menú
// NO toca las flechas
// NO toca la escala de nada
// NO mueve nada de posición
//
// Solo el overlay. Nada más.
// ==========================================================

using UnityEngine;

public class TVCalibrationManager : MonoBehaviour
{
    void Start()
    {
        // Buscar el overlay
        GameObject overlay = GameObject.Find("OverlayTV");

        if (overlay != null)
        {
            // Estirar el overlay a pantalla completa
            RectTransform rt = overlay.GetComponent<RectTransform>();
            ForzarPantallaCompleta(rt);

            // Estirar todos los hijos del overlay también
            for (int i = 0; i < rt.childCount; i++)
            {
                RectTransform hijo = rt.GetChild(i).GetComponent<RectTransform>();
                if (hijo != null)
                    ForzarPantallaCompleta(hijo);
            }

            Debug.Log("✅ OverlayTV estirado a pantalla completa");
        }
        else
        {
            Debug.LogError("❌ No se encontró 'OverlayTV'");
        }
    }

    private void ForzarPantallaCompleta(RectTransform rt)
    {
        if (rt == null) return;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }
}
