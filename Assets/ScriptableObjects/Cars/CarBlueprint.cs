using UnityEngine;

[CreateAssetMenu(fileName = "NewCar", menuName = "Car Company/Car Blueprint")]
public class CarBlueprint : ScriptableObject
{
    [Header("Основные параметры")]
    public string carName;
    public GameObject carPrefab;
    public GameObject[] levelPrefabs;
    public int basePrice;
    public int productionCost;
    public float demandMultiplier = 1f;
    public int currentLevel = 0;
    public Sprite carIcon;

    [Header("Тюнинг (максимальные значения)")]
    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;

    [Header("Тюнинг (текущие значения)")]
    public int currentPower = 0;
    public int currentEconomy = 0;
    public int currentDesign = 0;
    public int currentSafety = 0;

    [Header("Цвет и тонировка")]
    public Color bodyColor = Color.white;
    public int bodyColorIndex = 0;
    public float tintLevel = 0f;          // 0..1 – уровень затемнения
    public bool hasTint = false;          // наличие тонировки (используется в некоторых скриптах)

    [Header("Экономика")]
    public int currentPrice;              // актуальная цена (меняется с уровнем)

    public string GetDisplayName()
    {
        return currentLevel > 0 ? $"{carName} v{currentLevel + 1}" : carName;
    }

    public CarBlueprint Clone()
    {
        CarBlueprint clone = CreateInstance<CarBlueprint>();
        clone.carName = this.carName;
        clone.carPrefab = this.carPrefab;
        clone.levelPrefabs = this.levelPrefabs;
        clone.basePrice = this.basePrice;
        clone.productionCost = this.productionCost;
        clone.demandMultiplier = this.demandMultiplier;
        clone.currentLevel = this.currentLevel;
        clone.carIcon = this.carIcon;

        clone.tuningPower = this.tuningPower;
        clone.tuningEconomy = this.tuningEconomy;
        clone.tuningDesign = this.tuningDesign;
        clone.tuningSafety = this.tuningSafety;

        clone.currentPower = this.currentPower;
        clone.currentEconomy = this.currentEconomy;
        clone.currentDesign = this.currentDesign;
        clone.currentSafety = this.currentSafety;

        clone.bodyColor = this.bodyColor;
        clone.bodyColorIndex = this.bodyColorIndex;
        clone.tintLevel = this.tintLevel;
        clone.hasTint = this.hasTint;
        clone.currentPrice = this.currentPrice;

        return clone;
    }

    // ---- Модификаторы ----

    public float GetTuningPriceModifier()
    {
        int total = currentPower + currentEconomy + currentDesign + currentSafety;
        return 1f + total * 0.03f;
    }

    public float GetTuningDemandModifier()
    {
        int total = currentPower + currentEconomy + currentDesign + currentSafety;
        float bonus = Mathf.Min(total * 0.0125f, 0.5f);
        return 1f + bonus;
    }

    public float GetDemandPriceModifier()
    {
        return 0.8f + 0.4f * demandMultiplier;
    }

    public int GetProductionCostWithLevel()
    {
        return Mathf.RoundToInt(productionCost * (1f + currentLevel * 0.1f));
    }

    public int GetModifiedPrice(float priceModifier)
    {
        float tuningPrice = currentPrice * GetTuningPriceModifier();
        float finalPrice = tuningPrice * priceModifier * GetDemandPriceModifier();
        return Mathf.RoundToInt(finalPrice);
    }

    public int GetModifiedProductionCost(float costModifier)
    {
        return Mathf.RoundToInt(productionCost * costModifier);
    }

    public void SyncCurrentToMax()
    {
        currentPower = tuningPower;
        currentEconomy = tuningEconomy;
        currentDesign = tuningDesign;
        currentSafety = tuningSafety;
    }
}