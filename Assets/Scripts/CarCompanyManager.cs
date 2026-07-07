using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum DifficultyLevel
{
    Easy,
    Normal,
    Hard
}

[System.Serializable]
public class SaveData
{
    public double money;
    public double passiveIncome;
    public int conveyorLevel;
    public int engineerCount;
    public string[] researchedTechNames;
    public int productionCount;
    public int currentDifficulty;
    public List<CarDemandData> carDemands = new List<CarDemandData>();
    public List<CarLevelData> carLevels = new List<CarLevelData>();
}

[System.Serializable]
public class CarDemandData
{
    public string carName;
    public float demandMultiplier;
}

[System.Serializable]
public class CarLevelData
{
    public string carName;
    public int currentLevel;
}

// ---- КЛАСС COMPETITOR (внутри этого файла, без дублирования) ----
[System.Serializable]
public class Competitor
{
    public string companyName;
    public float money;
    public int factoryLevel;
    public int researchLevel;
    public int reputation;
    public float priceMultiplier;
    public float marketShare;
    public List<CarBlueprint> availableCars = new List<CarBlueprint>();
    public List<string> researchedTechs = new List<string>();

    // ---- НОВЫЕ ПОЛЯ ----
    public bool isAlly;
    public int engineers;
    public float espionageLevel;
    public float marketingPower;
    public float loyalty;
    public List<string> stolenTechs = new List<string>();
}

public class CarCompanyManager : MonoBehaviour
{
    // --- UI элементы ---
    private UIDocument uiDoc;
    private VisualElement mainPanel;
    private VisualElement notificationContainer;
    private Label eventLabel;

    private VisualElement carsOverlay;
    private VisualElement carsContainer;
    private Label countLabel;
    private int productionCount = 1;

    private VisualElement techOverlay;
    private ScrollView techScrollView;
    private VisualElement techGraphRoot;

    private VisualElement upgradeOverlay;
    private Label conveyorLevelLabel;
    private Label engineerCountLabel;
    private Button buyConveyorButton;
    private Button hireEngineerButton;

    private VisualElement settingsOverlay;
    private Button easyButton;
    private Button normalButton;
    private Button hardButton;

    private VisualElement welcomeOverlay;
    private Button welcomeEasyButton;
    private Button welcomeNormalButton;
    private Button welcomeHardButton;

    private Button hamburgerButton;
    private VisualElement menuContainer;

    // --- Окно конкурентов ---
    private VisualElement competitorsOverlay;
    private VisualElement competitorsContainer;

    // --- Элементы главной панели ---
    private Label moneyLabel;
    private Label incomeLabel;
    private Label savedDifficultyLabel;
    private Label versionLabel;

    // --- Экономика ---
    private double money = 100;
    private double passiveIncome = 0;

    // --- Уровни улучшений ---
    private int conveyorLevel = 0;
    private int engineerCount = 0;

    // --- Данные ---
    public CarBlueprint[] startCars;
    public CarBlueprint[] availableCars;
    public Technology[] technologies;

    // --- Визуализация ---
    public GameObject CarDisplay;
    private GameObject currentCarInstance;
    public Transform startPoint;
    public Transform endPoint;

    // --- Уровни сложности ---
    public DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
    private float costMultiplier = 1f;
    private float profitMultiplier = 1f;
    private float startMoney = 100f;
    private float techCostMultiplier = 1f;

    private bool isProductionInProgress = false;
    public string bulkProductionTechName = "Массовое производство";
    public string carUpgradeTechName = "Улучшить авто";

    // --- Экономические события (только Hard) ---
    private float currentEventMultiplier = 1f;
    private string currentEventText = "";
    private Coroutine eventCoroutine;

    // --- Конкуренты ---
    private List<Competitor> competitors = new List<Competitor>();
    private Coroutine competitorCoroutine;
    private string[] competitorNames = { "АвтоСтар", "ТехноТранс", "ЭкоДрайв", "СпортМотор", "ГородскойАвто" };

    // --- Модификаторы от технологий ---
    private float totalPriceModifier = 1f;
    private float totalDemandModifier = 1f;
    private float totalCostModifier = 1f;

    // --- Репутация игрока ---
    private int reputation = 50;

    // --- Для дерева технологий ---
    private List<TechNode> techNodes = new List<TechNode>();
    private class TechNode
    {
        public Technology tech;
        public VisualElement element;
        public Vector2 position;
        public List<TechNode> children = new List<TechNode>();
        public List<TechNode> parents = new List<TechNode>();
        public int level;
    }

    // --- Для динамического спроса ---
    private float demandUpdateInterval = 5f;
    private List<CarCardData> carCards = new List<CarCardData>();
    private class CarCardData
    {
        public CarBlueprint car;
        public VisualElement card;
        public Label profitLabel;
        public Label demandLabel;
        public Label trendLabel;
        public VisualElement graphContainer;
        public Button upgradeButton;

