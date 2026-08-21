using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

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

    // ---- Для расчёта годового налога ----
    private double yearlyIncome = 0;
    private double yearlyExpenses = 0;

    // ---- Для месячного дохода (график) ----
    private double monthlyIncome = 0;

    // ---- Скидки ----
    public float DiscountMultiplier = 1f;
    public float DiscountDuration = 0f;

    public float inflationRate = 0.0002f;
    public float basePriceMultiplier = 1f;

    public float DifficultyTechCostMultiplier = 1f;

    private int lastTaxYear;

    // ---- История доходов для графика ----
    public List<float> monthlyIncomeHistory = new List<float>();

    // ---- Скидка на производство (от инвестиций) ----
    private float productionDiscount = 0f;

    // ---- Достижения по продаже машин ----
    private int totalCarsSold = 0;

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
        DiscountMultiplier = 1f;
        DiscountDuration = 0f;
        DifficultyTechCostMultiplier = 1f;
        productionDiscount = 0f;
        monthlyIncomeHistory.Clear();
        totalCarsSold = 0;
        monthlyIncome = 0;
        if (GameTimeManager.Instance != null)
            lastTaxYear = GameTimeManager.Instance.currentYear;
        else
            lastTaxYear = 2025;
        UpdatePassiveIncome();
        OnMoneyChanged?.Invoke();
        yearlyIncome = 0;
        yearlyExpenses = 0;
        ApplyDifficultySettings(true); // применяем настройки сложности при старте
    }

    public void AddMoney(double amount)
    {
        Money += amount;
        yearlyIncome += amount;
        monthlyIncome += amount;
        OnMoneyChanged?.Invoke();
        var achManager = CarCompanyManager.Instance.AchievementManager;
        if (achManager != null)
            achManager.UpdateProgress("money", (int)Money);
    }

    public bool SpendMoney(double amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        yearlyExpenses += amount;
        OnMoneyChanged?.Invoke();
        return true;
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
        int cost = Mathf.RoundToInt((15 + ConveyorLevel * 8) * CostMultiplier * basePriceMultiplier);
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
        int cost = Mathf.RoundToInt((40 + EngineerCount * 15) * CostMultiplier * basePriceMultiplier);
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
        TotalPriceModifier = priceMod * basePriceMultiplier * TemporaryPriceModifier * DiscountMultiplier;
        CostMultiplier = costMod * basePriceMultiplier * TemporaryPriceModifier * DiscountMultiplier;
        TechCostMultiplier = techCostMod * basePriceMultiplier * TemporaryPriceModifier * DiscountMultiplier * DifficultyTechCostMultiplier;
        TotalDemandModifier = 1f;
    }

    public void ApplyDiscount(float discount, int months)
    {
        if (discount < 0f || discount > 1f || months <= 0)
        {
            Debug.LogWarning($"Некорректные параметры скидки: {discount}, {months}");
            return;
        }
        DiscountMultiplier = 1f - discount;
        DiscountDuration = months;
        RecalculateModifiers(null);
        CarCompanyManager.Instance.CompetitorManager.OnPlayerAppliesDiscount(discount);
        CarCompanyManager.Instance.UIManager?.UpdateMoneyLabels();
        CarCompanyManager.Instance.UIManager?.ShowNotification($"Скидка {discount * 100:F0}% на все машины на {months} мес.");
    }

    public void UpdateDiscount()
    {
        if (DiscountDuration > 0)
        {
            DiscountDuration -= 1f;
            if (DiscountDuration <= 0)
            {
                DiscountMultiplier = 1f;
                RecalculateModifiers(null);
                CarCompanyManager.Instance.UIManager?.UpdateMoneyLabels();
                CarCompanyManager.Instance.UIManager?.ShowNotification("Скидка закончилась.");
            }
        }
    }

    public void ApplyProductionDiscount(float discount)
    {
        productionDiscount = Mathf.Clamp(discount, 0f, 0.3f);
    }

    public int GetProductionCostWithLevel(CarBlueprint car)
    {
        if (car == null || car.recipe == null) return 50;
        int baseCost = car.recipe.assemblyCost + car.currentLevel * 20;
        float finalCost = baseCost * (1f - productionDiscount);
        return Mathf.RoundToInt(finalCost);
    }

    public float GetSeasonalDemandModifier()
    {
        if (GameTimeManager.Instance == null) return 1f;
        int month = GameTimeManager.Instance.currentMonth;
        float angle = ((month - 1) / 12f) * Mathf.PI * 2f;
        return 0.8f + 0.4f * (0.5f + 0.5f * Mathf.Sin(angle));
    }

    public float GetTaxRate(CarBlueprint car)
    {
        float baseTax = 0f;
        var difficulty = CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
        switch (difficulty)
        {
            case DifficultyManager.DifficultyLevel.Easy:   baseTax = 0.05f; break;
            case DifficultyManager.DifficultyLevel.Normal: baseTax = 0.05f; break;
            case DifficultyManager.DifficultyLevel.Hard:   baseTax = 0.15f; break;
        }
        float levelBonus = car.currentLevel * 0.1f;
        int totalTuning = car.currentPower + car.currentEconomy + car.currentDesign + car.currentSafety;
        float tuningBonus = totalTuning * 0.02f;
        return Mathf.Min(baseTax + levelBonus + tuningBonus, 0.5f);
    }

    public float GetPartCostForCar(CarBlueprint car)
    {
        if (car == null || car.recipe == null) return 0f;
        var recipe = car.recipe;
        var partsMarket = CarCompanyManager.Instance.PartsMarketManager;
        float total = 0f;

        total += (recipe.enginePrice >= 0 ? recipe.enginePrice : partsMarket.GetPartPrice(PartType.Engine)) * recipe.engineRequired;
        total += (recipe.bodyPrice >= 0 ? recipe.bodyPrice : partsMarket.GetPartPrice(PartType.Body)) * recipe.bodyRequired;
        total += (recipe.wheelsPrice >= 0 ? recipe.wheelsPrice : partsMarket.GetPartPrice(PartType.Wheels)) * recipe.wheelsRequired;
        total += (recipe.electronicsPrice >= 0 ? recipe.electronicsPrice : partsMarket.GetPartPrice(PartType.Electronics)) * recipe.electronicsRequired;

        return total;
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
        data.discountMultiplier = DiscountMultiplier;
        data.discountDuration = DiscountDuration;
        data.monthlyIncomeHistory = monthlyIncomeHistory;
        data.totalCarsSold = totalCarsSold;
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
        DiscountMultiplier = data.discountMultiplier;
        DiscountDuration = data.discountDuration;
        monthlyIncomeHistory = data.monthlyIncomeHistory ?? new List<float>();
        totalCarsSold = data.totalCarsSold;
        monthlyIncome = 0;
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
        DiscountMultiplier = 1f;
        DiscountDuration = 0f;
        DifficultyTechCostMultiplier = 1f;
        productionDiscount = 0f;
        monthlyIncomeHistory.Clear();
        totalCarsSold = 0;
        monthlyIncome = 0;
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
                AddMoney(PassiveIncome);
            }
        }
    }

    public void RegisterCarSold(int count)
    {
        if (count <= 0) return;
        totalCarsSold += count;
        CarCompanyManager.Instance.AchievementManager?.UpdateProgress("carsProduced", totalCarsSold);
    }

    // ---- Применение настроек сложности (вызывается при старте и при смене сложности) ----
    public void ApplyDifficultySettings(bool isNewGame = false)
    {
        var dm = CarCompanyManager.Instance.DifficultyManager;
        if (dm == null) return;

        TotalPriceModifier = dm.CurrentPriceModifier;
        CostMultiplier = dm.CurrentProductionCostModifier;
        TechCostMultiplier = dm.CurrentResearchCostModifier;
        inflationRate = dm.CurrentInflationRate;
        StartMoney = dm.CurrentStartMoney;

        if (isNewGame)
        {
            Money = StartMoney;
            OnMoneyChanged?.Invoke();
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

    private void OnMonthChanged()
    {
        basePriceMultiplier *= (1f + inflationRate);
        UpdateDiscount();
        RecalculateModifiers(null);

        monthlyIncomeHistory.Add((float)monthlyIncome);
        if (monthlyIncomeHistory.Count > 12)
            monthlyIncomeHistory.RemoveAt(0);
        monthlyIncome = 0;

        if (GameTimeManager.Instance != null)
        {
            int month = GameTimeManager.Instance.currentMonth;
            int year = GameTimeManager.Instance.currentYear;

            if (month == 1 && year > lastTaxYear)
            {
                double yearlyProfit = yearlyIncome - yearlyExpenses;
                float taxRate = CarCompanyManager.Instance.DifficultyManager.GetYearlyTaxRate();
                double taxAmount = 0;

                if (yearlyProfit > 0)
                {
                    taxAmount = yearlyProfit * taxRate;
                    Money -= taxAmount;
                    if (Money < 0) Money = 0;
                }

                yearlyIncome = 0;
                yearlyExpenses = 0;
                lastTaxYear = year;
                CarCompanyManager.Instance.DemandManager?.UpdateDemand();
                CarCompanyManager.Instance.UIManager?.ShowNotification(
                    $"Годовой налог: ${taxAmount:F0} ({(taxRate * 100):F0}% от прибыли ${yearlyProfit:F0})"
                );
                OnMoneyChanged?.Invoke();
            }
        }

        CarCompanyManager.Instance.UIManager?.UpdateDateTimeDisplay();
        CarCompanyManager.Instance.DemandManager?.UpdateDemand();
    }
}