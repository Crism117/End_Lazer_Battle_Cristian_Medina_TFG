using UnityEngine;
using System.Collections;

// ============================================================
// LASER BEAM
// Rayo instantáneo directo entre el arma y el enemigo.
// Usa LineRenderer para la línea y un ParticleSystem
// para el destello de impacto.
//
// SETUP DEL PREFAB:
//   1. GameObject vacío → nombre: LaserBeamPrefab
//   2. Add Component → LineRenderer
//      - Material: Sprite-Default (o un Unlit/Transparent)
//      - Width Curve: 0.15 constante o ligeramente cónico
//      - Color Gradient: color sólido → alpha 0 al final
//      - Use World Space: ✓
//      - Positions: 2 (se asignan por código)
//   3. Add Component → LaserBeam (este script)
//   4. (Opcional) Crea un hijo "ImpactFX" con ParticleSystem
//      y asígnalo al campo impactParticles
// ============================================================
[RequireComponent(typeof(LineRenderer))]
public class LaserBeam : MonoBehaviour
{
    [Header("─── RAYO ──────────────────────────")]
    [Tooltip("Tiempo que el rayo permanece visible")]
    public float beamDuration = 0.12f;

    [Tooltip("Si true, el rayo se desvanece hacia el final de su vida")]
    public bool fadeOut = true;

    [Header("─── IMPACTO ────────────────────────")]
    [Tooltip("ParticleSystem hijo que se activa en el punto de impacto (opcional)")]
    public ParticleSystem impactParticles;

    [Tooltip("Luz puntual en el impacto (opcional, requiere Light 2D o PointLight)")]
    public Light impactLight;

    [Header("─── GROSOR ANIMADO ─────────────────")]
    [Tooltip("El rayo arranca grueso y se adelgaza al desvanecerse")]
    public bool animateWidth = true;
    public float startWidthMax = 0.25f;
    public float endWidthMax = 0.12f;

    // ─────────────────────────────────────────────────────────────────────
    LineRenderer _lr;
    Color _startColorOriginal;
    Color _endColorOriginal;
    float _aliveTime;
    bool _fired;

    void Awake()
    {
        _lr = GetComponent<LineRenderer>();
        _lr.positionCount = 2;
        _lr.useWorldSpace = true;

        // Guarda los colores originales para el fade
        _startColorOriginal = _lr.startColor;
        _endColorOriginal = _lr.endColor;
    }

    // ─────────────────────────────────────────────────────────────────────
    // DISPARO
    // origin   = posición mundial del arma
    // target   = posición mundial del enemigo (o el punto más lejano en
    //            la dirección si no hay enemigo en ese punto exacto)
    // damage   = daño que aplica
    // ─────────────────────────────────────────────────────────────────────
    public void Fire(Vector3 origin, Vector3 target, int damage)
    {
        _aliveTime = 0f;
        _fired = true;

        // ── Coloca la línea ────────────────────────────────────────────
        _lr.SetPosition(0, origin);
        _lr.SetPosition(1, target);

        // ── Grosor inicial ─────────────────────────────────────────────
        if (animateWidth)
        {
            _lr.startWidth = startWidthMax;
            _lr.endWidth = endWidthMax;
        }

        // ── Activa las partículas de impacto en el punto de llegada ────
        if (impactParticles != null)
        {
            impactParticles.transform.position = target;
            impactParticles.Play();
        }

        // ── Activa la luz de impacto ───────────────────────────────────
        if (impactLight != null)
        {
            impactLight.transform.position = target;
            impactLight.enabled = true;
        }

        // ── Aplica daño al enemigo más cercano al punto de impacto ─────
        var hit = Physics2D.OverlapCircle(target, 0.5f);
        if (hit != null && hit.CompareTag("Enemy"))
        {
            hit.GetComponent<EnemyRogueLite>()?.TakeDamage(damage);
            // Efecto de quemadura opcional
            BurnEffect.Apply(hit.gameObject, dotPerTick: 1, ticks: 3);
        }

        StartCoroutine(BeamLifeRoutine());
    }

    // Versión alternativa: dispara en una dirección hasta el primer enemigo
    // o hasta maxRange si no hay ninguno
    public void FireDirection(Vector3 origin, Vector2 direction,
                               int damage, float maxRange = 15f)
    {
        // Raycast en la dirección para encontrar el punto de impacto
        var hit = Physics2D.Raycast(origin, direction.normalized,
                                    maxRange, LayerMask.GetMask("Enemy", "Default"));

        Vector3 target = hit.collider != null
            ? (Vector3)hit.point
            : origin + (Vector3)(direction.normalized * maxRange);

        Fire(origin, target, damage);

        // Si el raycast tocó un enemigo, aplica daño
        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
            hit.collider.GetComponent<EnemyRogueLite>()?.TakeDamage(damage);
    }

    // ─────────────────────────────────────────────────────────────────────
    IEnumerator BeamLifeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < beamDuration)
        {
            elapsed += Time.deltaTime;
            _aliveTime = elapsed;
            float t = elapsed / beamDuration;

            if (fadeOut)
            {
                // Desvanece el alpha
                Color cs = _startColorOriginal;
                Color ce = _endColorOriginal;
                cs.a = Mathf.Lerp(_startColorOriginal.a, 0f, t);
                ce.a = Mathf.Lerp(_endColorOriginal.a, 0f, t);
                _lr.startColor = cs;
                _lr.endColor = ce;

                // Adelgaza el rayo
                if (animateWidth)
                {
                    _lr.startWidth = Mathf.Lerp(startWidthMax, 0f, t);
                    _lr.endWidth = Mathf.Lerp(endWidthMax, 0f, t);
                }
            }

            yield return null;
        }

        // Apaga la luz al terminar
        if (impactLight != null)
            impactLight.enabled = false;

        Destroy(gameObject);
    }
}