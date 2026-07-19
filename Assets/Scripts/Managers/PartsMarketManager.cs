using UnityEngine;
using System.Collections.Generic;

public class PartsMarketManager : MonoBehaviour
{
    public static PartsMarketManager Instance { get; private set; }

    [Header("Price Settings")]
    public float basePriceEngine = 50f;
    public float basePriceBody = 40f;
    public float basePriceWheels = 15f;
    public float basePriceElectronics = 30f;

    private Dictionary<PartType, float> currentPrices = new Dictionary<PartType, float>();
    private float priceVolatility = 0.15f; // ±15% в месяц

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ResetPrices();
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += UpdatePrices;
    }

    private void ResetPrices()
    {
        currentPrices[PartType.Engine] = basePriceEngine;
        currentPrices[PartType.Body] = basePriceBody;
        currentPrices[PartType.Wheels] = basePriceWheels;
        currentPrices[PartType.Electronics] = basePriceElectronics;
    }

    public void UpdatePrices()
    {
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
        {
            float change = Random.Range(-priceVolatility, priceVolatility);
            currentPrices[type] *= (1f + change);
            currentPrices[type] = Mathf.Max(currentPrices[type], 1f); // не ниже 1$
        }
        // Применяем рыночные события
        ApplyMarketEvents();
    }

    private void ApplyMarketEvents()
    {
        // Рандомные события (можно вызывать из DifficultyManager)
        if (Random.value < 0.05f) // 5% шанс в месяц
        {
            PartType type = (PartType)Random.Range(0, 4);
            float multiplier = Random.Range(1.3f, 1.8f);
            currentPrices[type] *= multiplier;
            CarCompanyManager.Instance.UIManager?.ShowNotification($"⚠️ Рынок {type} вырос на {(multiplier - 1f) * 100:F0}%!");
        }
    }

    public float GetPrice(PartType type) => currentPrices.TryGetValue(type, out float price) ? price : 0f;

    public bool BuyParts(PartType type, int amount)
    {
        float price = GetPrice(type) * amount;
        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(price)) return false;
        if (!WarehouseManager.Instance.AddParts(type, amount))
        {
            // Если склад переполнен, возвращаем деньги
            CarCompanyManager.Instance.EconomyManager.AddMoney(price);
            return false;
        }
        return true;
    }

    public float GetTotalPartCost(CarRecipe recipe)
    {
        return GetPrice(PartType.Engine) * recipe.engineRequired +
               GetPrice(PartType.Body) * recipe.bodyRequired +
               GetPrice(PartType.Wheels) * recipe.wheelsRequired +
               GetPrice(PartType.Electronics) * recipe.electronicsRequired;
    }

    public float GetProductionCost(CarRecipe recipe)
    {
        return GetTotalPartCost(recipe) + recipe.assemblyCost;
    }

    public void FillSaveData(SaveData data)
    {
        data.partPrices = new List<PartPriceSaveData>();
        foreach (var kvp in currentPrices)
        {
            data.partPrices.Add(new PartPriceSaveData { partType = kvp.Key, price = kvp.Value });
        }
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.partPrices != null)
        {
            foreach (var saved in data.partPrices)
                currentPrices[saved.partType] = saved.price;
        }
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= UpdatePrices;
    }
}

[System.Serializable]
public class PartPriceSaveData
{
    public PartType partType;
    public float price;
}