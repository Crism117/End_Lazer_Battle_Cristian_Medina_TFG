using UnityEngine;
using UnityEngine.UI;

// ============================================================
// BRIGHTNESS OVERLAY
// Image negro encima de toda la UI que oscurece la pantalla
// según el valor de brillo guardado por AjustesManager.
//
// IMPORTANTE: este Image debe tener Raycast Target = false para
// no bloquear los clicks de la UI.
// ============================================================
[RequireComponent(typeof(Image))]
public class BrightnessOverlay : MonoBehaviour
{
    [Header("─── CONFIGURACIÓN ──")]
    [Tooltip("Máxima oscuridad (alpha). Debe coincidir con AjustesManager.maxOscuridad")]
    [Range(0.3f, 0.9f)] public float maxDarkness = 0.7f;

    Image _image;

    void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null) _image.raycastTarget = false;
    }

    void Start()
    {
        // Leer valor guardado e inicializar
        float brightness = LoadSavedBrightness();
        SetBrightness(brightness);
    }

    void OnEnable()
    {
        // Si volvemos a la escena (reentry desde pausa), recargar brillo
        if (_image != null)
        {
            float brightness = LoadSavedBrightness();
            SetBrightness(brightness);
        }
    }

    /// <summary>
    /// Aplica el brillo (1 = totalmente claro, 0 = totalmente oscuro)
    /// </summary>
    public void SetBrightness(float brightness)
    {
        if (_image == null) return;

        Color c = _image.color;
        c.a = (1f - brightness) * maxDarkness;
        _image.color = c;
    }

    /// <summary>
    /// Lee el brillo guardado. Compatible con tu sistema GuardarAjustes.
    /// </summary>
    float LoadSavedBrightness()
    {
        try
        {
            // Intenta usar tu clase GuardarAjustes (reflection)
            var type = System.Type.GetType("GuardarAjustes");
            if (type != null)
            {
                var method = type.GetMethod("CargarBrillo",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                if (method != null)
                {
                    object result = method.Invoke(null, null);
                    if (result is float v) return v;
                }
            }
        }
        catch { }

        // Fallback: PlayerPrefs directo
        return PlayerPrefs.GetFloat("Brillo", 1f);
    }
}
