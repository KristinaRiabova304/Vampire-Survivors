using UnityEngine;

public class XPDropper : MonoBehaviour
{
    [SerializeField] XPPickup xpPrefab;

    void OnEnable() => Enemy.AnyEnemyDied += Drop;
    void OnDisable() => Enemy.AnyEnemyDied -= Drop;

    void Drop(Enemy enemy)
    {
        if (xpPrefab == null || enemy.XPValue <= 0) return;
        var xp = Instantiate(xpPrefab, enemy.transform.position, Quaternion.Euler(0f, 0f, 45f));
        xp.Value = enemy.XPValue;
    }
}
