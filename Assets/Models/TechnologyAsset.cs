using UnityEngine;

[CreateAssetMenu(fileName = "NewTechnology", menuName = "Car Company/Technology Asset")]
public class TechnologyAsset : ScriptableObject
{
    public string techName;
    public string description;
    public int researchCost;
    public string[] requiredTechNames;
    public float priceModifier = 1f;
    public float demandModifier = 1f;
    public bool unlockCarOnResearch;
    public CarBlueprint unlockedCar;

    public int availableYear = 2025; // год, с которого технология доступна без штрафа
}