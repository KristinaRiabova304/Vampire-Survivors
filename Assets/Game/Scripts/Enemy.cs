using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Health))]
public class Enemy : MonoBehaviour
{
    public static readonly List<Enemy> All = new List<Enemy>();
    public static event Action<Enemy> AnyEnemyDied;

    [SerializeField] float moveSpeed = 2f;
    [SerializeField] int contactDamage = 1;
    [SerializeField] float damageInterval = 0.5f;
    [SerializeField] int scoreValue = 1;
    [SerializeField] int xpValue = 1;

    [Header("Feedback")]
    [SerializeField] float knockbackSpeed = 6f;
    [SerializeField] float knockbackTime = 0.1f;
    [SerializeField] float deathAnimTime = 0.2f;

    public int ScoreValue => scoreValue;
    public int XPValue => xpValue;
    public bool IsDead => health.IsDead;
    public Color Color { get; private set; } = Color.white;

    Rigidbody2D rb;
    Health health;
    float nextDamageTime;
    float knockbackUntil;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        health = GetComponent<Health>();
        health.Damaged += OnDamaged;
        health.Died += OnDied;

        var sr = GetComponent<SpriteRenderer>();
        if (sr != null) Color = sr.color;
    }

    void OnEnable() => All.Add(this);
    void OnDisable() => All.Remove(this);

    void FixedUpdate()
    {
        if (health.IsDead || Time.time < knockbackUntil) return;

        var player = PlayerController.Instance;
        if (player == null || !player.enabled)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = ((Vector2)player.transform.position - rb.position).normalized;
        rb.linearVelocity = dir * moveSpeed;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (health.IsDead || Time.time < nextDamageTime) return;
        if (!collision.collider.TryGetComponent(out PlayerController _)) return;
        if (!collision.collider.TryGetComponent(out Health playerHealth)) return;

        playerHealth.TakeDamage(contactDamage);
        nextDamageTime = Time.time + damageInterval;
    }

    void OnDamaged(int amount)
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        Vector2 away = (rb.position - (Vector2)player.transform.position).normalized;
        rb.linearVelocity = away * knockbackSpeed;
        knockbackUntil = Time.time + knockbackTime;
    }

    void OnDied()
    {
        AnyEnemyDied?.Invoke(this);

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        Vector3 startScale = transform.localScale;
        float t = 0f;
        while (t < deathAnimTime)
        {
            t += Time.deltaTime;
            float k = t / deathAnimTime;
            transform.localScale = startScale * (1f + 0.3f * Mathf.Sin(k * Mathf.PI) - k);
            transform.Rotate(0f, 0f, 720f * Time.deltaTime);
            yield return null;
        }
        Destroy(gameObject);
    }
}
