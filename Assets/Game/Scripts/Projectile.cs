using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    [SerializeField] float lifetime = 3f;

    Rigidbody2D rb;
    ProjectilePool pool;
    int damage;
    float despawnTime;
    bool active;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void Launch(Vector2 position, Vector2 direction, float speed, int dmg, ProjectilePool owner)
    {
        pool = owner;
        damage = dmg;
        transform.position = position;
        transform.right = direction;
        gameObject.SetActive(true);
        rb.position = position;
        rb.linearVelocity = direction * speed;
        despawnTime = Time.time + lifetime;
        active = true;
    }

    void Update()
    {
        if (active && Time.time >= despawnTime) Despawn();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;
        var enemy = other.GetComponent<Enemy>();
        if (enemy == null || enemy.IsDead) return;

        enemy.GetComponent<Health>().TakeDamage(damage);
        SoundFX.Play(SoundFX.Sfx.Hit);
        Despawn();
    }

    void Despawn()
    {
        active = false;
        rb.linearVelocity = Vector2.zero;
        if (pool != null) pool.Release(this);
        else Destroy(gameObject);
    }
}
