using UnityEngine;
using System.IO;

public class SaveLoadManager : MonoBehaviour
{
    // ---- ПУТЬ К ФАЙЛУ СОХРАНЕНИЯ ----
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

        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null)
            data.difficulty = (int)ui.GetCurrentDifficulty();

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

        // Загрузка экономики
        CarCompanyManager.Instance.EconomyManager.LoadFromSave(data);

        // Загрузка технологий и машин
        CarCompanyManager.Instance.TechManager.LoadFromSave(data);

        // Загрузка даты (только месяц и год)
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.LoadFromSave(data.currentMonth, data.currentYear);

        // Загрузка сложности
        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null && data.difficulty >= 0 && data.difficulty <= 2)
            ui.SetDifficulty((UIManager.Difficulty)data.difficulty);

        // Обновление UI
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

        var ui = CarCompanyManager.Instance.UIManager;
        ui.ShowWelcomeScreen();
        ui.UpdateMoneyLabels();
        ui.UpdateDateTimeDisplay();

        Debug.Log("Новая игра начата");
    }
}