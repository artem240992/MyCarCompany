using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PartsMarketManager : MonoBehaviour
{
    public static PartsMarketManager Instance { get; private set; }

    [Header("Базовые цены для сброса")]
    public float basePriceEngine = 50f;
    public float basePriceBody = 40f;
    public float basePriceWheels = 15f;
    public float basePriceElectronics = 30f;
    public float basePriceBattery = 80f; // <-- Добавлено

    [Header("Текущие цены деталей (для быстрого доступа)")]
    public float enginePrice = 15f;
    public float bodyPrice = 12f;
    public float wheelsPrice = 10f;
    public float electronicsPrice = 18f;
    public float batteryPrice = 80f; // <-- Добавлено

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
        currentPrices[PartType.Battery] = basePriceBattery; // <-- Теперь безопасно
    }

    public void UpdatePrices()
    {
        // Перебираем все типы, включая батарею
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
        {
            if (!currentPrices.ContainsKey(type))
                currentPrices[type] = 20f; // защита от отсутствующих ключей

            float change = Random.Range(-priceVolatility, priceVolatility);
            currentPrices[type] *= (1f + change);
            currentPrices[type] = Mathf.Max(currentPrices[type], 1f); // не ниже 1$
        }

        // Синхронизируем публичные поля для удобства UI
        enginePrice = currentPrices[PartType.Engine];
        bodyPrice = currentPrices[PartType.Body];
        wheelsPrice = currentPrices[PartType.Wheels];
        electronicsPrice = currentPrices[PartType.Electronics];
        batteryPrice = currentPrices[PartType.Battery];

        ApplyMarketEvents();
    }

    private void ApplyMarketEvents()
    {
        if (Random.value < 0.05f) // 5% шанс в месяц
        {
            // Учитываем все типы деталей, включая новую
            var allTypes = System.Enum.GetValues(typeof(PartType));
            PartType type = (PartType)allTypes.GetValue(Random.Range(0, allTypes.Length));
            
            float multiplier = Random.Range(1.3f, 1.8f);
            currentPrices[type] *= multiplier;
            CarCompanyManager.Instance.UIManager?.ShowNotification($"⚠️ Рынок {type} вырос на {(multiplier - 1f) * 100:F0}%!");
        }
    }

    public float GetPartPrice(PartType type)
    {
        return currentPrices.TryGetValue(type, out float price) ? price : 20f;
    }

    public float GetPrice(PartType type) => GetPartPrice(type);

    public bool BuyParts(PartType type, int amount)
    {
        float cost = GetPrice(type) * amount;
        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(cost)) return false;

        if (!WarehouseManager.Instance.AddParts(type, amount))
        {
            CarCompanyManager.Instance.EconomyManager.AddMoney(cost);
            return false;
        }
        return true;
    }

    public float GetTotalPartCost(CarRecipe recipe)
    {
        if (recipe == null) return 0f;

        float total = GetPrice(PartType.Engine) * recipe.engineRequired +
                      GetPrice(PartType.Body) * recipe.bodyRequired +
                      GetPrice(PartType.Wheels) * recipe.wheelsRequired +
                      GetPrice(PartType.Electronics) * recipe.electronicsRequired;

        // Учитываем батареи, если это электромобиль
        if (recipe.isElectric && recipe.batteryRequired > 0)
        {
            total += GetPrice(PartType.Battery) * recipe.batteryRequired;
        }

        return total;
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

            // Синхронизируем поля после загрузки
            if (currentPrices.ContainsKey(PartType.Engine)) enginePrice = currentPrices[PartType.Engine];
            if (currentPrices.ContainsKey(PartType.Body)) bodyPrice = currentPrices[PartType.Body];
            if (currentPrices.ContainsKey(PartType.Wheels)) wheelsPrice = currentPrices[PartType.Wheels];
            if (currentPrices.ContainsKey(PartType.Electronics)) electronicsPrice = currentPrices[PartType.Electronics];
            if (currentPrices.ContainsKey(PartType.Battery)) batteryPrice = currentPrices[PartType.Battery];
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