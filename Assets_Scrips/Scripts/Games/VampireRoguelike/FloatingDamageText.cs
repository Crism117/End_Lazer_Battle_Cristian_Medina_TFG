using UnityEngine;
using TMPro;

// ============================================================
// FLOATING DAMAGE TEXT
// Pequeños números que aparecen sobre los enemigos al recibir daño
// y suben mientras se desvanecen. Estilo RPG clásico.
// ============================================================
public class FloatingDamageText : MonoBehaviour
{
    [Header("Animación")]
    [Tooltip("Cuánto sube por segundo")]
    public float riseSpeed = 1.5f;

    [Tooltip("Cuántos segundos vive el número antes de desaparecer")]
    public float lifeTime = 0.8f;

    // Cuánto tiempo lleva vivo
    float _aliveTime;

    // Componentes
    TextMeshPro _text;
    Color _startColor;

    void Awake()
    {
        // Buscamos el TextMeshPro (no UI, sino 3D world space)
        _text = GetComponent<TextMeshPro>();
        if (_text != null) _startColor = _text.color;
    }

    // Llamado cuando se crea el número desde el pool / instancia
    public void Setup(int damage, Color color)
    {
        _aliveTime = 0f;
        if (_text != null)
        {
            _text.text = damage.ToString();
            _text.color = color;
            _startColor = color;
        }
    }

    void Update()
    {
        // Sube el número cada frame
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Cuenta el tiempo de vida
        _aliveTime += Time.deltaTime;

        // Calcula el porcentaje de vida (0 = recién creado, 1 = expirado)
        float t = _aliveTime / lifeTime;

        // Va desvaneciendo el color con el tiempo
        if (_text != null)
        {
            Color c = _startColor;
            c.a = 1f - t;
            _text.color = c;
        }

        // Cuando se acaba el tiempo, se destruye solo
        if (t >= 1f) Destroy(gameObject);
    }
}