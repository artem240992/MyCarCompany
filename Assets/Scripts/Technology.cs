using System;
using UnityEngine; // <-- ОБЯЗАТЕЛЬНО!

[System.Serializable]
public class Technology
{
    public string techName;
    public string description;
    public int researchCost;
    public bool isResearched;
    public string[] requiredTechNames;
    public CarBlueprint unlockedCar;

    [Tooltip("Открывать ли машину при изучении технологии")]
    public bool unlockCarOnResearch = true;
}