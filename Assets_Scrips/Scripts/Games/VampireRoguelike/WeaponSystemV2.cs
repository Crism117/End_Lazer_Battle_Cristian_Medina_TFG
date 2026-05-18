using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// ============================================================
// WEAPON SYSTEM V2 (v12)
// FIX iter 12: IsLaserWeapon detecta TAMBIÉN por ID del arma.
// Antes solo lo hacía por SpecialPower (Slow/ChainExplosion),
// y si el WeaponDefinition de laser_pistol no tenía el power
// correcto, no entraba en FireLaser y caía en SpawnShot.
// ============================================================
public class WeaponSystemV2 : MonoBehaviour
{
    [Header("─── ARMA INICIAL ──")]
    public WeaponDefinition startingWeapon;
    public bool equipStartingWeaponOnStart = false;

    [Header("─── ARMAS EQUIPADAS ──")]
    public List<WeaponDefinition> weapons = new List<WeaponDefinition>();
    public List<int> weaponLevels = new List<int>();

    [Header("─── PROYECTILES ──")]
    public GameObject projectilePrefab;
    public ObjectPoolRogueLite projectilePool;

    [Header("─── LÁSER ──")]
    public Color laserPistolColor = new Color(0.3f, 0.7f, 1f, 1f);
    public Color laserChaingunColor = new Color(1f, 0.3f, 0.3f, 1f);
    public float laserWidth = 0.15f;
    public float laserDuration = 0.1f;

    [Header("─── RANGOS BASE ──")]
    [Range(0.5f, 8f)] public float meleeBaseRadius = 1.5f;
    [Range(2f, 15f)] public float rangedBaseRange = 4f;

    [Header("─── GIZMOS ──")]
    public Color rangedGizmoColor = new Color(0.3f, 0.8f, 1f, 0.6f);
    public Color meleeGizmoColor = new Color(1f, 0.3f, 0.3f, 0.6f);
    public bool alwaysShowGizmos = true;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;

    Transform _rangedTarget;
    bool _hasMeleeEnemy;
    float[] _nextFireTimes;
    WeaponPositionManager _positionMgr;
    PlayerControllerRogueLite _player;

    public event Action<int> OnWeaponUpgraded;

    void Start()
    {
        _positionMgr = GetComponentInParent<WeaponPositionManager>();
        if (_positionMgr == null) _positionMgr = GetComponent<WeaponPositionManager>();

        _player = GetComponentInParent<PlayerControllerRogueLite>();
        if (_player == null) _player = GetComponent<PlayerControllerRogueLite>();

        if (equipStartingWeaponOnStart && startingWeapon != null)
            AddWeapon(startingWeapon);

        ResizeFireTimes();
    }

    float GetEffectiveRangedRange()
    {
        float bonus = _player != null ? _player.rangedAttackBonus : 0f;
        return rangedBaseRange + bonus;
    }

    void Update()
    {
        if (GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused) return;
        if (weapons.Count == 0) return;

        FindTargets();

        if (_nextFireTimes == null || _nextFireTimes.Length != weapons.Count)
            ResizeFireTimes();

        float t = Time.time;
        for (int i = 0; i < weapons.Count; i++)
        {
            var w = weapons[i];
            if (w == null) continue;
            if (t < _nextFireTimes[i]) continue;

            if (w.weaponType == WeaponType.Ranged)
            {
                if (_rangedTarget == null) continue;
                _nextFireTimes[i] = t + w.fireRate;
                FireRanged(w, _rangedTarget.position, i);
            }
            else
            {
                if (!_hasMeleeEnemy) continue;
                _nextFireTimes[i] = t + w.fireRate;
                // Daño y SFX se aplican desde MeleeHitbox del prefab.
            }
        }
    }

    void ResizeFireTimes()
    {
        var arr = new float[weapons.Count];
        if (_nextFireTimes != null)
            for (int i = 0; i < arr.Length && i < _nextFireTimes.Length; i++)
                arr[i] = _nextFireTimes[i];
        _nextFireTimes = arr;
    }

