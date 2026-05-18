using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

// ============================================================
// WEAPON POSITION MANAGER (v16) — FINAL CON LUNGE MELEE
//
// NUEVO en v16: LUNGE MELEE inteligente
// + Las armas melee detectan enemigos cercanos
// + Se lanzan hacia ellos con DOTween (animación suave)
// + Aplican daño automáticamente vía MeleeHitbox al contacto
// + Vuelven a su posición fija con DOTween
// + Cooldown configurable entre lunges
// + Garantías: OnComplete y OnKill resetean estado siempre
//
// Mantiene todo lo anterior:
// + Wobble + Bob manual (sin DOTween, inmune a timeScale=0)
// + Swing manual (fallback si no hay lunge)
// + LateUpdate defensivo (sobreescribe otros scripts)
// + Time.unscaledTime → funciona aunque haya pausa
// ============================================================
public class WeaponPositionManager : MonoBehaviour
{
    [Header("─── POSICIONES FIJAS ──")]
    public Vector2 frontOffset = new Vector2(0f, 0.6f);
    public Vector2 leftOffset = new Vector2(-0.7f, 0.3f);
    public Vector2 rightOffset = new Vector2(0.7f, 0.3f);
    public Vector2 leftFarOffset = new Vector2(-0.9f, -0.1f);
    public Vector2 rightFarOffset = new Vector2(0.9f, -0.1f);

    [Header("─── ESCALA POR DEFECTO ──")]
    public float defaultScale = 0.5f;

    [Header("─── APUNTADO ──")]
    public float aimRotateSpeed = 720f;

    [Header("─── SWING MELEE (fallback) ──")]
    [Range(10f, 90f)] public float defaultSwingAngle = 70f;
    [Range(0.05f, 0.8f)] public float defaultSwingDuration = 0.35f;

    [Header("─── TAMBALEO IDLE ──")]
    [Range(0f, 30f)] public float wobbleAngle = 12f;
    [Range(0.4f, 4f)] public float wobblePeriod = 1.2f;

    [Header("─── BOBBING VERTICAL ──")]
    [Range(0f, 0.3f)] public float bobAmount = 0.08f;
    [Range(0.4f, 4f)] public float bobPeriod = 1.0f;

    // ═══════════════════════════════════════════════════════════════════
    // ── LUNGE MELEE (NUEVO v16) ──
    // ═══════════════════════════════════════════════════════════════════
    [Header("─── ★ LUNGE MELEE (NUEVO v16) ★ ──")]
    [Tooltip("Distancia máxima a la que el arma melee inicia un lunge")]
    [Range(1f, 6f)] public float lungeRange = 2.5f;

    [Tooltip("Porcentaje del trayecto que recorre hacia el enemigo (0.7 = 70%)")]
    [Range(0.3f, 1.2f)] public float lungeReachPercent = 0.85f;

    [Tooltip("Duración del movimiento hacia el enemigo")]
    [Range(0.05f, 0.5f)] public float lungeOutDuration = 0.12f;

    [Tooltip("Duración del retorno a la posición de reposo")]
    [Range(0.1f, 0.8f)] public float lungeReturnDuration = 0.25f;

    [Tooltip("Tiempo entre lunges de la misma arma")]
    [Range(0.2f, 2f)] public float lungeCooldown = 0.6f;

    [Tooltip("Curva de salida del lunge")]
    public Ease lungeOutEase = Ease.OutQuad;

    [Tooltip("Curva de retorno del lunge")]
    public Ease lungeReturnEase = Ease.InOutQuad;

    [Header("─── DEFENSA ──")]
    public bool disableWeaponAnimators = true;
    public bool enforceRotationInLateUpdate = true;

    [Header("─── GIZMOS ──")]
    public bool showPositionMarkers = true;
    public Color markerColor = new Color(1f, 0.9f, 0f, 0.8f);
    public Color firePointColor = new Color(0f, 1f, 0.3f, 1f);
    public Color lungeRangeColor = new Color(1f, 0.3f, 0.3f, 0.3f);
    public bool showFirePointGizmos = true;
    public bool showLungeRangeGizmo = true;

