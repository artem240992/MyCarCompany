using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class TechManager : MonoBehaviour
{
    private Technology[] technologies;
    private CarBlueprint[] availableCars;
    private CarBlueprint[] startCars;
    private List<CarBlueprint> createdCars = new List<CarBlueprint>();

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;

    private string[] tuningParamNames = { "power", "economy", "design", "safety" };
    private string[] tuningParamDisplay = { "Мощность", "Экономичность", "Дизайн", "Безопасность" };
    private const int TUNING_MAX_LEVEL = 10;

    public Technology[] Technologies => technologies;
    public CarBlueprint[] AvailableCars => availableCars;

    public void Initialize(CarBlueprint[] startCarsArray)
    {
        startCars = startCarsArray;
        technologies = null;
        createdCars.Clear();

        if (startCars != null)
        {
            foreach (var car in startCars)
            {
                if (car != null)
                {
                    car.currentLevel = 0;
                    car.currentPrice = car.basePrice;
                    Debug.Log($"TechManager: инициализирован {car.carName} (уровень 0, цена {car.currentPrice})");
                }
            }
        }

        GenerateTuningTechnologies();
        GenerateCarUnlockTechnologies();
        GenerateSpecialTechnologies();
        AddAdditionalTechnologies();
        BuildAvailableCars();

        Debug.Log($"TechManager: Всего технологий: {technologies?.Length ?? 0}");
        if (technologies != null)
            foreach (var t in technologies) Debug.Log($"  - {t.techName} (исследована: {t.isResearched})");

        economy.RecalculateModifiers(technologies);
        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
    }

    // ---- Генерация тюнинговых технологий (с availableYear/Month) ----
    private void GenerateTuningTechnologies()
    {
        if (technologies != null && technologies.Any(t => t != null && t.techName == "power_1"))
            return;

        List<Technology> newTechs = new List<Technology>();
        if (technologies != null) newTechs.AddRange(technologies);

        for (int i = 0; i < tuningParamNames.Length; i++)
        {
            string param = tuningParamNames[i];
            string display = tuningParamDisplay[i];
            for (int level = 1; level <= TUNING_MAX_LEVEL; level++)
            {
                Technology tech = new Technology();
                tech.techName = $"{param}_{level}";
                tech.description = $"Улучшает {display} до уровня {level}";
                tech.researchCost = 40 + level * 25;
                tech.isResearched = false;
                tech.requiredTechNames = (level > 1) ? new string[] { $"{param}_{level - 1}" } : new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                tech.availableYear = (level == 1) ? 2025 : 9999;
                tech.availableMonth = 1;
                newTechs.Add(tech);
            }
        }
        technologies = newTechs.ToArray();
    }

    // ---- Генерация технологий открытия машин (с availableYear) ----
    private void GenerateCarUnlockTechnologies()
    {
        if (technologies != null && technologies.Any(t => t != null && t.unlockCarOnResearch))
            return;

        List<Technology> newTechs = new List<Technology>();
        if (technologies != null) newTechs.AddRange(technologies);

        List<CarBlueprint> allCars = new List<CarBlueprint>();
        if (startCars != null)
        {
            foreach (var car in startCars)
                if (car != null) allCars.Add(car);
        }

        for (int i = 1; i < allCars.Count; i++)
        {
            CarBlueprint car = allCars[i];
            if (car == null) continue;
            if (technologies != null && technologies.Any(t => t != null && t.unlockedCar == car))
                continue;

            Technology tech = new Technology();
            tech.techName = $"Открыть {car.carName}";
            tech.description = $"Позволяет производить {car.carName}";
            tech.researchCost = 150 + car.basePrice / 2;
            tech.isResearched = false;
            tech.requiredTechNames = new string[0];
            tech.priceModifier = 1f;
            tech.demandModifier = 1f;
            tech.unlockCarOnResearch = true;
            tech.unlockedCar = car;
            tech.availableYear = 2025 + i;
            tech.availableMonth = 1;
            newTechs.Add(tech);
        }
        technologies = newTechs.ToArray();
    }

    // ---- Создание специальных технологий ----
    private void GenerateSpecialTechnologies()
    {
        if (technologies == null) return;
        List<Technology> techList = technologies.ToList();

        // ---- Bulk Production ----
        string bulkName = CarCompanyManager.Instance.BulkProductionTechName;
        if (!techList.Any(t => t != null && t.techName == bulkName))
        {
            Technology bulkTech = new Technology();
            bulkTech.techName = bulkName;
            bulkTech.description = "Позволяет производить до 10 машин за раз";
            bulkTech.researchCost = 200;
            bulkTech.isResearched = false;
            bulkTech.requiredTechNames = new string[0];
            bulkTech.priceModifier = 1f;
            bulkTech.demandModifier = 1f;
            bulkTech.unlockCarOnResearch = false;
            bulkTech.unlockedCar = null;
            bulkTech.availableYear = 2025;
            bulkTech.availableMonth = 1;
            techList.Add(bulkTech);
        }

        // ---- Car Upgrade ----
        string upgradeName = CarCompanyManager.Instance.CarUpgradeTechName;
        if (!techList.Any(t => t != null && t.techName == upgradeName))
        {
            Technology upgradeTech = new Technology();
            upgradeTech.techName = upgradeName;
            upgradeTech.description = "Позволяет улучшать машины на уровень";
            upgradeTech.researchCost = 150;
            upgradeTech.isResearched = false;
            upgradeTech.requiredTechNames = new string[0];
            upgradeTech.priceModifier = 1f;
            upgradeTech.demandModifier = 1f;
            upgradeTech.unlockCarOnResearch = false;
            upgradeTech.unlockedCar = null;
            upgradeTech.availableYear = 2025;
            upgradeTech.availableMonth = 1;
            techList.Add(upgradeTech);
        }

        // ---- Производство деталей ----
        string[] partTypes = { "Engine", "Body", "Wheels", "Electronics" };
        foreach (string part in partTypes)
        {
            string techName = $"Производство {part}";
            if (!techList.Any(t => t != null && t.techName == techName))
            {
                Technology tech = new Technology();
                tech.techName = techName;
                tech.description = $"Позволяет производить {part} на складе";
                tech.researchCost = 100 + Array.IndexOf(partTypes, part) * 50;
                tech.isResearched = false;
                tech.requiredTechNames = new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                tech.availableYear = 2025;
                tech.availableMonth = 1;
                techList.Add(tech);
            }
        }

        // ---- Улучшение склада (до 5 уровней) ----
        for (int i = 1; i <= 5; i++)
        {
            string techName = $"Улучшение склада {i}";
            if (!techList.Any(t => t != null && t.techName == techName))
            {
                Technology tech = new Technology();
                tech.techName = techName;
                tech.description = $"Увеличивает склад на {100 * i}%";
                tech.researchCost = 200 + i * 100;
                tech.isResearched = false;
                tech.requiredTechNames = i > 1 ? new string[] { $"Улучшение склада {i - 1}" } : new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                tech.availableYear = 2025;
                tech.availableMonth = 1;
                techList.Add(tech);
            }
        }

        // ---- Типы рекламы как технологии ----
        string[] adTypes = { "TV", "Internet", "Print", "Social" };
        int[] adCosts = { 150, 200, 120, 180 };
        int[] adYears = { 2025, 2025, 2025, 2026 };
        int[] adMonths = { 1, 3, 6, 1 };

        for (int i = 0; i < adTypes.Length; i++)
        {
            string techName = $"Реклама {adTypes[i]}";
            if (!techList.Any(t => t != null && t.techName == techName))
            {
                Technology tech = new Technology();
                tech.techName = techName;
                tech.description = $"Позволяет запускать рекламные кампании типа {adTypes[i]}";
                tech.researchCost = adCosts[i];
                tech.isResearched = false;
                tech.requiredTechNames = new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                tech.availableYear = adYears[i];
                tech.availableMonth = adMonths[i];
                techList.Add(tech);
            }
        }

        // ---- Новые технологии (электромобили, гибрид, автопилот) ----
        string[] newTechs = { "Электромобиль", "Гибридный двигатель", "Автопилот" };
        int[] costs = { 800, 600, 1200 };
        int[] years = { 2026, 2025, 2027 };
        int[] months = { 1, 7, 1 };
        for (int i = 0; i < newTechs.Length; i++)
        {
            string techName = newTechs[i];
            if (techList.Any(t => t != null && t.techName == techName)) continue;
            Technology tech = new Technology();
            tech.techName = techName;
            tech.description = techName == "Электромобиль" ? "Позволяет производить электромобили (высокий спрос, дороже в производстве)" :
                                techName == "Гибридный двигатель" ? "Позволяет производить гибридные машины (средний спрос, экономичные)" :
                                "Повышает привлекательность всех машин на 30%";
            tech.researchCost = costs[i];
            tech.isResearched = false;
            tech.requiredTechNames = techName == "Электромобиль" ? new string[] { "Гибридный двигатель" } : new string[0];
            tech.priceModifier = techName == "Автопилот" ? 1.3f : 1f;
            tech.demandModifier = techName == "Автопилот" ? 1.2f : 1f;
            tech.unlockCarOnResearch = techName != "Автопилот";
            tech.unlockedCar = techName == "Электромобиль" ? CreateElectricCar() : 
                            techName == "Гибридный двигатель" ? CreateHybridCar() : null;
            tech.availableYear = years[i];
            tech.availableMonth = months[i];
            techList.Add(tech);
        }

        // ========== НОВЫЕ ТЕХНОЛОГИИ ДЛЯ РЕЛИЗА 1.14.0 ==========
        // ---- Технологии для международной экспансии и патентов ----
        string[] specialTechs = { "Международная экспансия", "Патентное право" };
        int[] specialCosts = { 600, 500 };
        int[] specialYears = { 2025, 2025 };
        int[] specialMonths = { 1, 1 };
        for (int i = 0; i < specialTechs.Length; i++)
        {
            string techName = specialTechs[i];
            if (!techList.Any(t => t != null && t.techName == techName))
            {
                Technology tech = new Technology();
                tech.techName = techName;
                tech.description = techName == "Международная экспансия" ? "Позволяет открывать представительства за рубежом" :
                                    "Позволяет патентовать технологии и выдавать лицензии";
                tech.researchCost = specialCosts[i];
                tech.isResearched = false;
                tech.requiredTechNames = new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                tech.availableYear = specialYears[i];
                tech.availableMonth = specialMonths[i];
                techList.Add(tech);
            }
        }

        technologies = techList.ToArray();
    }

    /// <summary>
    /// Создание электромобиля.
    /// </summary>
    private CarBlueprint CreateElectricCar()
    {
        CarBlueprint car = ScriptableObject.CreateInstance<CarBlueprint>();
        car.carName = "Электромобиль";
        car.basePrice = 400;
        car.recipe = CreateRecipe(2, 2, 3, 4);
        car.demandMultiplier = 1.5f;
        car.currentLevel = 0;
        return car;
    }

    /// <summary>
    /// Создание гибридного автомобиля.
    /// </summary>
    private CarBlueprint CreateHybridCar()
    {
        CarBlueprint car = ScriptableObject.CreateInstance<CarBlueprint>();
        car.carName = "Гибрид";
        car.basePrice = 300;
        car.recipe = CreateRecipe(2, 2, 2, 2);
        car.demandMultiplier = 1.2f;
        car.currentLevel = 0;
        return car;
    }

    /// <summary>
    /// Создание рецепта для машины.
    /// </summary>
    private CarRecipe CreateRecipe(int engine, int body, int wheels, int electronics)
    {
        CarRecipe recipe = ScriptableObject.CreateInstance<CarRecipe>();
        recipe.engineRequired = engine;
        recipe.bodyRequired = body;
        recipe.wheelsRequired = wheels;
        recipe.electronicsRequired = electronics;
        recipe.assemblyCost = 50 + (engine + body + wheels + electronics) * 10;
        return recipe;
    }

    /// <summary>
    /// Применение скидки на исследования (для инвестиций).
    /// </summary>
    public void ApplyResearchDiscount(float discount)
    {
        researchDiscount = Mathf.Clamp(discount, 0f, 0.3f);
    }

    private float researchDiscount = 0f;

    public bool IsAdTypeUnlocked(string adType)
    {
        string techName = $"Реклама {adType}";
        return IsTechResearched(techName);
    }

    // ---- Добавление кастомных технологий из ассетов ----
    private void AddAdditionalTechnologies()
    {
        var additionalAssets = CarCompanyManager.Instance.AdditionalTechnologies;
        if (additionalAssets == null || additionalAssets.Length == 0)
            return;

        List<Technology> techList = technologies.ToList();

        foreach (var asset in additionalAssets)
        {
            if (asset == null) continue;
            if (techList.Any(t => t != null && t.techName == asset.techName))
                continue;

            Technology newTech = new Technology();
            newTech.techName = asset.techName;
            newTech.description = asset.description;
            newTech.researchCost = asset.researchCost;
            newTech.isResearched = false;
            newTech.requiredTechNames = asset.requiredTechNames ?? new string[0];
            newTech.priceModifier = asset.priceModifier;
            newTech.demandModifier = asset.demandModifier;
            newTech.unlockCarOnResearch = asset.unlockCarOnResearch;
            newTech.unlockedCar = asset.unlockedCar;
            newTech.availableYear = 2025;
            newTech.availableMonth = 1;
            techList.Add(newTech);
        }

        technologies = techList.ToArray();
    }

    // ---- Доступные машины (пересборка из всех источников) ----
    private void BuildAvailableCars()
    {
        HashSet<CarBlueprint> uniqueCars = new HashSet<CarBlueprint>();

        if (startCars != null)
        {
            foreach (var car in startCars)
                if (car != null) uniqueCars.Add(car);
        }

        if (technologies != null)
        {
            foreach (var tech in technologies)
                if (tech != null && tech.isResearched && tech.unlockedCar != null && tech.unlockCarOnResearch)
                    uniqueCars.Add(tech.unlockedCar);
        }

        if (createdCars != null)
        {
            foreach (var car in createdCars)
                if (car != null) uniqueCars.Add(car);
        }

        availableCars = uniqueCars.ToArray();

        Debug.Log($"=== BuildAvailableCars: всего машин {availableCars.Length} ===");
        foreach (var c in availableCars)
            Debug.Log($" - {c.carName} (level {c.currentLevel})");

        ui.CreateCarCards(availableCars);
    }

    // ---- Исследование (с демо-ограничениями) ----
    public void ResearchTechnology(Technology tech)
    {
        if (tech == null) return;
        if (tech.isResearched)
        {
            ui.ShowNotification($"{tech.techName} уже исследована.");
            return;
        }

        // ---- ДЕМО-ОГРАНИЧЕНИЕ: проверка лимита технологий ----
        #if DEMO_BUILD
        if (DemoManager.Instance != null && !DemoManager.Instance.CanResearchTechnology())
        {
            return; // Демо не позволяет исследовать больше технологий
        }
        #endif

        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string requiredName in tech.requiredTechNames)
            {
                Technology requiredTech = technologies.FirstOrDefault(t => t != null && t.techName == requiredName);
                if (requiredTech == null || !requiredTech.isResearched)
                {
                    ui.ShowNotification($"Для {tech.techName} требуется исследовать '{requiredName}'");
                    return;
                }
            }
        }

        int actualCost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier * (1f - researchDiscount));
        int currentYear = GameTimeManager.Instance.currentYear;
        int currentMonth = GameTimeManager.Instance.currentMonth;
        bool hasPenalty = !tech.IsAvailable(currentYear, currentMonth);
        if (hasPenalty)
        {
            actualCost = Mathf.RoundToInt(actualCost * 2f);
        }
        if (!economy.SpendMoney(actualCost))
        {
            double missing = actualCost - economy.Money;
            ui.ShowNotification($"Не хватает денег для исследования {tech.techName}! Нужно ещё ${missing:F0}");
            return;
        }

        tech.isResearched = true;
        ui.ShowNotification($"Технология '{tech.techName}' исследована!");

        // ---- ДЕМО-ОГРАНИЧЕНИЕ: регистрация исследования ----
        #if DEMO_BUILD
        DemoManager.Instance?.RegisterTechnologyResearched();
        #endif

        string techName = tech.techName;
        for (int i = 0; i < tuningParamNames.Length; i++)
        {
            string param = tuningParamNames[i];
            if (techName.StartsWith(param + "_"))
            {
                int level = int.Parse(techName.Split('_')[1]);
                ApplyTuningToAllCars(param, level);
                break;
            }
        }

        economy.RecalculateModifiers(technologies);

        if (tech.unlockCarOnResearch && tech.unlockedCar != null && !availableCars.Contains(tech.unlockedCar))
        {
            var newList = availableCars.ToList();
            newList.Add(tech.unlockedCar);
            availableCars = newList.ToArray();
            ui.CreateCarCards(availableCars);
            ui.ShowNotification($"Открыта новая машина: {tech.unlockedCar.carName}");
        }

        ui.RefreshTechButtons();
        ui.UpdateMoneyLabels();
        ui.UpdateCarCards();

        if (tech.techName == CarCompanyManager.Instance.BulkProductionTechName)
        {
            CarCompanyManager.Instance.ProductionManager.UpdateButtons();
            CarCompanyManager.Instance.ProductionManager.ClampProductionCount();
            ui.ShowNotification($"Технология '{tech.techName}' изучена! Теперь вы можете производить до 10 машин за раз.");
        }
        if (tech.techName == CarCompanyManager.Instance.CarUpgradeTechName)
        {
            ui.CreateCarCards(availableCars);
            ui.ShowNotification($"Технология '{tech.techName}' изучена! Теперь вы можете улучшать машины.");
        }

        demand.UpdateDemand();

        for (int i = 0; i < tuningParamNames.Length; i++)
        {
            string param = tuningParamNames[i];
            if (techName.StartsWith(param + "_"))
            {
                int level = int.Parse(techName.Split('_')[1]);
                int nextLevel = level + 1;
                Technology nextTech = GetTuningTech(param, nextLevel);
                if (nextTech != null)
                {
                    int newMonth = GameTimeManager.Instance.currentMonth + 1;
                    int newYear = GameTimeManager.Instance.currentYear;
                    if (newMonth > 12) { newMonth = 1; newYear++; }
                    nextTech.availableYear = newYear;
                    nextTech.availableMonth = newMonth;
                }
                break;
            }
        }
    }

    // ---- Тюнинг ----
    public Technology GetTuningTech(string param, int level)
    {
        return technologies.FirstOrDefault(t => t != null && t.techName == $"{param}_{level}");
    }

    public int GetTuningLevel(CarBlueprint car, string param)
    {
        if (car == null) return 0;
        switch (param)
        {
            case "power": return car.tuningPower;
            case "economy": return car.tuningEconomy;
            case "design": return car.tuningDesign;
            case "safety": return car.tuningSafety;
            default: return 0;
        }
    }

    public bool CanUpgradeTuning(CarBlueprint car, string param)
    {
        int current = GetTuningLevel(car, param);
        if (current >= TUNING_MAX_LEVEL) return false;
        int nextLevel = current + 1;
        Technology tech = GetTuningTech(param, nextLevel);
        if (tech == null || tech.isResearched) return false;
        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string req in tech.requiredTechNames)
            {
                Technology reqTech = technologies.FirstOrDefault(t => t != null && t.techName == req);
                if (reqTech == null || !reqTech.isResearched) return false;
            }
        }
        int cost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier);
        int currentYear = GameTimeManager.Instance.currentYear;
        int currentMonth = GameTimeManager.Instance.currentMonth;
        if (!tech.IsAvailable(currentYear, currentMonth))
        {
            cost = Mathf.RoundToInt(cost * 2f);
        }
        return economy.Money >= cost;
    }

    public void UpgradeTuning(CarBlueprint car, string param)
    {
        if (car == null) return;
        int current = GetTuningLevel(car, param);
        if (current >= TUNING_MAX_LEVEL) { ui.ShowNotification("Максимальный уровень!"); return; }
        int nextLevel = current + 1;
        Technology tech = GetTuningTech(param, nextLevel);
        if (tech == null) { ui.ShowNotification("Технология не найдена!"); return; }
        if (tech.isResearched) { ui.ShowNotification("Технология уже изучена!"); return; }
        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string req in tech.requiredTechNames)
            {
                Technology reqTech = technologies.FirstOrDefault(t => t != null && t.techName == req);
                if (reqTech == null || !reqTech.isResearched)
                {
                    ui.ShowNotification($"Требуется исследовать {req}");
                    return;
                }
            }
        }

        int cost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier);
        int currentYear = GameTimeManager.Instance.currentYear;
        int currentMonth = GameTimeManager.Instance.currentMonth;
        if (!tech.IsAvailable(currentYear, currentMonth))
        {
            cost = Mathf.RoundToInt(cost * 2f);
        }
        if (!economy.SpendMoney(cost))
        {
            ui.ShowNotification($"Не хватает денег! Нужно ${cost}");
            return;
        }

        tech.isResearched = true;
        switch (param)
        {
            case "power":
                car.tuningPower = nextLevel;
                car.currentPower = nextLevel;
                break;
            case "economy":
                car.tuningEconomy = nextLevel;
                car.currentEconomy = nextLevel;
                break;
            case "design":
                car.tuningDesign = nextLevel;
                car.currentDesign = nextLevel;
                break;
            case "safety":
                car.tuningSafety = nextLevel;
                car.currentSafety = nextLevel;
                break;
        }
        int paramIndex = Array.IndexOf(tuningParamNames, param);
        ui.ShowNotification($"Улучшен параметр {tuningParamDisplay[paramIndex]} до {nextLevel} для {car.GetDisplayName()}!");
        ui.UpdateCarCards();
        ui.UpdateMoneyLabels();
        ui.RefreshTechButtons();
    }

    // ---- Улучшение машины (создание новой версии) ----
    public void UpgradeCar(CarBlueprint car)
    {
        if (car == null) return;

        if (!IsCarUpgradeUnlocked())
        {
            ui.ShowNotification($"Для улучшения машин изучите технологию '{CarCompanyManager.Instance.CarUpgradeTechName}'");
            return;
        }

        if (car.levels == null || car.levels.Length == 0)
        {
            ui.ShowNotification("Эту машину нельзя улучшить: нет данных об уровнях.");
            return;
        }

        int nextLevel = car.currentLevel + 1;
        if (nextLevel >= car.levels.Length)
        {
            ui.ShowNotification("Машина уже максимально улучшена!");
            return;
        }

        LevelData nextLevelData = car.levels[nextLevel];
        if (nextLevelData == null)
        {
            ui.ShowNotification("Данные для следующего уровня отсутствуют!");
            return;
        }

        // Стоимость улучшения (можно брать из nextLevelData или отдельно)
        int upgradeCost = 100 * (car.currentLevel + 1);
        if (!economy.SpendMoney(upgradeCost))
        {
            ui.ShowNotification($"Не хватает денег! Нужно ещё ${upgradeCost - economy.Money:F0}");
            return;
        }

        // ---- Создаём клон текущей машины ----
        CarBlueprint upgradedCar = car.Clone();
        if (upgradedCar == null)
        {
            ui.ShowNotification("Ошибка клонирования машины!");
            return;
        }

        // ---- Применяем параметры следующего уровня к клону ----
        upgradedCar.currentLevel = nextLevel;

        // Модель
        if (nextLevelData.prefab != null)
            upgradedCar.carPrefab = nextLevelData.prefab;

        // Экономика
        upgradedCar.currentPrice = nextLevelData.levelPrice;
        upgradedCar.productionCost = nextLevelData.productionCost;
        upgradedCar.demandMultiplier = nextLevelData.demandMultiplier;

        // Тюнинг (максимальные значения)
        upgradedCar.tuningPower = nextLevelData.tuningPower;
        upgradedCar.tuningEconomy = nextLevelData.tuningEconomy;
        upgradedCar.tuningDesign = nextLevelData.tuningDesign;
        upgradedCar.tuningSafety = nextLevelData.tuningSafety;

        // Сброс текущих значений тюнинга
        upgradedCar.currentPower = 0;
        upgradedCar.currentEconomy = 0;
        upgradedCar.currentDesign = 0;
        upgradedCar.currentSafety = 0;

        // Рецепт
        if (nextLevelData.recipe != null)
            upgradedCar.ApplyRecipe(nextLevelData.recipe);

        // Цвет и тонировка остаются как у старой машины (уже скопированы через Clone)

        // ---- Добавляем клон в список и обновляем ----
        createdCars.Add(upgradedCar);
        BuildAvailableCars();

        // ---- Обновление UI ----
        ui.ShowNotification($"Новая версия {upgradedCar.GetDisplayName()} создана! Цена: ${upgradedCar.currentPrice}");
        ui.UpdateMoneyLabels();
        ui.UpdateCarCards();
        demand.UpdateDemand();
    }



    // ---- Проверки ----
    public bool IsCarUpgradeUnlocked()
    {
        string upgradeTechName = CarCompanyManager.Instance.CarUpgradeTechName;
        if (string.IsNullOrEmpty(upgradeTechName)) return true;
        if (technologies == null) return false;
        foreach (var tech in technologies)
            if (tech != null && tech.techName == upgradeTechName && tech.isResearched)
                return true;
        return false;
    }

    public bool IsBulkProductionUnlocked()
    {
        string bulkTech = CarCompanyManager.Instance.BulkProductionTechName;
        if (string.IsNullOrEmpty(bulkTech)) return true;
        if (technologies == null) return false;
        foreach (var tech in technologies)
            if (tech != null && tech.techName == bulkTech && tech.isResearched)
                return true;
        return false;
    }

    public Technology GetTechnologyByName(string name)
    {
        return technologies.FirstOrDefault(t => t != null && t.techName == name);
    }

    public void RefreshTechButtons()
    {
        ui.RefreshTechButtons();
    }

    // ---- Сброс ----
    public void ResetTechs()
    {
        if (technologies != null)
            foreach (var tech in technologies) if (tech != null) tech.isResearched = false;
        var allCars = GetAllPossibleCars();
        foreach (var car in allCars)
            if (car != null)
            {
                car.tuningPower = 0;
                car.tuningEconomy = 0;
                car.tuningDesign = 0;
                car.tuningSafety = 0;
                car.currentPower = 0;
                car.currentEconomy = 0;
                car.currentDesign = 0;
                car.currentSafety = 0;
                car.currentPrice = car.basePrice;
                car.currentLevel = 0;
            }
        createdCars.Clear();
        BuildAvailableCars();
        economy.RecalculateModifiers(technologies);
        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
    }

    // ---- Загрузка из сохранения (ИСПРАВЛЕНО) ----
    public void LoadFromSave(SaveData data)
    {
        if (technologies != null)
            foreach (var tech in technologies) if (tech != null) tech.isResearched = false;

        if (data.technologyData != null)
        {
            foreach (var savedTech in data.technologyData)
            {
                Technology tech = technologies.FirstOrDefault(t => t.techName == savedTech.techName);
                if (tech != null)
                {
                    tech.isResearched = savedTech.isResearched;
                    tech.availableYear = savedTech.availableYear;
                    tech.availableMonth = savedTech.availableMonth;
                }
            }
        }
        else if (data.researchedTechNames != null)
        {
            foreach (string techName in data.researchedTechNames)
            {
                Technology t = technologies.FirstOrDefault(tech => tech.techName == techName);
                if (t != null) t.isResearched = true;
            }
        }

        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (var car in allCars)
        {
            if (car == null) continue;
            car.tuningPower = 0;
            car.tuningEconomy = 0;
            car.tuningDesign = 0;
            car.tuningSafety = 0;
            car.currentPower = 0;
            car.currentEconomy = 0;
            car.currentDesign = 0;
            car.currentSafety = 0;
        }

        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech == null || !tech.isResearched) continue;
                string name = tech.techName;
                if (name.StartsWith("power_")) { int lvl = int.Parse(name.Split('_')[1]); foreach (var car in allCars) if (car != null && lvl > car.tuningPower) car.tuningPower = lvl; }
                else if (name.StartsWith("economy_")) { int lvl = int.Parse(name.Split('_')[1]); foreach (var car in allCars) if (car != null && lvl > car.tuningEconomy) car.tuningEconomy = lvl; }
                else if (name.StartsWith("design_")) { int lvl = int.Parse(name.Split('_')[1]); foreach (var car in allCars) if (car != null && lvl > car.tuningDesign) car.tuningDesign = lvl; }
                else if (name.StartsWith("safety_")) { int lvl = int.Parse(name.Split('_')[1]); foreach (var car in allCars) if (car != null && lvl > car.tuningSafety) car.tuningSafety = lvl; }
            }
        }

        if (data.createdCarsData != null)
        {
            LoadCreatedCars(data.createdCarsData);
        }
        else
        {
            BuildAvailableCars();
        }

        if (data.carLevels != null && startCars != null)
        {
            foreach (CarLevelData lvl in data.carLevels)
            {
                CarBlueprint car = startCars.FirstOrDefault(c => c != null && c.carName == lvl.carName);
                if (car != null)
                {
                    car.currentLevel = lvl.currentLevel;
                    car.currentPower = lvl.currentPower;
                    car.currentEconomy = lvl.currentEconomy;
                    car.currentDesign = lvl.currentDesign;
                    car.currentSafety = lvl.currentSafety;
                    car.bodyColor = new Color(lvl.bodyColorR, lvl.bodyColorG, lvl.bodyColorB);
                    car.hasTint = lvl.hasTint;
                }
            }
            BuildAvailableCars();
        }

        economy.RecalculateModifiers(technologies);
        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
        ui.RefreshTechButtons();
    }

    public void FillSaveData(SaveData data)
    {
        data.technologyData = new List<TechnologySaveData>();
        List<string> researched = new List<string>();
        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech != null)
                {
                    if (tech.isResearched) researched.Add(tech.techName);
                    data.technologyData.Add(new TechnologySaveData
                    {
                        techName = tech.techName,
                        isResearched = tech.isResearched,
                        availableYear = tech.availableYear,
                        availableMonth = tech.availableMonth
                    });
                }
            }
        }
        data.researchedTechNames = researched.ToArray();

        data.carLevels = new List<CarLevelData>();
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (CarBlueprint car in allCars)
            if (car != null)
                data.carLevels.Add(new CarLevelData
                {
                    carName = car.carName,
                    currentLevel = car.currentLevel,
                    currentPower = car.currentPower,
                    currentEconomy = car.currentEconomy,
                    currentDesign = car.currentDesign,
                    currentSafety = car.currentSafety,
                    bodyColorR = car.bodyColor.r,
                    bodyColorG = car.bodyColor.g,
                    bodyColorB = car.bodyColor.b,
                    hasTint = car.hasTint
                });

        data.createdCarsData = GetCreatedCarsData();
    }

    // ---- Сохранение/загрузка созданных машин (с ценой) ----
    public List<CarBlueprintSaveData> GetCreatedCarsData()
    {
        List<CarBlueprintSaveData> list = new List<CarBlueprintSaveData>();
        foreach (var car in createdCars)
        {
            list.Add(new CarBlueprintSaveData
            {
                carName = car.carName,
                currentLevel = car.currentLevel,
                currentPrice = car.currentPrice,
                tuningPower = car.tuningPower,
                tuningEconomy = car.tuningEconomy,
                tuningDesign = car.tuningDesign,
                tuningSafety = car.tuningSafety,
                currentPower = car.currentPower,
                currentEconomy = car.currentEconomy,
                currentDesign = car.currentDesign,
                currentSafety = car.currentSafety,
                demandMultiplier = car.demandMultiplier,
                bodyColorR = car.bodyColor.r,
                bodyColorG = car.bodyColor.g,
                bodyColorB = car.bodyColor.b,
                hasTint = car.hasTint
            });
        }
        return list;
    }

    // ---- Загрузка созданных машин из сохранения ----
    public void LoadCreatedCars(List<CarBlueprintSaveData> dataList)
    {
        createdCars.Clear();

        foreach (var data in dataList)
        {
            CarBlueprint baseCar = FindBaseCar(data.carName);
            if (baseCar == null) continue;
            CarBlueprint newCar = baseCar.Clone();
            newCar.currentLevel = data.currentLevel;
            newCar.currentPrice = data.currentPrice;
            newCar.tuningPower = data.tuningPower;
            newCar.tuningEconomy = data.tuningEconomy;
            newCar.tuningDesign = data.tuningDesign;
            newCar.tuningSafety = data.tuningSafety;
            newCar.currentPower = data.currentPower;
            newCar.currentEconomy = data.currentEconomy;
            newCar.currentDesign = data.currentDesign;
            newCar.currentSafety = data.currentSafety;
            newCar.demandMultiplier = data.demandMultiplier;
            newCar.bodyColor = new Color(data.bodyColorR, data.bodyColorG, data.bodyColorB);
            newCar.hasTint = data.hasTint;

            createdCars.Add(newCar);
        }

        BuildAvailableCars();
    }

    private CarBlueprint FindBaseCar(string name)
    {
        if (startCars != null)
            foreach (var car in startCars)
                if (car != null && car.carName == name)
                    return car;
        if (technologies != null)
            foreach (var tech in technologies)
                if (tech != null && tech.unlockedCar != null && tech.unlockedCar.carName == name)
                    return tech.unlockedCar;
        return null;
    }

    private List<CarBlueprint> GetAllPossibleCars()
    {
        List<CarBlueprint> all = new List<CarBlueprint>();
        if (startCars != null)
        {
            foreach (var car in startCars)
                if (car != null) all.Add(car);
        }
        if (technologies != null)
        {
            foreach (var tech in technologies)
                if (tech != null && tech.unlockedCar != null && !all.Contains(tech.unlockedCar))
                    all.Add(tech.unlockedCar);
        }
        foreach (var car in createdCars)
            if (!all.Contains(car))
                all.Add(car);
        return all;
    }

    private void ApplyTuningToAllCars(string param, int level)
    {
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (var car in allCars)
        {
            if (car == null) continue;
            switch (param)
            {
                case "power":
                    if (level > car.tuningPower) car.tuningPower = level;
                    if (level > car.currentPower) car.currentPower = level;
                    break;
                case "economy":
                    if (level > car.tuningEconomy) car.tuningEconomy = level;
                    if (level > car.currentEconomy) car.currentEconomy = level;
                    break;
                case "design":
                    if (level > car.tuningDesign) car.tuningDesign = level;
                    if (level > car.currentDesign) car.currentDesign = level;
                    break;
                case "safety":
                    if (level > car.tuningSafety) car.tuningSafety = level;
                    if (level > car.currentSafety) car.currentSafety = level;
                    break;
            }
        }
    }

    public bool IsTechResearched(string techName)
    {
        if (technologies == null) return false;
        foreach (var tech in technologies)
            if (tech != null && tech.techName == techName && tech.isResearched)
                return true;
        return false;
    }
}