    void FindTargets()
    {
        _rangedTarget = null;
        _hasMeleeEnemy = false;
        float closest = float.MaxValue;
        float effectiveRange = GetEffectiveRangedRange();

        foreach (var e in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (e == null || !e.activeInHierarchy) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d <= meleeBaseRadius) _hasMeleeEnemy = true;
            if (d <= effectiveRange && d < closest)
            {
                closest = d;
                _rangedTarget = e.transform;
            }
        }
    }

    // ─────────────────────────────────────────────────────────────────────
    void FireRanged(WeaponDefinition w, Vector3 targetPos, int weaponIndex)
    {
        // ── SFX de disparo ──
        SoundManager.Instance?.PlayWeaponFire(w.id);

        if (IsLaserWeapon(w))
        {
            FireLaser(w, weaponIndex, targetPos);
            return;
        }

        switch (w.fireMode)
        {
            case FireMode.Single:
                SpawnShot(w, targetPos, 0f, weaponIndex);
                break;

            case FireMode.Spread:
                float half = (w.shotCount - 1) * w.spreadAngle * 0.5f;
                for (int i = 0; i < w.shotCount; i++)
                    SpawnShot(w, targetPos, -half + i * w.spreadAngle, weaponIndex);
                break;

            case FireMode.Burst:
                StartCoroutine(FireBurst(w, targetPos, weaponIndex));
                break;
        }
    }

    IEnumerator FireBurst(WeaponDefinition w, Vector3 targetPos, int weaponIndex)
    {
        for (int i = 0; i < w.shotCount; i++)
        {
            Vector3 currentTarget = (_rangedTarget != null) ? _rangedTarget.position : targetPos;
            SpawnShot(w, currentTarget, 0f, weaponIndex);
            yield return new WaitForSeconds(w.burstInterval);
        }
    }

    /// <summary>
    /// FIX v12: detecta láseres por TRES criterios (basta uno).
    /// </summary>
    bool IsLaserWeapon(WeaponDefinition w)
    {
        if (w == null) return false;

        // Criterio 1: por ID (más robusto)
        string idLower = (w.id ?? "").ToLower();
        if (idLower.Contains("laser")) return true;

        // Criterio 2: por SpecialPower (como antes)
        if (w.specialPower == SpecialPower.Slow ||
            w.specialPower == SpecialPower.ChainExplosion)
            return true;

        return false;
    }

    /// <summary>
    /// Determina qué color usar según el ID o el SpecialPower.
    /// </summary>
    Color GetLaserColor(WeaponDefinition w)
    {
        string idLower = (w.id ?? "").ToLower();
        if (idLower.Contains("chaingun") || idLower.Contains("metralla"))
            return laserChaingunColor;
        if (w.specialPower == SpecialPower.ChainExplosion)
            return laserChaingunColor;
        return laserPistolColor;
    }

    void FireLaser(WeaponDefinition w, int weaponIndex, Vector3 targetPos)
    {
        Vector3 origin = GetWeaponFirePosition(weaponIndex, w);
        Color col = GetLaserColor(w);

        if (debugMode)
            Debug.Log($"[WeaponV2] FIRE LASER '{w.id}' → desde {origin} hacia {targetPos}");

        LaserBeamRenderer.Fire(origin, targetPos, w.projectileDamage, col, laserWidth, laserDuration);

        var hit = Physics2D.OverlapPoint(targetPos);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            var enemy = hit.GetComponent<EnemyRogueLite>();
            if (enemy != null)
            {
                ItemEffectController.Instance?.OnHitEnemy(hit.gameObject);
                ItemEffectController.Instance?.OnDamageDealt(w.projectileDamage);

                if (ShouldProcPassive(weaponIndex, w))
                    ApplyPassiveEffect(w, hit.gameObject, enemy);
            }
        }
    }

    void SpawnShot(WeaponDefinition w, Vector3 targetPos, float angleOffset, int weaponIndex)
    {
        Vector3 spawnPos = GetWeaponFirePosition(weaponIndex, w);

        if (w.projectilePrefab == null)
        {
            if (debugMode) Debug.LogWarning($"[WeaponV2] '{w.id}' sin Projectile Prefab.");
            return;
        }

        Vector2 baseDir = ((Vector2)targetPos - (Vector2)spawnPos).normalized;
        float baseAngle = Mathf.Atan2(baseDir.y, baseDir.x) * Mathf.Rad2Deg;
        float finalAngle = (baseAngle + angleOffset) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(finalAngle), Mathf.Sin(finalAngle), 0f);

        GameObject proj = projectilePool != null ? projectilePool.Get() : null;
        if (proj == null) proj = Instantiate(w.projectilePrefab);
        if (proj == null) return;

        proj.transform.position = spawnPos;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);
        proj.SetActive(true);

        var sr = proj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            if (sr.color.a < 0.5f) { var c = sr.color; c.a = 1f; sr.color = c; }
        }

        bool procPassive = ShouldProcPassive(weaponIndex, w);
        SpecialPower passiveToApply = procPassive ? w.specialPower : SpecialPower.None;

        var pb = proj.GetComponent<ProjectileBase>();
        if (pb != null)
        {
            pb.Launch(dir, w.projectileDamage, w.projectileSpeed,
                      passiveToApply, w.dotDamage, w.effectDuration,
                      w.knockbackForce, projectilePool);
        }
        else
        {
            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * w.projectileSpeed;
        }
    }

    bool ShouldProcPassive(int weaponIndex, WeaponDefinition w)
    {
        if (w.specialPower == SpecialPower.None) return false;
        if (w.specialPower == SpecialPower.Pierce ||
            w.specialPower == SpecialPower.Knockback ||
            w.specialPower == SpecialPower.HighCrit ||
            w.specialPower == SpecialPower.LifeStealHit)
            return true;

        int level = GetWeaponLevel(weaponIndex);
        float chance = w.GetProcChanceAtLevel(level);
        return UnityEngine.Random.value < chance;
    }

    Vector3 GetWeaponFirePosition(int weaponIndex, WeaponDefinition w)
    {
        if (_positionMgr == null) return transform.position;

        Vector3 weaponPos = _positionMgr.GetWeaponWorldPosition(weaponIndex);
        float angleDeg = _positionMgr.GetWeaponAngle(weaponIndex);
        float rad = angleDeg * Mathf.Deg2Rad;

        Vector2 offset = w.firePointOffset;
        Vector2 rotated = new Vector2(
            offset.x * Mathf.Cos(rad) - offset.y * Mathf.Sin(rad),
            offset.x * Mathf.Sin(rad) + offset.y * Mathf.Cos(rad));

        return weaponPos + (Vector3)rotated;
    }

    void ApplyPassiveEffect(WeaponDefinition w, GameObject enemyGO, EnemyRogueLite enemy)
    {
        switch (w.specialPower)
        {
            case SpecialPower.Bleed:
                BleedEffect.Apply(enemyGO, w.dotDamage, Mathf.RoundToInt(w.effectDuration * 2));
                break;
            case SpecialPower.Burn:
                BurnEffect.Apply(enemyGO, w.dotDamage, Mathf.RoundToInt(w.effectDuration * 2));
                break;
            case SpecialPower.Poison:
                PoisonEffect.Apply(enemyGO, w.dotDamage, Mathf.RoundToInt(w.effectDuration * 2), 0.5f);
                break;
            case SpecialPower.Stun:
                StartCoroutine(StunEnemy(enemy, w.effectDuration));
                break;
            case SpecialPower.Slow:
                StartCoroutine(SlowEnemy(enemy, w.effectDuration));
                break;
        }
    }

    IEnumerator StunEnemy(EnemyRogueLite e, float dur)
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

    IEnumerator SlowEnemy(EnemyRogueLite e, float dur)
    {
        if (e == null) yield break;
        float orig = e.moveSpeed;
        e.moveSpeed *= 0.4f;
        yield return new WaitForSeconds(dur);
        if (e != null) e.moveSpeed = orig;
    }

    // ─────────────────────────────────────────────────────────────────────
    public void AddWeapon(WeaponDefinition w)
    {
        if (w == null) return;
        weapons.Add(w);
        weaponLevels.Add(1);
        ResizeFireTimes();

        if (_positionMgr == null)
        {
            _positionMgr = GetComponentInParent<WeaponPositionManager>();
            if (_positionMgr == null) _positionMgr = GetComponent<WeaponPositionManager>();
        }
        _positionMgr?.AddWeaponVisual(w);

        if (debugMode) Debug.Log($"[WeaponV2] Añadida: {w.displayName}");
    }

    public bool TryUpgradeWeapon(int index)
    {
        if (index < 0 || index >= weapons.Count) return false;
        if (index >= weaponLevels.Count) return false;

        if (weaponLevels[index] >= 10)
        {
            SoundManager.Instance?.PlayDeniedBuzz();
            return false;
        }

        int cost = GetUpgradeCost(index);
        if (CoinSystem.Instance == null) return false;

        if (!CoinSystem.Instance.SpendCoins(cost))
        {
            SoundManager.Instance?.PlayDeniedBuzz();
            return false;
        }

        weaponLevels[index]++;

        var w = weapons[index];
        w.projectileDamage += w.damagePerLevel;
        w.fireRate = Mathf.Max(0.05f, w.fireRate * (1f - w.fireRateImprovementPerLevel));

        OnWeaponUpgraded?.Invoke(index);

        SoundManager.Instance?.PlayWeaponLevelUp();

        return true;
    }

    public int GetUpgradeCost(int index)
    {
        if (index < 0 || index >= weapons.Count) return 0;
        if (index >= weaponLevels.Count) return 0;
        var w = weapons[index];
        int level = weaponLevels[index];
        int baseCost = w.cost > 0 ? w.cost / 2 : 50;
        return Mathf.RoundToInt(baseCost * Mathf.Pow(1.4f, level - 1));
    }

    public int GetWeaponLevel(int index)
    {
        if (index < 0 || index >= weaponLevels.Count) return 1;
        return weaponLevels[index];
    }

    public bool IsMaxLevel(int index) => GetWeaponLevel(index) >= 10;

    public static WeaponDefinition CloneConfig(WeaponDefinition o)
    {
        return new WeaponDefinition
        {
            id = o.id,
            displayName = o.displayName,
            description = o.description,
            weaponType = o.weaponType,
            rarity = o.rarity,
            isGadget = o.isGadget,
            cost = o.cost,
            specialPower = o.specialPower,
            fireMode = o.fireMode,
            burstInterval = o.burstInterval,
            visualPrefab = o.visualPrefab,
            icon = o.icon,
            projectilePrefab = o.projectilePrefab,
            gadgetPrefab = o.gadgetPrefab,
            spriteScale = o.spriteScale,
            firePointOffset = o.firePointOffset,
            projectileDamage = o.projectileDamage,
            fireRate = o.fireRate,
            projectileSpeed = o.projectileSpeed,
            shotCount = o.shotCount,
            spreadAngle = o.spreadAngle,
            range = o.range,
            damagePerLevel = o.damagePerLevel,
            fireRateImprovementPerLevel = o.fireRateImprovementPerLevel,
            procChanceBoostPerLevel = o.procChanceBoostPerLevel,
            baseProcChance = o.baseProcChance,
            knockbackForce = o.knockbackForce,
            aoeRadius = o.aoeRadius,
            dotDamage = o.dotDamage,
            effectDuration = o.effectDuration,
        };
    }

    void OnDrawGizmos()
    {
        if (!alwaysShowGizmos) return;
        Gizmos.color = meleeGizmoColor;
        DrawCircle(transform.position, meleeBaseRadius);
        Gizmos.color = rangedGizmoColor;
        DrawCircle(transform.position, GetEffectiveRangedRange());
    }

    void DrawCircle(Vector3 c, float r)
    {
        const int seg = 48;
        Vector3 prev = c + new Vector3(r, 0f, 0f);
        for (int i = 1; i <= seg; i++)
        {
            float a = (i / (float)seg) * Mathf.PI * 2f;
            Vector3 next = c + new Vector3(Mathf.Cos(a) * r, Mathf.Sin(a) * r, 0f);
            Gizmos.DrawLine(prev, next);
            prev = next;
        }
    }
}