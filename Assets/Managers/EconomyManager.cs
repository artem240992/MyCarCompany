using UnityEngine;
using System;
using System.Collections;

public class EconomyManager : MonoBehaviour
{
    public double Money = 100;
    public double PassiveIncome = 0;
    public int ConveyorLevel = 0;
    public int EngineerCount = 0;
    public int Reputation = 50;

    public float CostMultiplier = 1f;
    public float TotalPriceModifier = 1f;
    public float TechCostMultiplier = 1f;
    public float TotalDemandModifier = 1f;

    public float StartMoney = 100;
    public float ProfitMultiplier = 1f;
    public float TemporaryPriceModifier = 1f;

    public float inflationRate = 0.0005f; // теперь за месяц
    public float basePriceMultiplier = 1f;

    public float DifficultyTechCostMultiplier = 1f;

    private int lastTaxYear;

    public event Action OnMoneyChanged;

    public void Initialize(float startMoney, float profitMultiplier)
    {
        Money = startMoney;
        StartMoney = startMoney;
        ProfitMultiplier = profitMultiplier;
        Reputation = 50;
        ConveyorLevel = 0;
        EngineerCount = 0;
        PassiveIncome = 0;
        basePriceMultiplier = 1f;
        TemporaryPriceModifier = 1f;
        DifficultyTechCostMultiplier = 1f;
        if (GameTimeManager.Instance != null)
            lastTaxYear = GameTimeManager.Instance.currentYear;
        else
            lastTaxYear = 2025;
        UpdatePassiveIncome();
        OnMoneyChanged?.Invoke();
    }

    public bool SpendMoney(double amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        OnMoneyChanged?.Invoke();
        return true;
    }

    public void AddMoney(double amount)
{
    Money += amount;
    OnMoneyChanged?.Invoke();
    var achManager = CarCompanyManager.Instance.AchievementManager;
    if (achManager != null)
        achManager.UpdateProgress("money", (int)Money);
}

    public void AddReputation(int amount)
    {
        Reputation = Mathf.Max(0, Reputation + amount);
    }

    public void LoseEngineer()
    {
        EngineerCount = Mathf.Max(0, EngineerCount - 1);
        UpdatePassiveIncome();
    }

    public void AddEngineer()
    {
        EngineerCount++;
        UpdatePassiveIncome();
    }

    public void AddPassiveIncome(double amount)
    {
        PassiveIncome += amount;
    }

    private void UpdatePassiveIncome()
    {
        PassiveIncome = (ConveyorLevel * 0.5f) + (EngineerCount * 0.3f);
    }

    public void BuyConveyorUpgrade()
    {
        int cost = Mathf.RoundToInt((10 + ConveyorLevel * 5) * CostMultiplier * basePriceMultiplier);
        if (SpendMoney(cost))
        {
            ConveyorLevel++;
            UpdatePassiveIncome();
            CarCompanyManager.Instance.UIManager?.UpdateUpgradeUI();
            CarCompanyManager.Instance.UIManager?.ShowNotification($"Конвейер улучшен до уровня {ConveyorLevel}");
        }
        else
        {
            CarCompanyManager.Instance.UIManager?.ShowNotification($"Не хватает денег для улучшения конвейера (нужно ${cost})");
        }
    }

    public void HireEngineer()
    {
        int cost = Mathf.RoundToInt((50 + EngineerCount * 20) * CostMultiplier * basePriceMultiplier);
        if (SpendMoney(cost))
        {
            EngineerCount++;
            UpdatePassiveIncome();
            CarCompanyManager.Instance.UIManager?.UpdateUpgradeUI();
            CarCompanyManager.Instance.UIManager?.ShowNotification($"Нанят инженер (всего: {EngineerCount})");
        }
        else
        {
            CarCompanyManager.Instance.UIManager?.ShowNotification($"Не хватает денег для найма инженера (нужно ${cost})");
        }
    }

    public void RecalculateModifiers(Technology[] technologies)
    {
        float priceMod = 1f;
        float costMod = 1f;
        float techCostMod = 1f;
        TotalPriceModifier = priceMod * basePriceMultiplier * TemporaryPriceModifier;
        CostMultiplier = costMod * basePriceMultiplier * TemporaryPriceModifier;
        TechCostMultiplier = techCostMod * basePriceMultiplier * TemporaryPriceModifier * DifficultyTechCostMultiplier;
        TotalDemandModifier = 1f;
    }

