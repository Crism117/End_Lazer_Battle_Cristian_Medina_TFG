using UnityEngine;

// ============================================================
// ENEMY SOUND CONFIG
// Va en cada prefab de enemigo (Slime, Aranha, Gargola).
// Reproduce sonidos de movimiento (cíclico) y daño (al recibir golpes).
// ============================================================
[RequireComponent(typeof(AudioSource))]
public class EnemySoundConfig : MonoBehaviour
{
    [Header("─── CLIPS ──")]
    public AudioClip moveClip;
    public AudioClip hurtClip;

    [Header("─── VOLUMEN ──")]
    [Range(0f, 1f)] public float moveVolume = 0.4f;
    [Range(0f, 1f)] public float hurtVolume = 0.7f;

    [Header("─── MOVIMIENTO ──")]
    [Tooltip("Cada cuántos segundos reproducir sonido de movimiento")]
    public float moveCooldown = 0.6f;

    [Tooltip("Variación de pitch para que no suene siempre igual")]
    public Vector2 pitchRange = new Vector2(0.92f, 1.08f);

    [Tooltip("Distancia máxima desde el jugador a la que se oye este enemigo")]
    public float maxAudibleDistance = 12f;

    AudioSource _source;
    float _lastMoveSound;
    Transform _player;
    Rigidbody2D _rb;

    void Awake()
    {
        _source = GetComponent<AudioSource>();
        _rb = GetComponent<Rigidbody2D>();
        _source.playOnAwake = false;
        _source.loop = false;
        _source.spatialBlend = 0f; // 2D
    }

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p != null) _player = p.transform;
    }

    void Update()
    {
        if (moveClip == null || _source == null) return;

        // Solo si se está moviendo
        bool isMoving = _rb != null && _rb.velocity.sqrMagnitude > 0.05f;
        if (!isMoving) return;

        if (Time.time < _lastMoveSound + moveCooldown) return;
        _lastMoveSound = Time.time;

        // Atenuar por distancia al jugador
        if (_player != null)
        {
            float dist = Vector2.Distance(transform.position, _player.position);
            if (dist > maxAudibleDistance) return;

            float volMult = 1f - Mathf.Clamp01(dist / maxAudibleDistance);
            _source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            _source.PlayOneShot(moveClip, moveVolume * volMult);
        }
        else
        {
            _source.pitch = Random.Range(pitchRange.x, pitchRange.y);
            _source.PlayOneShot(moveClip, moveVolume);
        }
    }

    /// <summary>
    /// Llamado por EnemyRogueLite.TakeDamage() al recibir daño.
    /// </summary>
    public void PlayHurt()
    {
        if (hurtClip == null || _source == null) return;
        _source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        _source.PlayOneShot(hurtClip, hurtVolume);
    }
}
