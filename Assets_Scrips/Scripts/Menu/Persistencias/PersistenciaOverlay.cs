// Mantiene el OverlayTV en TODAS las escenas
// Necesita su propio Canvas para no depender del Canvas de cada escena

using UnityEngine;

public class PersistenciaOverlay : MonoBehaviour
{
    private static PersistenciaOverlay instancia;

    void Awake()
    {
        if (instancia != null && instancia != this)
        {
            Destroy(gameObject);
            return;
        }
        instancia = this;
        DontDestroyOnLoad(gameObject);
    }
}

