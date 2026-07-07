using UnityEngine;

[CreateAssetMenu(fileName = "NewCar", menuName = "Car Company/Car Blueprint")]
public class CarBlueprint : ScriptableObject
{
    public string carName;
    public GameObject carPrefab;
    public GameObject[] levelPrefabs;
    public int basePrice;
    public int productionCost;
    public float demandMultiplier = 1f;
    public int currentLevel = 0;
    public Sprite carIcon;

    // ---- Параметры тюнинга (0-10) ----
    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;

    // ---- Методы расчёта влияния тюнинга ----
    public float GetTuningPriceModifier()
    {
        int total = tuningPower + tuningEconomy + tuningDesign + tuningSafety;
        return 1f + total * 0.03f;
    }

    public float GetTuningDemandModifier()
    {
        int total = tuningPower + tuningEconomy + tuningDesign + tuningSafety;
        float bonus = Mathf.Min(total * 0.0125f, 0.5f);
        return 1f + bonus;
    }

    public int GetModifiedPrice(float priceModifier)
    {
        float tuningPrice = basePrice * GetTuningPriceModifier();
        return Mathf.RoundToInt(tuningPrice * priceModifier);
    }

    public int GetModifiedProductionCost(float costModifier)
    {
        return Mathf.RoundToInt(productionCost * costModifier);
    }
}