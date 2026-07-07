using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private UIDocument uiDoc;
    private Slider audioSlider;
    private DropdownField qualityDropdown;
    private DropdownField resolutionDropdown;
    private Toggle fullscreenToggle;
    private Button backButton;

    private const string AUDIO_KEY = "Settings_Audio";
    private const string QUALITY_KEY = "Settings_Quality";
    private const string RESOLUTION_KEY = "Settings_Resolution";
    private const string FULLSCREEN_KEY = "Settings_Fullscreen";

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

        if (audioSlider == null || qualityDropdown == null || resolutionDropdown == null || fullscreenToggle == null || backButton == null)
        {
            Debug.LogError("One or more UI elements not found in SettingsScreen. Check UXML names.");
            return;
        }

        // Загрузка сохранённых настроек
        LoadSettings();

        // Подписка на события
        audioSlider.RegisterValueChangedCallback(evt => OnAudioChanged(evt.newValue));
        qualityDropdown.RegisterValueChangedCallback(evt => OnQualityChanged(evt.newValue));
        resolutionDropdown.RegisterValueChangedCallback(evt => OnResolutionChanged(evt.newValue));
        fullscreenToggle.RegisterValueChangedCallback(evt => OnFullscreenChanged(evt.newValue));
        backButton.clicked += OnBackButtonClicked;
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
        if (resIndex == -1) resIndex = 2; // default to 1920x1080
        resolutionDropdown.index = resIndex;
        ApplyResolution(resolution);

        // Полноэкранный
        bool fullscreen = PlayerPrefs.GetInt(FULLSCREEN_KEY, 1) == 1;
        fullscreenToggle.value = fullscreen;
        ApplyFullscreen(fullscreen);
    }

    private void OnAudioChanged(float value)
    {
        ApplyAudio(value);
        PlayerPrefs.SetFloat(AUDIO_KEY, value);
        PlayerPrefs.Save();
    }

    private void OnQualityChanged(string value)
    {
        int index = qualityDropdown.choices.IndexOf(value);
        if (index >= 0)
        {
            ApplyQuality(index);
            PlayerPrefs.SetInt(QUALITY_KEY, index);
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

    private void ApplyAudio(float volume)
    {
        AudioListener.volume = volume;
    }

    private void ApplyQuality(int level)
    {
        QualitySettings.SetQualityLevel(level);
    }

    private void ApplyResolution(string resolution)
    {
        string[] parts = resolution.Split('x');
        if (parts.Length == 2 && int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
        }
    }

    private void ApplyFullscreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
    }

    private void OnBackButtonClicked()
    {
        // Возврат в главное меню
        SceneManager.LoadScene("MainMenu");
    }
}