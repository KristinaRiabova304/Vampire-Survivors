using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHP = 3;

    [Header("Hit Feedback")]
    [SerializeField] Color flashColor = Color.white;
    [SerializeField] float flashTime = 0.08f;
    [SerializeField] float punchScale = 1.25f;

    public int MaxHP => maxHP;
    public int CurrentHP { get; private set; }
    public bool IsDead { get; private set; }

    public event Action<int> Damaged;
    public event Action Died;

    SpriteRenderer sr;
    Color baseColor;
    Vector3 baseScale;
    Coroutine feedback;

    void Awake()
    {
        CurrentHP = maxHP;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null) baseColor = sr.color;
        baseScale = transform.localScale;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead || amount <= 0) return;

        CurrentHP = Mathf.Max(0, CurrentHP - amount);
        Damaged?.Invoke(amount);

        if (CurrentHP <= 0)
        {
            IsDead = true;
            StopFeedback();
            Died?.Invoke();
            return;
        }

        StopFeedback();
        feedback = StartCoroutine(HitFeedback());
    }

    public void Heal(int amount)
    {
        if (IsDead) return;
        CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
    }

    void StopFeedback()
    {
        if (feedback != null) StopCoroutine(feedback);
        feedback = null;
        if (sr != null) sr.color = baseColor;
        transform.localScale = baseScale;
    }

    IEnumerator HitFeedback()
    {
        if (sr != null) sr.color = flashColor;

        float t = 0f;
        while (t < flashTime)
        {
            t += Time.deltaTime;
            float k = 1f - t / flashTime;
            transform.localScale = baseScale * Mathf.Lerp(1f, punchScale, k);
            yield return null;
        }

        if (sr != null) sr.color = baseColor;
        transform.localScale = baseScale;
        feedback = null;
    }
}
