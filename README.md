# Survivors — прототип за референсом Vampire Survivors

Unity 6 (6000.6.4f1), 2D, вбудований render pipeline, старий Input Manager.

## Як запустити
1. Unity Hub → **Add** → папка проєкту (`SurvGame-main`).
2. Відкрити сцену `Assets/Game/Scenes/Game.unity` → **Play**.
3. Керування: **WASD / стрілки**. Зброя стріляє автоматично. Після смерті — кнопка **Restart** або **R**.

Якщо сцену потрібно перезібрати з нуля: меню **Tools → Survivors → Build Game**
(перезапише сцену, спрайти та префаби).

## Де що зроблено

| Пункт | Що | Де |
|---|---|---|
| 1. Player | SpriteRenderer + BoxCollider2D + Rigidbody2D, рух X/Y, `moveSpeed` в Inspector, `freezeRotation` | `PlayerController.cs`, об'єкт `Player` |
| 2. Enemy | Префаби зі SpriteRenderer + Collider2D, рухаються до гравця, `moveSpeed` в Inspector | `Enemy.cs`, `Prefabs/Enemy_*.prefab` |
| 3. Health і Damage | HP у гравця та ворогів, ворог зникає при HP ≤ 0, урон при контакті (раз на `damageInterval`), смерть гравця → Game Over | `Health.cs`, `Enemy.cs` |
| 4. Automatic Weapon | Projectile-префаб, постріл кожні `attackInterval` в найближчого ворога, зникає після влучання або `lifetime`; `damage`, `projectileSpeed`, `attackInterval` в Inspector | `AutoWeapon.cs`, `Projectile.cs` |
| 5. Enemy Spawner | Спавн навколо гравця, `Spawn Interval`, `min/max Spawn Distance` | `EnemySpawner.cs` |
| 6. Gameplay Loop | Score + Kills, Game Over, Restart, вороги з'являються одразу при старті (`initialEnemies`) | `GameManager.cs` |
| **Гілка A** | | |
| 7A. HUD | HP Bar, Score, Timer | `GameManager.cs`, `UIBar.cs`, Canvas |
| 8A. XP | XP pickup випадає з ворога, збір, XP Bar, Level | `XPPickup.cs`, `XPDropper.cs`, `PlayerLevel.cs` |
| 9A. Level Up | Пауза (`Time.timeScale = 0`) + 3 кнопки: +Damage, +Attack Speed, +Movement Speed | `GameManager.cs`, `LevelUpPanel` |
| **Гілка B** | | |
| 7B. Spawn Distribution | Випадкова точка на кільці між min і max дистанцією | `EnemySpawner.Spawn()` |
| 8B. Enemy Types | `Enemy_Normal`, `Enemy_Fast`, `Enemy_Tank`, вибір за вагами | `EnemySpawner.PickType()` |
| 9B. Difficulty | З часом: інтервал спавну ↓, вага Fast/Tank ↑, ліміт активних ворогів ↑ (`rampDuration`) | `EnemySpawner.Update()` |
| 10. Hit Feedback | Спалах кольором + scale punch + knockback | `Health.cs`, `Enemy.OnDamaged()` |
| 11. Death Polish | Particle System (колір ворога), анімація смерті (стиснення + обертання), звук | `EnemyDeathEffects.cs`, `Enemy.DeathAnimation()`, `SoundFX.cs` |
| 12. Advanced System | **Object Pool** для projectiles + **event-based Enemy Death** (`Enemy.AnyEnemyDied`: на нього підписані рахунок, випадання XP та ефекти) | `ProjectilePool.cs`, `Enemy.cs` |

Зроблено обидві гілки (A і B) — яка б не дісталась, вона вже є.

## Типи ворогів (8B)

| Тип | HP | Швидкість | Урон | Очки | XP | Вага на початку → наприкінці |
|---|---|---|---|---|---|---|
| Normal (червоний) | 3 | 2.0 | 1 | 1 | 1 | 1 → 1 |
| Fast (жовтий, маленький) | 1 | 3.6 | 1 | 2 | 1 | 0.15 → 0.9 |
| Tank (фіолетовий, великий) | 12 | 1.1 | 2 | 5 | 3 | 0.05 → 0.6 |

Усі значення можна змінювати в Inspector на префабах і на об'єкті `EnemySpawner`.

## Звуки
Звуки генеруються кодом (`SoundFX.cs`), щоб не були потрібні файли. Власні `.wav` можна перетягнути
в поля компонента `SoundFX` на `Main Camera` — тоді використовуватимуться вони.

## Використання AI
Опис того, де і як саме використовувався AI-асистент під час роботи над проєктом — у файлі
[`ai_usage.txt`](ai_usage.txt).
