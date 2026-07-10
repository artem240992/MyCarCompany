// класс для сохранения прогресса
using System;

[Serializable]
public class AchievementProgress
{
    public string achievementId;    // уникальный идентификатор
    public int currentValue;        // текущее значение (например, количество денег)
    public bool isUnlocked;         // получено ли достижение
}