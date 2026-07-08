using UnityEngine;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SaveLoadManager : MonoBehaviour
{
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "autofactory_save.json");

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private ProductionManager production => CarCompanyManager.Instance.ProductionManager;
    private CompetitorManager competitor => CarCompanyManager.Instance.CompetitorManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;
    private DifficultyManager difficulty => CarCompanyManager.Instance.DifficultyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;

    public void Initialize() { }

    public bool HasSaveFile() => File.Exists(SaveFilePath);

    public void SaveGame()
    {
        try
        {
            SaveData data = new SaveData();
            economy.FillSaveData(data);
            tech.FillSaveData(data);
            data.productionCount = production.ProductionCount;
            data.currentDifficulty = (int)difficulty.CurrentDifficulty;

            data.carDemands = new List<CarDemandData>();
            List<CarBlueprint> allCars = GetAllPossibleCars();
            foreach (CarBlueprint car in allCars)
                if (car != null)
                    data.carDemands.Add(new CarDemandData { carName = car.carName, demandMultiplier = car.demandMultiplier });

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            ui.ShowNotification("Игра сохранена!");
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка сохранения: " + e.Message);
            ui.ShowNotification("Ошибка сохранения!");
        }
    }

    public void LoadGame()
    {
        if (!HasSaveFile())
        {
            ui.ShowNotification("Нет сохранений!");
            return;
        }
        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null) { ui.ShowNotification("Ошибка загрузки: файл повреждён!"); return; }

            economy.LoadFromSave(data);
            tech.LoadFromSave(data);
            production.SetProductionCount(data.productionCount);
            difficulty.SetDifficulty((DifficultyLevel)data.currentDifficulty, false);

            List<CarBlueprint> allCars = GetAllPossibleCars();
            foreach (CarDemandData d in data.carDemands)
            {
                CarBlueprint car = allCars.FirstOrDefault(c => c != null && c.carName == d.carName);
                if (car != null) car.demandMultiplier = d.demandMultiplier;
            }

            ui.UpdateSavedDifficultyLabel();
            ui.UpdateUpgradeUI();
            ui.UpdateMoneyLabels();
            ui.CreateCarCards(tech.AvailableCars);
            ui.CreateTechTree(tech.Technologies.ToList(), economy.TechCostMultiplier);
            ui.RefreshTechButtons();
            ui.CloseAllWindows();
            ui.ShowNotification("Игра загружена!");
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка загрузки: " + e.Message);
            ui.ShowNotification("Ошибка загрузки!");
        }
    }

    // ---- ИСПРАВЛЕННЫЙ МЕТОД NEWGAME (без перезагрузки сцены) ----
    public void NewGame()
    {
        // Удаляем файл сохранения
        if (File.Exists(SaveFilePath)) File.Delete(SaveFilePath);

        // Сбрасываем состояние всех менеджеров
        economy.ResetState();
        tech.ResetTechs();
        production.SetProductionCount(1);
        competitor.ResetCompetitors();
        difficulty.SetDifficulty(DifficultyLevel.Normal, false); // сбрасываем на Normal без повторного сброса

        // Обновляем UI
        ui.UpdateMoneyLabels();
        ui.UpdateUpgradeUI();
        ui.UpdateSavedDifficultyLabel();
        ui.CreateCarCards(tech.AvailableCars);
        ui.CreateTechTree(tech.Technologies.ToList(), economy.TechCostMultiplier);
        ui.RefreshTechButtons();
        ui.CloseAllWindows();
        
        // Показываем окно приветствия с выбором сложности
        ui.ShowWelcomeScreen();
        
        ui.ShowNotification("Новая игра начата!");
    }

    private List<CarBlueprint> GetAllPossibleCars()
    {
        var all = new List<CarBlueprint>();
        all.AddRange(tech.AvailableCars);
        foreach (var t in tech.Technologies)
            if (t != null && t.unlockedCar != null && !all.Contains(t.unlockedCar))
                all.Add(t.unlockedCar);
        return all;
    }
}