using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] Health playerHealth;
    [SerializeField] PlayerController playerController;
    [SerializeField] AutoWeapon playerWeapon;
    [SerializeField] PlayerLevel playerLevel;

    [Header("HUD")]
    [SerializeField] UIBar hpBar;
    [SerializeField] UIBar xpBar;
    [SerializeField] Text scoreText;
    [SerializeField] Text timerText;
    [SerializeField] Text levelText;

    [Header("Game Over")]
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Text gameOverStatsText;
    [SerializeField] Button restartButton;

    [Header("Level Up")]
    [SerializeField] GameObject levelUpPanel;
    [SerializeField] Button damageButton;
    [SerializeField] Button attackSpeedButton;
    [SerializeField] Button moveSpeedButton;

    [Header("Upgrade values")]
    [SerializeField] int damageBonus = 1;
    [SerializeField] float attackIntervalMultiplier = 0.85f;
    [SerializeField] float moveSpeedBonus = 0.6f;

    int score;
    int kills;
    float survivedTime;
    bool isGameOver;
    int pendingLevelUps;

    void OnEnable() => Enemy.AnyEnemyDied += OnEnemyDied;
    void OnDisable() => Enemy.AnyEnemyDied -= OnEnemyDied;

    void Start()
    {
        Time.timeScale = 1f;
        gameOverPanel.SetActive(false);
        levelUpPanel.SetActive(false);

        playerHealth.Died += GameOver;
        playerHealth.Damaged += _ => SoundFX.Play(SoundFX.Sfx.PlayerHurt);
        playerLevel.LeveledUp += OnLevelUp;

        restartButton.onClick.AddListener(Restart);
        damageButton.onClick.AddListener(() => ChooseUpgrade(0));
        attackSpeedButton.onClick.AddListener(() => ChooseUpgrade(1));
        moveSpeedButton.onClick.AddListener(() => ChooseUpgrade(2));
    }

    void Update()
    {
        if (!isGameOver) survivedTime += Time.deltaTime;
        else if (GameInput.RestartPressed()) Restart();

        UpdateHUD();
    }

    void UpdateHUD()
    {
        hpBar.Set((float)playerHealth.CurrentHP / playerHealth.MaxHP);
        xpBar.Set(playerLevel.Progress01);
        scoreText.text = $"Score: {score}\nKills: {kills}";
        timerText.text = FormatTime(survivedTime);
        levelText.text = $"LV {playerLevel.Level}";
    }

    static string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        return $"{s / 60:00}:{s % 60:00}";
    }

    void OnEnemyDied(Enemy enemy)
    {
        if (isGameOver) return;
        kills++;
        score += enemy.ScoreValue;
    }

    void OnLevelUp()
    {
        pendingLevelUps++;
        if (!levelUpPanel.activeSelf) ShowLevelUp();
    }

    void ShowLevelUp()
    {
        if (isGameOver) return;
        SoundFX.Play(SoundFX.Sfx.LevelUp);
        Time.timeScale = 0f;
        levelUpPanel.SetActive(true);
    }

    void ChooseUpgrade(int index)
    {
        switch (index)
        {
            case 0: playerWeapon.Damage += damageBonus; break;
            case 1: playerWeapon.AttackInterval *= attackIntervalMultiplier; break;
            case 2: playerController.MoveSpeed += moveSpeedBonus; break;
        }

        pendingLevelUps--;
        if (pendingLevelUps > 0)
        {
            SoundFX.Play(SoundFX.Sfx.LevelUp);
            return;
        }

        levelUpPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void GameOver()
    {
        isGameOver = true;
        playerController.enabled = false;
        playerWeapon.enabled = false;
        levelUpPanel.SetActive(false);

        gameOverStatsText.text = $"Score: {score}   Kills: {kills}\nTime: {FormatTime(survivedTime)}   Level: {playerLevel.Level}";
        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
