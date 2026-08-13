using UnityEngine;

[CreateAssetMenu(fileName = "NewCar", menuName = "Car Company/Car Blueprint")]
public class CarBlueprint : ScriptableObject
{
    [Header("Основные параметры")]
    public string carName;
    public GameObject carPrefab;                // визуальный префаб для спавна (базовый)
    public int basePrice;
    public int productionCost;
    public float demandMultiplier = 1f;
    public int currentLevel = 0;
    public Sprite carIcon;
    public CarRecipe recipe;                    // рецепт (fallback)

    [Header("Настройки уровней (LevelData)")]
    public LevelData[] levels;                  // массив данных для каждого уровня

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
    public float tintLevel = 0f;
    public bool hasTint = false;

    [Header("Экономика")]
    public int currentPrice; // актуальная цена (если 0, берётся basePrice)

    public string GetDisplayName()
    {
        return currentLevel > 0 ? $"{carName} v{currentLevel + 1}" : carName;
    }

    // ---------- Клонирование ----------
    public CarBlueprint Clone()
    {
        CarBlueprint newCar = ScriptableObject.CreateInstance<CarBlueprint>();

        // Копирование простых полей
        newCar.carName = this.carName;
        newCar.carPrefab = this.carPrefab;
        newCar.basePrice = this.basePrice;
        newCar.productionCost = this.productionCost;
        newCar.currentPrice = this.currentPrice;
        newCar.demandMultiplier = this.demandMultiplier;
        newCar.currentLevel = this.currentLevel;
        newCar.tuningPower = this.tuningPower;
        newCar.tuningEconomy = this.tuningEconomy;
        newCar.tuningDesign = this.tuningDesign;
        newCar.tuningSafety = this.tuningSafety;
        newCar.currentPower = this.currentPower;
        newCar.currentEconomy = this.currentEconomy;
        newCar.currentDesign = this.currentDesign;
        newCar.currentSafety = this.currentSafety;
        newCar.bodyColor = this.bodyColor;
        newCar.bodyColorIndex = this.bodyColorIndex;
        newCar.tintLevel = this.tintLevel;
        newCar.hasTint = this.hasTint;
        newCar.carIcon = this.carIcon;

        // Клонирование основного рецепта
        if (this.recipe != null)
        {
            newCar.recipe = ScriptableObject.CreateInstance<CarRecipe>();
            CopyRecipe(this.recipe, newCar.recipe);
        }

        // Клонирование массива уровней (глубокое копирование LevelData)
        if (this.levels != null)
        {
            newCar.levels = new LevelData[this.levels.Length];
            for (int i = 0; i < this.levels.Length; i++)
            {
                if (this.levels[i] != null)
                {
                    LevelData original = this.levels[i];
                    LevelData copy = new LevelData();

                    copy.prefab = original.prefab;
                    copy.levelPrice = original.levelPrice;
                    copy.productionCost = original.productionCost;
                    copy.demandMultiplier = original.demandMultiplier;
                    copy.tuningPower = original.tuningPower;
                    copy.tuningEconomy = original.tuningEconomy;
                    copy.tuningDesign = original.tuningDesign;
                    copy.tuningSafety = original.tuningSafety;

                    if (original.recipe != null)
                    {
                        copy.recipe = ScriptableObject.CreateInstance<CarRecipe>();
                        CopyRecipe(original.recipe, copy.recipe);
                    }

                    newCar.levels[i] = copy;
                }
            }
        }

        return newCar;
    }

    private void CopyRecipe(CarRecipe source, CarRecipe target)
    {
        target.engineRequired = source.engineRequired;
        target.bodyRequired = source.bodyRequired;
        target.wheelsRequired = source.wheelsRequired;
        target.electronicsRequired = source.electronicsRequired;
        target.assemblyCost = source.assemblyCost;
        target.enginePrice = source.enginePrice;
        target.bodyPrice = source.bodyPrice;
        target.wheelsPrice = source.wheelsPrice;
        target.electronicsPrice = source.electronicsPrice;
    }

    // ---------- Применение рецепта ----------
    public void ApplyRecipe(CarRecipe newRecipe)
    {
        if (newRecipe == null) return;
        if (recipe == null)
            recipe = ScriptableObject.CreateInstance<CarRecipe>();

        recipe.engineRequired = newRecipe.engineRequired;
        recipe.bodyRequired = newRecipe.bodyRequired;
        recipe.wheelsRequired = newRecipe.wheelsRequired;
        recipe.electronicsRequired = newRecipe.electronicsRequired;
        recipe.assemblyCost = newRecipe.assemblyCost;
        recipe.enginePrice = newRecipe.enginePrice;
        recipe.bodyPrice = newRecipe.bodyPrice;
        recipe.wheelsPrice = newRecipe.wheelsPrice;
        recipe.electronicsPrice = newRecipe.electronicsPrice;
    }

    // ---------- Расчётные методы ----------
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
        if (levels != null && currentLevel >= 0 && currentLevel < levels.Length)
            return Mathf.RoundToInt(levels[currentLevel].productionCost * (1f + currentLevel * 0.1f));
        return productionCost;
    }

    public int GetModifiedPrice(float priceModifier)
    {
        int baseForPrice = (currentPrice != 0) ? currentPrice : basePrice;
        float tuningPrice = baseForPrice * GetTuningPriceModifier();
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

    public CarRecipe GetCurrentRecipe()
    {
        if (levels != null && currentLevel >= 0 && currentLevel < levels.Length && levels[currentLevel].recipe != null)
            return levels[currentLevel].recipe;
        return recipe;
    }
}

// ---------- Класс LevelData ----------
[System.Serializable]
public class LevelData
{
    [Header("Визуал")]
    public GameObject prefab;              // префаб модели для этого уровня

    [Header("Экономика")]
    public int levelPrice;                 // базовая цена на этом уровне
    public int productionCost;             // стоимость производства
    public float demandMultiplier = 1f;

    [Header("Рецепт")]
    public CarRecipe recipe;               // рецепт для этого уровня

    [Header("Тюнинг (максимум)")]
    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;
}