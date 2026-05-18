using UnityEngine;
using DG.Tweening;

// ============================================================
// MELEE ATTACK REPOSITION
// Va en el prefab del arma melee. Cuando un enemigo entra en su
// radio, el arma hace un "lunge" (estocada) temporalmente hacia él.
// Tras atacar, vuelve a su posición orbital normal.
//
// Funciona EN COMBINACIÓN con WeaponPositionManager.
// El manager pone la posición base; este script añade un offset temporal.
// ============================================================
public class MeleeAttackReposition : MonoBehaviour
{
    [Header("─── DETECCIÓN ──")]
    [Tooltip("Radio donde el arma detecta enemigos para hacer lunge")]
    public float detectRadius = 2.0f;

    [Header("─── LUNGE (estocada) ──")]
    [Tooltip("Cuánta distancia se mueve el arma hacia el enemigo")]
    public float lungeDistance = 0.6f;

    [Tooltip("Tiempo del lunge (ida)")]
    public float lungeDuration = 0.15f;

    [Tooltip("Tiempo de vuelta a posición original")]
    public float returnDuration = 0.25f;

    [Tooltip("Cooldown entre lunges")]
    public float cooldown = 0.4f;

    [Header("─── DEBUG ──")]
    public bool showDetectGizmo = true;

    Tween _lungeTween;
    Vector3 _localBasePosition;
    bool _isLunging = false;
    float _lastLungeTime;

    void Update()
    {
        if (_isLunging) return;
        if (Time.time < _lastLungeTime + cooldown) return;

        // Buscar enemigo cercano
        var enemyT = FindNearestEnemyInRadius();
        if (enemyT == null) return;

        // Hacer lunge
        StartLunge(enemyT.position);
    }

    Transform FindNearestEnemyInRadius()
    {
        Transform closest = null;
        float minDist = float.MaxValue;

        var enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var e in enemies)
        {
            if (e == null || !e.activeInHierarchy) continue;
            float d = Vector2.Distance(transform.position, e.transform.position);
            if (d <= detectRadius && d < minDist)
            {
                minDist = d;
                closest = e.transform;
            }
        }
        return closest;
    }

    void StartLunge(Vector3 targetWorldPos)
    {
        _isLunging = true;
        _lastLungeTime = Time.time;

        // Guardar posición base actual (la que pone WeaponPositionManager)
        _localBasePosition = transform.localPosition;

        // Calcular dirección hacia el enemigo (en local)
        Vector3 dirWorld = (targetWorldPos - transform.position).normalized;
        Vector3 lungeOffset = dirWorld * lungeDistance;

        // Convertir a local (parent's space)
        Vector3 lungeTarget = _localBasePosition + transform.parent.InverseTransformDirection(lungeOffset);

        _lungeTween?.Kill();

        var seq = DOTween.Sequence();
        seq.Append(transform.DOLocalMove(lungeTarget, lungeDuration).SetEase(Ease.OutCubic));
        seq.Append(transform.DOLocalMove(_localBasePosition, returnDuration).SetEase(Ease.InOutSine));
        seq.OnComplete(() => _isLunging = false);
        _lungeTween = seq;
    }

    void OnDisable()
    {
        _lungeTween?.Kill();
        _isLunging = false;
    }

    void OnDrawGizmos()
    {
        if (!showDetectGizmo) return;
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}
