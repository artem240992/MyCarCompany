using UnityEngine;

[System.Serializable]
public class CarBlueprint
{
    public string carName;
    public int price;                 // базовая цена
    public int productionCost;        // базовая себестоимость
    public float demandMultiplier = 1f;
    public Sprite carIcon;

    public GameObject carPrefab;
    public GameObject[] levelPrefabs;
    public int currentLevel = 0;

    // ---- НОВЫЕ МЕТОДЫ ДЛЯ УЧЁТА МОДИФИКАТОРОВ ----
    public int GetModifiedPrice(float totalPriceModifier)
    {
        return Mathf.RoundToInt(price * totalPriceModifier);
    }

    public int GetModifiedProductionCost(float costModifier)
    {
        return Mathf.RoundToInt(productionCost * costModifier);
    }

    // Текущие цена и себестоимость (без модификаторов) – для обратной совместимости
    public int CurrentPrice => price;
    public int CurrentProductionCost => productionCost;

    public int profit => CurrentPrice - CurrentProductionCost;

    public double GetCurrentProfit()
    {
        return (CurrentPrice * demandMultiplier) - CurrentProductionCost;
    }
}