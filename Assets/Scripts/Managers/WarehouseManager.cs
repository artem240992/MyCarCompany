using UnityEngine;
using System.Collections.Generic;

public class WarehouseManager : MonoBehaviour
{
    public static WarehouseManager Instance { get; private set; }

    [Header("Warehouse Settings")]
    public int warehouseLevel = 0;
    public int maxCapacity = 100;

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
    }

    // ---- ПРОИЗВОДСТВО ДЕТАЛЕЙ ----
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

    // ---- Сохранение/загрузка ----
    public void FillSaveData(SaveData data)
    {
        data.warehouseLevel = warehouseLevel;
        data.partsInventory = new List<PartSaveData>();
        foreach (var kvp in partsInventory)
        {
            data.partsInventory.Add(new PartSaveData { partType = kvp.Key, amount = kvp.Value });
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
    }
}

[System.Serializable]
public class PartSaveData
{
    public PartType partType;
    public int amount;
}