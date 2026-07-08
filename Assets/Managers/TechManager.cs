using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class TechManager : MonoBehaviour
{
    private Technology[] technologies;
    private CarBlueprint[] availableCars;
    private CarBlueprint[] startCars;

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
        GenerateTuningTechnologies();
        GenerateCarUnlockTechnologies();
        AddAdditionalTechnologies(); // <-- НОВЫЙ ВЫЗОВ
        BuildAvailableCars();

        economy.RecalculateModifiers(technologies);
        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
    }

    // ---- Генерация технологий ----
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
                tech.researchCost = 50 + level * 30;
                tech.isResearched = false;
                tech.requiredTechNames = (level > 1) ? new string[] { $"{param}_{level - 1}" } : new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                tech.unlockCarOnResearch = false;
                tech.unlockedCar = null;
                newTechs.Add(tech);
            }
        }
        technologies = newTechs.ToArray();
    }

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
            newTechs.Add(tech);
        }
        technologies = newTechs.ToArray();
    }

    // ---- НОВЫЙ МЕТОД: добавление кастомных технологий из ассетов ----
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

            techList.Add(newTech);
        }

        technologies = techList.ToArray();
    }

    // ---- Доступные машины ----
    private void BuildAvailableCars()
    {
        List<CarBlueprint> cars = new List<CarBlueprint>();
        if (startCars != null)
            foreach (var car in startCars) if (car != null) cars.Add(car);
        if (technologies != null)
            foreach (var tech in technologies)
                if (tech != null && tech.isResearched && tech.unlockedCar != null && tech.unlockCarOnResearch && !cars.Contains(tech.unlockedCar))
                    cars.Add(tech.unlockedCar);
        availableCars = cars.ToArray();
        ui.CreateCarCards(availableCars);
    }

    // ---- Исследование ----
    public void ResearchTechnology(Technology tech)
    {
        if (tech == null) return;
        if (tech.isResearched)
        {
            ui.ShowNotification($"{tech.techName} уже исследована.");
            return;
        }

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

        int actualCost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier);
        if (!economy.SpendMoney(actualCost))
        {
            double missing = actualCost - economy.Money;
            ui.ShowNotification($"Не хватает денег для исследования {tech.techName}! Нужно ещё ${missing:F0}");
            return;
        }

        tech.isResearched = true;
        ui.ShowNotification($"Технология '{tech.techName}' исследована!");

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
        if (!economy.SpendMoney(cost))
        {
            ui.ShowNotification($"Не хватает денег! Нужно ${cost}");
            return;
        }

        tech.isResearched = true;
        switch (param)
        {
            case "power": car.tuningPower = nextLevel; break;
            case "economy": car.tuningEconomy = nextLevel; break;
            case "design": car.tuningDesign = nextLevel; break;
            case "safety": car.tuningSafety = nextLevel; break;
        }
        int paramIndex = Array.IndexOf(tuningParamNames, param);
        ui.ShowNotification($"Улучшен параметр {tuningParamDisplay[paramIndex]} до {nextLevel} для {car.carName}!");
        ui.UpdateCarCards();
        ui.UpdateMoneyLabels();
        ui.RefreshTechButtons();
    }

    // ---- Улучшение машины ----
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

    public void UpgradeCar(CarBlueprint car)
    {
        if (car == null) return;
        if (!IsCarUpgradeUnlocked())
        {
            ui.ShowNotification($"Для улучшения машин изучите технологию '{CarCompanyManager.Instance.CarUpgradeTechName}'");
            return;
        }
        if (car.levelPrefabs == null || car.levelPrefabs.Length == 0)
        {
            ui.ShowNotification("Эту машину нельзя улучшить!");
            return;
        }
        int maxLevel = car.levelPrefabs.Length - 1;
        if (car.currentLevel >= maxLevel)
        {
            ui.ShowNotification("Машина уже максимально улучшена!");
            return;
        }
        int cost = 100 * (car.currentLevel + 1);
        if (!economy.SpendMoney(cost))
        {
            ui.ShowNotification($"Не хватает денег! Нужно ещё ${cost - economy.Money:F0}");
            return;
        }
        car.currentLevel++;
        ui.ShowNotification($"Машина {car.carName} улучшена до уровня {car.currentLevel + 1}!");
        ui.UpdateCarCards();
        ui.UpdateMoneyLabels();
    }

    // ---- Массовое производство ----
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

    // ---- Получение технологии по имени ----
    public Technology GetTechnologyByName(string name)
    {
        return technologies.FirstOrDefault(t => t != null && t.techName == name);
    }

    // ---- Обновление UI технологий ----
    public void RefreshTechButtons()
    {
        ui.RefreshTechButtons();
    }

    // ---- Сброс и загрузка ----
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
            }
        BuildAvailableCars();
        economy.RecalculateModifiers(technologies);
        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
    }

    public void LoadFromSave(SaveData data)
    {
        if (technologies != null)
            foreach (var tech in technologies) if (tech != null) tech.isResearched = false;

        foreach (string techName in data.researchedTechNames)
        {
            Technology t = technologies.FirstOrDefault(tech => tech != null && tech.techName == techName);
            if (t != null) t.isResearched = true;
        }

        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (var car in allCars)
        {
            if (car == null) continue;
            car.tuningPower = 0;
            car.tuningEconomy = 0;
            car.tuningDesign = 0;
            car.tuningSafety = 0;
            foreach (var tech in technologies)
            {
                if (tech == null || !tech.isResearched) continue;
                string name = tech.techName;
                if (name.StartsWith("power_")) { int lvl = int.Parse(name.Split('_')[1]); if (lvl > car.tuningPower) car.tuningPower = lvl; }
                else if (name.StartsWith("economy_")) { int lvl = int.Parse(name.Split('_')[1]); if (lvl > car.tuningEconomy) car.tuningEconomy = lvl; }
                else if (name.StartsWith("design_")) { int lvl = int.Parse(name.Split('_')[1]); if (lvl > car.tuningDesign) car.tuningDesign = lvl; }
                else if (name.StartsWith("safety_")) { int lvl = int.Parse(name.Split('_')[1]); if (lvl > car.tuningSafety) car.tuningSafety = lvl; }
            }
        }

        economy.RecalculateModifiers(technologies);
        BuildAvailableCars();

        foreach (CarLevelData lvl in data.carLevels)
        {
            CarBlueprint car = allCars.FirstOrDefault(c => c != null && c.carName == lvl.carName);
            if (car != null) car.currentLevel = lvl.currentLevel;
        }

        ui.CreateTechTree(technologies.ToList(), economy.TechCostMultiplier);
        ui.CreateCarCards(availableCars);
        ui.RefreshTechButtons();
    }

    public void FillSaveData(SaveData data)
    {
        List<string> researched = new List<string>();
        if (technologies != null)
            foreach (var tech in technologies)
                if (tech != null && tech.isResearched) researched.Add(tech.techName);
        data.researchedTechNames = researched.ToArray();

        data.carLevels = new List<CarLevelData>();
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (CarBlueprint car in allCars)
            if (car != null)
                data.carLevels.Add(new CarLevelData { carName = car.carName, currentLevel = car.currentLevel });
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
        return all;
    }
}