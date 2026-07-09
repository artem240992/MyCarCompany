using System;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public double money;
    public double passiveIncome;
    public int conveyorLevel;
    public int engineerCount;
    public string[] researchedTechNames;
    public int productionCount;
    public int currentDifficulty;
    public List<CarDemandData> carDemands = new List<CarDemandData>();
    public List<CarLevelData> carLevels = new List<CarLevelData>();
    public List<CarBlueprintSaveData> createdCars = new List<CarBlueprintSaveData>();
}

[System.Serializable]
public class CarDemandData
{
    public string carName;
    public float demandMultiplier;
}

[System.Serializable]
public class CarLevelData
{
    public string carName;
    public int currentLevel;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;

    // ---- НОВЫЕ ПОЛЯ ДЛЯ ЦВЕТА И ТОНИРОВКИ ----
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}

[System.Serializable]
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
    // ---- Также добавим цвет и тонировку для созданных машин ----
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}