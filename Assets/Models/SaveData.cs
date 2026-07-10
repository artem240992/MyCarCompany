using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // ---- Экономика ----
    public double money;
    public int conveyorLevel;
    public int engineerCount;
    public int reputation;
    public double passiveIncome;
    public float basePriceMultiplier;

    // ---- Время ----
    public int currentMonth;
    public int currentYear;
    public int lastTaxYear;

    // ---- Технологии ----
    public string[] researchedTechNames; // для обратной совместимости
    public List<TechnologySaveData> technologyData;

    // ---- Машины ----
    public List<CarLevelData> carLevels;
    public List<CarBlueprintSaveData> createdCarsData;

    // ---- Логи действий конкурентов ----
    public List<ActionLogEntry> actionLogs;

    // ---- Достижения ----
    public List<AchievementProgress> achievementProgress;

    // ---- Сложность ----
    public int difficulty; // 0=Easy, 1=Normal, 2=Hard
}

[Serializable]
public class CarLevelData
{
    public string carName;
    public int currentLevel;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}

[Serializable]
public class CarBlueprintSaveData
{
    public string carName;
    public int currentLevel;
    public int currentPrice;
    public int tuningPower;
    public int tuningEconomy;
    public int tuningDesign;
    public int tuningSafety;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public float demandMultiplier;
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}

[Serializable]
public class TechnologySaveData
{
    public string techName;
    public bool isResearched;
    public int availableYear;
    public int availableMonth;
}