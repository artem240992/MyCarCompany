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
    public List<CarBlueprintSaveData> createdCars = new List<CarBlueprintSaveData>(); // созданные версии
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
}

[System.Serializable]
public class CarBlueprintSaveData
{
    public string carName;
    public int currentLevel;
    public int currentPrice;          // цена конкретной версии
    public int tuningPower;
    public int tuningEconomy;
    public int tuningDesign;
    public int tuningSafety;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public float demandMultiplier;
}