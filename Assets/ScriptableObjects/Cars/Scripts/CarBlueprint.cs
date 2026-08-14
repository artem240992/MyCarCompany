using UnityEngine;

[CreateAssetMenu(fileName = "NewCar", menuName = "Car Company/Car Blueprint")]
public class CarBlueprint : ScriptableObject
{
    [Header("Основные параметры")]
    public string carName;
    public GameObject carPrefab;
    public string transmissionType = "АКПП"; // по умолчанию
    public int basePrice;
    public int productionCost;
    public int currentLevel = 0;
    public Sprite carIcon;
    public CarRecipe recipe;

    [Header("Тип автомобиля")]
    public CarType carType; // если не задан, используется своя сезонность

    [Header("Настройки уровней")]
    public LevelData[] levels;

    [Header("Тюнинг (макс.)")]
    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;

    [Header("Тюнинг (тек.)")]
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
    public int currentPrice;

    [Header("Спрос (резервный)")]
    public float demandMultiplier = 1f;

    [Header("Настройки влияния новостей")]
    public DemandRange[] newsDemandRanges;
    public float newsMultiplier = 1f;

    [Header("Сезонные множители спроса (по месяцам, январь-декабрь)")]
    public float[] seasonalDemandMultipliers = new float[12]
    {
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
    };

    // ------------------------------------------------------------

    public string GetDisplayName()
    {
        return currentLevel > 0 ? $"{carName} v{currentLevel + 1}" : carName;
    }

    public CarBlueprint Clone()
    {
        CarBlueprint newCar = ScriptableObject.CreateInstance<CarBlueprint>();

        newCar.carName = this.carName;
        newCar.carPrefab = this.carPrefab;
        newCar.transmissionType = this.transmissionType;
        newCar.carType = this.carType;
        newCar.basePrice = this.basePrice;
        newCar.productionCost = this.productionCost;
        newCar.currentPrice = this.currentPrice;
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
        newCar.demandMultiplier = this.demandMultiplier;
        newCar.newsMultiplier = this.newsMultiplier;

        if (this.newsDemandRanges != null)
        {
            newCar.newsDemandRanges = new DemandRange[this.newsDemandRanges.Length];
            for (int i = 0; i < this.newsDemandRanges.Length; i++)
                newCar.newsDemandRanges[i] = this.newsDemandRanges[i];
        }

        // Клонирование основного рецепта
        if (this.recipe != null)
        {
            newCar.recipe = ScriptableObject.CreateInstance<CarRecipe>();
            CopyRecipe(this.recipe, newCar.recipe);
        }

        // Копирование уровней (LevelData)
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

                    // Клонирование рецепта уровня
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

        // Копирование сезонных множителей
        if (source.seasonalCostMultipliers != null)
        {
            target.seasonalCostMultipliers = (float[])source.seasonalCostMultipliers.Clone();
        }
        else
        {
            target.seasonalCostMultipliers = new float[12];
            for (int i = 0; i < 12; i++) target.seasonalCostMultipliers[i] = 1f;
        }
    }

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

        // Копируем сезонные множители
        if (newRecipe.seasonalCostMultipliers != null)
        {
            recipe.seasonalCostMultipliers = (float[])newRecipe.seasonalCostMultipliers.Clone();
        }
    }

    // ------------------------------------------------------------
    // РАСЧЁТНЫЕ МЕТОДЫ
    // ------------------------------------------------------------

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
        // Используем CurrentDemandMultiplier, который уже включает сезонность спроса
        float demand = CurrentDemandMultiplier;
        return 0.8f + 0.4f * demand;
    }

    // Получение модифицированной стоимости сборки (учитывает сезонность рецепта)
    public int GetModifiedAssemblyCost()
    {
        CarRecipe currentRecipe = GetCurrentRecipe();
        if (currentRecipe == null) return 0;
        return currentRecipe.GetModifiedAssemblyCost();
    }

    public int GetProductionCostWithLevel()
    {
        float baseCost = (levels != null && currentLevel > 0 && currentLevel - 1 < levels.Length && levels[currentLevel - 1] != null)
            ? levels[currentLevel - 1].productionCost
            : productionCost;
        return Mathf.RoundToInt(baseCost * (1f + currentLevel * 0.1f));
    }

    public int GetModifiedPrice(float priceModifier)
    {
        int baseForPrice = (currentPrice != 0) ? currentPrice : basePrice;
        float tuningPrice = baseForPrice * GetTuningPriceModifier();
        float finalPrice = tuningPrice * priceModifier * GetDemandPriceModifier();
        return Mathf.RoundToInt(finalPrice);
    }

    /// <summary>
/// Вычисляет рейтинг автомобиля (от 1 до 10) на основе цены, качества и уровня.
/// </summary>
    public float CalculateRating(float priceModifier)
    {
        // Качество: сумма параметров тюнинга (каждый даёт 0.5 балла) + бонус за уровень (2 балла за уровень)
        float quality = (currentPower + currentEconomy + currentDesign + currentSafety) * 0.5f + currentLevel * 2f;
        // Цена с учётом модификаторов
        float price = GetModifiedPrice(priceModifier);
        // Базовый коэффициент: чем ниже цена относительно базовой, тем выше рейтинг
        float priceFactor = (basePrice + 50f) / (price + 50f); // сглаживание для крайних цен
        // Итоговая оценка
        float rating = quality * priceFactor * 0.8f;
        // Ограничиваем диапазон 1-10
        return Mathf.Clamp(rating, 1f, 10f);
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
        if (levels != null && currentLevel > 0 && currentLevel - 1 < levels.Length && levels[currentLevel - 1].recipe != null)
            return levels[currentLevel - 1].recipe;
        return recipe;
    }

    public float CurrentDemandMultiplier
    {
        get
        {
            float baseDemand = (levels != null && currentLevel > 0 && currentLevel - 1 < levels.Length) 
                ? levels[currentLevel - 1].demandMultiplier 
                : demandMultiplier;

            // Определяем массив сезонных множителей:
            // если задан carType и у него есть массив, берём оттуда, иначе из своих seasonalDemandMultipliers
            float[] seasonalArray = (carType != null && carType.seasonalDemandMultipliers != null && carType.seasonalDemandMultipliers.Length == 12)
                ? carType.seasonalDemandMultipliers
                : seasonalDemandMultipliers;

            int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
            float seasonalFactor = 1.0f;
            if (seasonalArray != null && seasonalArray.Length == 12)
                seasonalFactor = seasonalArray[currentMonth - 1];

            return baseDemand * newsMultiplier * seasonalFactor;
        }
    }
}

// ------------------------------------------------------------
// КЛАСС LevelData
// ------------------------------------------------------------
[System.Serializable]
public class LevelData
{
    [Header("Визуал")]
    public GameObject prefab;

    [Header("Экономика")]
    public int levelPrice;
    public int productionCost;
    public float demandMultiplier = 1f;

    [Header("Рецепт")]
    public CarRecipe recipe;

    [Header("Тюнинг (макс.)")]
    public int tuningPower = 0;
    public int tuningEconomy = 0;
    public int tuningDesign = 0;
    public int tuningSafety = 0;
}

// ------------------------------------------------------------
// СТРУКТУРА DemandRange
// ------------------------------------------------------------
[System.Serializable]
public struct DemandRange
{
    public float min;
    public float max;
}