        public Label powerLabel, economyLabel, designLabel, safetyLabel;
        public Label powerCostLabel, economyCostLabel, designCostLabel, safetyCostLabel;
        public Button powerUpgradeBtn, economyUpgradeBtn, designUpgradeBtn, safetyUpgradeBtn;
    }

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "autofactory_save.json");
    private bool isGameInitialized = false;

    // --- Параметры тюнинга ---
    private string[] tuningParamNames = { "power", "economy", "design", "safety" };
    private string[] tuningParamDisplay = { "Мощность", "Экономичность", "Дизайн", "Безопасность" };
    private const int TUNING_MAX_LEVEL = 10;

    // ============================== START ==============================
    void Start()
    {
        uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) { Debug.LogError("UIDocument не найден!"); return; }

        VisualElement root = uiDoc.rootVisualElement;
        if (root == null) { Debug.LogError("rootVisualElement null!"); return; }

        mainPanel = root.Q<VisualElement>("MainPanel");
        if (mainPanel == null) Debug.LogError("MainPanel не найден!");

        notificationContainer = root.Q<VisualElement>("NotificationContainer");
        if (notificationContainer == null) Debug.LogError("NotificationContainer не найден!");

        versionLabel = root.Q<Label>("VersionLabel");
        if (versionLabel != null)
        {
            versionLabel.text = $"v. {Application.version}";
            versionLabel.style.color = Color.black;
        }

        carsOverlay = root.Q<VisualElement>("CarsOverlay");
        carsContainer = root.Q<VisualElement>("CarsContainer");
        techOverlay = root.Q<VisualElement>("TechOverlay");
        techScrollView = root.Q<ScrollView>("TechContainer");
        upgradeOverlay = root.Q<VisualElement>("UpgradeOverlay");
        settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        competitorsOverlay = root.Q<VisualElement>("CompetitorsOverlay");
        competitorsContainer = root.Q<VisualElement>("CompetitorsContainer");

        welcomeOverlay = root.Q<VisualElement>("WelcomeOverlay");
        welcomeEasyButton = welcomeOverlay?.Q<Button>("WelcomeEasyButton");
        welcomeNormalButton = welcomeOverlay?.Q<Button>("WelcomeNormalButton");
        welcomeHardButton = welcomeOverlay?.Q<Button>("WelcomeHardButton");

        if (carsOverlay == null) Debug.LogError("CarsOverlay не найден!");
        if (carsContainer == null) Debug.LogError("CarsContainer не найден!");
        if (techOverlay == null) Debug.LogError("TechOverlay не найден!");
        if (techScrollView == null) Debug.LogError("TechContainer (ScrollView) не найден!");
        if (upgradeOverlay == null) Debug.LogError("UpgradeOverlay не найден!");
        if (settingsOverlay == null) Debug.LogError("SettingsOverlay не найден!");
        if (competitorsOverlay == null) Debug.LogError("CompetitorsOverlay не найден!");

        if (welcomeEasyButton != null) welcomeEasyButton.clicked += () => { SetDifficulty(DifficultyLevel.Easy); CloseWelcomeScreen(); };
        if (welcomeNormalButton != null) welcomeNormalButton.clicked += () => { SetDifficulty(DifficultyLevel.Normal); CloseWelcomeScreen(); };
        if (welcomeHardButton != null) welcomeHardButton.clicked += () => { SetDifficulty(DifficultyLevel.Hard); CloseWelcomeScreen(); };

        HideAllOverlays();

        // ---- Если startCars не задан, создаём базовую машину ----
        if (startCars == null || startCars.Length == 0)
        {
            Debug.LogWarning("startCars не назначен! Создаю временную базовую машину.");
            GameObject defaultPrefab = Resources.Load<GameObject>("Prefabs/DefaultCar");
            if (defaultPrefab == null)
            {
                Debug.LogWarning("Префаб DefaultCar не найден в Resources/Prefabs! Будет создан временный куб.");
                defaultPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(defaultPrefab.GetComponent<Collider>());
                defaultPrefab.name = "DefaultCar";
            }

            CarBlueprint defaultCar = ScriptableObject.CreateInstance<CarBlueprint>();
            defaultCar.carName = "Стандартная машина";
            defaultCar.basePrice = 100;
            defaultCar.productionCost = 50;
            defaultCar.carPrefab = defaultPrefab;

            startCars = new CarBlueprint[] { defaultCar };
        }

        // ---- Автоматическое создание контейнера и точек ----
        if (CarDisplay == null)
        {
            GameObject container = new GameObject("CarDisplay");
            container.transform.parent = transform;
            CarDisplay = container;
            Debug.Log("Создан автоматический контейнер CarDisplay.");
        }

        if (startPoint == null)
        {
            GameObject start = new GameObject("StartPoint");
            start.transform.parent = CarDisplay.transform;
            start.transform.localPosition = new Vector3(-3, 0, 0);
            startPoint = start.transform;
            Debug.Log("Создана автоматическая точка старта.");
        }
        if (endPoint == null)
        {
            GameObject end = new GameObject("EndPoint");
            end.transform.parent = CarDisplay.transform;
            end.transform.localPosition = new Vector3(3, 0, 0);
            endPoint = end.transform;
            Debug.Log("Создана автоматическая точка финиша.");
        }

        GenerateTuningTechnologies();

        if (File.Exists(SaveFilePath))
        {
            LoadGame();
            InitGame();
        }
        else
        {
            ShowWelcomeScreen();
        }

        if (MarketSystem.Instance == null)
        {
            GameObject marketGO = new GameObject("MarketSystem");
            marketGO.AddComponent<MarketSystem>();
        }
    }

    // ============================== UPDATE (ESC) ==============================
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    // ============================== ГЕНЕРАЦИЯ ТЕХНОЛОГИЙ ТЮНИНГА ==============================
    private void GenerateTuningTechnologies()
    {
        if (technologies != null && technologies.Any(t => t != null && t.techName == "power_1"))
            return;

        List<Technology> newTechs = new List<Technology>();
        if (technologies != null)
            newTechs.AddRange(technologies);

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
                tech.requiredTechNames = (level > 1) ? new string[] { $"{param}_{level-1}" } : new string[0];
                tech.priceModifier = 1f;
                tech.demandModifier = 1f;
                newTechs.Add(tech);
            }
        }

        technologies = newTechs.ToArray();
        Debug.Log($"Сгенерировано {newTechs.Count} технологий тюнинга");
    }

    private Technology GetTuningTech(string param, int level)
    {
        return technologies.FirstOrDefault(t => t != null && t.techName == $"{param}_{level}");
    }

    private bool CanUpgradeTuning(CarBlueprint car, string param)
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
        int cost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);
        return money >= cost;
    }

    private int GetTuningLevel(CarBlueprint car, string param)
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

    public void UpgradeTuning(CarBlueprint car, string param)
    {
        if (car == null) return;
        int current = GetTuningLevel(car, param);
        if (current >= TUNING_MAX_LEVEL) { ShowNotification("Максимальный уровень!"); return; }
        int nextLevel = current + 1;
        Technology tech = GetTuningTech(param, nextLevel);
        if (tech == null) { ShowNotification("Технология не найдена!"); return; }
        if (tech.isResearched) { ShowNotification("Технология уже изучена!"); return; }
        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string req in tech.requiredTechNames)
            {
                Technology reqTech = technologies.FirstOrDefault(t => t != null && t.techName == req);
                if (reqTech == null || !reqTech.isResearched)
                {
                    ShowNotification($"Требуется исследовать {req}");
                    return;
                }
            }
        }
        int cost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        money -= cost;
        tech.isResearched = true;
        switch (param)
        {
            case "power": car.tuningPower = nextLevel; break;
            case "economy": car.tuningEconomy = nextLevel; break;
            case "design": car.tuningDesign = nextLevel; break;
            case "safety": car.tuningSafety = nextLevel; break;
        }
        int paramIndex = Array.IndexOf(tuningParamNames, param);
        ShowNotification($"Улучшен параметр {tuningParamDisplay[paramIndex]} до {nextLevel} для {car.carName}!");

        UpdateCarCards();
        UpdateUI();
        RefreshTechButtons();
    }

    // ============================== ЗАГРУЗКА ИКОНКИ ИЗ RESOURCES ==============================
    private Sprite LoadCarIcon(string carName)
    {
        if (string.IsNullOrEmpty(carName)) return null;
        string path = $"Images/{carName}";
        Sprite icon = Resources.Load<Sprite>(path);
        if (icon == null)
            Debug.LogWarning($"Иконка для {carName} не найдена по пути {path}. Положите изображение в Resources/Images/{carName}.png");
        return icon;
    }

    // ============================== ОКНА ==============================
    private void ShowWelcomeScreen()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.style.display = DisplayStyle.Flex;
            AnimateWindowOpen(welcomeOverlay);
        }
    }

    private void CloseWelcomeScreen()
    {
        if (welcomeOverlay != null)
            welcomeOverlay.style.display = DisplayStyle.None;

        if (!isGameInitialized)
            InitGame();
    }

    private void AnimateWindowOpen(VisualElement window)
    {
        window.style.display = DisplayStyle.Flex;
        window.style.opacity = 0;
        window.style.scale = new Scale(new Vector3(0.9f, 0.9f, 1f));
        window.schedule.Execute(() =>
        {
            window.style.opacity = 1;
            window.style.scale = new Scale(new Vector3(1f, 1f, 1f));
        }).ExecuteLater(10);
    }

    private void AnimateWindowClose(VisualElement window, System.Action onComplete)
    {
        window.style.opacity = 0;
        window.style.scale = new Scale(new Vector3(0.9f, 0.9f, 1f));
        window.schedule.Execute(() =>
        {
            onComplete?.Invoke();
        }).ExecuteLater(150);
    }

    // ============================== ВСПОМОГАТЕЛЬНЫЙ МЕТОД ==============================
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

    public CarBlueprint[] GetAllCars()
    {
        return GetAllPossibleCars().ToArray();
    }

    // ============================== ПЕРЕСЧЁТ МОДИФИКАТОРОВ ==============================
    private void RecalculateModifiers()
    {
        totalPriceModifier = 1f;
        totalDemandModifier = 1f;
        totalCostModifier = 1f;

        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech != null && tech.isResearched)
                {
                    totalPriceModifier *= tech.priceModifier;
                    totalDemandModifier *= tech.demandModifier;
                    if (tech.techName == "Гибридный привод")
                        totalCostModifier *= 0.9f;
                }
            }
        }
        totalPriceModifier = Mathf.Clamp(totalPriceModifier, 0.5f, 5f);
        totalDemandModifier = Mathf.Clamp(totalDemandModifier, 0.5f, 5f);
        totalCostModifier = Mathf.Clamp(totalCostModifier, 0.5f, 2f);
    }

    // ============================== ИНИЦИАЛИЗАЦИЯ ==============================
    private void InitGame()
    {
        if (isGameInitialized) return;
        isGameInitialized = true;

        VisualElement root = uiDoc.rootVisualElement;
        if (root == null) return;

        if (eventLabel == null)
            eventLabel = root.Q<Label>("EventLabel");

        // --- Кнопки ---
        Button openCarsButton = root.Q<Button>("OpenCarsButton");
        if (openCarsButton != null) openCarsButton.clicked += OpenCarsWindow;
        else Debug.LogError("OpenCarsButton не найден!");

        Button openTechButton = root.Q<Button>("OpenTechButton");
        if (openTechButton != null) openTechButton.clicked += OpenTechWindow;
        else Debug.LogError("OpenTechButton не найден!");

        Button openUpgradeButton = root.Q<Button>("OpenUpgradeButton");
        if (openUpgradeButton != null) openUpgradeButton.clicked += OpenUpgradeWindow;
        else Debug.LogError("OpenUpgradeButton не найден!");

        Button openSettingsButton = root.Q<Button>("OpenSettingsButton");
        if (openSettingsButton != null) openSettingsButton.clicked += OpenSettingsWindow;
        else Debug.LogError("OpenSettingsButton не найден!");

        Button openCompetitorsButton = root.Q<Button>("OpenCompetitorsButton");
        if (openCompetitorsButton != null) openCompetitorsButton.clicked += OpenCompetitorsWindow;
        else Debug.LogWarning("OpenCompetitorsButton не найден!");

        Button closeCarsButton = root.Q<Button>("CloseCarsButton");
        if (closeCarsButton != null) closeCarsButton.clicked += CloseCarsWindow;
        else Debug.LogError("CloseCarsButton не найден!");

        Button closeTechButton = root.Q<Button>("CloseTechButton");
        if (closeTechButton != null) closeTechButton.clicked += CloseTechWindow;
        else Debug.LogError("CloseTechButton не найден!");

        Button closeUpgradeButton = root.Q<Button>("CloseUpgradeButton");
        if (closeUpgradeButton != null) closeUpgradeButton.clicked += CloseUpgradeWindow;
        else Debug.LogError("CloseUpgradeButton не найден!");

        Button closeSettingsButton = root.Q<Button>("CloseSettingsButton");
        if (closeSettingsButton != null) closeSettingsButton.clicked += CloseSettingsWindow;
        else Debug.LogError("CloseSettingsButton не найден!");

        Button closeCompetitorsButton = root.Q<Button>("CloseCompetitorsButton");
        if (closeCompetitorsButton != null) closeCompetitorsButton.clicked += CloseCompetitorsWindow;
        else Debug.LogWarning("CloseCompetitorsButton не найден!");

        Button refreshCompetitorsButton = root.Q<Button>("RefreshCompetitorsButton");
        if (refreshCompetitorsButton != null) refreshCompetitorsButton.clicked += RefreshCompetitorsList;
        else Debug.LogWarning("RefreshCompetitorsButton не найден!");

        Button saveButton = root.Q<Button>("SaveButton");
        if (saveButton != null) saveButton.clicked += SaveGame;
        else Debug.LogWarning("SaveButton не найден!");

        Button loadButton = root.Q<Button>("LoadButton");
        if (loadButton != null) loadButton.clicked += LoadGame;
        else Debug.LogWarning("LoadButton не найден!");

        Button newGameButton = root.Q<Button>("NewGameButton");
        if (newGameButton != null) newGameButton.clicked += NewGame;
        else Debug.LogWarning("NewGameButton не найден!");

        hamburgerButton = root.Q<Button>("HamburgerButton");
        menuContainer = root.Q<VisualElement>("MenuContainer");
        if (hamburgerButton != null && menuContainer != null)
        {
            hamburgerButton.clicked += () =>
            {
                menuContainer.style.display = (menuContainer.style.display == DisplayStyle.Flex)
                    ? DisplayStyle.None
                    : DisplayStyle.Flex;
            };
        }

        easyButton = settingsOverlay.Q<Button>("EasyButton");
        normalButton = settingsOverlay.Q<Button>("NormalButton");
        hardButton = settingsOverlay.Q<Button>("HardButton");
        if (easyButton != null) easyButton.clicked += () => { SetDifficulty(DifficultyLevel.Easy); CloseSettingsWindow(); };
        if (normalButton != null) normalButton.clicked += () => { SetDifficulty(DifficultyLevel.Normal); CloseSettingsWindow(); };
        if (hardButton != null) hardButton.clicked += () => { SetDifficulty(DifficultyLevel.Hard); CloseSettingsWindow(); };

        countLabel = root.Q<Label>("CountLabel");
        Button decreaseButton = root.Q<Button>("DecreaseCountButton");
        Button increaseButton = root.Q<Button>("IncreaseCountButton");
        if (countLabel == null) Debug.LogError("CountLabel не найден!");
        if (decreaseButton != null) decreaseButton.clicked += DecreaseCount;
        if (increaseButton != null) increaseButton.clicked += IncreaseCount;

        moneyLabel = mainPanel.Q<Label>("MoneyLabel");
        incomeLabel = mainPanel.Q<Label>("IncomeLabel");
        if (moneyLabel == null) Debug.LogError("MoneyLabel не найден!");
        if (incomeLabel == null) Debug.LogError("IncomeLabel не найден!");

        savedDifficultyLabel = mainPanel.Q<Label>("SavedDifficultyLabel");
        if (savedDifficultyLabel != null)
            UpdateSavedDifficultyLabel();

        Button produceButton = mainPanel.Q<Button>("ProduceButton");
        if (produceButton != null) produceButton.clicked += ProduceBasicCar;

        conveyorLevelLabel = upgradeOverlay.Q<Label>("ConveyorLevelLabel");
        engineerCountLabel = upgradeOverlay.Q<Label>("EngineerCountLabel");
        buyConveyorButton = upgradeOverlay.Q<Button>("BuyConveyorButton");
        hireEngineerButton = upgradeOverlay.Q<Button>("HireEngineerButton");
        if (conveyorLevelLabel == null) Debug.LogError("ConveyorLevelLabel не найден!");
        if (engineerCountLabel == null) Debug.LogError("EngineerCountLabel не найден!");
        if (buyConveyorButton != null) buyConveyorButton.clicked += BuyConveyorUpgrade;
        if (hireEngineerButton != null) hireEngineerButton.clicked += HireEngineer;

        InitCompetitors();
        RecalculateModifiers();
        BuildAvailableCars();
        CreateCarButtons();
        CreateTechButtons();
        CloseAllWindows();

        if (money == 0) money = startMoney;
        ClampProductionCount();
        UpdateUI();

        StartCoroutine(PassiveIncome());
        UpdateProductionButtons();

        if (currentDifficulty == DifficultyLevel.Hard)
            StartEconomicEvents();
        else
            StopEconomicEvents();

        StartCompetitorAI();
    }

    // ============================== КОНКУРЕНТЫ ==============================
    private void InitCompetitors()
    {
        competitors.Clear();
        int count = 3 + (int)currentDifficulty;
        for (int i = 0; i < count && i < competitorNames.Length; i++)
        {
            Competitor comp = new Competitor();
            comp.companyName = competitorNames[i];
            comp.money = Random.Range(200f, 500f);
            comp.factoryLevel = Random.Range(1, 3);
            comp.researchLevel = Random.Range(1, 3);
            comp.reputation = Random.Range(30, 70);
            comp.priceMultiplier = Random.Range(0.8f, 1.2f);
            comp.marketShare = Random.Range(0.05f, 0.15f);
            int carCount = Mathf.Min(Random.Range(1, 3), availableCars.Length);
            for (int j = 0; j < carCount; j++)
            {
                if (availableCars[j] != null)
                    comp.availableCars.Add(availableCars[j]);
            }
            comp.engineers = Random.Range(1, 5);
            comp.espionageLevel = Random.Range(1, 6);
            comp.marketingPower = Random.Range(1, 6);
            comp.loyalty = Random.Range(50, 100);
            comp.isAlly = false;
            comp.stolenTechs = new List<string>();
            competitors.Add(comp);
        }
        RefreshCompetitorsList();
    }

    private void StartCompetitorAI()
    {
        if (competitorCoroutine != null) StopCoroutine(competitorCoroutine);
        competitorCoroutine = StartCoroutine(CompetitorAILoop());
    }

    private IEnumerator CompetitorAILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetDecisionInterval());
            foreach (var comp in competitors)
            {
                RunCompetitorDecision(comp);
            }
            UpdateDemand();
            RefreshCompetitorsList();
        }
    }

    private float GetDecisionInterval()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy: return 10f;
            case DifficultyLevel.Normal: return 7f;
            case DifficultyLevel.Hard: return 5f;
            default: return 8f;
        }
    }

    private void RunCompetitorDecision(Competitor comp)
    {
        float decision = Random.value;

        // ---- Базовая логика ----
        if (comp.money < 200 && decision < 0.4f)
        {
            comp.priceMultiplier = Mathf.Min(comp.priceMultiplier + 0.1f, 1.5f);
            ShowNotification($"{comp.companyName} поднял цены!");
            return;
        }

        if (comp.marketShare < 0.2f && comp.money > 300 && decision < 0.5f)
        {
            comp.priceMultiplier = Mathf.Max(comp.priceMultiplier - 0.05f, 0.6f);
            comp.reputation += 10;
            comp.money -= 50;
            ShowNotification($"{comp.companyName} снизил цены для захвата рынка!");
            return;
        }

        if (comp.researchLevel < 5 && comp.money > 400 && decision < 0.6f)
        {
            List<Technology> availableTechs = technologies.Where(t => t != null && !t.isResearched && !comp.researchedTechs.Contains(t.techName) && t.priceModifier > 1f).ToList();
            if (availableTechs.Count > 0 && comp.money > 300)
            {
                Technology chosen = availableTechs[Random.Range(0, availableTechs.Count)];
                int cost = Mathf.RoundToInt(chosen.researchCost * 0.8f);
                if (comp.money >= cost)
                {
                    comp.money -= cost;
                    comp.researchedTechs.Add(chosen.techName);
                    comp.researchLevel++;
                    comp.reputation += 5;
                    comp.marketShare += 0.02f;
                    ShowNotification($"{comp.companyName} исследовал технологию '{chosen.techName}'!");
                    return;
                }
            }
            if (comp.money > 300)
            {
                comp.money -= 150;
                comp.factoryLevel++;
                ShowNotification($"{comp.companyName} модернизирует завод!");
                return;
            }
        }

        if (comp.money > 300 && decision < 0.7f)
        {
            comp.money -= 150;
            comp.factoryLevel++;
            ShowNotification($"{comp.companyName} модернизирует завод!");
            return;
        }

        // ---- НОВЫЕ ДЕЙСТВИЯ AI ----
        float actionChance = Random.value;

        if (!comp.isAlly && comp.money > 150 && actionChance < 0.15f)
        {
            float attackSuccess = Random.value;
            float attackChance = 0.2f + (comp.marketingPower * 0.03f);
            if (attackSuccess < attackChance)
            {
                int repLoss = Random.Range(5, 15);
                reputation = Mathf.Max(0, reputation - repLoss);
                ShowNotification($"{comp.companyName} провёл маркетинговую атаку! Ваша репутация -{repLoss}");
                UpdateUI();
            }
            comp.money -= 80;
            return;
        }

        if (!comp.isAlly && comp.money > 300 && actionChance < 0.08f)
        {
            float prSuccess = Random.value;
            float prChance = 0.1f + (comp.marketingPower * 0.02f);
            if (prSuccess < prChance)
            {
                int repLoss = Random.Range(15, 30);
                reputation = Mathf.Max(0, reputation - repLoss);
                ShowNotification($"{comp.companyName} запустил чёрный PR! Ваша репутация -{repLoss}");
                UpdateUI();
            }
            comp.money -= 150;
            return;
        }

        if (!comp.isAlly && comp.researchLevel > 2 && comp.money > 200 && actionChance < 0.10f)
        {
            List<string> playerTechs = new List<string>();
            foreach (var tech in technologies)
            {
                if (tech != null && tech.isResearched)
                    playerTechs.Add(tech.techName);
            }
            if (playerTechs.Count > 0)
            {
                float stealSuccess = Random.value;
                float stealChance = 0.1f + (comp.espionageLevel * 0.02f);
                if (stealSuccess < stealChance)
                {
                    string stolenTech = playerTechs[Random.Range(0, playerTechs.Count)];
                    if (!comp.researchedTechs.Contains(stolenTech))
                        comp.researchedTechs.Add(stolenTech);
                    comp.researchLevel++;
                    ShowNotification($"{comp.companyName} украл технологию {stolenTech}!");
                }
                comp.money -= 100;
                return;
            }
        }

        if (!comp.isAlly && comp.money > 100 && engineerCount > 0 && actionChance < 0.12f)
        {
            float poachSuccess = Random.value;
            float poachChance = 0.15f + (comp.marketingPower * 0.02f);
            if (poachSuccess < poachChance)
            {
                engineerCount = Mathf.Max(0, engineerCount - 1);
                comp.engineers++;
                passiveIncome -= 2;
                ShowNotification($"{comp.companyName} переманил одного из ваших инженеров!");
                UpdateUI();
            }
            comp.money -= 50;
            return;
        }
    }

    // ============================== ОКНО КОНКУРЕНТОВ (С ВЫПАДАЮЩИМ СПИСКОМ) ==============================
    private void OpenCompetitorsWindow()
    {
        if (competitorsOverlay != null)
        {
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(competitorsOverlay);
            RefreshCompetitorsList();
        }
    }

    private void CloseCompetitorsWindow()
    {
        if (competitorsOverlay != null)
        {
            AnimateWindowClose(competitorsOverlay, () =>
            {
                competitorsOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
        }
    }

    private void RefreshCompetitorsList()
    {
        if (competitorsContainer == null) return;
        competitorsContainer.Clear();

        // ---- Репутация игрока ----
        Label playerRepLabel = new Label($"Ваша репутация: {reputation}");
        playerRepLabel.style.color = Color.white;
        playerRepLabel.style.fontSize = 14;
        playerRepLabel.style.marginBottom = 10;
        competitorsContainer.Add(playerRepLabel);

        // Заголовок таблицы
        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        header.style.paddingTop = 5;
        header.style.paddingBottom = 5;
        header.style.paddingLeft = 10;
        header.style.paddingRight = 10;
        header.style.borderBottomWidth = 1;
        header.style.borderBottomColor = new StyleColor(Color.gray);

        string[] headers = { "Компания", "Деньги", "Репутация", "Доля рынка", "Цена", "Завод", "Иссл.", "Инж.", "Действие" };
        float colWidthPercent = 100f / headers.Length;

        foreach (var h in headers)
        {
            Label lbl = new Label(h);
            lbl.style.width = new Length(colWidthPercent, LengthUnit.Percent);
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(lbl);
        }
        competitorsContainer.Add(header);

        // Список опций для выпадающего меню
        List<string> actionOptions = new List<string>
        {
            "Выберите действие",
            "Маркетинговая атака",
            "Чёрный PR",
            "Предложить союз",
            "Украсть технологию",
            "Переманить инженера"
        };

        foreach (var comp in competitors)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 5;
            row.style.paddingBottom = 5;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            string[] values = {
                comp.companyName + (comp.isAlly ? " 🤝" : ""),
                $"${comp.money:F0}",
                comp.reputation.ToString(),
                $"{(comp.marketShare * 100):F1}%",
                $"{comp.priceMultiplier:F2}x",
                comp.factoryLevel.ToString(),
                comp.researchLevel.ToString(),
                comp.engineers.ToString()
            };

            foreach (var val in values)
            {
                Label lbl = new Label(val);
                lbl.style.width = new Length(colWidthPercent, LengthUnit.Percent);
                lbl.style.color = Color.white;
                lbl.style.fontSize = 12;
                row.Add(lbl);
            }

            // ---- Выпадающий список и кнопка выполнения ----
            VisualElement actionContainer = new VisualElement();
            actionContainer.style.flexDirection = FlexDirection.Row;
            actionContainer.style.width = new Length(colWidthPercent, LengthUnit.Percent);
            actionContainer.style.justifyContent = Justify.SpaceEvenly;
            actionContainer.style.alignItems = Align.Center;

            DropdownField actionDropdown = new DropdownField();
            actionDropdown.choices = actionOptions;
            actionDropdown.index = 0;
            actionDropdown.style.width = new Length(100, LengthUnit.Pixel);
            actionDropdown.style.height = 24;
            actionDropdown.style.fontSize = 11;

            Button executeBtn = new Button();
            executeBtn.text = "Выполнить";
            executeBtn.style.width = 60;
            executeBtn.style.height = 24;
            executeBtn.style.marginLeft = 4;

            Competitor localComp = comp;
            executeBtn.clicked += () =>
            {
                int selectedIndex = actionDropdown.index;
                if (selectedIndex <= 0)
                {
                    ShowNotification("Выберите действие!");
                    return;
                }
                switch (selectedIndex)
                {
                    case 1: PerformMarketingAttack(localComp); break;
                    case 2: PerformBlackPR(localComp); break;
                    case 3: ProposeAlliance(localComp); break;
                    case 4: StealTechnology(localComp); break;
                    case 5: PoachEngineer(localComp); break;
                    default: break;
                }
                actionDropdown.index = 0;
            };

            actionContainer.Add(actionDropdown);
            actionContainer.Add(executeBtn);
            row.Add(actionContainer);
            competitorsContainer.Add(row);
        }

        if (competitors.Count == 0)
        {
            Label empty = new Label("Нет конкурентов");
            empty.style.color = Color.white;
            empty.style.alignSelf = Align.Center;
            empty.style.marginTop = 20;
            competitorsContainer.Add(empty);
        }
    }

    // ============================== ДЕЙСТВИЯ ИГРОКА ПРОТИВ КОНКУРЕНТОВ ==============================
    private void PerformMarketingAttack(Competitor target)
    {
        if (target == null) return;
        if (target.isAlly) { ShowNotification("Нельзя атаковать союзника!"); return; }
        int cost = 100;
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        money -= cost;
        float success = Random.Range(0f, 1f);
        float chance = 0.3f + (engineerCount * 0.02f);
        if (success < chance)
        {
            int repLoss = Random.Range(10, 25);
            target.reputation = Mathf.Max(0, target.reputation - repLoss);
            target.marketShare *= (1f - 0.05f);
            ShowNotification($"Маркетинговая атака на {target.companyName} удалась! Репутация -{repLoss}");
            UpdateDemand();
            UpdateUI();
        }
        else
        {
            ShowNotification($"Маркетинговая атака на {target.companyName} провалилась.");
        }
        RefreshCompetitorsList();
    }

    private void PerformBlackPR(Competitor target)
    {
        if (target == null) return;
        if (target.isAlly) { ShowNotification("Нельзя атаковать союзника!"); return; }
        int cost = 200;
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        money -= cost;
        float success = Random.Range(0f, 1f);
        float chance = 0.2f + (engineerCount * 0.01f);
        if (success < chance)
        {
            int repLoss = Random.Range(20, 40);
            target.reputation = Mathf.Max(0, target.reputation - repLoss);
            target.marketShare *= (1f - 0.1f);
            ShowNotification($"Чёрный PR против {target.companyName} удался! Репутация -{repLoss}");
            if (Random.value < 0.3f)
            {
                int retaliation = Random.Range(10, 20);
                reputation = Mathf.Max(0, reputation - retaliation);
                ShowNotification($"{target.companyName} ответил! Ваша репутация -{retaliation}");
                UpdateUI();
            }
            UpdateDemand();
            UpdateUI();
        }
        else
        {
            ShowNotification($"Чёрный PR против {target.companyName} провалился.");
            if (Random.value < 0.2f)
            {
                int retaliation = Random.Range(5, 15);
                reputation = Mathf.Max(0, reputation - retaliation);
                ShowNotification($"{target.companyName} заметил и ответил! Ваша репутация -{retaliation}");
                UpdateUI();
            }
        }
        RefreshCompetitorsList();
    }

    private void ProposeAlliance(Competitor target)
    {
        if (target == null) return;
        if (target.isAlly) { ShowNotification("Уже в союзе!"); return; }
        int cost = 150;
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        float chance = 0.4f + (reputation / 200f);
        if (Random.value < chance)
        {
            money -= cost;
            target.isAlly = true;
            passiveIncome *= 1.2f;
            target.money *= 1.1f;
            ShowNotification($"Союз с {target.companyName} заключён! Доход увеличен на 20%.");
            UpdateUI();
        }
        else
        {
            ShowNotification($"{target.companyName} отклонил предложение о союзе.");
        }
        RefreshCompetitorsList();
    }

    private void BreakAlliance(Competitor target)
    {
        if (target == null) return;
        if (!target.isAlly) { ShowNotification("Нет союза для разрыва."); return; }
        target.isAlly = false;
        passiveIncome /= 1.2f;
        ShowNotification($"Союз с {target.companyName} разорван.");
        UpdateUI();
        RefreshCompetitorsList();
    }

    private void StealTechnology(Competitor target)
    {
        if (target == null) return;
        if (target.isAlly) { ShowNotification("Нельзя красть у союзника!"); return; }
        if (target.researchedTechs.Count == 0) { ShowNotification($"У {target.companyName} нет технологий для кражи."); return; }
        int cost = 250;
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        money -= cost;
        float success = Random.Range(0f, 1f);
        float chance = 0.2f + (engineerCount * 0.015f) + (target.espionageLevel * 0.01f);
        if (success < chance)
        {
            string techToSteal = target.researchedTechs[Random.Range(0, target.researchedTechs.Count)];
            Technology techToAdd = System.Array.Find(technologies, t => t != null && t.techName == techToSteal);
            if (techToAdd != null && !techToAdd.isResearched)
            {
                techToAdd.isResearched = true;
                RecalculateModifiers();
                UpdateCarPrices();
                ShowNotification($"Украдена технология {techToSteal} у {target.companyName}!");
            }
            else
            {
                double bonus = 500;
                money += bonus;
                ShowNotification($"Кража не дала технологии, но вы нашли {bonus} денег.");
            }
            target.reputation = Mathf.Max(0, target.reputation - 10);
            UpdateUI();
        }
        else
        {
            ShowNotification($"Попытка кражи технологии у {target.companyName} провалилась.");
            if (Random.value < 0.2f)
            {
                int repLoss = Random.Range(5, 15);
                reputation = Mathf.Max(0, reputation - repLoss);
                ShowNotification($"{target.companyName} обнаружил кражу! Ваша репутация -{repLoss}");
                UpdateUI();
            }
        }
        RefreshCompetitorsList();
    }

    private void PoachEngineer(Competitor target)
    {
        if (target == null) return;
        if (target.isAlly) { ShowNotification("Нельзя переманивать инженеров у союзника!"); return; }
        if (target.engineers <= 0) { ShowNotification($"У {target.companyName} нет инженеров для переманивания."); return; }
        int cost = 150;
        if (money < cost) { ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }

        money -= cost;
        float success = Random.Range(0f, 1f);
        float chance = 0.3f + (reputation * 0.002f) + (target.loyalty * 0.001f);
        if (success < chance)
        {
            target.engineers--;
            engineerCount++;
            passiveIncome += 2;
            ShowNotification($"Инженер переманен у {target.companyName}! Теперь у вас {engineerCount} инженеров.");
            UpdateUI();
        }
        else
        {
            ShowNotification($"Не удалось переманить инженера у {target.companyName}.");
            if (Random.value < 0.2f)
            {
                int repLoss = Random.Range(5, 15);
                reputation = Mathf.Max(0, reputation - repLoss);
                ShowNotification($"{target.companyName} разгневан! Ваша репутация -{repLoss}");
                UpdateUI();
            }
        }
        RefreshCompetitorsList();
    }

    // ============================== ПОСТРОЕНИЕ СПИСКА ДОСТУПНЫХ МАШИН ==============================
    private void BuildAvailableCars()
    {
        List<CarBlueprint> cars = new List<CarBlueprint>();
        if (startCars != null)
        {
            foreach (var car in startCars)
                if (car != null) cars.Add(car);
        }
        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech != null && tech.isResearched && tech.unlockedCar != null && tech.unlockCarOnResearch && !cars.Contains(tech.unlockedCar))
                {
                    cars.Add(tech.unlockedCar);
                }
            }
        }
        availableCars = cars.ToArray();
    }

    // ============================== ПРОВЕРКА ДОСТУПНОСТИ УЛУЧШЕНИЯ ==============================
    private bool IsCarUpgradeUnlocked()
    {
        if (string.IsNullOrEmpty(carUpgradeTechName)) return true;
        if (technologies == null) return false;
        foreach (var tech in technologies)
        {
            if (tech != null && tech.techName == carUpgradeTechName && tech.isResearched)
                return true;
        }
        return false;
    }

    // ============================== СОХРАНЕНИЕ / ЗАГРУЗКА / НОВАЯ ИГРА ==============================
    public void SaveGame()
    {
        try
        {
            SaveData data = new SaveData();
            data.money = money;
            data.passiveIncome = passiveIncome;
            data.conveyorLevel = conveyorLevel;
            data.engineerCount = engineerCount;
            data.productionCount = productionCount;
            data.currentDifficulty = (int)currentDifficulty;

            List<string> researched = new List<string>();
            if (technologies != null)
            {
                foreach (var tech in technologies)
                    if (tech != null && tech.isResearched) researched.Add(tech.techName);
            }
            data.researchedTechNames = researched.ToArray();

            data.carDemands = new List<CarDemandData>();
            List<CarBlueprint> allCars = GetAllPossibleCars();
            foreach (CarBlueprint car in allCars)
            {
                if (car != null)
                    data.carDemands.Add(new CarDemandData { carName = car.carName, demandMultiplier = car.demandMultiplier });
            }

            data.carLevels = new List<CarLevelData>();
            foreach (CarBlueprint car in allCars)
            {
                if (car != null)
                    data.carLevels.Add(new CarLevelData { carName = car.carName, currentLevel = car.currentLevel });
            }

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SaveFilePath, json);
            ShowNotification("Игра сохранена!");
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка сохранения: " + e.Message);
            ShowNotification("Ошибка сохранения!");
        }
    }

    public void LoadGame()
    {
        if (!File.Exists(SaveFilePath))
        {
            ShowNotification("Нет сохранений!");
            return;
        }

        try
        {
            string json = File.ReadAllText(SaveFilePath);
            SaveData data = JsonUtility.FromJson<SaveData>(json);
            if (data == null)
            {
                ShowNotification("Ошибка загрузки: файл повреждён!");
                return;
            }

            money = data.money;
            passiveIncome = data.passiveIncome;
            conveyorLevel = data.conveyorLevel;
            engineerCount = data.engineerCount;
            productionCount = data.productionCount;
            ClampProductionCount();

            if (technologies != null)
            {
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
            }

            RecalculateModifiers();

            List<CarBlueprint> allCarsForDemand = GetAllPossibleCars();
            foreach (CarDemandData d in data.carDemands)
            {
                CarBlueprint car = allCarsForDemand.FirstOrDefault(c => c != null && c.carName == d.carName);
                if (car != null) car.demandMultiplier = d.demandMultiplier;
            }
            foreach (CarLevelData lvl in data.carLevels)
            {
                CarBlueprint car = allCarsForDemand.FirstOrDefault(c => c != null && c.carName == lvl.carName);
                if (car != null) car.currentLevel = lvl.currentLevel;
            }

            currentDifficulty = (DifficultyLevel)data.currentDifficulty;
            ApplyDifficulty(currentDifficulty);
            UpdateSavedDifficultyLabel();

            BuildAvailableCars();

            if (isGameInitialized)
            {
                CreateCarButtons();
                CreateTechButtons();
                RefreshTechButtons();
                UpdateUI();
                UpdateUpgradeUI();
                CloseAllWindows();
                ShowNotification("Игра загружена!");
            }
            else
            {
                InitGame();
                ShowNotification("Игра загружена!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Ошибка загрузки: " + e.Message);
            ShowNotification("Ошибка загрузки!");
        }
    }

    public void NewGame()
    {
        if (File.Exists(SaveFilePath))
        {
            File.Delete(SaveFilePath);
            Debug.Log("Файл сохранения удалён.");
        }
        SceneManager.LoadScene("SampleScene");
    }

    // ============================== УПРАВЛЕНИЕ СЛОЖНОСТЬЮ ==============================
    private void SetDifficulty(DifficultyLevel level)
    {
        currentDifficulty = level;
        PlayerPrefs.SetInt("Difficulty", (int)level);
        PlayerPrefs.Save();
        ApplyDifficulty(level);
        UpdateSavedDifficultyLabel();
        ResetGameState();
    }

    private void ApplyDifficulty(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Easy:
                startMoney = 200f;
                costMultiplier = 0.8f;
                profitMultiplier = 1.2f;
                techCostMultiplier = 1f;
                StopEconomicEvents();
                break;
            case DifficultyLevel.Normal:
                startMoney = 100f;
                costMultiplier = 1f;
                profitMultiplier = 1f;
                techCostMultiplier = 1f;
                StopEconomicEvents();
                break;
            case DifficultyLevel.Hard:
                startMoney = 50f;
                costMultiplier = 1.5f;
                profitMultiplier = 0.8f;
                techCostMultiplier = 5f;
                StartEconomicEvents();
                break;
        }
    }

    private void ResetGameState()
    {
        money = startMoney;
        passiveIncome = 0;
        conveyorLevel = 0;
        engineerCount = 0;
        productionCount = 1;
        ClampProductionCount();
        isProductionInProgress = false;
        reputation = 50;
        if (technologies != null)
        {
            foreach (var tech in technologies) if (tech != null) tech.isResearched = false;
        }
        foreach (var comp in competitors)
        {
            comp.isAlly = false;
            comp.engineers = Random.Range(1, 5);
            comp.espionageLevel = Random.Range(1, 6);
            comp.marketingPower = Random.Range(1, 6);
            comp.loyalty = Random.Range(50, 100);
            comp.stolenTechs.Clear();
        }
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (var car in allCars) { if (car != null) { car.demandMultiplier = 1f; car.tuningPower = 0; car.tuningEconomy = 0; car.tuningDesign = 0; car.tuningSafety = 0; } }
        currentEventMultiplier = 1f;
        currentEventText = "";
        UpdateEventUI();
        RecalculateModifiers();
        BuildAvailableCars();
        CreateCarButtons();
        CreateTechButtons();
        UpdateUI();
        UpdateUpgradeUI();
        CloseAllWindows();
        ShowNotification($"Сложность изменена на {currentDifficulty}");
    }

    // ============================== ЭКОНОМИЧЕСКИЕ СОБЫТИЯ (HARD) ==============================
    private void StartEconomicEvents()
    {
        if (eventCoroutine != null)
            StopCoroutine(eventCoroutine);
        eventCoroutine = StartCoroutine(EconomicEvents());
    }

    private void StopEconomicEvents()
    {
        if (eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }
        currentEventMultiplier = 1f;
        currentEventText = "";
        UpdateEventUI();
    }

    private IEnumerator EconomicEvents()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);

            float multiplier = Random.Range(0.5f, 2.0f);
            multiplier = Mathf.Round(multiplier * 10f) / 10f;

            string text;
            if (multiplier < 0.8f)
                text = $"⚠️ Кризис! Спрос упал на {(1f - multiplier) * 100:F0}%";
            else if (multiplier > 1.2f)
                text = $"🚀 Бум! Спрос вырос на {(multiplier - 1f) * 100:F0}%";
            else
                text = $"📊 Спрос стабилен ({multiplier:F1}x)";

            currentEventMultiplier = multiplier;
            currentEventText = text;
            UpdateEventUI();
            UpdateCarCards();
        }
    }

    private void UpdateEventUI()
    {
        if (eventLabel != null)
        {
            eventLabel.text = currentEventText;
            if (currentEventMultiplier < 0.8f)
                eventLabel.style.color = new StyleColor(Color.red);
            else if (currentEventMultiplier > 1.2f)
                eventLabel.style.color = new StyleColor(Color.green);
            else
                eventLabel.style.color = new StyleColor(Color.white);
        }
    }

    // ============================== СПРОС ==============================
    private void UpdateDemand()
    {
        if (MarketSystem.Instance == null) return;

        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (CarBlueprint car in allCars)
        {
            if (car == null) continue;
            float min, max;
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: min = 0.9f; max = 1.1f; break;
                case DifficultyLevel.Normal: min = 0.7f; max = 1.3f; break;
                case DifficultyLevel.Hard: min = 0.5f; max = 1.8f; break;
                default: min = 0.8f; max = 1.2f; break;
            }
            float baseDemand = Random.Range(min, max);

            float competitorFactor = 1f;
            foreach (var comp in competitors)
            {
                if (comp.availableCars.Contains(car))
                {
                    competitorFactor *= (1f - comp.marketShare * 0.5f);
                    float priceEffect = 1f - (comp.priceMultiplier - 0.8f) * 0.2f;
                    competitorFactor *= Mathf.Clamp(priceEffect, 0.5f, 1.2f);
                }
            }

            float playerTechDemandModifier = totalDemandModifier;
            float competitorTechDemandModifier = 1f;
            foreach (var comp in competitors)
                competitorTechDemandModifier *= (1f - comp.researchLevel * 0.02f);

            float baseWithTech = baseDemand * currentEventMultiplier * competitorFactor * playerTechDemandModifier * competitorTechDemandModifier;
            float tuningDemandModifier = car.GetTuningDemandModifier();
            float finalDemand = MarketSystem.Instance.GetDemandMultiplier(car, baseWithTech * tuningDemandModifier);
            car.demandMultiplier = finalDemand;
        }
        UpdateCarCards();
    }

    // ============================== ОБНОВЛЕНИЕ ЦЕН МАШИН ==============================
    private void UpdateCarPrices()
    {
        CreateCarButtons();
        UpdateUI();
    }

    // ============================== УПРАВЛЕНИЕ КОЛИЧЕСТВОМ ==============================
    private void DecreaseCount()
    {
        if (productionCount > 1) productionCount--;
        UpdateCountLabel();
        UpdateProductionButtons();
    }

    private void IncreaseCount()
    {
        int max = IsBulkProductionUnlocked() ? 10 : 1;
        if (productionCount >= max)
        {
            if (max == 1)
                ShowNotification($"Для производства более 1 машины за раз изучите технологию '{bulkProductionTechName}'");
            else
                ShowNotification("Достигнут максимум (10)");
            return;
        }
        productionCount++;
        UpdateCountLabel();
        UpdateProductionButtons();
    }

    private void UpdateCountLabel()
    {
        if (countLabel != null)
        {
            countLabel.text = productionCount.ToString();
            countLabel.MarkDirtyRepaint();
        }
    }

    private bool IsBulkProductionUnlocked()
    {
        if (string.IsNullOrEmpty(bulkProductionTechName)) return true;
        if (technologies == null) return false;
        foreach (var tech in technologies)
        {
            if (tech != null && tech.techName == bulkProductionTechName && tech.isResearched)
                return true;
        }
        return false;
    }

    private void UpdateProductionButtons()
    {
        var root = uiDoc?.rootVisualElement;
        if (root == null) return;
        Button increaseButton = root.Q<Button>("IncreaseCountButton");
        if (increaseButton != null)
        {
            int max = IsBulkProductionUnlocked() ? 10 : 1;
            increaseButton.SetEnabled(productionCount < max);
        }
    }

    private void ClampProductionCount()
    {
        int max = IsBulkProductionUnlocked() ? 10 : 1;
        if (productionCount > max) productionCount = max;
        UpdateCountLabel();
        UpdateProductionButtons();
    }

    // ============================== ОТКРЫТИЕ/ЗАКРЫТИЕ ОКОН ==============================
    private void OpenCarsWindow()
    {
        if (carsOverlay != null)
        {
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(carsOverlay);
            UpdateCountLabel();
            UpdateCarCards();
        }
    }
    private void CloseCarsWindow()
    {
        if (carsOverlay != null)
        {
            AnimateWindowClose(carsOverlay, () =>
            {
                carsOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
        }
    }

    private void OpenTechWindow()
    {
        if (techOverlay != null)
        {
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(techOverlay);
            RefreshTechButtons();
        }
    }
    private void CloseTechWindow()
    {
        if (techOverlay != null)
        {
            AnimateWindowClose(techOverlay, () =>
            {
                techOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
        }
    }

    private void OpenUpgradeWindow()
    {
        if (upgradeOverlay != null)
        {
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(upgradeOverlay);
            UpdateUpgradeUI();
        }
    }
    private void CloseUpgradeWindow()
    {
        if (upgradeOverlay != null)
        {
            AnimateWindowClose(upgradeOverlay, () =>
            {
                upgradeOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
        }
    }

    private void OpenSettingsWindow()
    {
        if (settingsOverlay != null)
        {
            if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(settingsOverlay);
        }
    }
    private void CloseSettingsWindow()
    {
        if (settingsOverlay != null)
        {
            AnimateWindowClose(settingsOverlay, () =>
            {
                settingsOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
        }
    }

    private void CloseAllWindows()
    {
        CloseCarsWindow();
        CloseTechWindow();
        CloseUpgradeWindow();
        CloseSettingsWindow();
        CloseCompetitorsWindow();
        if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
    }

    private void HideAllOverlays()
    {
        if (carsOverlay != null) carsOverlay.style.display = DisplayStyle.None;
        if (techOverlay != null) techOverlay.style.display = DisplayStyle.None;
        if (upgradeOverlay != null) upgradeOverlay.style.display = DisplayStyle.None;
        if (settingsOverlay != null) settingsOverlay.style.display = DisplayStyle.None;
        if (competitorsOverlay != null) competitorsOverlay.style.display = DisplayStyle.None;
        if (welcomeOverlay != null) welcomeOverlay.style.display = DisplayStyle.None;
    }

    // ============================== ОБНОВЛЕНИЕ UI ==============================
    private void UpdateSavedDifficultyLabel()
    {
        if (savedDifficultyLabel != null)
            savedDifficultyLabel.text = $"Сложность: {currentDifficulty}";
    }

    private void UpdateUpgradeUI()
    {
        if (conveyorLevelLabel != null)
            conveyorLevelLabel.text = $"Уровень конвейера: {conveyorLevel}";
        if (engineerCountLabel != null)
            engineerCountLabel.text = $"Инженеров: {engineerCount}";
        if (buyConveyorButton != null)
        {
            int cost = Mathf.RoundToInt((10 + conveyorLevel * 5) * costMultiplier);
            buyConveyorButton.text = $"Улучшить конвейер ({cost})";
        }
        if (hireEngineerButton != null)
        {
            int cost = Mathf.RoundToInt((50 + engineerCount * 20) * costMultiplier);
            hireEngineerButton.text = $"Нанять инженера ({cost})";
        }
    }

    public void ShowNotification(string message)
    {
        if (notificationContainer == null) return;
        Label notificationLabel = new Label();
        notificationLabel.text = message;
        notificationLabel.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f, 0.9f));
        notificationLabel.style.color = Color.white;
        notificationLabel.style.paddingTop = 10;
        notificationLabel.style.paddingBottom = 10;
        notificationLabel.style.paddingLeft = 15;
        notificationLabel.style.paddingRight = 15;
        notificationLabel.style.marginBottom = 5;
        notificationLabel.style.fontSize = 14;
        notificationLabel.style.borderTopLeftRadius = 5;
        notificationLabel.style.borderTopRightRadius = 5;
        notificationLabel.style.borderBottomLeftRadius = 5;
        notificationLabel.style.borderBottomRightRadius = 5;
        notificationLabel.style.whiteSpace = WhiteSpace.Normal;
        notificationLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
        notificationContainer.Add(notificationLabel);
        uiDoc.rootVisualElement.schedule.Execute(() =>
        {
            if (notificationLabel.parent != null)
                notificationLabel.RemoveFromHierarchy();
        }).ExecuteLater(3000);
    }

    // ============================== УЛУЧШЕНИЕ МАШИНЫ (LEVEL) ==============================
    public void UpgradeCar(CarBlueprint car)
    {
        if (car == null)
        {
            ShowNotification("Ошибка: машина не найдена!");
            return;
        }

        if (!IsCarUpgradeUnlocked())
        {
            ShowNotification($"Для улучшения машин изучите технологию '{carUpgradeTechName}'");
            return;
        }

        if (car.levelPrefabs == null || car.levelPrefabs.Length == 0)
        {
            ShowNotification("Эту машину нельзя улучшить!");
            return;
        }

        int maxLevel = car.levelPrefabs.Length - 1;
        if (car.currentLevel >= maxLevel)
        {
            ShowNotification("Машина уже максимально улучшена!");
            return;
        }

        int cost = 100 * (car.currentLevel + 1);
        if (money < cost)
        {
            ShowNotification($"Не хватает денег! Нужно ещё ${cost - money:F0}");
            return;
        }

        money -= cost;
        car.currentLevel++;
        ShowNotification($"Машина {car.carName} улучшена до уровня {car.currentLevel + 1}!");

        CreateCarButtons();
        UpdateUI();
    }

    // ============================== КНОПКИ МАШИН (С ЗАГРУЗКОЙ ИКОНОК) ==============================
    private void CreateCarButtons()
    {
        if (carsContainer == null) return;
        carsContainer.Clear();
        carCards.Clear();

        bool upgradeUnlocked = IsCarUpgradeUnlocked();

        foreach (CarBlueprint car in availableCars)
        {
            if (car == null) continue;

            VisualElement card = new VisualElement();
            card.AddToClassList("car-card");
            card.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.paddingLeft = 15;
            card.style.paddingRight = 15;
            card.style.marginBottom = 8;
            card.style.flexDirection = FlexDirection.Column;
            card.style.alignItems = Align.Stretch;

            // ---- Верхняя часть: иконка + текст ----
            VisualElement topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.alignItems = Align.Center;

            // Загрузка иконки из поля car.carIcon или из Resources
            Sprite iconSprite = car.carIcon != null ? car.carIcon : LoadCarIcon(car.carName);
            if (iconSprite != null)
            {
                Image iconImage = new Image();
                iconImage.sprite = iconSprite;
                iconImage.style.width = 60;
                iconImage.style.height = 60;
                iconImage.style.marginRight = 15;
                topRow.Add(iconImage);
            }
            else
            {
                // Заглушка, если иконки нет
                VisualElement placeholder = new VisualElement();
                placeholder.style.width = 60;
                placeholder.style.height = 60;
                placeholder.style.marginRight = 15;
                placeholder.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                topRow.Add(placeholder);
            }

            VisualElement textContainer = new VisualElement();
            textContainer.style.flexDirection = FlexDirection.Column;
            textContainer.style.flexGrow = 1;

            Label nameLabel = new Label(car.carName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            int modPrice = car.GetModifiedPrice(totalPriceModifier);
            int modCost = car.GetModifiedProductionCost(totalCostModifier);
            Label detailsLabel = new Label($"Цена: ${modPrice}  |  Себ: ${modCost}");
            detailsLabel.style.fontSize = 13;
            detailsLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

            double profit = modPrice - modCost;
            Label profitLabel = new Label($"Прибыль: {profit:F0}");
            profitLabel.style.fontSize = 14;
            profitLabel.style.color = profit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
            profitLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            Label demandLabel = new Label($"Спрос: {car.demandMultiplier:F1}x");
            demandLabel.style.fontSize = 12;
            demandLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

            Label levelLabel = new Label($"Уровень: {car.currentLevel + 1}");
            levelLabel.style.fontSize = 12;
            levelLabel.style.color = new Color(0.6f, 0.8f, 1f);

            Label trendLabel = new Label("тренд: ...");
            trendLabel.style.fontSize = 11;
            trendLabel.style.color = Color.gray;

            textContainer.Add(nameLabel);
            textContainer.Add(detailsLabel);
            textContainer.Add(profitLabel);
            textContainer.Add(demandLabel);
            textContainer.Add(levelLabel);
            textContainer.Add(trendLabel);

            topRow.Add(textContainer);

            // Кнопка улучшения уровня машины
            Button upgradeButton = new Button();
            upgradeButton.text = "Улучшить";
            upgradeButton.style.width = 80;
            upgradeButton.style.height = 30;
            upgradeButton.style.marginLeft = 10;
            upgradeButton.style.alignSelf = Align.Center;

            bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
            upgradeButton.SetEnabled(canUpgrade);

            CarBlueprint localCar = car;
            upgradeButton.clicked += () => UpgradeCar(localCar);

            topRow.Add(upgradeButton);
            card.Add(topRow);

            // ---- Панель тюнинга ----
            VisualElement tuningPanel = new VisualElement();
            tuningPanel.style.flexDirection = FlexDirection.Row;
            tuningPanel.style.flexWrap = Wrap.Wrap;
            tuningPanel.style.marginTop = 5;
            tuningPanel.style.marginBottom = 5;
            tuningPanel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            tuningPanel.style.paddingTop = 5;
            tuningPanel.style.paddingBottom = 5;
            tuningPanel.style.paddingLeft = 5;
            tuningPanel.style.paddingRight = 5;
            tuningPanel.style.borderTopLeftRadius = 4;
            tuningPanel.style.borderTopRightRadius = 4;
            tuningPanel.style.borderBottomLeftRadius = 4;
            tuningPanel.style.borderBottomRightRadius = 4;

            for (int i = 0; i < tuningParamNames.Length; i++)
            {
                string param = tuningParamNames[i];
                string display = tuningParamDisplay[i];

                VisualElement paramContainer = new VisualElement();
                paramContainer.style.flexDirection = FlexDirection.Row;
                paramContainer.style.alignItems = Align.Center;
                paramContainer.style.marginRight = 8;

                Label nameLabelParam = new Label(display);
                nameLabelParam.style.color = Color.white;
                nameLabelParam.style.fontSize = 10;
                nameLabelParam.style.width = 50;

                Label valueLabel = new Label("0");
                valueLabel.style.color = Color.white;
                valueLabel.style.fontSize = 12;
                valueLabel.style.width = 20;
                valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;

                Button upgradeBtn = new Button();
                upgradeBtn.text = "↑";
                upgradeBtn.style.width = 24;
                upgradeBtn.style.height = 24;
                upgradeBtn.style.marginLeft = 2;
                upgradeBtn.style.marginRight = 2;
                upgradeBtn.style.fontSize = 12;
                upgradeBtn.style.unityFontStyleAndWeight = FontStyle.Bold;

                Label costLabel = new Label("$0");
                costLabel.style.color = Color.yellow;
                costLabel.style.fontSize = 9;
                costLabel.style.width = 30;

                paramContainer.Add(nameLabelParam);
                paramContainer.Add(valueLabel);
                paramContainer.Add(upgradeBtn);
                paramContainer.Add(costLabel);

                tuningPanel.Add(paramContainer);

                CarBlueprint localCarTuning = car;
                string localParam = param;
                upgradeBtn.clicked += () => UpgradeTuning(localCarTuning, localParam);
            }

            card.Add(tuningPanel);

            // ---- График спроса ----
            VisualElement graphContainer = new VisualElement();
            graphContainer.style.width = new Length(100, LengthUnit.Percent);
            graphContainer.style.height = new Length(60, LengthUnit.Pixel);
            graphContainer.style.marginTop = 5;
            card.Add(graphContainer);

            CarCardData cardData = new CarCardData();
            cardData.car = car;
            cardData.card = card;
            cardData.profitLabel = profitLabel;
            cardData.demandLabel = demandLabel;
            cardData.trendLabel = trendLabel;
            cardData.graphContainer = graphContainer;
            cardData.upgradeButton = upgradeButton;

            if (tuningPanel != null)
            {
                var children = tuningPanel.Children().ToList();
                for (int i = 0; i < tuningParamNames.Length && i < children.Count; i++)
                {
                    var container = children[i] as VisualElement;
                    if (container != null)
                    {
                        var labels = container.Children().OfType<Label>().ToList();
                        var buttons = container.Children().OfType<Button>().ToList();
                        if (labels.Count >= 2 && buttons.Count >= 1)
                        {
                            Label valLabel = labels[1];
                            Label costLabel = labels.Count > 2 ? labels[2] : null;
                            Button btn = buttons[0];
                            switch (i)
                            {
                                case 0: cardData.powerLabel = valLabel; cardData.powerUpgradeBtn = btn; cardData.powerCostLabel = costLabel; break;
                                case 1: cardData.economyLabel = valLabel; cardData.economyUpgradeBtn = btn; cardData.economyCostLabel = costLabel; break;
                                case 2: cardData.designLabel = valLabel; cardData.designUpgradeBtn = btn; cardData.designCostLabel = costLabel; break;
                                case 3: cardData.safetyLabel = valLabel; cardData.safetyUpgradeBtn = btn; cardData.safetyCostLabel = costLabel; break;
                            }
                        }
                    }
                }
            }

            carCards.Add(cardData);

            CarBlueprint localCarForProduction = car;
            card.RegisterCallback<ClickEvent>(evt => ProduceSpecificCar(localCarForProduction));

            carsContainer.Add(card);
        }
        UpdateCarCards();
    }

    private void UpdateCarCards()
    {
        bool upgradeUnlocked = IsCarUpgradeUnlocked();

        foreach (var cardData in carCards)
        {
            if (cardData.car == null) continue;
            CarBlueprint car = cardData.car;

            int modPrice = car.GetModifiedPrice(totalPriceModifier);
            int modCost = car.GetModifiedProductionCost(totalCostModifier);
            double profit = modPrice - modCost;
            if (cardData.profitLabel != null)
            {
                cardData.profitLabel.text = $"Прибыль: {profit:F0}";
                cardData.profitLabel.style.color = profit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
            }

            float demand = car.demandMultiplier;
            if (cardData.demandLabel != null)
            {
                cardData.demandLabel.text = $"Спрос: {demand:F1}x";
                cardData.demandLabel.style.color = demand > 1.2f ? Color.green : (demand < 0.8f ? Color.red : Color.yellow);
            }

            if (cardData.upgradeButton != null)
            {
                bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
                cardData.upgradeButton.SetEnabled(canUpgrade);
                cardData.upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            }

            if (cardData.trendLabel != null && MarketSystem.Instance != null)
            {
                string trend = MarketSystem.Instance.GetDemandTrend(car.carName);
                cardData.trendLabel.text = $"Тренд: {trend}";
                if (trend.Contains("растёт"))
                    cardData.trendLabel.style.color = Color.green;
                else if (trend.Contains("падает"))
                    cardData.trendLabel.style.color = Color.red;
                else
                    cardData.trendLabel.style.color = Color.gray;
            }

            if (cardData.graphContainer != null && MarketSystem.Instance != null)
            {
                MarketSystem.Instance.DrawDemandGraph(cardData.graphContainer, car.carName);
            }

            UpdateTuningUI(cardData);
        }
    }

    private void UpdateTuningUI(CarCardData cardData)
    {
        CarBlueprint car = cardData.car;
        if (car == null) return;

        if (cardData.powerLabel != null) cardData.powerLabel.text = car.tuningPower.ToString();
        if (cardData.powerUpgradeBtn != null)
        {
            bool can = CanUpgradeTuning(car, "power");
            cardData.powerUpgradeBtn.SetEnabled(can);
            if (cardData.powerCostLabel != null)
            {
                if (car.tuningPower < TUNING_MAX_LEVEL)
                {
                    Technology tech = GetTuningTech("power", car.tuningPower + 1);
                    int cost = tech != null ? Mathf.RoundToInt(tech.researchCost * techCostMultiplier) : 0;
                    cardData.powerCostLabel.text = $"${cost}";
                }
                else
                {
                    cardData.powerCostLabel.text = "MAX";
                }
            }
        }

        if (cardData.economyLabel != null) cardData.economyLabel.text = car.tuningEconomy.ToString();
        if (cardData.economyUpgradeBtn != null)
        {
            bool can = CanUpgradeTuning(car, "economy");
            cardData.economyUpgradeBtn.SetEnabled(can);
            if (cardData.economyCostLabel != null)
            {
                if (car.tuningEconomy < TUNING_MAX_LEVEL)
                {
                    Technology tech = GetTuningTech("economy", car.tuningEconomy + 1);
                    int cost = tech != null ? Mathf.RoundToInt(tech.researchCost * techCostMultiplier) : 0;
                    cardData.economyCostLabel.text = $"${cost}";
                }
                else
                {
                    cardData.economyCostLabel.text = "MAX";
                }
            }
        }

        if (cardData.designLabel != null) cardData.designLabel.text = car.tuningDesign.ToString();
        if (cardData.designUpgradeBtn != null)
        {
            bool can = CanUpgradeTuning(car, "design");
            cardData.designUpgradeBtn.SetEnabled(can);
            if (cardData.designCostLabel != null)
            {
                if (car.tuningDesign < TUNING_MAX_LEVEL)
                {
                    Technology tech = GetTuningTech("design", car.tuningDesign + 1);
                    int cost = tech != null ? Mathf.RoundToInt(tech.researchCost * techCostMultiplier) : 0;
                    cardData.designCostLabel.text = $"${cost}";
                }
                else
                {
                    cardData.designCostLabel.text = "MAX";
                }
            }
        }

        if (cardData.safetyLabel != null) cardData.safetyLabel.text = car.tuningSafety.ToString();
        if (cardData.safetyUpgradeBtn != null)
        {
            bool can = CanUpgradeTuning(car, "safety");
            cardData.safetyUpgradeBtn.SetEnabled(can);
            if (cardData.safetyCostLabel != null)
            {
                if (car.tuningSafety < TUNING_MAX_LEVEL)
                {
                    Technology tech = GetTuningTech("safety", car.tuningSafety + 1);
                    int cost = tech != null ? Mathf.RoundToInt(tech.researchCost * techCostMultiplier) : 0;
                    cardData.safetyCostLabel.text = $"${cost}";
                }
                else
                {
                    cardData.safetyCostLabel.text = "MAX";
                }
            }
        }
    }

    // ============================== ПРОИЗВОДСТВО ==============================
    private void SpawnCar(CarBlueprint car)
    {
        if (car == null)
        {
            ShowNotification("Ошибка: нет данных о машине!");
            return;
        }

        GameObject prefabToSpawn = null;

        if (car.levelPrefabs != null && car.levelPrefabs.Length > 0)
        {
            int level = Mathf.Clamp(car.currentLevel, 0, car.levelPrefabs.Length - 1);
            prefabToSpawn = car.levelPrefabs[level];
        }

        if (prefabToSpawn == null)
            prefabToSpawn = car.carPrefab;

        if (prefabToSpawn == null)
        {
            string prefabPath = $"Prefabs/{car.carName}";
            prefabToSpawn = Resources.Load<GameObject>(prefabPath);
            if (prefabToSpawn != null)
                Debug.Log($"Загружен префаб для {car.carName} из {prefabPath}");
        }

        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"У машины {car.carName} нет префаба и не найден в Resources/Prefabs! Создаю временный куб.");
            prefabToSpawn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(prefabToSpawn.GetComponent<Collider>());
            prefabToSpawn.name = "TempCar";
        }

        if (isProductionInProgress)
        {
            ShowNotification("Производство занято! Дождитесь завершения.");
            return;
        }

        if (CarDisplay == null)
        {
            GameObject container = new GameObject("CarDisplay");
            container.transform.parent = transform;
            CarDisplay = container;
            Debug.Log("Создан автоматический контейнер CarDisplay в SpawnCar.");
        }

        if (startPoint == null)
        {
            GameObject start = new GameObject("StartPoint");
            start.transform.parent = CarDisplay.transform;
            start.transform.localPosition = new Vector3(-3, 0, 0);
            startPoint = start.transform;
            Debug.Log("Создана автоматическая точка старта в SpawnCar.");
        }
        if (endPoint == null)
        {
            GameObject end = new GameObject("EndPoint");
            end.transform.parent = CarDisplay.transform;
            end.transform.localPosition = new Vector3(3, 0, 0);
            endPoint = end.transform;
            Debug.Log("Создана автоматическая точка финиша в SpawnCar.");
        }

        if (currentCarInstance != null)
            Destroy(currentCarInstance);

        isProductionInProgress = true;

        currentCarInstance = Instantiate(prefabToSpawn, CarDisplay.transform);
        currentCarInstance.transform.localPosition = Vector3.zero;
        currentCarInstance.transform.localRotation = Quaternion.identity;
        currentCarInstance.transform.localScale = Vector3.one;

        CarAnimation anim = currentCarInstance.GetComponent<CarAnimation>();
        if (anim == null)
            anim = currentCarInstance.AddComponent<CarAnimation>();
        anim.startPoint = startPoint;
        anim.endPoint = endPoint;
        anim.duration = 2f;

        anim.OnProductionComplete += () =>
        {
            isProductionInProgress = false;
        };

        anim.PlayProduction();
    }

    public void ProduceBasicCar()
    {
        if (isProductionInProgress)
        {
            ShowNotification("Производство занято! Дождитесь завершения.");
            return;
        }

        if (availableCars == null || availableCars.Length == 0)
        {
            ShowNotification("Нет доступных машин для производства!");
            return;
        }

        CarBlueprint car = availableCars[0];
        if (car == null) return;
        int modPrice = car.GetModifiedPrice(totalPriceModifier);
        int modCost = car.GetModifiedProductionCost(totalCostModifier);
        double profit = (modPrice - modCost) * profitMultiplier;
        money += profit;
        SpawnCar(car);
        UpdateUI();
    }

    private void ProduceSpecificCar(CarBlueprint car)
    {
        if (isProductionInProgress)
        {
            ShowNotification("Производство занято! Дождитесь завершения.");
            return;
        }

        if (car == null) return;

        int modPrice = car.GetModifiedPrice(totalPriceModifier);
        int modCost = car.GetModifiedProductionCost(totalCostModifier);
        double profit = modPrice - modCost;
        double totalProfit = profit * productionCount * profitMultiplier;
        money += totalProfit;
        SpawnCar(car);
        UpdateUI();
        ShowNotification($"Произведено {productionCount} шт. {car.carName}, прибыль: ${totalProfit:F0}");
    }

    // ============================== УЛУЧШЕНИЯ (ЗАВОД) ==============================
    public void BuyConveyorUpgrade()
    {
        int cost = Mathf.RoundToInt((10 + conveyorLevel * 5) * costMultiplier);
        if (money >= cost)
        {
            money -= cost;
            conveyorLevel++;
            passiveIncome += 0.5;
            UpdateUI();
            UpdateUpgradeUI();
            ShowNotification($"Конвейер улучшен до {conveyorLevel} уровня!");
        }
        else
        {
            double missing = cost - money;
            ShowNotification($"Не хватает денег для улучшения конвейера! Нужно ещё ${missing:F0}");
        }
    }

    public void HireEngineer()
    {
        int cost = Mathf.RoundToInt((50 + engineerCount * 20) * costMultiplier);
        if (money >= cost)
        {
            money -= cost;
            engineerCount++;
            passiveIncome += 2;
            UpdateUI();
            UpdateUpgradeUI();
            ShowNotification($"Нанят инженер! Всего: {engineerCount}");
        }
        else
        {
            double missing = cost - money;
            ShowNotification($"Не хватает денег для найма инженера! Нужно ещё ${missing:F0}");
        }
    }

    // ============================== ПАССИВНЫЙ ДОХОД ==============================
    private IEnumerator PassiveIncome()
    {
        float demandTimer = 0f;
        while (true)
        {
            yield return new WaitForSeconds(1f);
            money += passiveIncome;
            UpdateUI();

            demandTimer += 1f;
            if (demandTimer >= demandUpdateInterval)
            {
                demandTimer = 0f;
                UpdateDemand();
            }
        }
    }

    // ============================== ДЕРЕВО ТЕХНОЛОГИЙ ==============================
    private void CreateTechButtons()
    {
        if (techScrollView == null) return;
        techScrollView.Clear();

        if (technologies == null || technologies.Length == 0)
        {
            Label emptyLabel = new Label("Нет доступных технологий");
            emptyLabel.style.color = Color.white;
            techScrollView.Add(emptyLabel);
            return;
        }

        Dictionary<Technology, int> techLevels = new Dictionary<Technology, int>();
        foreach (var tech in technologies)
        {
            if (tech != null)
                techLevels[tech] = CalculateTechLevel(tech);
        }

        if (techLevels.Count == 0) return;

        int maxLevel = techLevels.Values.Max();
        Dictionary<int, List<Technology>> levelGroups = new Dictionary<int, List<Technology>>();
        for (int i = 0; i <= maxLevel; i++)
            levelGroups[i] = new List<Technology>();

        foreach (var kvp in techLevels)
            levelGroups[kvp.Value].Add(kvp.Key);

        int totalWidth = (maxLevel + 1) * 300 + 100;
        int totalHeight = Mathf.Max(600, technologies.Length * 100 + 100);
        techGraphRoot = new VisualElement();
        techGraphRoot.style.width = new Length(totalWidth, LengthUnit.Pixel);
        techGraphRoot.style.height = new Length(totalHeight, LengthUnit.Pixel);
        techGraphRoot.style.position = Position.Relative;
        techScrollView.Add(techGraphRoot);

        const float nodeWidth = 200;
        const float nodeHeight = 80;
        const float horizontalGap = 100;
        const float verticalGap = 60;

        Dictionary<Technology, TechNode> nodeMap = new Dictionary<Technology, TechNode>();
        techNodes.Clear();

        foreach (var tech in technologies)
        {
            if (tech == null) continue;
            TechNode node = new TechNode();
            node.tech = tech;
            node.element = CreateTechNodeElement(tech);
            nodeMap[tech] = node;
            techNodes.Add(node);
        }

        foreach (var tech in technologies)
        {
            if (tech == null) continue;
            TechNode childNode = nodeMap[tech];
            if (tech.requiredTechNames != null)
            {
                foreach (string parentName in tech.requiredTechNames)
                {
                    Technology parentTech = System.Array.Find(technologies, t => t != null && t.techName == parentName);
                    if (parentTech != null && nodeMap.ContainsKey(parentTech))
                    {
                        TechNode parentNode = nodeMap[parentTech];
                        childNode.parents.Add(parentNode);
                        parentNode.children.Add(childNode);
                    }
                }
            }
        }

        float containerHeight = totalHeight;
        foreach (var kvp in levelGroups)
        {
            int level = kvp.Key;
            var techList = kvp.Value;
            int count = techList.Count;
            for (int i = 0; i < count; i++)
            {
                Technology tech = techList[i];
                if (tech == null || !nodeMap.ContainsKey(tech)) continue;
                TechNode node = nodeMap[tech];
                float x = level * (nodeWidth + horizontalGap) + 20;
                float totalNodeHeight = count * (nodeHeight + verticalGap) - verticalGap;
                float y = (i * (nodeHeight + verticalGap)) + (containerHeight - totalNodeHeight) / 2;
                if (y < 0) y = 10;
                node.position = new Vector2(x, y);
                node.element.style.position = Position.Absolute;
                node.element.style.left = x;
                node.element.style.top = y;
            }
        }

        VisualElement lineLayer = new VisualElement();
        lineLayer.style.position = Position.Absolute;
        lineLayer.style.left = 0;
        lineLayer.style.top = 0;
        lineLayer.style.right = 0;
        lineLayer.style.bottom = 0;
        techGraphRoot.Add(lineLayer);
        foreach (var node in techNodes)
            if (node != null && node.element != null)
                techGraphRoot.Add(node.element);

        lineLayer.generateVisualContent += (meshGenerationContext) =>
        {
            var rect = lineLayer.contentRect;
            if (rect.width < 1 || rect.height < 1) return;
            var painter = meshGenerationContext.painter2D;
            painter.lineWidth = 3;
            painter.strokeColor = Color.white;
            foreach (var node in techNodes)
            {
                if (node == null) continue;
                foreach (var child in node.children)
                {
                    if (child == null) continue;
                    Vector2 start = node.position + new Vector2(nodeWidth, nodeHeight / 2);
                    Vector2 end = child.position + new Vector2(0, nodeHeight / 2);
                    painter.BeginPath();
                    painter.MoveTo(start);
                    painter.LineTo(end);
                    painter.Stroke();
                }
            }
        };

        techGraphRoot.RegisterCallback<GeometryChangedEvent>(evt => lineLayer.MarkDirtyRepaint());
        RefreshTechButtons();
    }

    private int CalculateTechLevel(Technology tech)
    {
        if (tech == null) return 0;
        if (tech.requiredTechNames == null || tech.requiredTechNames.Length == 0)
            return 0;
        int maxLevel = 0;
        foreach (string reqName in tech.requiredTechNames)
        {
            Technology reqTech = System.Array.Find(technologies, t => t != null && t.techName == reqName);
            if (reqTech != null)
            {
                int level = CalculateTechLevel(reqTech) + 1;
                if (level > maxLevel) maxLevel = level;
            }
        }
        return maxLevel;
    }

    private VisualElement CreateTechNodeElement(Technology tech)
    {
        VisualElement node = new VisualElement();
        node.style.width = 200;
        node.style.height = 80;
        node.style.backgroundColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));
        node.style.borderTopLeftRadius = 8;
        node.style.borderTopRightRadius = 8;
        node.style.borderBottomLeftRadius = 8;
        node.style.borderBottomRightRadius = 8;
        node.style.paddingTop = 5;
        node.style.paddingBottom = 5;
        node.style.paddingLeft = 5;
        node.style.paddingRight = 5;
        node.style.alignItems = Align.Center;
        node.style.justifyContent = Justify.Center;

        Button btn = new Button();
        int actualCost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);
        btn.text = $"{tech.techName}\n{tech.description}\nСтоимость: ${actualCost}";
        btn.style.width = new Length(100, LengthUnit.Percent);
        btn.style.height = new Length(100, LengthUnit.Percent);
        btn.style.whiteSpace = WhiteSpace.Normal;
        btn.style.unityTextAlign = TextAnchor.MiddleCenter;
        btn.style.fontSize = 12;
        btn.userData = tech;

        btn.clicked += () =>
        {
            Technology localTech = tech;
            ResearchTechnology(localTech);
        };

        node.Add(btn);
        node.userData = tech;
        return node;
    }

    private void RefreshTechButtons()
    {
        if (techGraphRoot == null) return;
        foreach (var child in techGraphRoot.Children())
        {
            if (child is VisualElement node && node.userData is Technology tech)
            {
                Button btn = node.Q<Button>();
                if (btn != null)
                    UpdateTechButtonState(btn, tech);
            }
        }
    }

    private void UpdateTechButtonState(Button button, Technology tech)
    {
        button.style.color = Color.black;

        if (tech.isResearched)
        {
            button.text = $"{tech.techName} (Изучено)";
            button.SetEnabled(false);
            button.style.backgroundColor = new StyleColor(Color.gray);
            button.style.unityFontStyleAndWeight = FontStyle.Normal;
            return;
        }

        button.style.unityFontStyleAndWeight = FontStyle.Bold;

        bool requirementsMet = true;
        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string requiredName in tech.requiredTechNames)
            {
                Technology requiredTech = System.Array.Find(technologies, t => t != null && t.techName == requiredName);
                if (requiredTech == null || !requiredTech.isResearched)
                {
                    requirementsMet = false;
                    break;
                }
            }
        }

        int actualCost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);

        if (!requirementsMet)
        {
            button.SetEnabled(false);
            button.style.backgroundColor = new StyleColor(Color.red);
            button.text = $"{tech.techName}\n(Требования не выполнены)\nСтоимость: ${actualCost}";
        }
        else
        {
            button.SetEnabled(true);
            button.style.backgroundColor = new StyleColor(Color.green);
            button.text = $"{tech.techName}\n{tech.description}\nСтоимость: ${actualCost}";
        }
    }

    // ============================== ИССЛЕДОВАНИЕ ТЕХНОЛОГИЙ ==============================
    private void ResearchTechnology(Technology tech)
    {
        if (tech == null) return;

        if (tech.isResearched)
        {
            ShowNotification($"{tech.techName} уже исследована.");
            return;
        }

        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string requiredName in tech.requiredTechNames)
            {
                Technology requiredTech = System.Array.Find(technologies, t => t != null && t.techName == requiredName);
                if (requiredTech == null || !requiredTech.isResearched)
                {
                    ShowNotification($"Для {tech.techName} требуется исследовать '{requiredName}'");
                    return;
                }
            }
        }

        int actualCost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);
        if (money < actualCost)
        {
            double missing = actualCost - money;
            ShowNotification($"Не хватает денег для исследования {tech.techName}! Нужно ещё ${missing:F0}");
            return;
        }

        money -= actualCost;
        tech.isResearched = true;
        ShowNotification($"Технология '{tech.techName}' исследована!");

        RecalculateModifiers();
        UpdateCarPrices();

        if (tech.unlockCarOnResearch && tech.unlockedCar != null && !availableCars.Contains(tech.unlockedCar))
        {
            var newList = availableCars.ToList();
            newList.Add(tech.unlockedCar);
            availableCars = newList.ToArray();
            CreateCarButtons();
            ShowNotification($"Открыта новая машина: {tech.unlockedCar.carName}");
        }

        RefreshTechButtons();
        UpdateUI();

        if (tech.techName == bulkProductionTechName)
        {
            UpdateProductionButtons();
            ClampProductionCount();
            ShowNotification($"Технология '{tech.techName}' изучена! Теперь вы можете производить до 10 машин за раз.");
        }

        if (tech.techName == carUpgradeTechName)
        {
            CreateCarButtons();
            ShowNotification($"Технология '{tech.techName}' изучена! Теперь вы можете улучшать машины.");
        }
    }

    // ============================== СТИЛИ КНОПОК ==============================
    private void SetButtonStyle(Button button, float width, float height)
    {
        button.style.marginLeft = new Length(5, LengthUnit.Pixel);
        button.style.marginRight = new Length(5, LengthUnit.Pixel);
        button.style.marginTop = new Length(5, LengthUnit.Pixel);
        button.style.marginBottom = new Length(5, LengthUnit.Pixel);
        button.style.width = new Length(width, LengthUnit.Pixel);
        button.style.height = new Length(height, LengthUnit.Pixel);
        button.style.whiteSpace = WhiteSpace.Normal;
    }

    // ============================== РЕКЛАМНАЯ КАМПАНИЯ ==============================
    public void StartAdCampaign(string carName)
    {
        if (MarketSystem.Instance == null) return;
        int cost = 50;
        if (money >= cost)
        {
            money -= cost;
            MarketSystem.Instance.StartAdvertising(carName);
            UpdateUI();
            ShowNotification($"Реклама для {carName} запущена за ${cost}!");
        }
        else
        {
            ShowNotification($"Не хватает денег на рекламу! (нужно ${cost})");
        }
    }

    // ============================== ОБНОВЛЕНИЕ UI ==============================
    private void UpdateUI()
    {
        StopCoroutine(UpdateTextDelayed());
        StartCoroutine(UpdateTextDelayed());
    }

    private IEnumerator UpdateTextDelayed()
    {
        yield return new WaitForEndOfFrame();
        if (moneyLabel != null)
        {
            moneyLabel.text = $"Денег: ${money:F0}";
            moneyLabel.MarkDirtyRepaint();
        }
        if (incomeLabel != null)
        {
            incomeLabel.text = $"Авто/сек: {passiveIncome:F1}";
            incomeLabel.MarkDirtyRepaint();
        }
        if (carsOverlay != null && carsOverlay.style.display == DisplayStyle.Flex)
            UpdateCarCards();
        uiDoc.rootVisualElement.MarkDirtyRepaint();
    }

    // ============================== АВТОСОХРАНЕНИЕ ==============================
    private void OnApplicationQuit()
    {
        SaveGame();
    }
}