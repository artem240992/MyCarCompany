using System;

[Serializable]
public class Technology
{
    public string techName;
    public string description;
    public int researchCost;
    public bool isResearched;
    public string[] requiredTechNames; // вместо массива Technology
    public CarBlueprint unlockedCar;
}