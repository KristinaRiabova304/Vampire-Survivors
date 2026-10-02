using UnityEngine;

public class AutoWeapon : MonoBehaviour
{
    [SerializeField] ProjectilePool pool;
    [SerializeField] int damage = 1;
    [SerializeField] float projectileSpeed = 10f;
    [SerializeField] float attackInterval = 0.8f;
    [SerializeField] float range = 12f;

    public int Damage { get => damage; set => damage = value; }
    public float AttackInterval { get => attackInterval; set => attackInterval = Mathf.Max(0.05f, value); }

    float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < attackInterval) return;

        Enemy target = FindNearestEnemy();
        if (target == null) return;

        timer = 0f;
        Vector2 from = transform.position;
        Vector2 dir = ((Vector2)target.transform.position - from).normalized;
        pool.Get().Launch(from, dir, projectileSpeed, damage, pool);
        SoundFX.Play(SoundFX.Sfx.Shoot);
    }

    Enemy FindNearestEnemy()
    {
        Enemy best = null;
        float bestDist = range * range;
        Vector2 pos = transform.position;

        foreach (var e in Enemy.All)
        {
            if (e.IsDead) continue;
            float d = ((Vector2)e.transform.position - pos).sqrMagnitude;
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }
        return best;
    }
}
