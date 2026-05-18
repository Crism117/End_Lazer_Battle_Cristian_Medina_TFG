using UnityEngine;

// ============================================================
// CAMERA FOLLOW ROGUE
// Sigue al jugador suavemente. La distancia/zoom se controla
// directamente desde el Inspector con el campo "cameraSize".
//
// SETUP:
//   1. Adjunta este script a la Main Camera
//   2. Asegúrate de que la cámara es ORTHOGRÁFICA (no perspective)
//   3. Asigna el Player al campo "target"
//   4. Ajusta "cameraSize" en runtime para zoom in/out
//
// VALORES TÍPICOS:
//   cameraSize = 5  → cámara muy cerca (zoom in)
//   cameraSize = 8  → distancia media (recomendado)
//   cameraSize = 12 → cámara alejada (zoom out)
// ============================================================
public class CameraFollowRogue : MonoBehaviour
{
    [Header("Objetivo")]
    [Tooltip("Arrastra el Player aquí")]
    public Transform target;

    [Header("Zoom / Distancia de la cámara")]
    [Tooltip("Tamaño ortográfico. MÁS ALTO = MÁS LEJOS")]
    [Range(3f, 20f)]
    public float cameraSize = 8f;

    [Tooltip("Velocidad a la que la cámara se ajusta al cambiar cameraSize")]
    public float zoomSpeed = 4f;

    [Header("Seguimiento")]
    [Tooltip("Velocidad con la que la cámara sigue al jugador")]
    public float followSpeed = 6f;

    [Tooltip("Offset respecto al jugador (Z debe ser -10 para 2D)")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("Límites del mapa (opcional)")]
    public bool useLimits = false;
    public float minX, maxX, minY, maxY;

    Camera _cam;

    void Awake()
    {
        _cam = GetComponent<Camera>();
        if (_cam != null) _cam.orthographic = true;
    }

    void LateUpdate()
    {
        // Aplica el zoom suavemente desde el Inspector en runtime
        if (_cam != null)
            _cam.orthographicSize = Mathf.Lerp(
                _cam.orthographicSize, cameraSize, zoomSpeed * Time.deltaTime);

        // Busca el target si no está asignado
        if (target == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) target = p.transform;
            return;
        }

        // Sigue al jugador con suavizado
        Vector3 desired = target.position + offset;

        if (useLimits)
        {
            desired.x = Mathf.Clamp(desired.x, minX, maxX);
            desired.y = Mathf.Clamp(desired.y, minY, maxY);
        }

        transform.position = Vector3.Lerp(
            transform.position, desired, followSpeed * Time.deltaTime);
    }
}