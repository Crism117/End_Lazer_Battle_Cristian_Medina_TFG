using UnityEngine;
using UnityEngine.UI;

// ============================================================
// ARCADE OVERLAY SETUP
// Configura automáticamente los dos Canvas para el efecto arcade:
//
//   GameCanvas (Screen Space - Camera, Sort Order 5)
//     → HUD, barras, textos
//     → El Volume/post-procesado SÍ afecta a este canvas
//     → Se dibuja ENCIMA de enemigos y player
//
//   OverlayCanvas (Screen Space - Overlay, Sort Order 100)
//     → Solo el efecto CRT/scanlines
//     → Se dibuja encima de ABSOLUTAMENTE TODO
//     → Incluido encima del HUD
//
// SETUP:
//   Adjunta este script a un GameObject vacío en la escena.
//   Arrastra los dos Canvas y la cámara en el Inspector.
// ============================================================
public class ArcadeOverlaySetup : MonoBehaviour
{
    [Header("Canvas del juego (HUD)")]
    [Tooltip("El Canvas que contiene el HUD. Debe estar en Screen Space - Camera")]
    public Canvas gameCanvas;

    [Header("Canvas del overlay CRT")]
    [Tooltip("El Canvas que contiene solo el efecto CRT/scanlines. Screen Space - Overlay")]
    public Canvas overlayCanvas;

    [Header("Cámara principal")]
    public Camera mainCamera;

    void Awake()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        SetupGameCanvas();
        SetupOverlayCanvas();
    }

    void SetupGameCanvas()
    {
        if (gameCanvas == null) return;

        // Screen Space - Camera: el Volume y post-procesado SÍ afectan al HUD
        gameCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        gameCanvas.worldCamera = mainCamera;
        gameCanvas.planeDistance = 1f; // cerca de la cámara pero delante de todo

        // Sort Order alto: se dibuja ENCIMA de los sprites del mundo
        // Los enemigos/player usan Sorting Layer "Default" (order 0)
        // El HUD usa order 5, así siempre queda delante
        gameCanvas.sortingOrder = 5;

        // Scaler para que se adapte a cualquier resolución
        var scaler = gameCanvas.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        Debug.Log("✅ GameCanvas configurado: Screen Space - Camera, Sort Order 5");
    }

    void SetupOverlayCanvas()
    {
        if (overlayCanvas == null) return;

        // Screen Space - Overlay: se dibuja encima de TODO, incluyendo el HUD
        // El post-procesado NO afecta a este canvas (es lo que queremos para el CRT)
        overlayCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        overlayCanvas.sortingOrder = 100; // Siempre el último en dibujarse

        Debug.Log("✅ OverlayCanvas configurado: Screen Space - Overlay, Sort Order 100");
    }
}
