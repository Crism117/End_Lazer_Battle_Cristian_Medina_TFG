using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class ProjectileRogueLite : MonoBehaviour
{
    private Rigidbody2D rb;
    private int damage = 1;
    private float lifeTime = 3f;
    private ObjectPoolRogueLite pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void Initialize(Vector2 velocity, int dmg, ObjectPoolRogueLite returnPool)
    {
        damage = dmg;
        pool = returnPool;
        rb.velocity = velocity;
        CancelInvoke(nameof(DisableSelf));
        Invoke(nameof(DisableSelf), lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Enemy"))
        {
            EnemyRogueLite e = col.GetComponent<EnemyRogueLite>();
            if (e != null) e.TakeDamage(damage);
            DisableSelf();
        }
    }

    private void DisableSelf()
    {
        CancelInvoke(nameof(DisableSelf));
        gameObject.SetActive(false);
        pool?.Return(gameObject);
    }
}
