using UnityEngine;

// ============================================================
// COIN ANIMATOR
// Anima la moneda física con el sprite sheet de 7 frames.
// La moneda gira en loop mientras espera ser recogida.
//
// SETUP:
// 1. Adjunta al prefab de la moneda (Coin)
// 2. Arrastra los 7 sprites del sheet al array "frames"
// 3. Frame Rate recomendado: 10-12
// ============================================================
public class CoinAnimator : MonoBehaviour
{
    [Header("Sprites de la moneda (7 frames)")]
    public Sprite[] frames;

    [Tooltip("Cuadros por segundo")]
    public float frameRate = 10f;

    SpriteRenderer _sr;
    float _timer;
    int _currentFrame;

    void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        _timer = 0f;
        _currentFrame = 0;
        if (_sr != null && frames.Length > 0)
            _sr.sprite = frames[0];
    }

    void Update()
    {
        if (frames == null || frames.Length == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / frameRate)
        {
            _timer = 0f;
            _currentFrame = (_currentFrame + 1) % frames.Length;
            if (_sr != null) _sr.sprite = frames[_currentFrame];
        }
    }
}
