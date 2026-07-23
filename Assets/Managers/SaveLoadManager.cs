using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SaveLoadManager : MonoBehaviour
{
    private const int MAX_SLOTS = 3;
    private int currentSlot = 0;
    private SaveData currentSaveData;

    // Автосохранение
    private Coroutine autoSaveCoroutine;
    [SerializeField] private float autoSaveInterval = 30f; // секунд

    private string GetSavePath(int slot) => Application.persistentDataPath + $"/save_{slot}.json";

    public SaveData GetCurrentSaveData() => currentSaveData;

    public bool HasSaveFile(int slot = -1)
    {
        if (slot < 0) slot = currentSlot;
        return File.Exists(GetSavePath(slot));
    }

    // ---- Сохранение ----
    public void SaveGame(int slot = -1)
    {
        if (slot < 0) slot = currentSlot;
        currentSlot = slot;

        if (CarCompanyManager.Instance == null)
        {
            Debug.LogWarning("CarCompanyManager ещё не инициализирован, сохранение отложено");
            return;
        }

        SaveData data = new SaveData();

        CarCompanyManager.Instance.EconomyManager.FillSaveData(data);
        CarCompanyManager.Instance.TechManager.FillSaveData(data);
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.FillSaveData(data);
        CarCompanyManager.Instance.ActionLogManager.FillSaveData(data);
        CarCompanyManager.Instance.AchievementManager.FillSaveData(data);
        CarCompanyManager.Instance.WarehouseManager.FillSaveData(data);
        CarCompanyManager.Instance.PartsMarketManager.FillSaveData(data);

        // Маркетинг
        MarketingManager.Instance?.FillSaveData(data);

        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null)
            data.difficulty = (int)ui.GetCurrentDifficulty();

        // Туториалы – флаги уже сохраняются через SaveData, но добавим прогресс
        data.tutorialProgress = TutorialManager.Instance?.CurrentStep ?? -1;

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(slot), json);
        currentSaveData = data;
        Debug.Log($"Игра сохранена в слот {slot} ({GetSavePath(slot)})");
    }

    // ---- Загрузка ----
    public void LoadGame(int slot = -1)
    {
        if (slot < 0) slot = currentSlot;
        currentSlot = slot;

        string path = GetSavePath(slot);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"Файл сохранения слота {slot} не найден");
            return;
        }

        string json = File.ReadAllText(path);
        SaveData data = JsonUtility.FromJson<SaveData>(json);
        if (data == null)
        {
            Debug.LogError("Ошибка десериализации сохранения");
            return;
        }

        currentSaveData = data;

        // Загружаем данные во все менеджеры
        CarCompanyManager.Instance.CompetitorManager.Initialize();
        CarCompanyManager.Instance.EconomyManager.LoadFromSave(data);
        CarCompanyManager.Instance.TechManager.LoadFromSave(data);
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.LoadFromSave(data.currentMonth, data.currentYear);
        CarCompanyManager.Instance.ActionLogManager.LoadFromSave(data);
        CarCompanyManager.Instance.AchievementManager.LoadFromSave(data);
        CarCompanyManager.Instance.WarehouseManager.LoadFromSave(data);
        CarCompanyManager.Instance.PartsMarketManager.LoadFromSave(data);
        MarketingManager.Instance?.LoadFromSave(data);

        var ui = CarCompanyManager.Instance.UIManager;
        if (ui != null && data.difficulty >= 0 && data.difficulty <= 2)
            ui.SetDifficulty((UIManager.Difficulty)data.difficulty);

        ui.UpdateMoneyLabels();
        ui.UpdateReputationLabel();
        ui.UpdateDateTimeDisplay();
        ui.UpdateSavedDifficultyLabel();
        ui.UpdateUpgradeUI();
        ui.UpdateWarehouseLabels();

        Debug.Log($"Игра загружена из слота {slot}");
    }

    // ---- Новая игра ----
    public void NewGame()
    {
        int slot = currentSlot;
        if (File.Exists(GetSavePath(slot)))
            File.Delete(GetSavePath(slot));

        currentSaveData = new SaveData();

        CarCompanyManager.Instance.EconomyManager.ResetState();
        CarCompanyManager.Instance.TechManager.ResetTechs();
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.LoadFromSave(1, 2025);
        CarCompanyManager.Instance.ActionLogManager.ClearLogs();
        CarCompanyManager.Instance.AchievementManager.ResetProgress();
        CarCompanyManager.Instance.WarehouseManager.LoadFromSave(new SaveData()); // сброс
        MarketingManager.Instance?.LoadFromSave(new SaveData());

        TutorialManager.Instance?.ResetProgress();

        var ui = CarCompanyManager.Instance.UIManager;
        ui.ShowWelcomeScreen();
        ui.UpdateMoneyLabels();
        ui.UpdateDateTimeDisplay();

        Debug.Log("Новая игра начата в слоте " + slot);
    }

    // ---- Автосохранение ----
    public void StartAutoSave()
    {
        StopAutoSave();
        if (autoSaveCoroutine == null)
            autoSaveCoroutine = StartCoroutine(AutoSaveLoop());
    }

    public void StopAutoSave()
    {
        if (autoSaveCoroutine != null)
        {
            StopCoroutine(autoSaveCoroutine);
            autoSaveCoroutine = null;
        }
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            SaveGame();
            Debug.Log("Автосохранение выполнено");
        }
    }

    private void OnDestroy()
    {
        StopAutoSave();
    }

    // ---- Утилиты для UI ----
    public void SaveSlot1() => SaveGame(0);
    public void SaveSlot2() => SaveGame(1);
    public void SaveSlot3() => SaveGame(2);
    public void LoadSlot1() => LoadGame(0);
    public void LoadSlot2() => LoadGame(1);
    public void LoadSlot3() => LoadGame(2);

    // Для совместимости со старыми вызовами (без параметра)
    public void SaveGame() => SaveGame(currentSlot);
    public void LoadGame() => LoadGame(currentSlot);

    // ---- Инициализация (если нужна) ----
    public void Initialize()
    {
        // Можно добавить логику инициализации, если потребуется
        Debug.Log("SaveLoadManager инициализирован");
    }
}