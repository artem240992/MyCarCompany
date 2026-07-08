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
    public int currentPrice;

    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;

    public int currentPower = 0;
    public int currentEconomy = 0;
    public int currentDesign = 0;
    public int currentSafety = 0;

    public string GetDisplayName()
    {
        if (currentLevel > 0)
            return $"{carName} v{currentLevel + 1}";
        else
            return carName;
    }

    public CarBlueprint Clone()
    {
        CarBlueprint clone = ScriptableObject.CreateInstance<CarBlueprint>();
        clone.carName = this.carName;
        clone.carPrefab = this.carPrefab;
        clone.levelPrefabs = this.levelPrefabs;
        clone.basePrice = this.basePrice;
        clone.productionCost = this.productionCost;
        clone.demandMultiplier = this.demandMultiplier;
        clone.currentLevel = this.currentLevel;
        clone.carIcon = this.carIcon;
        clone.currentPrice = this.currentPrice;
        clone.tuningPower = this.tuningPower;
        clone.tuningEconomy = this.tuningEconomy;
        clone.tuningDesign = this.tuningDesign;
        clone.tuningSafety = this.tuningSafety;
        clone.currentPower = this.currentPower;
        clone.currentEconomy = this.currentEconomy;
        clone.currentDesign = this.currentDesign;
        clone.currentSafety = this.currentSafety;
        return clone;
    }

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

    // ---- НОВЫЙ МЕТОД: себестоимость с учётом уровня ----
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
        // Этот метод теперь не используется напрямую, оставлен для совместимости
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