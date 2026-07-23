using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [SerializeField] private UIDocument uiDocument;
    private VisualElement overlay;
    private Label titleLabel;
    private Label descriptionLabel;
    private Button nextButton;
    private Button skipButton;

    // ---- Данные модульных туториалов ----
    private Dictionary<string, List<TutorialStep>> moduleTutorials;
    private List<TutorialStep> currentSteps;
    private int currentStepIndex;
    private string currentModule;

    // ---- Событие для уведомления UI о смене шага ----
    public event Action OnStepChanged;

    // ---- Свойства для совместимости со старым TutorialOverlay ----
    public int CurrentStep => currentStepIndex;
    public List<TutorialStep> GetSteps() => currentSteps;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (uiDocument == null) uiDocument = GetComponent<UIDocument>();
    }

    private void Start()
    {
        if (uiDocument != null)
        {
            var root = uiDocument.rootVisualElement;
            overlay = root.Q<VisualElement>("TutorialOverlayPanel");
            titleLabel = root.Q<Label>("TutorialTitle");
            descriptionLabel = root.Q<Label>("TutorialDescription");
            nextButton = root.Q<Button>("TutorialNextButton");
            skipButton = root.Q<Button>("TutorialSkipButton");

            if (nextButton != null) nextButton.clicked += NextStep;
            if (skipButton != null) skipButton.clicked += SkipTutorial;
        }

        InitializeModuleTutorials();
    }

    private void InitializeModuleTutorials()
    {
        moduleTutorials = new Dictionary<string, List<TutorialStep>>();

        // ---- Туториал по машинам ----
        moduleTutorials["cars"] = new List<TutorialStep>
        {
            new TutorialStep("🚗 Производство машин", "Здесь вы можете производить выбранные модели. Для этого нужны детали: двигатели, кузова, колёса и электроника."),
            new TutorialStep("Тюнинг", "Вы можете улучшать параметры машины: мощность, экономичность, дизайн и безопасность. Каждый уровень требует изучения соответствующей технологии."),
            new TutorialStep("Уровни машин", "Машины можно улучшать до более высоких уровней, что повышает их цену и спрос. Для этого нужна технология «Улучшение машин».")
        };

        // ---- Туториал по технологиям ----
        moduleTutorials["tech"] = new List<TutorialStep>
        {
            new TutorialStep("🔬 Дерево технологий", "Исследуйте технологии, чтобы открывать новые машины, улучшать параметры и получать бонусы. Каждая технология требует времени и денег."),
            new TutorialStep("Маркетинговые технологии", "В дереве есть специальные технологии маркетинга, которые усиливают ваши рекламные кампании и повышают спрос.")
        };

        // ---- Туториал по маркетингу ----
        moduleTutorials["marketing"] = new List<TutorialStep>
        {
            new TutorialStep("📢 Маркетинговые кампании", "Запускайте рекламу для конкретных машин, чтобы повысить на них спрос. Чем выше бюджет и длительность, тем сильнее эффект."),
            new TutorialStep("Качество бренда", "Рекламные кампании повышают качество бренда, что даёт постоянный бонус к спросу на все машины."),
            new TutorialStep("Скидки", "Вы можете временно снизить цены на все машины – это увеличит продажи, но уменьшит прибыль. Конкуренты могут ответить тем же.")
        };

        // ---- Туториал по улучшениям ----
        moduleTutorials["upgrade"] = new List<TutorialStep>
        {
            new TutorialStep("🏭 Улучшения завода", "Здесь вы можете улучшать конвейер (увеличивает пассивный доход) и нанимать инженеров (ускоряют производство и исследования)."),
            new TutorialStep("Производство деталей", "Если вы изучили технологии производства деталей, вы можете сами создавать их на складе – это дешевле, чем покупать.")
        };

        // ---- Туториал по конкурентам ----
        moduleTutorials["competitors"] = new List<TutorialStep>
        {
            new TutorialStep("👥 Конкуренты", "Здесь вы видите всех конкурентов на рынке. Вы можете атаковать их, воровать технологии или предлагать союзы. Каждый конкурент имеет свою стратегию.")
        };
    }

    // ---- Запуск модульного туториала ----
    public void StartModuleTutorial(string moduleName)
    {
        if (moduleTutorials == null || !moduleTutorials.ContainsKey(moduleName))
        {
            Debug.LogWarning($"Модуль туториала '{moduleName}' не найден");
            return;
        }

        // Проверяем, не запущен ли уже другой туториал
        if (overlay != null && overlay.style.display == DisplayStyle.Flex)
        {
            Debug.Log("Туториал уже запущен");
            return;
        }

        currentSteps = moduleTutorials[moduleName];
        currentStepIndex = 0;
        currentModule = moduleName;

        ShowStep(currentStepIndex);
        OnStepChanged?.Invoke();
    }

    // ---- Запуск обычного туториала (для совместимости со старым кодом) ----
    public void StartTutorial()
    {
        // Если передан модуль, запускаем его, иначе стартуем с машинами
        StartModuleTutorial("cars");
    }

    private void ShowStep(int index)
    {
        if (currentSteps == null || index >= currentSteps.Count)
        {
            CloseTutorial();
            return;
        }

        var step = currentSteps[index];
        if (titleLabel != null) titleLabel.text = step.title;
        if (descriptionLabel != null) descriptionLabel.text = step.description;
        if (nextButton != null)
        {
            nextButton.text = (index == currentSteps.Count - 1) ? "Завершить" : "Далее";
        }

        if (overlay != null)
        {
            overlay.style.display = DisplayStyle.Flex;
        }
        OnStepChanged?.Invoke();
    }

    // ---- Переход к следующему шагу (public) ----
    public void NextStep()
    {
        if (currentSteps == null) return;

        if (currentStepIndex < currentSteps.Count - 1)
        {
            currentStepIndex++;
            ShowStep(currentStepIndex);
        }
        else
        {
            CloseTutorial();
            SaveTutorialCompleted(currentModule);
        }
    }

    // ---- Пропуск туториала (public) ----
    public void SkipTutorial()
    {
        CloseTutorial();
        SaveTutorialCompleted(currentModule);
    }

    private void CloseTutorial()
    {
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;

        currentSteps = null;
        currentStepIndex = 0;
        currentModule = null;
        OnStepChanged?.Invoke();
    }

    private void SaveTutorialCompleted(string module)
    {
        if (string.IsNullOrEmpty(module)) return;

        var saveData = CarCompanyManager.Instance.SaveLoadManager?.GetCurrentSaveData();
        if (saveData == null)
        {
            PlayerPrefs.SetInt($"Tutorial_{module}", 1);
            PlayerPrefs.Save();
            return;
        }

        switch (module)
        {
            case "cars": saveData.tutorialCarsCompleted = true; break;
            case "tech": saveData.tutorialTechCompleted = true; break;
            case "marketing": saveData.tutorialMarketingCompleted = true; break;
            case "upgrade": saveData.tutorialUpgradeCompleted = true; break;
            case "competitors": saveData.tutorialCompetitorsCompleted = true; break;
        }
    }

    public bool IsModuleTutorialCompleted(string module)
    {
        if (PlayerPrefs.HasKey($"Tutorial_{module}") && PlayerPrefs.GetInt($"Tutorial_{module}") == 1)
            return true;

        var saveData = CarCompanyManager.Instance.SaveLoadManager?.GetCurrentSaveData();
        if (saveData == null) return false;

        switch (module)
        {
            case "cars": return saveData.tutorialCarsCompleted;
            case "tech": return saveData.tutorialTechCompleted;
            case "marketing": return saveData.tutorialMarketingCompleted;
            case "upgrade": return saveData.tutorialUpgradeCompleted;
            case "competitors": return saveData.tutorialCompetitorsCompleted;
            default: return false;
        }
    }

    public void ResetProgress()
    {
        var saveData = CarCompanyManager.Instance.SaveLoadManager?.GetCurrentSaveData();
        if (saveData != null)
        {
            saveData.tutorialCarsCompleted = false;
            saveData.tutorialTechCompleted = false;
            saveData.tutorialMarketingCompleted = false;
            saveData.tutorialUpgradeCompleted = false;
            saveData.tutorialCompetitorsCompleted = false;
        }

        PlayerPrefs.DeleteKey("Tutorial_cars");
        PlayerPrefs.DeleteKey("Tutorial_tech");
        PlayerPrefs.DeleteKey("Tutorial_marketing");
        PlayerPrefs.DeleteKey("Tutorial_upgrade");
        PlayerPrefs.DeleteKey("Tutorial_competitors");
        PlayerPrefs.Save();

        CloseTutorial();
        Debug.Log("Все туториалы сброшены");
    }

    // ---- Загрузка прогресса из сохранения (для совместимости) ----
    public void LoadProgress(SaveData data)
    {
        // Флаги уже загружены в SaveData, ничего дополнительно не нужно
        // Но если старый код ожидает что-то, можно оставить пустым
    }

    // ---- Публичный класс шага туториала ----
    [System.Serializable]
    public class TutorialStep
    {
        public string title;
        public string description;
        public string targetElementName;   // для подсветки (если нужно)
        public bool highlightTarget;

        public TutorialStep(string title, string description, string targetElementName = null, bool highlightTarget = false)
        {
            this.title = title;
            this.description = description;
            this.targetElementName = targetElementName;
            this.highlightTarget = highlightTarget;
        }
    }
}