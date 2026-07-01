using UnityEngine;

[System.Serializable]
public class CarBlueprint
{
    public string carName;
    public int price;                 // базовая цена (используется, если нет массива)
    public int productionCost;        // базовая себестоимость
    public float demandMultiplier = 1f;
    public Sprite carIcon;

    // ---- ОСНОВНОЙ ПРЕФАБ (для обратной совместимости) ----
    public GameObject carPrefab;

    // ---- ПРЕФАБЫ И ЭКОНОМИКА ПО УРОВНЯМ ----
    public GameObject[] levelPrefabs;
    public int[] pricePerLevel;          // цены для каждого уровня
    public int[] productionCostPerLevel; // себестоимости для каждого уровня
    public int currentLevel = 0;

    // Текущие цена и себестоимость (вычисляются по уровню)
    public int CurrentPrice => (pricePerLevel != null && currentLevel < pricePerLevel.Length)
        ? pricePerLevel[currentLevel]
        : price;

    public int CurrentProductionCost => (productionCostPerLevel != null && currentLevel < productionCostPerLevel.Length)
        ? productionCostPerLevel[currentLevel]
        : productionCost;

    public int profit => CurrentPrice - CurrentProductionCost;

    public double GetCurrentProfit()
    {
        return (CurrentPrice * demandMultiplier) - CurrentProductionCost;
    }
}