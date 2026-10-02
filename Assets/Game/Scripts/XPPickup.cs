using UnityEngine;

public class XPPickup : MonoBehaviour
{
    [SerializeField] int value = 1;
    [Tooltip("Distance at which XP starts flying to the player")]
    [SerializeField] float magnetRadius = 1.5f;
    [SerializeField] float magnetSpeed = 8f;

    public int Value { get => value; set => this.value = value; }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player == null) return;

        Vector3 target = player.transform.position;
        if ((target - transform.position).sqrMagnitude < magnetRadius * magnetRadius)
            transform.position = Vector3.MoveTowards(transform.position, target, magnetSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent(out PlayerLevel level)) return;
        level.AddXP(value);
        SoundFX.Play(SoundFX.Sfx.Pickup);
        Destroy(gameObject);
    }
}
