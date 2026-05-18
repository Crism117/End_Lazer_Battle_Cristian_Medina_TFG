using UnityEngine;
using System.Collections.Generic;

// ============================================================
// MELEE HITBOX (v11)
// FIX iter 11: Update() reintenta EnsureInitialized() cada frame
// hasta encontrar el WeaponPositionManager. Esto resuelve el bug
// donde tras múltiples Replays, el arma no encontraba su manager
// por orden de inicialización.
// ============================================================
[RequireComponent(typeof(Collider2D))]
public class MeleeHitbox : MonoBehaviour
{
    [Header("─── DAÑO ──")]
    public int damage = 4;

    [Tooltip("Tiempo entre golpes al MISMO enemigo")]
    public float hitCooldown = 0.5f;

    [Header("─── EFECTO PASIVO ──")]
    public SpecialPower specialPower = SpecialPower.None;
    public int dotDamage = 1;
    public float effectDuration = 2f;
    public float knockbackForce = 0f;

    [Header("─── SFX ──")]
    [Tooltip("ID del arma (debe coincidir con WeaponDefinition.id)")]
    public string weaponId = "";

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    Dictionary<EnemyRogueLite, float> _lastHitTime = new Dictionary<EnemyRogueLite, float>();
    WeaponPositionManager _posMgr;
    int _myWeaponIndex = -1;
    bool _initialized = false;

    // ═══════════════════════════════════════════════════════════════════
    // FIX v11: reset al activarse + reintentos en Update
    // ═══════════════════════════════════════════════════════════════════
    void OnEnable()
    {
        _initialized = false;
        _posMgr = null;
        _myWeaponIndex = -1;
        _lastHitTime.Clear();

        if (debugMode) Debug.Log($"[MeleeHitbox] OnEnable - {gameObject.name} reinicializado.");
    }

    void Start()
    {
        EnsureInitialized();
    }

    /// <summary>
    /// FIX v11: si no estoy inicializado, intento serlo CADA frame
    /// hasta conseguirlo. Resuelve race conditions del orden de carga.
    /// </summary>
    void Update()
    {
        if (!_initialized)
            EnsureInitialized();
    }

    void EnsureInitialized()
    {
        if (_initialized && _posMgr != null && _myWeaponIndex >= 0) return;

        if (_posMgr == null)
        {
            _posMgr = GetComponentInParent<WeaponPositionManager>();
            if (_posMgr == null)
            {
                var playerGO = GameObject.FindGameObjectWithTag("Player");
                if (playerGO != null)
                    _posMgr = playerGO.GetComponentInChildren<WeaponPositionManager>();
            }
        }

        if (_posMgr != null && _myWeaponIndex < 0)
        {
            Transform armTransform = transform;
            while (armTransform.parent != null && armTransform.parent != _posMgr.transform)
                armTransform = armTransform.parent;

            _myWeaponIndex = _posMgr.FindWeaponIndexByTransform(armTransform);
        }

        if (_posMgr != null && _myWeaponIndex >= 0)
        {
            _initialized = true;
            if (debugMode)
                Debug.Log($"[MeleeHitbox] {gameObject.name} inicializado correctamente (idx={_myWeaponIndex}).");
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!other.CompareTag("Enemy")) return;

        var enemy = other.GetComponent<EnemyRogueLite>();
        if (enemy == null) return;

        if (_lastHitTime.TryGetValue(enemy, out float last))
        {
            if (Time.time < last + hitCooldown) return;
        }
        _lastHitTime[enemy] = Time.time;

        // ── Swing animation ──
        TriggerSwingAnimation();

        // ── SFX ──
        if (!string.IsNullOrEmpty(weaponId))
            SoundManager.Instance?.PlayWeaponFire(weaponId);
        SoundManager.Instance?.PlayHit();

        // ── Daño ──
        enemy.TakeDamage(damage);

        // ── Items ──
        ItemEffectController.Instance?.OnHitEnemy(other.gameObject);
        ItemEffectController.Instance?.OnDamageDealt(damage);

        if (debugMode)
            Debug.Log($"[MeleeHitbox] {gameObject.name} → {enemy.name} (-{damage})");

        ApplyPassive(enemy, other.gameObject);
    }

    void TriggerSwingAnimation()
    {
        EnsureInitialized();
        if (_posMgr == null || _myWeaponIndex < 0) return;
        _posMgr.TriggerMeleeSwing(_myWeaponIndex);
    }

    void ApplyPassive(EnemyRogueLite enemy, GameObject enemyGO)
    {
        switch (specialPower)
        {
            case SpecialPower.Bleed:
                BleedEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(effectDuration * 2));
                break;

            case SpecialPower.Burn:
                BurnEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(effectDuration * 2));
                break;

            case SpecialPower.Poison:
                PoisonEffect.Apply(enemyGO, dotDamage, Mathf.RoundToInt(effectDuration * 2), 0.5f);
                break;

            case SpecialPower.Stun:
                StartCoroutine(StunRoutine(enemy, effectDuration));
                break;

            case SpecialPower.Knockback:
                var rb = enemyGO.GetComponent<Rigidbody2D>();
                if (rb != null && transform.parent != null)
                {
                    Vector2 dir = ((Vector2)enemyGO.transform.position - (Vector2)transform.parent.position).normalized;
                    rb.AddForce(dir * knockbackForce, ForceMode2D.Impulse);
                }
                break;
        }
    }

    System.Collections.IEnumerator StunRoutine(EnemyRogueLite e, float dur)
    {
        if (e == null) yield break;
        var vfx = e.GetComponent<EnemyEffectVFX>();
        vfx?.Show(EffectType.Stun);

        float orig = e.moveSpeed;
        e.moveSpeed = 0f;
        yield return new WaitForSeconds(dur);

        if (e != null) e.moveSpeed = orig;
        vfx?.Hide(EffectType.Stun);
    }

    void OnDisable()
    {
        _lastHitTime.Clear();
    }
}