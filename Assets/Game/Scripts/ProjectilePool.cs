using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : MonoBehaviour
{
    [SerializeField] Projectile prefab;
    [SerializeField] int prewarmCount = 30;

    readonly Queue<Projectile> free = new Queue<Projectile>();

    void Awake()
    {
        for (int i = 0; i < prewarmCount; i++)
            free.Enqueue(Create());
    }

    Projectile Create()
    {
        var p = Instantiate(prefab, transform);
        p.gameObject.SetActive(false);
        return p;
    }

    public Projectile Get() => free.Count > 0 ? free.Dequeue() : Create();

    public void Release(Projectile p)
    {
        p.gameObject.SetActive(false);
        free.Enqueue(p);
    }
}
