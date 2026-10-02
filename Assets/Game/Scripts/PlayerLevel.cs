using System;
using UnityEngine;

public class PlayerLevel : MonoBehaviour
{
    [SerializeField] int xpToFirstLevel = 5;
    [SerializeField] int xpGrowthPerLevel = 3;

    public int Level { get; private set; } = 1;
    public int XP { get; private set; }
    public int XPToNext { get; private set; }
    public float Progress01 => XPToNext > 0 ? (float)XP / XPToNext : 0f;

    public event Action LeveledUp;

    void Awake() => XPToNext = xpToFirstLevel;

    public void AddXP(int amount)
    {
        XP += amount;
        while (XP >= XPToNext)
        {
            XP -= XPToNext;
            Level++;
            XPToNext += xpGrowthPerLevel;
            LeveledUp?.Invoke();
        }
    }
}
