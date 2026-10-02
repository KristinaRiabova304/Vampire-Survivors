using UnityEngine;

public class EnemyDeathEffects : MonoBehaviour
{
    [SerializeField] ParticleSystem deathParticlesPrefab;

    void OnEnable() => Enemy.AnyEnemyDied += HandleDeath;
    void OnDisable() => Enemy.AnyEnemyDied -= HandleDeath;

    void HandleDeath(Enemy enemy)
    {
        if (deathParticlesPrefab != null)
        {
            var ps = Instantiate(deathParticlesPrefab, enemy.transform.position, Quaternion.identity);
            var main = ps.main;
            main.startColor = enemy.Color;
            ps.Play();
        }
        SoundFX.Play(SoundFX.Sfx.Death);
    }
}