    [Header("─── DEBUG ──")]
    public bool debugMode = false;
    public float debugLogInterval = 2f;

    class WeaponVisual
    {
        public Transform transform;
        public WeaponDefinition config;
        public bool isMelee;
        public float wobbleSeed;
        public float bobSeed;
        public float currentAngle;
        public float swingT;

        // Lunge state (v16)
        public bool lunging;
        public float lastLungeTime;
        public Tween lungeTween;
        public Vector3 restLocalPos;     // posición a la que vuelve
    }

    List<WeaponVisual> _visuals;
    Transform _nearestEnemy;
    float _aimAngle = 0f;
    float _nextDebugLog;

    public int VisualCount => _visuals != null ? _visuals.Count : 0;

    // ═══════════════════════════════════════════════════════════════════
    void Awake()
    {
        _visuals = new List<WeaponVisual>();
        _aimAngle = 0f;
        _nearestEnemy = null;

        if (debugMode)
            Debug.Log("✅ [WeaponPos v16] Awake — Lunge melee disponible.");
    }

    void OnEnable()
    {
        if (_visuals == null)
            _visuals = new List<WeaponVisual>();
    }

    void OnDestroy()
    {
        KillAllLungeTweens();
    }

    void OnDisable()
    {
        KillAllLungeTweens();
    }

    void KillAllLungeTweens()
    {
        if (_visuals == null) return;
        foreach (var v in _visuals)
        {
            if (v == null) continue;
            v.lungeTween?.Kill();
            v.lungeTween = null;
            v.lunging = false;
        }
    }

    void Update()
    {
        if (_visuals == null) return;

        // No actualizar si juego pausado (pero DOTween puede seguir si SetUpdate(true))
        bool paused = GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused;
        if (paused) return;

        if (debugMode && Time.unscaledTime > _nextDebugLog)
        {
            _nextDebugLog = Time.unscaledTime + debugLogInterval;
            Debug.Log($"🔄 [WeaponPos v16] Update. Armas: {_visuals.Count}. " +
                      $"aimAngle: {_aimAngle:F1}");
        }

        FindNearestEnemy();

        if (_nearestEnemy != null)
        {
            Vector2 dir = (Vector2)_nearestEnemy.position - (Vector2)transform.position;
            float target = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            float dt = Time.unscaledDeltaTime;
            _aimAngle = Mathf.MoveTowardsAngle(_aimAngle, target, aimRotateSpeed * dt);
        }

        if (_visuals.Count == 0) return;

        for (int i = 0; i < _visuals.Count; i++)
        {
            var v = _visuals[i];
            if (v == null || v.transform == null) continue;

            // Actualizar la posición de reposo (puede cambiar si se añaden/quitan armas)
            v.restLocalPos = (Vector3)GetAutoLayoutPosition(i, _visuals.Count);

            // ── LUNGE: comprobar si melee + enemigo cercano + no en lunge + cooldown OK ──
            if (v.isMelee && !v.lunging
                && Time.unscaledTime - v.lastLungeTime >= lungeCooldown)
            {
                Transform enemyNear = FindEnemyNearWeapon(v, lungeRange);
                if (enemyNear != null)
                {
                    LaunchLunge(v, i, enemyNear.position);
                    continue; // saltar wobble este frame
                }
            }

            // Si está en lunge, DOTween controla la posición/rotación → no tocar
            if (v.lunging) continue;

            // Animación idle normal (wobble + bob + swing)
            UpdateWeaponVisual(v, i);
        }
    }

    void LateUpdate()
    {
        if (!enforceRotationInLateUpdate) return;
        if (_visuals == null) return;

        bool paused = GameManagerRogueLite.Instance != null && GameManagerRogueLite.Instance.IsPaused;
        if (paused) return;

        for (int i = 0; i < _visuals.Count; i++)
        {
            var v = _visuals[i];
            if (v == null || v.transform == null) continue;

            // Respetar DOTween si está en lunge
            if (v.lunging) continue;

            UpdateWeaponVisual(v, i);
        }
    }

