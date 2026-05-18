using UnityEngine;

// ============================================================
// BLEED EFFECT (v7) — Sangrado
// El Lobo ahora se gestiona desde ItemEffectController.OnHitEnemy()
// que aplica BleedEffect garantizado mientras el item está activo.
// Aquí no hay lógica de probabilidad de Lobo.
// ============================================================
public class BleedEffect : MonoBehaviour
{
    public int dotPerTick = 2;
    public float tickInterval = 0.7f;
    public int totalTicks = 4;

    EnemyRogueLite _enemy;
    EnemyEffectVFX _vfx;
    int _ticksLeft;
    float _nextTickTime;

    public static void Apply(GameObject enemy, int dotPerTick = 2, int ticks = 4)
    {
        if (enemy == null) return;

        var existing = enemy.GetComponent<BleedEffect>();
        if (existing != null)
        {
            // Refrescar duración (no apilar daño)
            existing._ticksLeft = ticks;
            return;
        }

        var bleed = enemy.AddComponent<BleedEffect>();
        bleed.dotPerTick = dotPerTick;
        bleed.totalTicks = ticks;
    }

    void Start()
    {
        _enemy = GetComponent<EnemyRogueLite>();
        _ticksLeft = totalTicks;
        _nextTickTime = Time.time + tickInterval;

        _vfx = GetComponent<EnemyEffectVFX>();
        _vfx?.Show(EffectType.Bleed);
    }

    void Update()
    {
        if (_enemy == null) { Cleanup(); return; }

        if (Time.time >= _nextTickTime)
        {
            _nextTickTime = Time.time + tickInterval;
            _enemy.TakeDamage(dotPerTick);
            _ticksLeft--;
            if (_ticksLeft <= 0) Cleanup();
        }
    }

    void Cleanup()
    {
        _vfx?.Hide(EffectType.Bleed);
        Destroy(this);
    }

    void OnDisable() => Cleanup();
}