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
}