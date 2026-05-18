using UnityEngine;

// ============================================================
// ENEMY ANIMATOR
// Anima los enemigos con sus sprites en loop.
// Compatible con Slime, Araña y Gárgola.
// Basta con asignar los frames del sprite sheet.
//
// SETUP:
// 1. Adjunta al prefab del enemigo
// 2. Arrastra los sprites de la animación al array "frames"
// 3. Ajusta "frameRate" (cuadros por segundo)
// ============================================================
public class EnemyAnimator : MonoBehaviour
{
    [Header("Frames de la animación")]
    [Tooltip("Arrastra aquí cada sprite del sheet en orden")]
    public Sprite[] frames;

    [Tooltip("Cuadros por segundo de la animación")]
    public float frameRate = 8f;

    SpriteRenderer _sr;
    float _timer;
    int _currentFrame;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // Reinicia la animación al activarse del pool
        _timer = 0f;
        _currentFrame = 0;
        if (_sr != null && frames.Length > 0)
            _sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;
        if (_sr == null) return;

        // Avanza el timer y cambia frame cuando toca
        _timer += Time.deltaTime;
        if (_timer >= 1f / frameRate)
        {
            _timer = 0f;
            _currentFrame = (_currentFrame + 1) % frames.Length;
            _sr.sprite = frames[_currentFrame];
        }
    }
}
