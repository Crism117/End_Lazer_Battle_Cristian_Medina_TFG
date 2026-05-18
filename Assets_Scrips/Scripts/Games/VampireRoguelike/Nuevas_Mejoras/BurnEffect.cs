using UnityEngine;

// ============================================================
// BURN EFFECT (v6) — Quemadura
// DOT + EnemyEffectVFX gestiona tint amarillo-rojizo y llamas
// ============================================================
public class BurnEffect : MonoBehaviour
{
    public int dotPerTick = 1;
    public float tickInterval = 0.5f;
    public int totalTicks = 4;

    EnemyRogueLite _enemy;
    EnemyEffectVFX _vfx;
    int _ticksLeft;
    float _nextTickTime;

    public static void Apply(GameObject enemy, int dotPerTick = 1, int ticks = 4)
    {
        if (enemy == null) return;

        var existing = enemy.GetComponent<BurnEffect>();
        if (existing != null) { existing._ticksLeft = ticks; return; }

        var burn = enemy.AddComponent<BurnEffect>();
        burn.dotPerTick = dotPerTick;
        burn.totalTicks = ticks;
    }

    void Start()
    {
        _enemy = GetComponent<EnemyRogueLite>();
        _ticksLeft = totalTicks;
        _nextTickTime = Time.time + tickInterval;

        _vfx = GetComponent<EnemyEffectVFX>();
        _vfx?.Show(EffectType.Burn);
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
        _vfx?.Hide(EffectType.Burn);
        Destroy(this);
    }

    void OnDisable() => Cleanup();
}
