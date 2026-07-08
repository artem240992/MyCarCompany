using UnityEngine;
using System.Collections;

public class EconomyManager : MonoBehaviour
{
    // ---- События ----
    public System.Action OnMoneyChanged;

    // ---- Данные ----
    private double money = 100;
    private double passiveIncome = 0;
    private int conveyorLevel = 0;
    private int engineerCount = 0;
    private float totalPriceModifier = 1f;
    private float totalDemandModifier = 1f;
    private float totalCostModifier = 1f;
    private int reputation = 50;

    // ---- Множители (устанавливаются DifficultyManager) ----
    public float CostMultiplier { get; set; } = 1f;
    public float ProfitMultiplier { get; set; } = 1f;
    public float TechCostMultiplier { get; set; } = 1f;
    public float StartMoney { get; set; } = 100f;

    // ---- Свойства ----
    public double Money => money;
    public double PassiveIncome => passiveIncome;
    public int ConveyorLevel => conveyorLevel;
    public int EngineerCount => engineerCount;
    public float TotalPriceModifier => totalPriceModifier;
    public float TotalDemandModifier => totalDemandModifier;
    public float TotalCostModifier => totalCostModifier;
    public int Reputation => reputation;

    public void Initialize()
    {
        // Ничего особенного
    }

    // ---- Методы изменения денег ----
    public void AddMoney(double amount)
    {
        money += amount;
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(double amount)
    {
        if (money < amount) return false;
        money -= amount;
        OnMoneyChanged?.Invoke();
        return true;
    }

    public void AddPassiveIncome(double amount)
    {
        passiveIncome += amount;
    }

    public void SetPassiveIncome(double value)
    {
        passiveIncome = value;
    }

    // ---- Улучшения ----
    public void BuyConveyorUpgrade()
    {
        int cost = Mathf.RoundToInt((10 + conveyorLevel * 5) * CostMultiplier);
        if (SpendMoney(cost))
        {
            conveyorLevel++;
            passiveIncome += 0.5;
            OnMoneyChanged?.Invoke();
            CarCompanyManager.Instance.UIManager.UpdateUpgradeUI();
            CarCompanyManager.Instance.UIManager.ShowNotification($"Конвейер улучшен до {conveyorLevel} уровня!");
        }
        else
        {
            double missing = cost - money;
            CarCompanyManager.Instance.UIManager.ShowNotification($"Не хватает денег! Нужно ещё ${missing:F0}");
        }
    }

    public void HireEngineer()
    {
        int cost = Mathf.RoundToInt((50 + engineerCount * 20) * CostMultiplier);
        if (SpendMoney(cost))
        {
            engineerCount++;
            passiveIncome += 2;
            OnMoneyChanged?.Invoke();
            CarCompanyManager.Instance.UIManager.UpdateUpgradeUI();
            CarCompanyManager.Instance.UIManager.ShowNotification($"Нанят инженер! Всего: {engineerCount}");
        }
        else
        {
            double missing = cost - money;
            CarCompanyManager.Instance.UIManager.ShowNotification($"Не хватает денег! Нужно ещё ${missing:F0}");
        }
    }

    // ---- Репутация ----
    public void AddReputation(int value)
    {
        reputation = Mathf.Max(0, reputation + value);
    }

    public void SetReputation(int value)
    {
        reputation = Mathf.Max(0, value);
    }

    // ---- Модификаторы ----
    public void RecalculateModifiers(Technology[] technologies)
    {
        totalPriceModifier = 1f;
        totalDemandModifier = 1f;
        totalCostModifier = 1f;

        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech != null && tech.isResearched)
                {
                    totalPriceModifier *= tech.priceModifier;
                    totalDemandModifier *= tech.demandModifier;
                    if (tech.techName == "Гибридный привод")
                        totalCostModifier *= 0.9f;
                }
            }
        }
        totalPriceModifier = Mathf.Clamp(totalPriceModifier, 0.5f, 5f);
        totalDemandModifier = Mathf.Clamp(totalDemandModifier, 0.5f, 5f);
        totalCostModifier = Mathf.Clamp(totalCostModifier, 0.5f, 2f);
    }

    // ---- Пассивный доход (корутина) ----
    public IEnumerator PassiveIncomeLoop()
    {
        float demandTimer = 0f;
        float demandUpdateInterval = CarCompanyManager.Instance.DemandManager.DemandUpdateInterval;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            money += passiveIncome;
            OnMoneyChanged?.Invoke();
            demandTimer += 1f;
            if (demandTimer >= demandUpdateInterval)
            {
                demandTimer = 0f;
                CarCompanyManager.Instance.DemandManager.UpdateDemand();
            }
        }
    }

    // ---- Сброс состояния (для новой игры) ----
    public void ResetState()
    {
        money = StartMoney;
        passiveIncome = 0;
        conveyorLevel = 0;
        engineerCount = 0;
        reputation = 50;
        OnMoneyChanged?.Invoke();
    }

    // ---- Методы для сохранения/загрузки ----
    public void LoadFromSave(SaveData data)
    {
        money = data.money;
        passiveIncome = data.passiveIncome;
        conveyorLevel = data.conveyorLevel;
        engineerCount = data.engineerCount;
        OnMoneyChanged?.Invoke();
    }

    public void FillSaveData(SaveData data)
    {
        data.money = money;
        data.passiveIncome = passiveIncome;
        data.conveyorLevel = conveyorLevel;
        data.engineerCount = engineerCount;
    }
}