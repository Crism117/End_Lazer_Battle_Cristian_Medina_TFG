using UnityEngine;

// ============================================================
// ENVIRONMENT PARTICLES (v6 — FIX Game View)
// Spawnea partículas ambientales DENTRO del área visible de la cámara.
// Ya no aparecen fuera de la pantalla.
// ============================================================
public class EnvironmentParticles : MonoBehaviour
{
    [Header("─── PREFAB ──")]
    public GameObject particlePrefab;

    [Header("─── ÁREA DE SPAWN ──")]
    [Tooltip("Si está activo, spawna dentro del área visible de la cámara principal")]
    public bool useCameraBounds = true;

    [Tooltip("Área manual (solo si Use Camera Bounds = false)")]
    public Vector2 spawnArea = new Vector2(30f, 20f);

    [Tooltip("Margen extra respecto al borde de la cámara")]
    public float cameraMargin = 1f;

    [Header("─── DENSIDAD ──")]
    public int activeParticles = 25;

    [Header("─── MOVIMIENTO ──")]
    public Vector2 windSpeed = new Vector2(0.3f, 1f);
    public Vector2 lifeTime = new Vector2(3f, 6f);

    [Header("─── RENDERIZADO ──")]
    [Tooltip("Sorting Order de las partículas (encima del fondo, debajo del player)")]
    public int sortingOrder = 1;

    [Tooltip("Sorting Layer (Default normalmente)")]
    public string sortingLayer = "Default";

    Camera _cam;

    void Start()
    {
        _cam = Camera.main;
        for (int i = 0; i < activeParticles; i++)
            SpawnParticle();
    }

    void SpawnParticle()
    {
        if (particlePrefab == null) return;

        Vector3 pos = GetSpawnPosition();
        var particle = Instantiate(particlePrefab, pos, Quaternion.identity, transform);

        // Forzar visibilidad y sorting correcto
        var sr = particle.GetComponent<SpriteRenderer>();
        if (sr == null) sr = particle.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingLayerName = sortingLayer;
            sr.sortingOrder = sortingOrder;
            if (sr.color.a < 0.5f) { Color c = sr.color; c.a = 1f; sr.color = c; }
        }

        float speed = Random.Range(windSpeed.x, windSpeed.y);
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));

        var mover = particle.AddComponent<EnvironmentParticleMover>();
        mover.velocity = dir * speed;
        mover.life = Random.Range(lifeTime.x, lifeTime.y);
        mover.spawner = this;
    }

    Vector3 GetSpawnPosition()
    {
        if (useCameraBounds && _cam != null)
        {
            // Calcular bounds visibles de la cámara
            float halfHeight = _cam.orthographicSize - cameraMargin;
            float halfWidth = halfHeight * _cam.aspect;

            return _cam.transform.position + new Vector3(
                Random.Range(-halfWidth, halfWidth),
                Random.Range(-halfHeight, halfHeight),
                0f);
        }

        // Fallback: área manual
        return transform.position + new Vector3(
            Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
            Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
            0f);
    }

    public void OnParticleDied()
    {
        SpawnParticle();
    }

    void OnDrawGizmosSelected()
    {
        if (useCameraBounds && Camera.main != null)
        {
            var cam = Camera.main;
            float halfHeight = cam.orthographicSize - cameraMargin;
            float halfWidth = halfHeight * cam.aspect;
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.3f);
            Gizmos.DrawWireCube(cam.transform.position, new Vector3(halfWidth * 2f, halfHeight * 2f, 0.1f));
        }
        else
        {
            Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.2f);
            Gizmos.DrawCube(transform.position, new Vector3(spawnArea.x, spawnArea.y, 0f));
        }
    }
}

public class EnvironmentParticleMover : MonoBehaviour
{
    public Vector2 velocity;
    public float life;
    public EnvironmentParticles spawner;

    SpriteRenderer _sr;
    Color _startColor;
    float _aliveTime;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _startColor = _sr.color;
    }

    void Update()
    {
        transform.position += (Vector3)(velocity * Time.deltaTime);
        _aliveTime += Time.deltaTime;

        if (_sr != null)
        {
            float t = _aliveTime / life;
            Color c = _startColor;
            c.a = Mathf.Lerp(_startColor.a, 0f, t);
            _sr.color = c;
        }

        if (_aliveTime >= life)
        {
            spawner?.OnParticleDied();
            Destroy(gameObject);
        }
    }
}
