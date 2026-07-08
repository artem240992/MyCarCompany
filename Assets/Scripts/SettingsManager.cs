using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class SettingsManager : MonoBehaviour
{
    private UIDocument uiDoc;

    private Slider audioSlider;
    private DropdownField qualityDropdown;
    private DropdownField resolutionDropdown;
    private Toggle fullscreenToggle;
    private Button backButton;
    private Button saveButton;
    private DropdownField difficultyDropdown;
    private Label currentDifficultyLabel;

    private const string AUDIO_KEY = "Settings_Audio";
    private const string QUALITY_KEY = "Settings_Quality";
    private const string RESOLUTION_KEY = "Settings_Resolution";
    private const string FULLSCREEN_KEY = "Settings_Fullscreen";

    // ---- ИЗМЕНИТЕ НА ИМЯ ВАШЕЙ СЦЕНЫ ГЛАВНОГО МЕНЮ ----
    private const string MAIN_MENU_SCENE = "SampleScene"; // или "MainMenu"

    private void Start()
    {
        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("UIDocument not found on SettingsManager GameObject.");
            return;
        }

        var root = uiDoc.rootVisualElement;

        audioSlider = root.Q<Slider>("AudioSlider");
        qualityDropdown = root.Q<DropdownField>("QualityDropdown");
        resolutionDropdown = root.Q<DropdownField>("ResolutionDropdown");
        fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
        backButton = root.Q<Button>("BackButton");
        saveButton = root.Q<Button>("SaveButton");
        difficultyDropdown = root.Q<DropdownField>("DifficultyDropdown");
        currentDifficultyLabel = root.Q<Label>("CurrentDifficultyLabel");

        // ---- ОТЛАДКА ----
        Debug.Log($"SettingsManager: SaveButton found = {saveButton != null}");
        Debug.Log($"SettingsManager: BackButton found = {backButton != null}");

        if (audioSlider == null || qualityDropdown == null || resolutionDropdown == null ||
            fullscreenToggle == null || backButton == null || saveButton == null || difficultyDropdown == null)
        {
            Debug.LogError("One or more UI elements not found. Check UXML names.");
            return;
        }

        difficultyDropdown.choices = new List<string> { "Easy", "Normal", "Hard" };

        LoadSettings();

        // ---- ПОДПИСКИ ----
        audioSlider.RegisterValueChangedCallback(evt => OnAudioChanged(evt.newValue));
        qualityDropdown.RegisterValueChangedCallback(evt => OnQualityChanged(evt.newValue));
        resolutionDropdown.RegisterValueChangedCallback(evt => OnResolutionChanged(evt.newValue));
        fullscreenToggle.RegisterValueChangedCallback(evt => OnFullscreenChanged(evt.newValue));
        difficultyDropdown.RegisterValueChangedCallback(evt => OnDifficultyChanged(evt.newValue));

        backButton.clicked += OnBackButtonClicked;
        saveButton.clicked += OnSaveButtonClicked;
    }

    private void LoadSettings()
    {
        // Аудио
        float audio = PlayerPrefs.GetFloat(AUDIO_KEY, 1f);
        audioSlider.value = audio;
        ApplyAudio(audio);

        // Качество
        int quality = PlayerPrefs.GetInt(QUALITY_KEY, QualitySettings.GetQualityLevel());
        qualityDropdown.index = Mathf.Clamp(quality, 0, qualityDropdown.choices.Count - 1);
        ApplyQuality(quality);

        // Разрешение
        string resolution = PlayerPrefs.GetString(RESOLUTION_KEY, "1920x1080");
        int resIndex = resolutionDropdown.choices.IndexOf(resolution);
        if (resIndex == -1) resIndex = 2;
        resolutionDropdown.index = resIndex;
        ApplyResolution(resolution);

        // Полноэкранный
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        fullscreenToggle.value = fullscreen;
        ApplyFullscreen(fullscreen);

        // Сложность
        if (CarCompanyManager.Instance != null && CarCompanyManager.Instance.DifficultyManager != null)
        {
            DifficultyLevel current = CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
            difficultyDropdown.index = (int)current;
            UpdateDifficultyLabel(current);
        }
        else
        {
            difficultyDropdown.index = 1;
            UpdateDifficultyLabel(DifficultyLevel.Normal);
        }
    }

    private void UpdateDifficultyLabel(DifficultyLevel level)
    {
        if (currentDifficultyLabel != null)
            currentDifficultyLabel.text = $"Текущая сложность: {level}";
    }

    private void OnAudioChanged(float value)
    {
        ApplyAudio(value);
        PlayerPrefs.SetFloat(AUDIO_KEY, value);
        PlayerPrefs.Save();
    }

    private void OnQualityChanged(string value)
    {
        int idx = qualityDropdown.choices.IndexOf(value);
        if (idx >= 0)
        {
            ApplyQuality(idx);
            PlayerPrefs.SetInt(QUALITY_KEY, idx);
            PlayerPrefs.Save();
        }
    }

    private void OnResolutionChanged(string value)
    {
        ApplyResolution(value);
        PlayerPrefs.SetString(RESOLUTION_KEY, value);
        PlayerPrefs.Save();
    }

    private void OnFullscreenChanged(bool value)
    {
        ApplyFullscreen(value);
        PlayerPrefs.SetInt(FULLSCREEN_KEY, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void OnDifficultyChanged(string value)
    {
        int index = difficultyDropdown.choices.IndexOf(value);
        if (index < 0) return;

        DifficultyLevel newLevel = (DifficultyLevel)index;

        if (CarCompanyManager.Instance != null && CarCompanyManager.Instance.DifficultyManager != null)
        {
            CarCompanyManager.Instance.DifficultyManager.SetDifficulty(newLevel, true);
            if (CarCompanyManager.Instance.SaveLoadManager != null)
                CarCompanyManager.Instance.SaveLoadManager.SaveGame();
            UpdateDifficultyLabel(newLevel);
            ShowNotification($"Сложность изменена на {value}");
        }
        else
        {
            Debug.LogWarning("CarCompanyManager отсутствует, сложность не применена.");
        }
    }

    private void OnSaveButtonClicked()
    {
        Debug.Log("Save button clicked!");

        if (CarCompanyManager.Instance != null && CarCompanyManager.Instance.SaveLoadManager != null)
            CarCompanyManager.Instance.SaveLoadManager.SaveGame();

        ShowNotification("Настройки сохранены!");
        GoToMainMenu();
    }

    private void OnBackButtonClicked()
    {
        Debug.Log("Back button clicked!");
        GoToMainMenu();
    }

    private void GoToMainMenu()
    {
        Debug.Log($"Пытаемся загрузить сцену: {MAIN_MENU_SCENE}");

        if (Application.CanStreamedLevelBeLoaded(MAIN_MENU_SCENE))
        {
            SceneManager.LoadScene(MAIN_MENU_SCENE);
        }
        else
        {
            Debug.LogWarning($"Сцена '{MAIN_MENU_SCENE}' не найдена. Загружаем сцену с индексом 0.");
            SceneManager.LoadScene(0);
        }
    }

    // ---- ПРИМЕНЕНИЕ НАСТРОЕК ----
    private void ApplyAudio(float volume) => AudioListener.volume = volume;
    private void ApplyQuality(int level) => QualitySettings.SetQualityLevel(level);
    private void ApplyResolution(string resolution)
    {
        string[] parts = resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
            Screen.SetResolution(w, h, Screen.fullScreen);
    }
    private void ApplyFullscreen(bool fullscreen) => Screen.fullScreen = fullscreen;

    private void ShowNotification(string message)
    {
        Debug.Log(message);
        // Здесь можно добавить визуальное уведомление.
    }
}