    public float GetSeasonalDemandModifier()
    {
        if (GameTimeManager.Instance == null) return 1f;
        int month = GameTimeManager.Instance.currentMonth;
        // 12-месячный цикл: январь = 1 -> sin(0), пик в середине года
        float angle = ((month - 1) / 12f) * Mathf.PI * 2f;
        return 0.8f + 0.4f * (0.5f + 0.5f * Mathf.Sin(angle));
    }

    public float GetTaxRate(CarBlueprint car)
    {
        float baseTax = 0f;
        var difficulty = CarCompanyManager.Instance.UIManager?.GetCurrentDifficulty() ?? UIManager.Difficulty.Normal;
        switch (difficulty)
        {
            case UIManager.Difficulty.Easy:   baseTax = 0.05f; break;
            case UIManager.Difficulty.Normal: baseTax = 0.10f; break;
            case UIManager.Difficulty.Hard:   baseTax = 0.15f; break;
        }
        float levelBonus = car.currentLevel * 0.1f;
        int totalTuning = car.currentPower + car.currentEconomy + car.currentDesign + car.currentSafety;
        float tuningBonus = totalTuning * 0.02f;
        return Mathf.Min(baseTax + levelBonus + tuningBonus, 0.5f);
    }

    public void FillSaveData(SaveData data)
    {
        data.money = Money;
        data.conveyorLevel = ConveyorLevel;
        data.engineerCount = EngineerCount;
        data.reputation = Reputation;
        data.passiveIncome = PassiveIncome;
        data.basePriceMultiplier = basePriceMultiplier;
        data.lastTaxYear = lastTaxYear;
    }

    public void LoadFromSave(SaveData data)
    {
        Money = data.money;
        ConveyorLevel = data.conveyorLevel;
        EngineerCount = data.engineerCount;
        Reputation = data.reputation;
        PassiveIncome = data.passiveIncome;
        basePriceMultiplier = data.basePriceMultiplier;
        lastTaxYear = data.lastTaxYear;
        OnMoneyChanged?.Invoke();
    }

    public void ResetState()
    {
        Money = StartMoney;
        ConveyorLevel = 0;
        EngineerCount = 0;
        Reputation = 50;
        PassiveIncome = 0;
        basePriceMultiplier = 1f;
        TemporaryPriceModifier = 1f;
        DifficultyTechCostMultiplier = 1f;
        if (GameTimeManager.Instance != null)
            lastTaxYear = GameTimeManager.Instance.currentYear;
        OnMoneyChanged?.Invoke();
    }

    public IEnumerator PassiveIncomeLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            if (PassiveIncome > 0)
            {
                Money += PassiveIncome;
                OnMoneyChanged?.Invoke();
            }
        }
    }

    private void Awake()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += OnMonthChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= OnMonthChanged;
    }

    // ---- ОБРАБОТЧИК СМЕНЫ МЕСЯЦА ----
    private void OnMonthChanged()
    {
        // Инфляция
        basePriceMultiplier *= (1f + inflationRate);
        RecalculateModifiers(null);

        // ---- ГОДОВОЙ НАЛОГ (в январе) ----
        if (GameTimeManager.Instance != null)
        {
            int month = GameTimeManager.Instance.currentMonth;
            int year = GameTimeManager.Instance.currentYear;

            if (month == 1 && year > lastTaxYear)
            {
                float taxRate = CarCompanyManager.Instance.DifficultyManager.GetYearlyTaxRate();
                double taxAmount = Money * taxRate;
                Money -= taxAmount;
                if (Money < 0) Money = 0;
                lastTaxYear = year;

                CarCompanyManager.Instance.UIManager?.ShowNotification(
                    $"Годовой налог: ${taxAmount:F0} ({(taxRate * 100):F0}%) списано."
                );
                OnMoneyChanged?.Invoke();
            }
        }

        CarCompanyManager.Instance.UIManager?.UpdateDateTimeDisplay();
    }
}