    void UpdateWeaponVisual(WeaponVisual v, int i)
    {
        // ── POSICIÓN base + BOBBING vertical ──
        Vector2 baseOffset = GetAutoLayoutPosition(i, _visuals.Count);
        v.restLocalPos = (Vector3)baseOffset;

        float bobOffset = 0f;
        if (bobAmount > 0f)
        {
            bobOffset = Mathf.Sin(
                (Time.unscaledTime + v.bobSeed) * 2f * Mathf.PI / bobPeriod
            ) * bobAmount;
        }

        v.transform.localPosition = new Vector3(baseOffset.x, baseOffset.y + bobOffset, 0f);

        // ── ESCALA ──
        float scale = (v.config != null && v.config.spriteScale > 0f)
                      ? v.config.spriteScale : defaultScale;
        v.transform.localScale = Vector3.one * scale;

        // ── WOBBLE de rotación ──
        float wobbleOffset = 0f;
        if (wobbleAngle > 0f)
        {
            wobbleOffset = Mathf.Sin(
                (Time.unscaledTime + v.wobbleSeed) * 2f * Mathf.PI / wobblePeriod
            ) * wobbleAngle;
        }

        float angle = _aimAngle + wobbleOffset;
        v.currentAngle = angle;

        // ── SWING manual (solo si no hay lunge en curso) ──
        if (v.swingT >= 0f)
        {
            v.swingT += Time.unscaledDeltaTime / Mathf.Max(0.01f, defaultSwingDuration);

            if (v.swingT >= 1f)
            {
                v.swingT = -1f;
            }
            else
            {
                float swingDelta = Mathf.Sin(v.swingT * Mathf.PI) * defaultSwingAngle;
                v.transform.localRotation = Quaternion.Euler(0f, 0f, angle + swingDelta);
                return;
            }
        }

        v.transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ★ LUNGE MELEE ★ (NUEVO v16, con DOTween)
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Busca el enemigo más cercano a la POSICIÓN ACTUAL del arma (no del player).
    /// </summary>
    Transform FindEnemyNearWeapon(WeaponVisual v, float maxDist)
    {
        if (v == null || v.transform == null) return null;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = float.MaxValue;
        Vector3 weaponPos = v.transform.position;

        foreach (var e in enemies)
        {
            if (e == null || !e.activeInHierarchy) continue;
            float d = Vector2.Distance(weaponPos, e.transform.position);
            if (d < maxDist && d < minDist)
            {
                minDist = d;
                closest = e.transform;
            }
        }
        return closest;
    }

    /// <summary>
    /// Lanza el arma hacia el enemigo con DOTween, y la hace volver.
    /// El daño se aplica automáticamente vía MeleeHitbox al contacto.
    /// </summary>
    void LaunchLunge(WeaponVisual v, int index, Vector3 enemyWorldPos)
    {
        if (v == null || v.transform == null) return;

        // Matar tween anterior por seguridad
        v.lungeTween?.Kill();

        v.lunging = true;
        v.lastLungeTime = Time.unscaledTime;

        // Posición de reposo (calculada por layout actual)
        Vector3 restPos = (Vector3)GetAutoLayoutPosition(index, _visuals.Count);
        v.restLocalPos = restPos;

        // Convertir posición del enemigo a local (relativa al manager)
        Vector3 enemyLocalPos = transform.InverseTransformPoint(enemyWorldPos);

        // Punto objetivo = entre rest y enemy según reachPercent
        Vector3 lungeTarget = Vector3.Lerp(restPos, enemyLocalPos, lungeReachPercent);

        // Ángulo del arma para "apuntar" al enemigo
        Vector2 toEnemy = (Vector2)(enemyLocalPos - restPos);
        float targetAngle = Mathf.Atan2(toEnemy.y, toEnemy.x) * Mathf.Rad2Deg;

        if (debugMode)
            Debug.Log($"⚔️ [Lunge] {v.config?.displayName} → enemy local {enemyLocalPos} (target {lungeTarget})");

        // ─── DOTween Sequence ───
        var seq = DOTween.Sequence().SetUpdate(false); // afectado por timeScale (se pausa con pausa)

        // Fase 1: ir hacia el enemigo (move + rotate juntos)
        seq.Append(v.transform.DOLocalMove(lungeTarget, lungeOutDuration).SetEase(lungeOutEase));
        seq.Join(v.transform.DOLocalRotate(new Vector3(0f, 0f, targetAngle), lungeOutDuration));

        // Fase 2: volver a la posición de reposo
        seq.Append(v.transform.DOLocalMove(restPos, lungeReturnDuration).SetEase(lungeReturnEase));

        // GARANTÍAS de reset del flag
        seq.OnComplete(() =>
        {
            v.lunging = false;
            if (debugMode) Debug.Log($"✅ [Lunge] {v.config?.displayName} completado");
        });

        seq.OnKill(() =>
        {
            v.lunging = false;
            // Restaurar posición por si el tween se mató a mitad
            if (v.transform != null) v.transform.localPosition = v.restLocalPos;
        });

        v.lungeTween = seq;
    }

    // ═══════════════════════════════════════════════════════════════════
    // LAYOUT
    // ═══════════════════════════════════════════════════════════════════
    Vector2 GetAutoLayoutPosition(int index, int total)
    {
        switch (total)
        {
            case 1: return frontOffset;
            case 2: return index == 0 ? leftOffset : rightOffset;
            case 3:
                if (index == 0) return leftOffset;
                if (index == 1) return frontOffset;
                return rightOffset;
            case 4:
                if (index == 0) return leftFarOffset;
                if (index == 1) return leftOffset;
                if (index == 2) return rightOffset;
                return rightFarOffset;
            case 5:
                if (index == 0) return leftFarOffset;
                if (index == 1) return leftOffset;
                if (index == 2) return frontOffset;
                if (index == 3) return rightOffset;
                return rightFarOffset;
            default: return frontOffset;
        }
    }

    void FindNearestEnemy()
    {
        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float closest = float.MaxValue;
        _nearestEnemy = null;
        foreach (var e in enemies)
        {
            if (e == null || !e.activeInHierarchy) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d < closest) { closest = d; _nearestEnemy = e.transform; }
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // API PÚBLICA (sin cambios)
    // ═══════════════════════════════════════════════════════════════════
    public Vector3 GetWeaponWorldPosition(int weaponIndex)
    {
        if (_visuals == null || weaponIndex < 0 || weaponIndex >= _visuals.Count) return transform.position;
        var v = _visuals[weaponIndex];
        return v.transform != null ? v.transform.position : transform.position;
    }

    public float GetWeaponAngle(int weaponIndex)
    {
        if (_visuals == null || weaponIndex < 0 || weaponIndex >= _visuals.Count) return 0f;
        return _visuals[weaponIndex].currentAngle;
    }

    public Vector3 GetWeaponFirePosition(int weaponIndex)
    {
        if (_visuals == null || weaponIndex < 0 || weaponIndex >= _visuals.Count) return transform.position;

        var v = _visuals[weaponIndex];
        if (v.transform == null || v.config == null) return transform.position;

        float rad = v.currentAngle * Mathf.Deg2Rad;
        Vector2 offset = v.config.firePointOffset;
        Vector2 rotated = new Vector2(
            offset.x * Mathf.Cos(rad) - offset.y * Mathf.Sin(rad),
            offset.x * Mathf.Sin(rad) + offset.y * Mathf.Cos(rad));

        return v.transform.position + (Vector3)rotated;
    }

    public void AddWeaponVisual(WeaponDefinition config)
    {
        if (config == null) return;
        if (_visuals == null) _visuals = new List<WeaponVisual>();

        if (config.visualPrefab == null)
        {
            Debug.LogWarning($"⚠️ [WeaponPos] '{config.displayName}' SIN visualPrefab.");
            return;
        }

        var go = Instantiate(config.visualPrefab, transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        float scale = config.spriteScale > 0f ? config.spriteScale : defaultScale;
        go.transform.localScale = Vector3.one * scale;

        if (disableWeaponAnimators)
        {
            var animators = go.GetComponentsInChildren<Animator>(true);
            foreach (var a in animators)
                if (a != null) a.enabled = false;
        }

        var melee = go.GetComponent<MeleeHitbox>();
        var col = go.GetComponent<Collider2D>();
        if (col != null && melee == null) col.enabled = false;
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb != null && melee == null) Destroy(rb);

        var sr = go.GetComponent<SpriteRenderer>();
        if (sr == null) sr = go.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.sortingLayerName = "Default";
            sr.sortingOrder = 5;
            if (sr.color.a < 0.5f) { Color c = sr.color; c.a = 1f; sr.color = c; }
        }

        var v = new WeaponVisual
        {
            transform = go.transform,
            config = config,
            isMelee = config.weaponType == WeaponType.Melee,
            wobbleSeed = UnityEngine.Random.Range(0f, 10f),
            bobSeed = UnityEngine.Random.Range(0f, 10f),
            currentAngle = 0f,
            swingT = -1f,
            lunging = false,
            lastLungeTime = -999f,
            lungeTween = null,
            restLocalPos = Vector3.zero,
        };
        _visuals.Add(v);

        if (debugMode)
            Debug.Log($"✅ [WeaponPos] Añadido: {config.displayName} (total: {_visuals.Count})");
    }

    public void RemoveWeaponVisual(int index)
    {
        if (_visuals == null || index < 0 || index >= _visuals.Count) return;
        var v = _visuals[index];
        v.lungeTween?.Kill();
        if (v.transform != null) Destroy(v.transform.gameObject);
        _visuals.RemoveAt(index);
    }

    public int FindWeaponIndexByTransform(Transform t)
    {
        if (_visuals == null || t == null) return -1;
        for (int i = 0; i < _visuals.Count; i++)
        {
            if (_visuals[i] == null || _visuals[i].transform == null) continue;
            if (_visuals[i].transform == t) return i;
            if (t.IsChildOf(_visuals[i].transform)) return i;
        }
        return -1;
    }

    // ─────────────────────────────────────────────────────────────────────
    // SWING (fallback manual)
    // ─────────────────────────────────────────────────────────────────────
    public void TriggerMeleeSwing(int weaponIndex)
    {
        if (_visuals == null || weaponIndex < 0 || weaponIndex >= _visuals.Count) return;
        var v = _visuals[weaponIndex];
        if (!v.isMelee || v.transform == null) return;
        if (v.swingT >= 0f) return;
        if (v.lunging) return; // si está en lunge, no swing
        v.swingT = 0f;
    }

    public void TriggerMeleeSwing()
    {
        if (_visuals == null) return;
        for (int i = 0; i < _visuals.Count; i++)
        {
            var v = _visuals[i];
            if (!v.isMelee || v.transform == null) continue;
            if (v.swingT >= 0f) continue;
            if (v.lunging) continue;
            v.swingT = 0f;
        }
    }

    public void ClearAll()
    {
        if (_visuals == null) return;
        foreach (var v in _visuals)
        {
            v.lungeTween?.Kill();
            if (v != null && v.transform != null) Destroy(v.transform.gameObject);
        }
        _visuals.Clear();
    }

    // ═══════════════════════════════════════════════════════════════════
    // GIZMOS
    // ═══════════════════════════════════════════════════════════════════
    void OnDrawGizmos()
    {
        if (showPositionMarkers)
        {
            Gizmos.color = markerColor;
            Gizmos.DrawWireSphere(transform.position + (Vector3)frontOffset, 0.12f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)leftOffset, 0.12f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)rightOffset, 0.12f);
            Gizmos.color = new Color(markerColor.r, markerColor.g, markerColor.b, 0.4f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)leftFarOffset, 0.12f);
            Gizmos.DrawWireSphere(transform.position + (Vector3)rightFarOffset, 0.12f);
        }

        if (showLungeRangeGizmo)
        {
            Gizmos.color = lungeRangeColor;
            DrawCircleGizmo(transform.position, lungeRange);
        }

        if (showFirePointGizmos && Application.isPlaying && _visuals != null)
        {
            Gizmos.color = firePointColor;
            for (int i = 0; i < _visuals.Count; i++)
            {
                Vector3 firePos = GetWeaponFirePosition(i);
                Gizmos.DrawSphere(firePos, 0.07f);
                Gizmos.DrawLine(GetWeaponWorldPosition(i), firePos);
            }
        }
    }

    void DrawCircleGizmo(Vector3 c, float r)
    {
        const int seg = 32;
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