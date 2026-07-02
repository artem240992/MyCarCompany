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
    public GameObject carDisplayContainer;
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

    // --- НОВЫЕ МОДИФИКАТОРЫ ОТ ТЕХНОЛОГИЙ ---
    private float totalPriceModifier = 1f;
    private float totalDemandModifier = 1f;
    private float totalCostModifier = 1f; // для снижения себестоимости

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
        public Button upgradeButton;
    }

    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "autofactory_save.json");
    private bool isGameInitialized = false;

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

        if (File.Exists(SaveFilePath))
        {
            LoadGame();
            InitGame();
        }
        else
        {
            ShowWelcomeScreen();
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
            all.AddRange(startCars);
        if (technologies != null)
        {
            foreach (var tech in technologies)
                if (tech.unlockedCar != null && !all.Contains(tech.unlockedCar))
                    all.Add(tech.unlockedCar);
        }
        return all;
    }

    // ============================== ПЕРЕСЧЁТ МОДИФИКАТОРОВ ==============================
    private void RecalculateModifiers()
    {
        totalPriceModifier = 1f;
        totalDemandModifier = 1f;
        totalCostModifier = 1f;

        foreach (var tech in technologies)
        {
            if (tech.isResearched)
            {
                totalPriceModifier *= tech.priceModifier;
                totalDemandModifier *= tech.demandModifier;
                // Специальная обработка для конкретных технологий (например, Гибридный привод)
                if (tech.techName == "Гибридный привод")
                    totalCostModifier *= 0.9f; // снижение себестоимости на 10%
            }
        }
        // Ограничиваем, чтобы не уйти в бесконечность
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

        // Поиск eventLabel
        if (eventLabel == null)
            eventLabel = root.Q<Label>("EventLabel");

        // --- Кнопки открытия и закрытия ---
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

        // --- Инициализация конкурентов ---
        InitCompetitors();

        // Пересчёт модификаторов при старте
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

        // Запускаем AI конкурентов
        StartCompetitorAI();
    }

    // ============================== КОНКУРЕНТЫ ==============================
    private void InitCompetitors()
    {
        competitors.Clear();
        int count = 3 + (int)currentDifficulty; // Easy=3, Normal=4, Hard=5
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
                comp.availableCars.Add(availableCars[j]);
            }
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

        // 1. Ценовая политика
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

        // 2. Инвестиции в исследования (новые технологии)
        if (comp.researchLevel < 5 && comp.money > 400 && decision < 0.6f)
        {
            // Попытка исследовать технологию, улучшающую цену
            List<Technology> availableTechs = technologies.Where(t => !t.isResearched && !comp.researchedTechs.Contains(t.techName) && t.priceModifier > 1f).ToList();
            if (availableTechs.Count > 0 && comp.money > 300)
            {
                Technology chosen = availableTechs[Random.Range(0, availableTechs.Count)];
                int cost = Mathf.RoundToInt(chosen.researchCost * 0.8f);
                if (comp.money >= cost)
                {
                    comp.money -= cost;
                    comp.researchedTechs.Add(chosen.techName);
                    comp.researchLevel++;
                    // Обновляем модификаторы конкурента (влияет на его цены и спрос)
                    // Упрощённо: повышаем его репутацию и долю рынка
                    comp.reputation += 5;
                    comp.marketShare += 0.02f;
                    ShowNotification($"{comp.companyName} исследовал технологию '{chosen.techName}'!");
                    return;
                }
            }
            // Если нет технологий – инвестируем в завод
            if (comp.money > 300)
            {
                comp.money -= 150;
                comp.factoryLevel++;
                ShowNotification($"{comp.companyName} модернизирует завод!");
                return;
            }
        }

        // 3. Инвестиции в завод (если есть деньги и не было других действий)
        if (comp.money > 300 && decision < 0.7f)
        {
            comp.money -= 150;
            comp.factoryLevel++;
            ShowNotification($"{comp.companyName} модернизирует завод!");
            return;
        }
    }

    // ============================== ОКНО КОНКУРЕНТОВ ==============================
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

        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        header.style.paddingTop = 5;
        header.style.paddingBottom = 5;
        header.style.paddingLeft = 10;
        header.style.paddingRight = 10;
        header.style.borderBottomWidth = 1;
        header.style.borderBottomColor = new StyleColor(Color.gray);

        string[] headers = { "Компания", "Деньги", "Репутация", "Доля рынка", "Цена", "Завод", "Иссл." };
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

        foreach (var comp in competitors)
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.paddingTop = 5;
            row.style.paddingBottom = 5;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new StyleColor(new Color(0.2f, 0.2f, 0.2f));

            string[] values = {
                comp.companyName,
                $"${comp.money:F0}",
                comp.reputation.ToString(),
                $"{(comp.marketShare * 100):F1}%",
                $"{comp.priceMultiplier:F2}x",
                comp.factoryLevel.ToString(),
                comp.researchLevel.ToString()
            };

            foreach (var val in values)
            {
                Label lbl = new Label(val);
                lbl.style.width = new Length(colWidthPercent, LengthUnit.Percent);
                lbl.style.color = Color.white;
                row.Add(lbl);
            }
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

    // ============================== ПОСТРОЕНИЕ СПИСКА ДОСТУПНЫХ МАШИН ==============================
    private void BuildAvailableCars()
    {
        List<CarBlueprint> cars = new List<CarBlueprint>();
        if (startCars != null)
            cars.AddRange(startCars);
        if (technologies != null)
        {
            foreach (var tech in technologies)
            {
                if (tech.isResearched && tech.unlockedCar != null && tech.unlockCarOnResearch && !cars.Contains(tech.unlockedCar))
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
        foreach (var tech in technologies)
        {
            if (tech.techName == carUpgradeTechName && tech.isResearched)
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
            foreach (var tech in technologies)
                if (tech.isResearched) researched.Add(tech.techName);
            data.researchedTechNames = researched.ToArray();

            data.carDemands = new List<CarDemandData>();
            List<CarBlueprint> allCars = GetAllPossibleCars();
            foreach (CarBlueprint car in allCars)
            {
                data.carDemands.Add(new CarDemandData { carName = car.carName, demandMultiplier = car.demandMultiplier });
            }

            data.carLevels = new List<CarLevelData>();
            foreach (CarBlueprint car in allCars)
            {
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

            foreach (var tech in technologies) tech.isResearched = false;
            foreach (string techName in data.researchedTechNames)
            {
                Technology t = technologies.FirstOrDefault(tech => tech.techName == techName);
                if (t != null) t.isResearched = true;
            }

            // Пересчёт модификаторов после загрузки
            RecalculateModifiers();

            List<CarBlueprint> allCars = GetAllPossibleCars();
            foreach (CarDemandData d in data.carDemands)
            {
                CarBlueprint car = allCars.FirstOrDefault(c => c.carName == d.carName);
                if (car != null) car.demandMultiplier = d.demandMultiplier;
            }
            foreach (CarLevelData lvl in data.carLevels)
            {
                CarBlueprint car = allCars.FirstOrDefault(c => c.carName == lvl.carName);
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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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
        foreach (var tech in technologies) tech.isResearched = false;
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (var car in allCars) car.demandMultiplier = 1f;
        currentEventMultiplier = 1f;
        currentEventText = "";
        UpdateEventUI();
        // Пересчёт модификаторов (все технологии сброшены)
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

    // ============================== СПРОС (с учётом технологий и конкурентов) ==============================
    private void UpdateDemand()
    {
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (CarBlueprint car in allCars)
        {
            float min, max;
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy: min = 0.9f; max = 1.1f; break;
                case DifficultyLevel.Normal: min = 0.7f; max = 1.3f; break;
                case DifficultyLevel.Hard: min = 0.5f; max = 1.8f; break;
                default: min = 0.8f; max = 1.2f; break;
            }
            float baseDemand = Random.Range(min, max);

            // Влияние конкурентов
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

            // ---- ВЛИЯНИЕ ТЕХНОЛОГИЙ ИГРОКА ----
            float playerTechDemandModifier = totalDemandModifier;

            // ---- ВЛИЯНИЕ ТЕХНОЛОГИЙ КОНКУРЕНТОВ (упрощённо) ----
            float competitorTechDemandModifier = 1f;
            foreach (var comp in competitors)
            {
                // Чем больше технологий у конкурента, тем меньше спрос на эту модель у игрока
                // (каждая исследованная технология конкурента снижает спрос на 2%)
                competitorTechDemandModifier *= (1f - comp.researchLevel * 0.02f);
            }

            float finalDemand = baseDemand * currentEventMultiplier * competitorFactor * playerTechDemandModifier * competitorTechDemandModifier;
            car.demandMultiplier = Mathf.Clamp(finalDemand, 0.1f, 5f);
        }
        UpdateCarCards();
    }

    // ============================== ОБНОВЛЕНИЕ ЦЕН МАШИН (ПОСЛЕ ИССЛЕДОВАНИЙ) ==============================
    private void UpdateCarPrices()
    {
        // Пересоздаём карточки машин, чтобы отобразить новые цены
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

    // ============================== МАССОВОЕ ПРОИЗВОДСТВО ==============================
    private bool IsBulkProductionUnlocked()
    {
        if (string.IsNullOrEmpty(bulkProductionTechName)) return true;
        foreach (var tech in technologies)
        {
            if (tech.techName == bulkProductionTechName && tech.isResearched)
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

    private void ShowNotification(string message)
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

    // ============================== УЛУЧШЕНИЕ МАШИНЫ ==============================
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

    // ============================== КНОПКИ МАШИН (с учётом модификаторов) ==============================
    private void CreateCarButtons()
    {
        if (carsContainer == null) return;
        carsContainer.Clear();
        carCards.Clear();

        bool upgradeUnlocked = IsCarUpgradeUnlocked();

        foreach (CarBlueprint car in availableCars)
        {
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
            card.style.flexDirection = FlexDirection.Row;
            card.style.alignItems = Align.Center;

            if (car.carIcon != null)
            {
                Image iconImage = new Image();
                iconImage.sprite = car.carIcon;
                iconImage.style.width = 60;
                iconImage.style.height = 60;
                iconImage.style.marginRight = 15;
                card.Add(iconImage);
            }

            VisualElement textContainer = new VisualElement();
            textContainer.style.flexDirection = FlexDirection.Column;
            textContainer.style.flexGrow = 1;

            Label nameLabel = new Label(car.carName);
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            // Используем модифицированные цену и себестоимость
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

            textContainer.Add(nameLabel);
            textContainer.Add(detailsLabel);
            textContainer.Add(profitLabel);
            textContainer.Add(demandLabel);
            textContainer.Add(levelLabel);

            card.Add(textContainer);

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

            card.Add(upgradeButton);

            CarCardData cardData = new CarCardData();
            cardData.car = car;
            cardData.card = card;
            cardData.profitLabel = profitLabel;
            cardData.demandLabel = demandLabel;
            cardData.upgradeButton = upgradeButton;
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
            cardData.profitLabel.text = $"Прибыль: {profit:F0}";
            cardData.profitLabel.style.color = profit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);

            float demand = car.demandMultiplier;
            cardData.demandLabel.text = $"Спрос: {demand:F1}x";
            cardData.demandLabel.style.color = demand > 1.2f ? Color.green : (demand < 0.8f ? Color.red : Color.yellow);

            if (cardData.upgradeButton != null)
            {
                bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
                cardData.upgradeButton.SetEnabled(canUpgrade);
                cardData.upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            }
        }
    }

    // ============================== ПРОИЗВОДСТВО (с учётом модификаторов) ==============================
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
            ShowNotification("Ошибка: нет префаба для этой машины!");
            return;
        }

        if (isProductionInProgress)
        {
            ShowNotification("Производство занято! Дождитесь завершения.");
            return;
        }

        if (currentCarInstance != null)
            Destroy(currentCarInstance);

        if (carDisplayContainer == null)
        {
            Debug.LogError("carDisplayContainer не назначен!");
            return;
        }

        isProductionInProgress = true;

        currentCarInstance = Instantiate(prefabToSpawn, carDisplayContainer.transform);
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

        if (availableCars.Length == 0)
        {
            ShowNotification("Нет доступных машин для производства!");
            return;
        }

        CarBlueprint car = availableCars[0];
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
            techLevels[tech] = CalculateTechLevel(tech);

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
            TechNode node = new TechNode();
            node.tech = tech;
            node.element = CreateTechNodeElement(tech);
            nodeMap[tech] = node;
            techNodes.Add(node);
        }

        foreach (var tech in technologies)
        {
            TechNode childNode = nodeMap[tech];
            if (tech.requiredTechNames != null)
            {
                foreach (string parentName in tech.requiredTechNames)
                {
                    Technology parentTech = System.Array.Find(technologies, t => t.techName == parentName);
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
                foreach (var child in node.children)
                {
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
        if (tech.requiredTechNames == null || tech.requiredTechNames.Length == 0)
            return 0;
        int maxLevel = 0;
        foreach (string reqName in tech.requiredTechNames)
        {
            Technology reqTech = System.Array.Find(technologies, t => t.techName == reqName);
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
                Technology requiredTech = System.Array.Find(technologies, t => t.techName == requiredName);
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

    // ============================== ИССЛЕДОВАНИЕ ТЕХНОЛОГИЙ (с пересчётом модификаторов) ==============================
    private void ResearchTechnology(Technology tech)
    {
        if (tech.isResearched)
        {
            ShowNotification($"{tech.techName} уже исследована.");
            return;
        }

        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string requiredName in tech.requiredTechNames)
            {
                Technology requiredTech = System.Array.Find(technologies, t => t.techName == requiredName);
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

        // ---- ПЕРЕСЧЁТ МОДИФИКАТОРОВ ----
        RecalculateModifiers();

        // ---- ОБНОВЛЕНИЕ ЦЕН МАШИН ----
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