using System;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Serializable]
    public class EnemyType
    {
        public Enemy prefab;
        [Tooltip("Weight (chance) at the start of the game")] public float startWeight = 1f;
        [Tooltip("Weight (chance) at maximum difficulty")] public float endWeight = 1f;
    }

    [SerializeField] EnemyType[] enemyTypes;

    [Header("Spawn area")]
    [SerializeField] float minSpawnDistance = 13f;
    [SerializeField] float maxSpawnDistance = 16f;

    [Header("Difficulty over time")]
    [SerializeField] int initialEnemies = 5;
    [SerializeField] float startSpawnInterval = 1.2f;
    [SerializeField] float minSpawnInterval = 0.2f;
    [SerializeField] int startMaxEnemies = 15;
    [SerializeField] int endMaxEnemies = 150;
    [Tooltip("Seconds until difficulty reaches maximum")]
    [SerializeField] float rampDuration = 180f;

    public float Difficulty01 => Mathf.Clamp01(elapsed / rampDuration);

    float elapsed;
    float timer;

    void Start()
    {
        for (int i = 0; i < initialEnemies; i++) Spawn();
    }

    void Update()
    {
        var player = PlayerController.Instance;
        if (player == null || !player.enabled) return;

        elapsed += Time.deltaTime;
        timer += Time.deltaTime;

        float interval = Mathf.Lerp(startSpawnInterval, minSpawnInterval, Difficulty01);
        int maxEnemies = Mathf.RoundToInt(Mathf.Lerp(startMaxEnemies, endMaxEnemies, Difficulty01));

        if (timer >= interval)
        {
            timer = 0f;
            if (Enemy.All.Count < maxEnemies) Spawn();
        }
    }

    void Spawn()
    {
        var player = PlayerController.Instance;
        if (player == null || enemyTypes == null || enemyTypes.Length == 0) return;

        Vector2 dir = UnityEngine.Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.right;
        float dist = UnityEngine.Random.Range(minSpawnDistance, maxSpawnDistance);
        Vector2 pos = (Vector2)player.transform.position + dir * dist;

        Instantiate(PickType(), pos, Quaternion.identity);
    }

    Enemy PickType()
    {
        float t = Difficulty01;
        float total = 0f;
        foreach (var e in enemyTypes) total += Mathf.Lerp(e.startWeight, e.endWeight, t);

        float r = UnityEngine.Random.Range(0f, total);
        foreach (var e in enemyTypes)
        {
            r -= Mathf.Lerp(e.startWeight, e.endWeight, t);
            if (r <= 0f) return e.prefab;
        }
        return enemyTypes[0].prefab;
    }
}
