using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CarStockEntry
{
    public CarBlueprint blueprint;
    public string trimName = "";
    public string transmissionType = "АКПП";
    public int count;

    public string GetGroupKey()
    {
        if (blueprint == null) return "";
        return $"{blueprint.carName}|{blueprint.currentLevel}|{trimName}|{transmissionType}";
    }
}

public class WarehouseManager : MonoBehaviour
{
    public static WarehouseManager Instance { get; private set; }

    [Header("Warehouse Settings")]
    public int warehouseLevel = 0;
    public int maxCapacity = 100;

    public CarTrim trim;
    public List<CarOption> installedOptions;

    [Header("Car Stock")]
    public int maxCarCapacity = 100;

    private List<CarStockEntry> carStock = new List<CarStockEntry>();
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

    // ==================== ДЕТАЛИ ====================
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

    // ==================== СКЛАД МАШИН ====================

    public bool AddCar(CarBlueprint car, int count = 1)
        => AddCar(car, "", "АКПП", count);

    public bool AddCar(CarBlueprint car, string trimName, string transmissionType, int count = 1)
    {
        if (car == null || count <= 0) return false;

        int total = GetTotalCarCount();
        if (total + count > maxCarCapacity)
        {
            UIManager.Instance?.ShowNotification("Склад машин переполнен!");
            return false;
        }

        if (string.IsNullOrEmpty(transmissionType)) transmissionType = "АКПП";
        if (trimName == null) trimName = "";

        string key = $"{car.carName}|{car.currentLevel}|{trimName}|{transmissionType}";
        var entry = carStock.FirstOrDefault(e => e.GetGroupKey() == key);
        if (entry != null)
        {
            entry.count += count;
        }
        else
        {
            carStock.Add(new CarStockEntry
            {
                blueprint = car,
                trimName = trimName,
                transmissionType = transmissionType,
                count = count
            });
        }
        return true;
    }

    /// <summary>Списывает суммарно по всем партиям модели (для контрактов).</summary>
    public bool RemoveCar(CarBlueprint car, int count = 1)
    {
        if (car == null || count <= 0) return false;
        int available = GetCarCount(car);
        if (available < count) return false;

        int remaining = count;
        for (int i = carStock.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var e = carStock[i];
            if (e.blueprint != car) continue;
            int take = Mathf.Min(e.count, remaining);
            e.count -= take;
            remaining -= take;
            if (e.count <= 0) carStock.RemoveAt(i);
        }
        return remaining == 0;
    }

    /// <summary>Списывает конкретную партию (модель + комплектация + коробка).</summary>
    public bool RemoveEntry(CarStockEntry entry, int count = 1)
    {
        if (entry == null || count <= 0) return false;
        var found = carStock.FirstOrDefault(e => e.GetGroupKey() == entry.GetGroupKey());
        if (found == null || found.count < count) return false;
        found.count -= count;
        if (found.count <= 0) carStock.Remove(found);
        return true;
    }

    public int GetCarCount(CarBlueprint car)
    {
        if (car == null) return 0;
        return carStock.Where(e => e.blueprint == car).Sum(e => e.count);
    }

    public int GetTotalCarCount() => carStock.Sum(e => e.count);

    public List<CarStockEntry> GetAllEntries() => new List<CarStockEntry>(carStock);

    /// <summary>Множитель цены комплектации по её имени (1f, если не найдена).</summary>
    public static float GetTrimPriceMultiplier(CarBlueprint car, string trimName)
    {
        if (car == null || car.recipe == null || car.recipe.availableTrims == null)
            return 1f;
        if (string.IsNullOrEmpty(trimName)) return 1f;

        var trim = car.recipe.availableTrims
            .FirstOrDefault(t => t != null && t.trimName == trimName);
        return trim != null ? trim.priceMultiplier : 1f;
    }

    /// <summary>Цена одной машины для конкретной партии (с учётом комплектации).</summary>
    public int GetUnitPriceForEntry(CarStockEntry entry)
    {
        if (entry?.blueprint == null) return 0;

        int basePrice = entry.blueprint.GetModifiedPrice(
            CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);

        float trimMult = GetTrimPriceMultiplier(entry.blueprint, entry.trimName);
        return Mathf.RoundToInt(basePrice * trimMult);
    }

    /// <summary>Агрегация по моделям (для старого UI/совместимости).</summary>
    public IReadOnlyDictionary<CarBlueprint, int> GetAllCarsWithCounts()
    {
        var dict = new Dictionary<CarBlueprint, int>();
        foreach (var e in carStock)
        {
            if (e.blueprint == null) continue;
            if (dict.ContainsKey(e.blueprint)) dict[e.blueprint] += e.count;
            else dict[e.blueprint] = e.count;
        }
        return dict;
    }

    // ==================== ПРОДАЖА ====================

    public int SellAllCarsOfModel(CarBlueprint car)
    {
        if (car == null) return 0;
        int total = GetCarCount(car);
        if (total == 0) return 0;

        int price = car.GetModifiedPrice(CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);
        double totalMoney = (double)total * price;
        CarCompanyManager.Instance.EconomyManager.AddMoney(totalMoney);

        carStock.RemoveAll(e => e.blueprint == car);
        UIManager.Instance?.ShowNotification($"Продано {total} {car.GetDisplayName()} за ${totalMoney:F0}");
        return total;
    }

