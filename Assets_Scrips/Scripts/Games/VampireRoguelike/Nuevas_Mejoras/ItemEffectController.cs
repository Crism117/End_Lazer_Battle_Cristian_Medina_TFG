using UnityEngine;
using System.Collections.Generic;

// ============================================================
// ITEM EFFECT CONTROLLER (singleton)
// FIX iter7: gestiona la activación/desactivación de items con
// timer. Cuando el timer expira, revierte el efecto.
//
// Items soportados:
//   - Boots: +velocidad temporal (revierte al expirar)
//   - Lifesteal: cura HP por golpe mientras dura
//   - Wolf: aplica Bleed garantizado a enemigos golpeados
//   - Collar: x2 monedas (gestionado por ItemSystem stacks)
//   - Magnet: imán de monedas (gestionado por CoinPickup)
//   - Shield: acumulativo (gestionado por PlayerShield)
// ============================================================
public class ItemEffectController : MonoBehaviour
{
    public static ItemEffectController Instance { get; private set; }

    [Header("─── BOTAS ──")]
    [Tooltip("Velocidad EXTRA mientras Botas está activo")]
    public float bootsSpeedBonus = 2.0f;

    [Header("─── CHUPASANGRE ──")]
    [Tooltip("HP que cura cada golpe mientras Chupasangre está activo")]
    public int lifestealPerHit = 2;

    [Header("─── LOBO ──")]
    [Tooltip("Daño Bleed garantizado por tick mientras Lobo está activo")]
    public int wolfBleedTick = 2;
    [Tooltip("Ticks totales del Bleed garantizado de Lobo")]
    public int wolfBleedTicks = 4;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    // ─── ESTADO ────────────────────────────────────────────────────────
    PlayerControllerRogueLite _player;

    // Activos: efecto -> (tiempo restante, valor original guardado)
    class ActiveEffect
    {
        public float remainingTime;
        public float originalValue;
        public bool active;
    }

    ActiveEffect _bootsEffect;
    ActiveEffect _lifestealEffect;
    ActiveEffect _wolfEffect;
    ActiveEffect _collarEffect;
    ActiveEffect _magnetEffect;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(this); return; }
        Instance = this;
    }

    void Start()
    {
        _player = FindObjectOfType<PlayerControllerRogueLite>();
    }

    void Update()
    {
        TickEffect(_bootsEffect, OnBootsExpired);
        TickEffect(_lifestealEffect, OnLifestealExpired);
        TickEffect(_wolfEffect, OnWolfExpired);
        TickEffect(_collarEffect, OnCollarExpired);
        TickEffect(_magnetEffect, OnMagnetExpired);
    }

    void TickEffect(ActiveEffect e, System.Action onExpired)
    {
        if (e == null || !e.active) return;
        e.remainingTime -= Time.deltaTime;
        if (e.remainingTime <= 0f)
        {
            e.active = false;
            onExpired?.Invoke();
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    // ACTIVAR EFECTOS
    // ─────────────────────────────────────────────────────────────────────
    public void ActivateBoots(float duration)
    {
        if (_player == null) _player = FindObjectOfType<PlayerControllerRogueLite>();
        if (_player == null) return;

        if (_bootsEffect == null) _bootsEffect = new ActiveEffect();

        // Si ya estaba activo, refrescar tiempo. Si no, guardar valor original.
        if (!_bootsEffect.active)
        {
            _bootsEffect.originalValue = _player.moveSpeed;
            _player.moveSpeed += bootsSpeedBonus;
        }

        _bootsEffect.remainingTime = duration;
        _bootsEffect.active = true;

        if (debugMode) Debug.Log($"[ItemEffect] Botas activado. Vel: {_player.moveSpeed:F2}");
    }

    void OnBootsExpired()
    {
        if (_player != null)
            _player.moveSpeed = _bootsEffect.originalValue;
        if (debugMode) Debug.Log($"[ItemEffect] Botas expirado. Vel: {_player?.moveSpeed:F2}");
    }

    // ─────────────────────────────────────────────────────────────────────
    public void ActivateLifesteal(float duration)
    {
        if (_lifestealEffect == null) _lifestealEffect = new ActiveEffect();
        _lifestealEffect.remainingTime = duration;
        _lifestealEffect.active = true;
        if (debugMode) Debug.Log("[ItemEffect] Chupasangre activado.");
    }

    void OnLifestealExpired()
    {
        if (debugMode) Debug.Log("[ItemEffect] Chupasangre expirado.");
    }

    public bool IsLifestealActive() => _lifestealEffect != null && _lifestealEffect.active;

    /// <summary>
    /// Llamado por proyectiles/melee al hacer daño. Si chupasangre activo, cura.
    /// </summary>
    public void OnDamageDealt(int damageDone)
    {
        if (!IsLifestealActive()) return;
        if (_player == null) return;
        _player.Heal(lifestealPerHit);
    }

    // ─────────────────────────────────────────────────────────────────────
    public void ActivateWolf(float duration)
    {
        if (_wolfEffect == null) _wolfEffect = new ActiveEffect();
        _wolfEffect.remainingTime = duration;
        _wolfEffect.active = true;
        if (debugMode) Debug.Log("[ItemEffect] Lobo activado.");
    }

    void OnWolfExpired()
    {
        if (debugMode) Debug.Log("[ItemEffect] Lobo expirado.");
    }

    public bool IsWolfActive() => _wolfEffect != null && _wolfEffect.active;

    /// <summary>
    /// Llamado por proyectiles/melee al golpear. Si Lobo activo, aplica Bleed.
    /// </summary>
    public void OnHitEnemy(GameObject enemyGO)
    {
        if (!IsWolfActive() || enemyGO == null) return;
        BleedEffect.Apply(enemyGO, wolfBleedTick, wolfBleedTicks);
    }

    // ─────────────────────────────────────────────────────────────────────
    public void ActivateCollar(float duration)
    {
        if (_collarEffect == null) _collarEffect = new ActiveEffect();
        _collarEffect.remainingTime = duration;
        _collarEffect.active = true;
        if (ItemSystem.Instance != null) ItemSystem.Instance.AddStack("collar", 1);
    }

    void OnCollarExpired()
    {
        if (ItemSystem.Instance != null) ItemSystem.Instance.AddStack("collar", -1);
    }

    // ─────────────────────────────────────────────────────────────────────
    public void ActivateMagnet(float duration)
    {
        if (_magnetEffect == null) _magnetEffect = new ActiveEffect();
        _magnetEffect.remainingTime = duration;
        _magnetEffect.active = true;
        if (ItemSystem.Instance != null) ItemSystem.Instance.AddStack("magnet", 1);
    }

    void OnMagnetExpired()
    {
        if (ItemSystem.Instance != null) ItemSystem.Instance.AddStack("magnet", -1);
    }

    // ─────────────────────────────────────────────────────────────────────
    public void ResetAll()
    {
        if (_bootsEffect != null && _bootsEffect.active) OnBootsExpired();
        _bootsEffect = null;
        _lifestealEffect = null;
        _wolfEffect = null;
        _collarEffect = null;
        _magnetEffect = null;
    }
}