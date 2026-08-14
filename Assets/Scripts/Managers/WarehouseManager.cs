using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WarehouseManager : MonoBehaviour
{
    public static WarehouseManager Instance { get; private set; }

    [Header("Warehouse Settings")]
    public int warehouseLevel = 0;
    public int maxCapacity = 100; // для деталей

    [Header("Car Stock")]
    public int maxCarCapacity = 100;
    private Dictionary<CarBlueprint, int> carStockByModel = new Dictionary<CarBlueprint, int>();

    private Dictionary<PartType, int> partsInventory = new Dictionary<PartType, int>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
            partsInventory[type] = 0;
        UpdateCapacity();
    }

    // ---- Детали (без изменений) ----
    public int GetPartCount(PartType type) => partsInventory.TryGetValue(type, out int count) ? count : 0;
    public bool HasParts(PartType type, int amount) => GetPartCount(type) >= amount;

    public bool AddParts(PartType type, int amount)
    {
        int current = GetPartCount(type);
        if (current + amount > maxCapacity)
        {
            Debug.Log($"Склад переполнен! Не хватает места для {type} (+{amount})");
            return false;
        }
        partsInventory[type] = current + amount;
        return true;
    }

    public bool RemoveParts(PartType type, int amount)
    {
        if (!HasParts(type, amount)) return false;
        partsInventory[type] = GetPartCount(type) - amount;
        return true;
    }

    public bool CanProduceCar(CarRecipe recipe)
    {
        return HasParts(PartType.Engine, recipe.engineRequired) &&
               HasParts(PartType.Body, recipe.bodyRequired) &&
               HasParts(PartType.Wheels, recipe.wheelsRequired) &&
               HasParts(PartType.Electronics, recipe.electronicsRequired);
    }

    public void ConsumePartsForCar(CarRecipe recipe)
    {
        if (!CanProduceCar(recipe)) return;
        RemoveParts(PartType.Engine, recipe.engineRequired);
        RemoveParts(PartType.Body, recipe.bodyRequired);
        RemoveParts(PartType.Wheels, recipe.wheelsRequired);
        RemoveParts(PartType.Electronics, recipe.electronicsRequired);
    }

    public int GetCurrentCapacity() => maxCapacity;

    public bool UpgradeWarehouse(int cost)
    {
        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(cost)) return false;
        warehouseLevel++;
        UpdateCapacity();
        return true;
    }

    private void UpdateCapacity()
    {
        maxCapacity = 100 * (int)Mathf.Pow(2, warehouseLevel);
        maxCarCapacity = 100 * (int)Mathf.Pow(2, warehouseLevel);
    }

    // ---- Склад машин (новое) ----
    public bool AddCar(CarBlueprint car, int count = 1)
    {
        if (car == null || count <= 0) return false;
        int current = GetCarCount(car);
        int total = carStockByModel.Values.Sum();
        if (total + count > maxCarCapacity)
        {
            UIManager.Instance?.ShowNotification("Склад машин переполнен!");
            return false;
        }
        carStockByModel[car] = current + count;
        return true;
    }

    public bool RemoveCar(CarBlueprint car, int count = 1)
    {
        if (car == null || count <= 0) return false;
        if (!carStockByModel.ContainsKey(car) || carStockByModel[car] < count) return false;
        carStockByModel[car] -= count;
        if (carStockByModel[car] == 0)
            carStockByModel.Remove(car);
        return true;
    }

    public int GetCarCount(CarBlueprint car)
    {
        return carStockByModel.TryGetValue(car, out int count) ? count : 0;
    }

    public int GetTotalCarCount()
    {
        return carStockByModel.Values.Sum();
    }

    /// <summary>
    /// Продаёт все машины указанной модели.
    /// </summary>
    public int SellAllCarsOfModel(CarBlueprint car)
    {
        if (car == null || !carStockByModel.ContainsKey(car)) return 0;
        int count = carStockByModel[car];
        int price = car.GetModifiedPrice(CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);
        double total = count * price;
        CarCompanyManager.Instance.EconomyManager.AddMoney(total);
        carStockByModel.Remove(car);
        UIManager.Instance?.ShowNotification($"Продано {count} {car.GetDisplayName()} за ${total:F0}");
        return count;
    }

    /// <summary>
    /// Продаёт указанное количество машин указанной модели.
    /// </summary>
    public int SellCars(CarBlueprint car, int count)
    {
        if (car == null || count <= 0 || !carStockByModel.ContainsKey(car)) return 0;
        int available = carStockByModel[car];
        int toSell = Mathf.Min(available, count);
        int price = car.GetModifiedPrice(CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);
        double total = toSell * price;
        CarCompanyManager.Instance.EconomyManager.AddMoney(total);
        carStockByModel[car] -= toSell;
        if (carStockByModel[car] == 0)
            carStockByModel.Remove(car);
        UIManager.Instance?.ShowNotification($"Продано {toSell} {car.GetDisplayName()} за ${total:F0}");
        return toSell;
    }

    // ---- Производство деталей (без изменений) ----
    public bool ProduceParts(PartType type, int count)
    {
        if (!IsPartProductionUnlocked(type)) 
        {
            UIManager.Instance?.ShowNotification($"Технология производства {type} не изучена!");
            return false;
        }

        int cost = 10 * count;
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < cost)
        {
            UIManager.Instance?.ShowNotification($"Не хватает денег! Нужно ${cost}");
            return false;
        }
        if (economy.EngineerCount < 1)
        {
            UIManager.Instance?.ShowNotification("Нужен хотя бы 1 инженер для производства деталей!");
            return false;
        }
        if (economy.ConveyorLevel < 1)
        {
            UIManager.Instance?.ShowNotification("Нужен хотя бы 1 уровень конвейера для производства деталей!");
            return false;
        }

        economy.Money -= cost;
        AddParts(type, count);
        UIManager.Instance?.UpdateMoneyLabels();
        UIManager.Instance?.UpdateWarehouseLabels();
        UIManager.Instance?.ShowNotification($"Произведено {count} {type} за ${cost}");
        return true;
    }

    private bool IsPartProductionUnlocked(PartType type)
    {
        string techName = type switch
        {
            PartType.Engine => "Производство Engine",
            PartType.Body => "Производство Body",
            PartType.Wheels => "Производство Wheels",
            PartType.Electronics => "Производство Electronics",
            _ => ""
        };
        return CarCompanyManager.Instance.TechManager.IsTechResearched(techName);
    }

    // ---- Продажа деталей (без изменений) ----
    public bool SellParts(PartType type, int count, float pricePerUnit)
    {
        if (!HasParts(type, count)) return false;
        var economy = CarCompanyManager.Instance.EconomyManager;
        double total = count * pricePerUnit;
        economy.AddMoney(total);
        RemoveParts(type, count);
        UIManager.Instance?.UpdateMoneyLabels();
        UIManager.Instance?.UpdateWarehouseLabels();
        UIManager.Instance?.ShowNotification($"Продано {count} {type} за ${total:F0}");
        return true;
    }

    public float GetMarketPrice(PartType type)
    {
        float basePrice = type switch
        {
            PartType.Engine => 30f,
            PartType.Body => 25f,
            PartType.Wheels => 20f,
            PartType.Electronics => 35f,
            _ => 20f
        };
        var economy = CarCompanyManager.Instance.EconomyManager;
        return basePrice * economy.TotalPriceModifier;
    }

    // ---- Сохранение/загрузка ----
    public void FillSaveData(SaveData data)
    {
        data.warehouseLevel = warehouseLevel;
        data.partsInventory = new List<PartSaveData>();
        foreach (var kvp in partsInventory)
        {
            data.partsInventory.Add(new PartSaveData { partType = kvp.Key, amount = kvp.Value });
        }

        // Сохраняем склад машин (сохраняем только количество по имени модели, т.к. ссылки могут не восстановиться)
        data.carStockByModel = new List<CarStockSaveData>();
        foreach (var kvp in carStockByModel)
        {
            if (kvp.Key != null)
                data.carStockByModel.Add(new CarStockSaveData { carName = kvp.Key.carName, amount = kvp.Value });
        }
    }

    public void LoadFromSave(SaveData data)
    {
        warehouseLevel = data.warehouseLevel;
        UpdateCapacity();
        partsInventory.Clear();
        foreach (PartType type in System.Enum.GetValues(typeof(PartType)))
            partsInventory[type] = 0;
        if (data.partsInventory != null)
        {
            foreach (var saved in data.partsInventory)
            {
                partsInventory[saved.partType] = saved.amount;
            }
        }

        // Восстанавливаем склад машин
        carStockByModel.Clear();
        if (data.carStockByModel != null)
        {
            var allCars = CarCompanyManager.Instance.TechManager.AvailableCars;
            foreach (var saved in data.carStockByModel)
            {
                CarBlueprint car = allCars?.FirstOrDefault(c => c.carName == saved.carName);
                if (car != null && saved.amount > 0)
                    carStockByModel[car] = saved.amount;
            }
        }
    }
}