    public int SellCars(CarBlueprint car, int count)
    {
        if (car == null || count <= 0) return 0;
        int toSell = Mathf.Min(GetCarCount(car), count);
        if (toSell == 0) return 0;

        int price = car.GetModifiedPrice(CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);
        double total = (double)toSell * price;
        CarCompanyManager.Instance.EconomyManager.AddMoney(total);

        int remaining = toSell;
        for (int i = carStock.Count - 1; i >= 0 && remaining > 0; i--)
        {
            var e = carStock[i];
            if (e.blueprint != car) continue;
            int take = Mathf.Min(e.count, remaining);
            e.count -= take;
            remaining -= take;
            if (e.count <= 0) carStock.RemoveAt(i);
        }

        UIManager.Instance?.ShowNotification($"Продано {toSell} {car.GetDisplayName()} за ${total:F0}");
        return toSell;
    }

    public int SellEntry(CarStockEntry entry, int count)
    {
        if (entry == null || count <= 0) return 0;
        var found = carStock.FirstOrDefault(e => e.GetGroupKey() == entry.GetGroupKey());
        if (found == null) return 0;
        int toSell = Mathf.Min(found.count, count);
        if (toSell == 0) return 0;

        int unitPrice = GetUnitPriceForEntry(found);           // ← цена с учётом комплектации
        double total = (double)toSell * unitPrice;
        CarCompanyManager.Instance.EconomyManager.AddMoney(total);

        found.count -= toSell;
        if (found.count <= 0) carStock.Remove(found);

        string trimDisplay = string.IsNullOrEmpty(found.trimName) ? "Стандарт" : found.trimName;
        UIManager.Instance?.ShowNotification(
            $"Продано {toSell} {found.blueprint.GetDisplayName()} [{trimDisplay}] за ${total:F0}");
        return toSell;
    }

    // ==================== ПРОИЗВОДСТВО ДЕТАЛЕЙ ====================
    public bool ProduceParts(PartType type, int count)
    {
        if (!IsPartProductionUnlocked(type))
        {
            UIManager.Instance?.ShowNotification($"Технология производства {type} не изучена!");
            return false;
        }
        int cost = 10 * count;
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < cost) { UIManager.Instance?.ShowNotification($"Не хватает денег! Нужно ${cost}"); return false; }
        if (economy.EngineerCount < 1) { UIManager.Instance?.ShowNotification("Нужен хотя бы 1 инженер для производства деталей!"); return false; }
        if (economy.ConveyorLevel < 1) { UIManager.Instance?.ShowNotification("Нужен хотя бы 1 уровень конвейера!"); return false; }

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
        return basePrice * CarCompanyManager.Instance.EconomyManager.TotalPriceModifier;
    }

    // ==================== СОХРАНЕНИЕ ====================

    public void FillSaveData(SaveData data)
    {
        data.warehouseLevel = warehouseLevel;
        data.partsInventory = new List<PartSaveData>();
        foreach (var kvp in partsInventory)
            data.partsInventory.Add(new PartSaveData { partType = kvp.Key, amount = kvp.Value });

        data.carStock = new List<CarSaveData>();
        foreach (var e in carStock)
        {
            if (e.blueprint == null || e.count <= 0) continue;
            var car = e.blueprint;
            data.carStock.Add(new CarSaveData
            {
                carName          = car.carName,
                currentLevel     = car.currentLevel,
                amount           = e.count,
                currentPrice     = car.currentPrice,
                currentPower     = car.currentPower,
                currentEconomy   = car.currentEconomy,
                currentDesign    = car.currentDesign,
                currentSafety    = car.currentSafety,
                transmissionType = e.transmissionType,
                trimName         = e.trimName,
                colorR = car.bodyColor.r,
                colorG = car.bodyColor.g,
                colorB = car.bodyColor.b,
                hasTint          = car.hasTint
            });
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
            foreach (var saved in data.partsInventory)
                partsInventory[saved.partType] = saved.amount;

        carStock.Clear();
        if (data.carStock == null) return;

        var allCars = CarCompanyManager.Instance.TechManager.AvailableCars;
        if (allCars == null) return;

        foreach (var saved in data.carStock)
        {
            CarBlueprint car = allCars.FirstOrDefault(c =>
                c.carName == saved.carName && c.currentLevel == saved.currentLevel);
            if (car == null)
                car = allCars.FirstOrDefault(c => c.carName == saved.carName);
            if (car == null) continue;

            car.currentLevel     = saved.currentLevel;
            car.currentPrice     = saved.currentPrice;
            car.currentPower     = saved.currentPower;
            car.currentEconomy   = saved.currentEconomy;
            car.currentDesign    = saved.currentDesign;
            car.currentSafety    = saved.currentSafety;
            car.transmissionType = saved.transmissionType;
            car.bodyColor        = new Color(saved.colorR, saved.colorG, saved.colorB);
            car.hasTint          = saved.hasTint;

            if (saved.amount <= 0) continue;

            string trim = string.IsNullOrEmpty(saved.trimName) ? "" : saved.trimName;
            string gearbox = string.IsNullOrEmpty(saved.transmissionType) ? "АКПП" : saved.transmissionType;
            string key = $"{car.carName}|{car.currentLevel}|{trim}|{gearbox}";
            var existing = carStock.FirstOrDefault(e => e.GetGroupKey() == key);
            if (existing != null)
            {
                existing.count += saved.amount;
            }
            else
            {
                carStock.Add(new CarStockEntry
                {
                    blueprint = car,
                    trimName = trim,
                    transmissionType = gearbox,
                    count = saved.amount
                });
            }
        }
    }
}