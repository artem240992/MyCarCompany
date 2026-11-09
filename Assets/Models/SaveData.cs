using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    // ===== Обучение =====
    public int tutorialProgress;
    public bool tutorialCarsCompleted;
    public bool tutorialTechCompleted;
    public bool tutorialMarketingCompleted;
    public bool tutorialUpgradeCompleted;
    public bool tutorialCompetitorsCompleted;

    // ===== Экономика =====
    public double money;
    public int conveyorLevel;
    public int engineerCount;
    public int reputation;
    public double passiveIncome;
    public float basePriceMultiplier;
    public int lastTaxYear;
    public double yearlyIncome;
    public double yearlyExpenses;
    public float discountMultiplier = 1f;
    public float discountDuration = 0f;
    public List<float> monthlyIncomeHistory = new List<float>();

    // ===== Сотрудники =====
    public int designerCount;
    public int marketerCount;
    public int managerCount;

    // ===== Статистика =====
    public int totalCarsSold;
    public int totalPartsSold;
    public int totalPartsProduced;
    public int totalOfficesOpened;

    // ===== Время =====
    public int currentMonth;
    public int currentYear;
    public int currentDay;

    // ===== Сложность =====
    public int difficulty;

    // ===== Склад =====
    public int warehouseLevel;
    public List<PartSaveData> partsInventory;
    public List<PartPriceSaveData> partPrices;
    public List<CarSaveData> carStock = new List<CarSaveData>();

    // ===== Технологии =====
    public string[] researchedTechNames;
    public List<TechnologySaveData> technologyData;

    // ===== Улучшения машин =====
    public List<CarLevelData> carLevels;
    public List<CarBlueprintSaveData> createdCarsData;

    // ===== Логи действий =====
    public List<ActionLogEntry> actionLogs;

    // ===== Достижения =====
    public List<AchievementProgress> achievementProgress;

    // ===== Конкуренты =====
    public List<CompetitorSaveData> competitors = new List<CompetitorSaveData>();

    // ===== Маркетинг =====
    public List<MarketingCampaign> activeCampaigns = new List<MarketingCampaign>();
    public float brandQuality = 50f;

    // ===== Международная экспансия и ИС =====
    public List<OfficeData> offices = new List<OfficeData>();
    public List<PatentData> patents = new List<PatentData>();
    public List<LicenseData> licenses = new List<LicenseData>();

    // ===== Кредиты и инвестиции =====
    public List<LoanManager.Loan> loans = new List<LoanManager.Loan>();
    public List<InvestmentManager.Investment> investments = new List<InvestmentManager.Investment>();

    // ===== Контракты =====
    public List<ContractSaveData> availableContracts = new List<ContractSaveData>();
    public List<ContractSaveData> activeContracts = new List<ContractSaveData>();

    // ===== Демо =====
    public float demoTimeRemaining = 1800f;
    public int demoTechsResearched = 0;

    // ===== Бренд =====
    public string companyName;
    public string companyLogoPath;
        // ===== Электромобили и инфраструктура =====
    public bool evSubsidyActive;
    public int evSubsidyMonthsLeft;
    public List<InternationalManager.ChargingStation> chargingStations = new List<InternationalManager.ChargingStation>();

    public List<CustomerRecord> customers = new List<CustomerRecord>();
    public List<TradeInOffer> tradeInOffers = new List<TradeInOffer>();
}

[Serializable]
public class CarSaveData
{
    public string carName;
    public int currentLevel;
    public int amount;
    public int currentPrice;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public string transmissionType;
    public string trimName;   // ← НОВОЕ
    public float colorR, colorG, colorB;
    public bool hasTint;

    // ===== Электромобили =====
    public List<InternationalManager.ChargingStation> chargingStations = new List<InternationalManager.ChargingStation>();
    public bool evSubsidyActive;
    public int evSubsidyMonthsLeft;
}

[Serializable]
public class ContractSaveData
{
    public string contractId;
    public string title;
    public int type;
    public int requiredQuantity;
    public int monthsRemaining;
    public float rewardMoney;       // ← было double
    public float rewardReputation;
    public float penaltyMoney;      // ← было double
    public int deliveredQuantity;
    public bool isAccepted;
    public bool isCompleted;
}

[Serializable]
public class CarLevelData
{
    public string carName;
    public int currentLevel;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}

[Serializable]
public class CarBlueprintSaveData
{
    public string carName;
    public int currentLevel;
    public int currentPrice;
    public int tuningPower;
    public int tuningEconomy;
    public int tuningDesign;
    public int tuningSafety;
    public int currentPower;
    public int currentEconomy;
    public int currentDesign;
    public int currentSafety;
    public float demandMultiplier;
    public float bodyColorR;
    public float bodyColorG;
    public float bodyColorB;
    public bool hasTint;
}

[Serializable]
public class TechnologySaveData
{
    public string techName;
    public bool isResearched;
    public int availableYear;
    public int availableMonth;
}

[Serializable]
public class CompetitorSaveData
{
    public string companyName;
    public float money;
    public int factoryLevel;
    public int researchLevel;
    public int reputation;
    public float priceMultiplier;
    public float marketShare;
    public int engineers;
    public int espionageLevel;
    public int marketingPower;
    public int loyalty;
    public bool isAlly;
    public float marketingBudget;
    public float brandQuality;
    public string strategy;
    public List<string> availableCarNames;
    public List<string> stolenTechs;
    public List<string> researchedTechs;
    public List<MarketingCampaign> activeCampaigns;
}