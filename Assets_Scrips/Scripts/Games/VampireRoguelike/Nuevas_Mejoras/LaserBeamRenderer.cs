using UnityEngine;
using System.Collections;

// ============================================================
// LASER BEAM RENDERER
// Crea un rayo láser TEMPORAL usando LineRenderer (sin sprite).
// El rayo aparece, daña a los enemigos en la línea y se desvanece.
//
// USO:
//   LaserBeamRenderer.Fire(origen, destino, daño, color, grosor);
// ============================================================
public class LaserBeamRenderer : MonoBehaviour
{
    LineRenderer _lr;
    float _duration = 0.1f;
    float _t;
    Color _startColor;

    /// <summary>
    /// Crea un rayo láser entre origen y destino. Aplica daño a enemigos en el camino.
    /// </summary>
    public static void Fire(Vector3 origin, Vector3 target, int damage,
                             Color color, float width = 0.15f, float duration = 0.1f)
    {
        var go = new GameObject("LaserBeam");
        go.transform.position = origin;

        var lr = go.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.SetPosition(0, origin);
        lr.SetPosition(1, target);
        lr.startWidth = width;
        lr.endWidth = width * 0.5f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = color;
        lr.endColor = new Color(color.r, color.g, color.b, 0.2f);
        lr.sortingLayerName = "Default";
        lr.sortingOrder = 6;

        // Aplicar daño a todos los enemigos en la línea
        Vector2 dir = (target - origin);
        float dist = dir.magnitude;
        var hits = Physics2D.RaycastAll(origin, dir.normalized, dist);
        foreach (var h in hits)
        {
            if (h.collider == null) continue;
            if (!h.collider.CompareTag("Enemy")) continue;
            var enemy = h.collider.GetComponent<EnemyRogueLite>();
            if (enemy != null) enemy.TakeDamage(damage);
        }

        // Componente para gestionar el fade-out
        var beam = go.AddComponent<LaserBeamRenderer>();
        beam._lr = lr;
        beam._duration = duration;
        beam._startColor = color;
    }

    void Update()
    {
        _t += Time.deltaTime;
        float k = Mathf.Clamp01(_t / _duration);

        // Fade-out
        Color c = _lr.startColor;
        c.a = 1f - k;
        _lr.startColor = c;

        Color end = _lr.endColor;
        end.a = (1f - k) * 0.2f;
        _lr.endColor = end;

        if (_t >= _duration)
        {
            Destroy(gameObject);
        }
    }
}