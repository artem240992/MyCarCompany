// ScriptableObject с настройками достижений
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "Car Company/Achievement Data")]
public class AchievementData : ScriptableObject
{
    public string achievementId;
    public string title;
    public string description;
    public int targetValue;              // цель (например, 10000 денег)
    public string trackedMetric;         // что отслеживаем: "money", "carsProduced", "techsResearched", ...
    public Sprite icon;
    public bool hiddenUntilUnlocked;     // показывать только после получения?
}