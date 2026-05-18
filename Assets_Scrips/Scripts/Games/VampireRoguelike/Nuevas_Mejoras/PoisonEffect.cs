using UnityEngine;

// ============================================================
// POISON EFFECT (v6) — Veneno
// DOT + slow + EnemyEffectVFX gestiona tint morado y burbujas
// ============================================================
public class PoisonEffect : MonoBehaviour
{
    public int dotPerTick = 1;
    public float tickInterval = 0.6f;
    public int totalTicks = 4;
    public float slowFactor = 0.5f;

    EnemyRogueLite _enemy;
    EnemyEffectVFX _vfx;
    float _originalSpeed;
    int _ticksLeft;
    float _nextTickTime;

    public static void Apply(GameObject enemy, int dotPerTick = 1, int ticks = 4, float slowFactor = 0.5f)
    {
        if (enemy == null) return;

        var existing = enemy.GetComponent<PoisonEffect>();
        if (existing != null) { existing._ticksLeft = ticks; return; }

        var poison = enemy.AddComponent<PoisonEffect>();
        poison.dotPerTick = dotPerTick;
        poison.totalTicks = ticks;
        poison.slowFactor = slowFactor;
    }

    void Start()
    {
        _enemy = GetComponent<EnemyRogueLite>();
        _ticksLeft = totalTicks;
        _nextTickTime = Time.time + tickInterval;

        if (_enemy != null)
        {
            _originalSpeed = _enemy.moveSpeed;
            _enemy.moveSpeed *= slowFactor;
        }

        _vfx = GetComponent<EnemyEffectVFX>();
        _vfx?.Show(EffectType.Poison);
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
        if (_enemy != null) _enemy.moveSpeed = _originalSpeed;
        _vfx?.Hide(EffectType.Poison);
        Destroy(this);
    }

    void OnDestroy() => Cleanup();
    void OnDisable() => Cleanup();
}