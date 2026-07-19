using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    private string savePath => Application.persistentDataPath + "/save.json";

    public void Initialize() { }

    public bool HasSaveFile() => File.Exists(savePath);

    public void SaveGame()
    {
        if (CarCompanyManager.Instance == null) return;

        SaveData data = new SaveData();

        CarCompanyManager.Instance.EconomyManager.FillSaveData(data);
        CarCompanyManager.Instance.TechManager.FillSaveData(data);

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.FillSaveData(data);

        CarCompanyManager.Instance.ActionLogManager.FillSaveData(data);
        CarCompanyManager.Instance.AchievementManager.FillSaveData(data);

        // ---- НОВЫЕ МЕНЕДЖЕРЫ ----
        CarCompanyManager.Instance.WarehouseManager.FillSaveData(data);
        CarCompanyManager.Instance.PartsMarketManager.FillSaveData(data);

        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null)
            data.difficulty = (int)ui.GetCurrentDifficulty();

        data.tutorialProgress = TutorialManager.Instance?.CurrentStep ?? -1;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"Игра сохранена в {savePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("Файл сохранения не найден");
            return;
        }

        string json = File.ReadAllText(savePath);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            Debug.LogError("Ошибка десериализации сохранения");
            return;
        }

        CarCompanyManager.Instance.CompetitorManager.Initialize();
        CarCompanyManager.Instance.EconomyManager.LoadFromSave(data);
        CarCompanyManager.Instance.TechManager.LoadFromSave(data);

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.LoadFromSave(data.currentMonth, data.currentYear);

        CarCompanyManager.Instance.ActionLogManager.LoadFromSave(data);
        CarCompanyManager.Instance.AchievementManager.LoadFromSave(data);

        // ---- НОВЫЕ МЕНЕДЖЕРЫ ----
        CarCompanyManager.Instance.WarehouseManager.LoadFromSave(data);
        CarCompanyManager.Instance.PartsMarketManager.LoadFromSave(data);

        TutorialManager.Instance?.LoadProgress(data.tutorialProgress);

        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null && data.difficulty >= 0 && data.difficulty <= 2)
            ui.SetDifficulty((UIManager.Difficulty)data.difficulty);

        ui.UpdateMoneyLabels();
        ui.UpdateReputationLabel();
        ui.UpdateDateTimeDisplay();
        ui.UpdateSavedDifficultyLabel();
        ui.UpdateUpgradeUI();

        Debug.Log($"Игра загружена из {savePath}");
    }

    public void NewGame()
    {
        if (File.Exists(savePath))
            File.Delete(savePath);

        CarCompanyManager.Instance.EconomyManager.ResetState();
        CarCompanyManager.Instance.TechManager.ResetTechs();

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.LoadFromSave(1, 2025);

        CarCompanyManager.Instance.ActionLogManager.ClearLogs();
        CarCompanyManager.Instance.AchievementManager.ResetProgress();

        TutorialManager.Instance?.ResetProgress();

        var ui = CarCompanyManager.Instance.UIManager;
        ui.ShowWelcomeScreen();
        ui.UpdateMoneyLabels();
        ui.UpdateDateTimeDisplay();

        Debug.Log("Новая игра начата");
    }
}