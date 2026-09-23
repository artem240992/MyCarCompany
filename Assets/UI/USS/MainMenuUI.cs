using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class MainMenuUI : MonoBehaviour
{
    private static Font cachedFont;

    private Canvas canvas;
    private GameObject mainPanel;
    private GameObject settingsPanel;
    private int currentStep = 0;
    private const int totalSteps = 3;

    private Text stepTitle;
    private Text stepIndicator;
    private GameObject[] stepContents;
    private Button nextButton, backButton, saveButton;

    // Настройки
    private Slider musicSlider, sfxSlider, sensitivitySlider;
    private Slider qualitySlider, resolutionSlider;
    private Text qualityValueText, resolutionValueText;
    private Button fullscreenButton, invertYButton;
    private Text fullscreenText, invertYText;
    private bool fullscreen = true;
    private bool invertY = false;

    void Start()
    {
        BuildUI();
        LoadSettings();
        ShowMainPanel();
    }

    void BuildUI()
    {
        // EventSystem (если нет в сцене)
        if (FindObjectOfType<EventSystem>() == null)
        {
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        // Canvas
        GameObject canvasGO = new GameObject("MainMenuCanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Фон с градиентом
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(canvasGO.transform, false);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        Image bgImage = bgGO.GetComponent<Image>();
        Texture2D gradTex = CreateGradientTexture(new Color(0.05f, 0.05f, 0.15f), new Color(0.2f, 0.1f, 0.3f));
        bgImage.sprite = Sprite.Create(gradTex, new Rect(0, 0, 1, 2), new Vector2(0.5f, 0.5f));
        bgImage.type = Image.Type.Simple;

        // Главная панель
        mainPanel = CreatePanel("MainPanel", canvasGO.transform, new Vector2(700, 600));
        CreateText("Title", mainPanel.transform, "My Car Company", 55, TextAnchor.MiddleCenter,
            new Vector2(0, 180), new Vector2(600, 100));
        CreateButton("PlayButton", mainPanel.transform, "ИГРАТЬ", new Vector2(0, 40), new Vector2(350, 70),
            () => { Debug.Log("Запуск игры..."); SceneManager.LoadScene(1); });
        CreateButton("SettingsButton", mainPanel.transform, "НАСТРОЙКИ", new Vector2(0, -50), new Vector2(350, 70),
            ShowSettings);
        CreateButton("QuitButton", mainPanel.transform, "ВЫХОД", new Vector2(0, -140), new Vector2(350, 70),QuitGame);

        // Панель настроек
        settingsPanel = CreatePanel("SettingsPanel", canvasGO.transform, new Vector2(900, 700));
        settingsPanel.SetActive(false);

        stepTitle = CreateText("StepTitle", settingsPanel.transform, "Аудио", 48, TextAnchor.MiddleCenter,
            new Vector2(0, 260), new Vector2(600, 80));
        stepIndicator = CreateText("StepIndicator", settingsPanel.transform, "Шаг 1 из 3", 24, TextAnchor.MiddleCenter,
            new Vector2(0, 210), new Vector2(300, 40));

        // Шаги
        stepContents = new GameObject[totalSteps];

        // Шаг 1: Аудио
        GameObject step1 = new GameObject("Step1_Audio", typeof(RectTransform));
        step1.transform.SetParent(settingsPanel.transform, false);
        RectTransform step1Rect = step1.GetComponent<RectTransform>();
        step1Rect.anchoredPosition = Vector2.zero;
        step1Rect.sizeDelta = new Vector2(800, 400);

        CreateLabel(step1.transform, "Громкость музыки", new Vector2(-300, 80));
        musicSlider = CreateSlider(step1.transform, new Vector2(100, 80), 0, 1, 0.8f);
        Text musicValue = CreateText("MusicValue", step1.transform, "80%", 24, TextAnchor.MiddleLeft,
            new Vector2(300, 80), new Vector2(100, 40));
        musicSlider.onValueChanged.AddListener((v) => musicValue.text = Mathf.RoundToInt(v * 100) + "%");

        CreateLabel(step1.transform, "Громкость звуков", new Vector2(-300, 0));
        sfxSlider = CreateSlider(step1.transform, new Vector2(100, 0), 0, 1, 0.8f);
        Text sfxValue = CreateText("SfxValue", step1.transform, "80%", 24, TextAnchor.MiddleLeft,
            new Vector2(300, 0), new Vector2(100, 40));
        sfxSlider.onValueChanged.AddListener((v) => sfxValue.text = Mathf.RoundToInt(v * 100) + "%");
        stepContents[0] = step1;

        // Шаг 2: Графика
        GameObject step2 = new GameObject("Step2_Graphics", typeof(RectTransform));
        step2.transform.SetParent(settingsPanel.transform, false);
        RectTransform step2Rect = step2.GetComponent<RectTransform>();
        step2Rect.anchoredPosition = Vector2.zero;
        step2Rect.sizeDelta = new Vector2(800, 400);

        CreateLabel(step2.transform, "Качество графики", new Vector2(-300, 80));
        qualitySlider = CreateSlider(step2.transform, new Vector2(100, 80), 0, 3, 2);
        qualitySlider.wholeNumbers = true;
        qualityValueText = CreateText("QualityValue", step2.transform, "Высокое", 24, TextAnchor.MiddleLeft,
            new Vector2(300, 80), new Vector2(150, 40));
        qualitySlider.onValueChanged.AddListener((v) =>
        {
            int q = Mathf.RoundToInt(v);
            string[] names = { "Низкое", "Среднее", "Высокое", "Ультра" };
            qualityValueText.text = names[q];
        });

        CreateLabel(step2.transform, "Полный экран", new Vector2(-300, 0));
        fullscreenButton = CreateButton("FullscreenButton", step2.transform, "Вкл", new Vector2(100, 0),
            new Vector2(120, 50), () =>
            {
                fullscreen = !fullscreen;
                fullscreenText.text = fullscreen ? "Вкл" : "Выкл";
            });
        fullscreenText = fullscreenButton.GetComponentInChildren<Text>();

        CreateLabel(step2.transform, "Разрешение", new Vector2(-300, -80));
        resolutionSlider = CreateSlider(step2.transform, new Vector2(100, -80), 0, 2, 1);
        resolutionSlider.wholeNumbers = true;
        resolutionValueText = CreateText("ResValue", step2.transform, "1920x1080", 24, TextAnchor.MiddleLeft,
            new Vector2(300, -80), new Vector2(150, 40));
        resolutionSlider.onValueChanged.AddListener((v) =>
        {
            int r = Mathf.RoundToInt(v);
            string[] res = { "1280x720", "1920x1080", "2560x1440" };
            resolutionValueText.text = res[r];
        });
        stepContents[1] = step2;

        // Шаг 3: Управление
        GameObject step3 = new GameObject("Step3_Controls", typeof(RectTransform));
        step3.transform.SetParent(settingsPanel.transform, false);
        RectTransform step3Rect = step3.GetComponent<RectTransform>();
        step3Rect.anchoredPosition = Vector2.zero;
        step3Rect.sizeDelta = new Vector2(800, 400);

        CreateLabel(step3.transform, "Чувствительность", new Vector2(-300, 80));
        sensitivitySlider = CreateSlider(step3.transform, new Vector2(100, 80), 0.1f, 5f, 1f);
        Text sensValue = CreateText("SensValue", step3.transform, "1.0", 24, TextAnchor.MiddleLeft,
            new Vector2(300, 80), new Vector2(100, 40));
        sensitivitySlider.onValueChanged.AddListener((v) => sensValue.text = v.ToString("F1"));

        CreateLabel(step3.transform, "Инверсия Y", new Vector2(-300, 0));
        invertYButton = CreateButton("InvertYButton", step3.transform, "Выкл", new Vector2(100, 0),
            new Vector2(120, 50), () =>
            {
                invertY = !invertY;
                invertYText.text = invertY ? "Вкл" : "Выкл";
            });
        invertYText = invertYButton.GetComponentInChildren<Text>();
        stepContents[2] = step3;

        // Кнопки навигации
        backButton = CreateButton("BackButton", settingsPanel.transform, "НАЗАД", new Vector2(-280, -280),
            new Vector2(200, 60), PreviousStep);
        nextButton = CreateButton("NextButton", settingsPanel.transform, "ДАЛЕЕ", new Vector2(0, -280),
            new Vector2(200, 60), NextStep);
        saveButton = CreateButton("SaveButton", settingsPanel.transform, "СОХРАНИТЬ", new Vector2(280, -280),
            new Vector2(200, 60), SaveSettings);
    }

    void ApplySettings()
    {
        // Аудио
        AudioListener.volume = musicSlider.value;   // общая громкость

        // Графика
        int q = Mathf.RoundToInt(qualitySlider.value);
        QualitySettings.SetQualityLevel(q, true);

        // Разрешение + полный экран
        string[] res = { "1280x720", "1920x1080", "2560x1440" };
        string[] parts = res[Mathf.RoundToInt(resolutionSlider.value)].Split('x');
        int w = int.Parse(parts[0]);
        int h = int.Parse(parts[1]);
        Screen.SetResolution(w, h, fullscreen);

        Debug.Log($"Применено: качество={q}, {w}x{h}, fullscreen={fullscreen}, " +
                $"music={musicSlider.value:F2}, sfx={sfxSlider.value:F2}, " +
                $"sens={sensitivitySlider.value:F2}, invertY={invertY}");
    }

    void QuitGame()
    {
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        Application.Quit();
    #endif
    }
    // ---------- Логика шагов ----------
    void ShowMainPanel()
    {
        mainPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    void ShowSettings()
    {
        mainPanel.SetActive(false);
        settingsPanel.SetActive(true);
        currentStep = 0;
        UpdateStep();
    }

    void UpdateStep()
    {
        for (int i = 0; i < totalSteps; i++)
            stepContents[i].SetActive(i == currentStep);

        string[] titles = { "Аудио", "Графика", "Управление" };
        stepTitle.text = titles[currentStep];
        stepIndicator.text = $"Шаг {currentStep + 1} из {totalSteps}";

        backButton.gameObject.SetActive(currentStep > 0);
        nextButton.gameObject.SetActive(currentStep < totalSteps - 1);
        saveButton.gameObject.SetActive(currentStep == totalSteps - 1);
    }

    void NextStep()
    {
        if (currentStep < totalSteps - 1)
        {
            currentStep++;
            UpdateStep();
        }
    }

    void PreviousStep()
    {
        if (currentStep > 0)
        {
            currentStep--;
            UpdateStep();
        }
        else
        {
            ShowMainPanel();
        }
    }

    void SaveSettings()
    {
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);
        PlayerPrefs.SetInt("Quality", Mathf.RoundToInt(qualitySlider.value));
        PlayerPrefs.SetInt("Fullscreen", fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("Resolution", Mathf.RoundToInt(resolutionSlider.value));
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        PlayerPrefs.SetInt("InvertY", invertY ? 1 : 0);
        PlayerPrefs.Save();
        ApplySettings(); 
        Debug.Log("Настройки сохранены!");
        ShowMainPanel();
    }

    void LoadSettings()
    {
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.8f);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        qualitySlider.value = PlayerPrefs.GetInt("Quality", 2);
        fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenText.text = fullscreen ? "Вкл" : "Выкл";
        resolutionSlider.value = PlayerPrefs.GetInt("Resolution", 1);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 1f);
        invertY = PlayerPrefs.GetInt("InvertY", 0) == 1;
        invertYText.text = invertY ? "Вкл" : "Выкл";
        ApplySettings();
    }

    // ---------- Вспомогательные методы ----------
    GameObject CreatePanel(string name, Transform parent, Vector2 size)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = Vector2.zero;
        Image image = panel.GetComponent<Image>();
        image.color = new Color(0.1f, 0.1f, 0.2f, 0.95f);
        Shadow shadow = panel.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.5f);
        shadow.effectDistance = new Vector2(5, -5);
        return panel;
    }

    Text CreateText(string name, Transform parent, string content, int fontSize, TextAnchor alignment,
        Vector2 anchoredPos, Vector2 size)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;

        Text text = go.GetComponent<Text>();
        text.font = GetFont();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.6f);
        shadow.effectDistance = new Vector2(2, -2);
        return text;
    }

    Text CreateLabel(Transform parent, string text, Vector2 anchoredPos)
    {
        return CreateText("Label", parent, text, 28, TextAnchor.MiddleLeft, anchoredPos, new Vector2(250, 40));
    }

    Button CreateButton(string name, Transform parent, string label, Vector2 anchoredPos, Vector2 size,
        UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = anchoredPos;
        Image image = go.GetComponent<Image>();
        image.color = new Color(0.2f, 0.5f, 0.8f);
        Button button = go.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = new Color(0.2f, 0.5f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.6f, 0.9f);
        colors.pressedColor = new Color(0.1f, 0.4f, 0.7f);
        button.colors = colors;
        button.onClick.AddListener(onClick);

        Text text = CreateText("Text", go.transform, label, 28, TextAnchor.MiddleCenter, Vector2.zero, size);
        text.rectTransform.anchorMin = Vector2.zero;
        text.rectTransform.anchorMax = Vector2.one;
        text.rectTransform.offsetMin = Vector2.zero;
        text.rectTransform.offsetMax = Vector2.zero;
        text.raycastTarget = false;

        return button;
    }

    Slider CreateSlider(Transform parent, Vector2 anchoredPos, float min, float max, float value)
    {
        GameObject sliderGO = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGO.transform.SetParent(parent, false);
        RectTransform rect = sliderGO.GetComponent<RectTransform>();
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(300, 20);

        // Background
        GameObject bgGO = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGO.transform.SetParent(sliderGO.transform, false);
        RectTransform bgRect = bgGO.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(1, 0.75f);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        bgGO.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f);

        // Fill Area
        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1, 0.75f);
        fillAreaRect.offsetMin = Vector2.zero;
        fillAreaRect.offsetMax = Vector2.zero;

        GameObject fillGO = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGO.transform.SetParent(fillArea.transform, false);
        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(1, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        fillGO.GetComponent<Image>().color = new Color(0.3f, 0.7f, 1f);

        // Handle Slide Area
        GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGO.transform, false);
        RectTransform handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = Vector2.zero;
        handleAreaRect.offsetMax = Vector2.zero;

        GameObject handleGO = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGO.transform.SetParent(handleArea.transform, false);
        RectTransform handleRect = handleGO.GetComponent<RectTransform>();
        handleRect.sizeDelta = new Vector2(20, 0);
        Image handleImage = handleGO.GetComponent<Image>();
        handleImage.color = Color.white;

        Slider slider = sliderGO.GetComponent<Slider>();
        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.minValue = min;
        slider.maxValue = max;
        slider.value = value;

        return slider;
    }

    Texture2D CreateGradientTexture(Color top, Color bottom)
    {
        Texture2D tex = new Texture2D(1, 2);
        tex.SetPixel(0, 0, bottom);
        tex.SetPixel(0, 1, top);
        tex.Apply();
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Bilinear;
        return tex;
    }

    private static Font GetFont()
    {
        if (cachedFont != null) return cachedFont;

        // Unity 2022+
        cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        // Unity 2019–2021
        if (cachedFont == null)
            cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

        // Крайний случай — системный шрифт
        if (cachedFont == null)
            cachedFont = Font.CreateDynamicFontFromOSFont("Arial", 16);

        return cachedFont;
    }
}