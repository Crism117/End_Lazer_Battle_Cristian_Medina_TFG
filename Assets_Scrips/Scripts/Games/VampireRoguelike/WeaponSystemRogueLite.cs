using UnityEngine;
using System.Collections.Generic;

// ============================================================
// WEAPON SYSTEM
// - RANGED: solo dispara cuando hay un enemigo dentro del rango.
//   Proyectil va recto hacia ese enemigo.
// - MELEE:  daño en zona mediante OverlapCircle cuando hay enemigo
//           dentro del rango melee. No se necesita collider extra
//           en las armas visuales.
// ============================================================
public class WeaponSystemRogueLite : MonoBehaviour
{
    [System.Serializable]
    public class Weapon
    {
        public string name = "Pistola";
        public WeaponType weaponType = WeaponType.Ranged;
        public GameObject projectilePrefab;         // solo Ranged
        public int projectileDamage = 1;
        public float fireRate = 0.45f;
        public float projectileSpeed = 7f;
        public int shotCount = 1;
        public float spreadAngle = 10f;

        [HideInInspector] public float nextFireTime = 0f;
    }

    [Header("Armas equipadas")]
    public List<Weapon> weapons = new List<Weapon>();

    [Header("Punto de spawn de proyectiles")]
    public Transform firePoint;

    [Header("Pool de proyectiles")]
    public ObjectPoolRogueLite projectilePool;

    [Header("Rangos (se mejoran con AttackRange)")]
    [Tooltip("Distancia máxima para activar armas a distancia")]
    public float rangedRadius = 8f;

    [Tooltip("Distancia máxima para activar armas melee")]
    public float meleeRadius = 1.8f;

    [Header("Debug")]
    public bool debugMode = false;

    // Target más cercano dentro del rango ranged
    Transform _rangedTarget;
    // ¿Hay algún enemigo dentro del rango melee?
    bool _hasMeleeEnemy;

    float _lastMeleeTick;
    const float MELEE_TICK = 0.5f;  // daño melee cada 0.5s

    void Start()
    {
        if (firePoint == null) firePoint = transform;
    }

    void Update()
    {
        if (GameManagerRogueLite.Instance != null &&
            GameManagerRogueLite.Instance.IsPaused) return;

        FindTargets();

        float t = Time.time;

        // ── RANGED: solo dispara si hay target ────────────────────────
        if (_rangedTarget != null)
        {
            foreach (var w in weapons)
            {
                if (w.weaponType != WeaponType.Ranged) continue;
                if (w.projectilePrefab == null)
                {
                    if (debugMode) Debug.LogWarning(
                        $"[WeaponSystem] '{w.name}' sin Projectile Prefab");
                    continue;
                }
                if (t >= w.nextFireTime)
                {
                    FireAt(_rangedTarget.position, w);
                    w.nextFireTime = t + w.fireRate;
                }
            }
        }

        // ── MELEE: daño en zona, cada MELEE_TICK segundos ─────────────
        if (_hasMeleeEnemy && t >= _lastMeleeTick + MELEE_TICK)
        {
            ApplyMeleeDamage();
            _lastMeleeTick = t;
        }
    }

    // Busca el enemigo más cercano dentro del rango ranged y si hay
    // alguno en el rango melee
    void FindTargets()
    {
        _rangedTarget = null;
        _hasMeleeEnemy = false;
        float closest = float.MaxValue;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (e == null || !e.activeInHierarchy) continue;

            float d = Vector2.Distance(transform.position, e.transform.position);

            if (d <= meleeRadius)
                _hasMeleeEnemy = true;

            if (d <= rangedRadius && d < closest)
            {
                closest = d;
                _rangedTarget = e.transform;
            }
        }

        if (debugMode && Time.frameCount % 90 == 0)
            Debug.Log($"[WeaponSystem] RangedTarget: {_rangedTarget?.name ?? "null"}" +
                      $" | MeleeEnemy: {_hasMeleeEnemy}");
    }

    // Dispara UNO o VARIOS proyectiles directo hacia 'targetPos'
    void FireAt(Vector3 targetPos, Weapon w)
    {
        Vector2 dir = ((Vector2)targetPos -
                       (Vector2)firePoint.position).normalized;
        float baseAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        if (w.shotCount <= 1)
        {
            SpawnProjectile(dir, w);
        }
        else
        {
            // Escopeta: dispersión en abanico centrada en el enemigo
            float half = (w.shotCount - 1) * w.spreadAngle * 0.5f;
            for (int i = 0; i < w.shotCount; i++)
            {
                float a = (baseAngle - half + i * w.spreadAngle) * Mathf.Deg2Rad;
                Vector2 d2 = new Vector2(Mathf.Cos(a), Mathf.Sin(a));
                SpawnProjectile(d2, w);
            }
        }
    }

    void SpawnProjectile(Vector2 dir, Weapon w)
    {
        GameObject proj = projectilePool != null
            ? projectilePool.Get()
            : Object.Instantiate(w.projectilePrefab,
                                  firePoint.position,
                                  Quaternion.identity);
        if (proj == null) return;

        proj.transform.position = firePoint.position;
        proj.SetActive(true);

        // Apunta la rotación del sprite hacia donde va
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        proj.transform.rotation = Quaternion.Euler(0f, 0f, angle);

        var p = proj.GetComponent<ProjectileRogueLite>();
        if (p != null)
            p.Initialize((Vector3)dir * w.projectileSpeed,
                          w.projectileDamage, projectilePool);
        else
        {
            var rb = proj.GetComponent<Rigidbody2D>();
            if (rb != null) rb.velocity = dir * w.projectileSpeed;
        }
    }

    // Daño en zona melee: todas las armas melee equipadas suman daño
    void ApplyMeleeDamage()
    {
        int totalDmg = 0;
        foreach (var w in weapons)
            if (w.weaponType == WeaponType.Melee)
                totalDmg += w.projectileDamage;

        if (totalDmg <= 0) return;

        // Swing visual
        var visual = GetComponentInParent<WeaponPositionManager>();
        if (visual == null) visual = GetComponent<WeaponPositionManager>();
        visual?.TriggerMeleeSwing();

        // OverlapCircle: sin necesidad de colliders en los sprites
        var hits = Physics2D.OverlapCircleAll(transform.position, meleeRadius);
        foreach (var hit in hits)
        {
            if (!hit.CompareTag("Enemy")) continue;
            hit.GetComponent<EnemyRogueLite>()?.TakeDamage(totalDmg);
        }
    }

    // Gizmos: círculos de rango en la Scene
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.3f, 0.3f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, meleeRadius);
        Gizmos.color = new Color(0.3f, 0.8f, 1f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, rangedRadius);
    }

    // API pública
    public void AddWeapon(Weapon w) => weapons.Add(w);
    public void IncreaseFireRate(float f) { foreach (var w in weapons) w.fireRate = Mathf.Max(0.05f, w.fireRate * f); }
    public void IncreaseRangedRadius(float a) => rangedRadius += a;
    public void IncreaseMeleeRadius(float a) => meleeRadius += a;
}