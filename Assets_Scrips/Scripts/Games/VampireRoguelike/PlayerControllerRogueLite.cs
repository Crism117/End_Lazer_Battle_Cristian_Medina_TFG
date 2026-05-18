using UnityEngine;
using DG.Tweening;

// ============================================================
// PLAYER CONTROLLER (v7)
// + Sonidos: caminar (cíclico), recibir daño
// + Integración escudo
// + rangedAttackBonus
// + HealProgressive
// ============================================================
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerControllerRogueLite : MonoBehaviour
{
    [Header("─── MOVIMIENTO ──")]
    public float moveSpeed = 5f;

    [Header("─── VIDA ──")]
    public int maxHealth = 10;

    [Header("─── STATS RPG ──")]
    [Range(0f, 0.5f)] public float resistance = 0f;
    [Range(0f, 1f)] public float critChance = 0f;
    [Range(0f, 1f)] public float lifeSteal = 0f;

    [Header("─── RANGO ATAQUE A DISTANCIA ──")]
    [Tooltip("Bonus al rango ranged. Empieza en 4 y sube por nivel.")]
    public float rangedAttackBonus = 4f;

    [Header("─── SONIDOS ──")]
    [Tooltip("Cada cuántos segundos reproducir sonido de paso")]
    public float walkSoundInterval = 0.35f;

    [Header("─── REFS ──")]
    [HideInInspector] public WeaponSystemRogueLite weaponSystem;

    int currentHealth;
    Rigidbody2D rb;
    SpriteRenderer sr;
    Color originalColor;
    Tween _healTween;
    PlayerEnemyCollision _collision;
    float _lastWalkSound;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        currentHealth = maxHealth;
        if (sr != null) originalColor = sr.color;

        _collision = GetComponent<PlayerEnemyCollision>();
    }

    void Start()
    {
        if (weaponSystem == null)
            weaponSystem = GetComponentInChildren<WeaponSystemRogueLite>();

        GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);
    }

    void Update()
    {
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused) return;

        // Movimiento (bloquear si stunned por colisión enemiga)
        bool stunned = _collision != null && _collision.IsStunned;

        Vector2 input = Vector2.zero;
        if (!stunned)
        {
            input = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")).normalized;
        }
        rb.velocity = input * moveSpeed;

        // ── SONIDO DE PASOS (cíclico mientras se mueve) ─────────────────
        if (input.sqrMagnitude > 0.05f && Time.time >= _lastWalkSound + walkSoundInterval)
        {
            _lastWalkSound = Time.time;
            SoundManager.Instance?.PlayPlayerWalkStep();
        }

        // ESC: pausa via menú
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (RogueliteMenuController.Instance != null)
                RogueliteMenuController.Instance.TogglePause();
            else if (GameManagerRogueLite.Instance != null)
            {
                if (GameManagerRogueLite.Instance.IsPaused)
                    GameManagerRogueLite.Instance.ResumeGame();
                else
                    GameManagerRogueLite.Instance.PauseGame();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        if (currentHealth <= 0) return;

        // ── ESCUDO ──
        if (PlayerShield.Instance != null && PlayerShield.Instance.TryConsumeShield())
        {
            if (sr != null) StartCoroutine(ShieldFlash());
            return;
        }

        int finalDmg = Mathf.Max(1, Mathf.RoundToInt(amount * (1f - resistance)));
        currentHealth -= finalDmg;

        GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);

        // ── SONIDO DE DAÑO ──
        SoundManager.Instance?.PlayPlayerHurt();

        if (sr != null) StartCoroutine(DamageFlash());

        if (currentHealth <= 0) Die();
    }

    System.Collections.IEnumerator DamageFlash()
    {
        sr.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    System.Collections.IEnumerator ShieldFlash()
    {
        sr.color = new Color(0.4f, 0.8f, 1f);
        yield return new WaitForSeconds(0.15f);
        sr.color = originalColor;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);
    }

    public void FullHeal()
    {
        currentHealth = maxHealth;
        GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);
    }

    public void HealProgressive(float duration = 1.5f)
    {
        if (currentHealth >= maxHealth) return;
        _healTween?.Kill();

        int startHP = currentHealth;
        float elapsed = 0f;

        _healTween = DOTween.To(() => elapsed, x =>
        {
            elapsed = x;
            float t = Mathf.Clamp01(elapsed / duration);
            currentHealth = Mathf.RoundToInt(Mathf.Lerp(startHP, maxHealth, t));
            GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);
        }, duration, duration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                currentHealth = maxHealth;
                GameManagerRogueLite.Instance?.uiManager?.UpdateHP(currentHealth, maxHealth);
            });
    }

    public void AddResistance(float amount)
    {
        resistance = Mathf.Clamp(resistance + amount, 0f, 0.5f);
    }

    void Die()
    {
        GameManagerRogueLite.Instance?.GameOver();
        gameObject.SetActive(false);
    }
}