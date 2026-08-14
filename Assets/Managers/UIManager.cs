using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    private VisualElement newsOverlay;
    private Toggle musicToggle;
    private Label newsTitleLabel;
    private Label newsDescriptionLabel;
    private Label newsImportanceLabel;
    private VisualElement newsImportanceBar;
    private Button closeNewsButton;

    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement notificationContainer;
    private Label eventLabel;

    // ---- Демо-версия ----
    private Label demoTimerLabel;
    private Label demoProgressLabel;

    // ---- Оверлеи для международной экспансии и патентов ----
    private VisualElement officesOverlay;
    private VisualElement patentsOverlay;
    private Label moneyLabel;
    private Label incomeLabel;
    private Label savedDifficultyLabel;
    private Label versionLabel;

    private Label engineLabel;
    private Label bodyLabel;
    private Label wheelsLabel;
    private Label electronicsLabel;

    private Label dateLabel;
    private Label inflationLabel;
    private Label reputationLabel;
    private Label seasonLabel;
    

    private VisualElement carsOverlay;

    private VisualElement carsContainer;
    private VisualElement techOverlay;
    private ScrollView techScrollView;
    private VisualElement upgradeOverlay;
    private VisualElement settingsOverlay;
    private VisualElement competitorsOverlay;
    private VisualElement competitorsContent;
    private VisualElement welcomeOverlay;

    private VisualElement achievementsOverlay;
    private ScrollView achievementsContainer;
    private Button closeAchievementsButton;

    private VisualElement marketingOverlay;
    private VisualElement marketVisOverlay;
    private VisualElement loansOverlay;
    private VisualElement investmentsOverlay;

    private Button competitorsTabButton;
    private Button actionLogTabButton;
    private VisualElement actionLogContent;

    private Button hamburgerButton;
    private VisualElement menuContainer;
    private Label countLabel;
    private Button decreaseCountBtn;
    private Button increaseCountBtn;
    private Button produceButton;

    private Label conveyorLevelLabel;
    private Label engineerCountLabel;
    private Button buyConveyorButton;
    private Button hireEngineerButton;

    private Label carStockLabel;

    private Button buyPartsButton;

    private Button upgradeTabFactoryButton;
    private Button upgradeTabPartsButton;
    private VisualElement upgradeFactoryContent;
    private VisualElement upgradePartsContent;

    [Header("Требования для улучшения конвейера")]
    public int conveyorEngineRequired = 2;
    public int conveyorBodyRequired = 1;
    public int conveyorWheelsRequired = 4;
    public int conveyorElectronicsRequired = 1;

    [Header("Требования для найма инженера")]
    public int engineerEngineRequired = 1;
    public int engineerBodyRequired = 0;
    public int engineerWheelsRequired = 0;
    public int engineerElectronicsRequired = 1;

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;
    private CompetitorManager competitor => CarCompanyManager.Instance.CompetitorManager;
    private ProductionManager production => CarCompanyManager.Instance.ProductionManager;
    private WarehouseManager warehouse => WarehouseManager.Instance;

    private List<CarCardData> carCards = new List<CarCardData>();
    private List<(Competitor competitor, DropdownField dropdown)> competitorActionRows = new List<(Competitor, DropdownField)>();
    private Dictionary<string, int> selectedActions = new Dictionary<string, int>();

    public enum Difficulty { Easy, Normal, Hard }
    private Difficulty currentDifficulty = Difficulty.Normal;
    public Difficulty GetCurrentDifficulty() => currentDifficulty;

    private Color[] carColors = new Color[]
    {
        new Color(0.2f, 0.8f, 0.2f),
        new Color(0.9f, 0.1f, 0.1f),
        new Color(0.1f, 0.1f, 0.1f),
        new Color(0.4f, 0.4f, 0.45f),
        new Color(0.9f, 0.9f, 0.9f)
    };
    private string[] colorNames = { "Зелёный", "Красный", "Чёрный", "Мокрый асфальт", "Белый" };

    private class CarCardData
    {
        public CarBlueprint car;
        public Button produceButton;

        public DropdownField transmissionDropdown; // новое поле

        public Label ratingLabel; 

        public Button sellButton;
        public Label stockLabel;
        
        public Label profitDetailsLabel;
        public VisualElement card;
        public Label profitLabel;
        public Label demandLabel;
        public Label trendLabel;
        public Label levelLabel;
        public VisualElement graphContainer;
        public Button upgradeButton;
        public SliderInt powerSlider, economySlider, designSlider, safetySlider;
        public Label powerMaxLabel, economyMaxLabel, designMaxLabel, safetyMaxLabel;
        public Label powerValueLabel, economyValueLabel, designValueLabel, safetyValueLabel;
        public Button powerUpgradeBtn, economyUpgradeBtn, designUpgradeBtn, safetyUpgradeBtn;
        public Button[] colorButtons;
        public Toggle tintToggle;
        public Label taxRateLabel;
        public Label modifierLabel;

        // ========== НОВЫЕ ПОЛЯ ==========
        public Label detailsLabel;          // лейбл "Цена: $X | Себ: $Y"
        public Label priceLabel;            // лейбл текущей цены для регулировки
        public Button decreasePriceBtn;
        public Button increasePriceBtn;
    }

    private string[] tuningParamNames = { "power", "economy", "design", "safety" };
    private string[] tuningParamDisplay = { "Мощность", "Экономичность", "Дизайн", "Безопасность" };
    private const int TUNING_MAX_LEVEL = 10;
    private VisualElement techGraphRoot;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Debug.LogWarning("Дублирующийся UIManager");
    }

    private int GetMaxTuning(CarBlueprint car, string param)
    {
        switch (param)
        {
            case "power": return car.tuningPower;
            case "economy": return car.tuningEconomy;
            case "design": return car.tuningDesign;
            case "safety": return car.tuningSafety;
            default: return 0;
        }
    }

    private int GetCurrentTuning(CarBlueprint car, string param)
    {
        switch (param)
        {
            case "power": return car.currentPower;
            case "economy": return car.currentEconomy;
            case "design": return car.currentDesign;
            case "safety": return car.currentSafety;
            default: return 0;
        }
    }

    private void SetCurrentTuning(CarBlueprint car, string param, int value)
    {
        switch (param)
        {
            case "power": car.currentPower = value; break;
            case "economy": car.currentEconomy = value; break;
            case "design": car.currentDesign = value; break;
            case "safety": car.currentSafety = value; break;
        }
    }


    private void UpdateDemoUI()
    {
        #if DEMO_BUILD
        if (DemoManager.Instance == null) return;

        if (demoTimerLabel != null)
        {
            float time = DemoManager.Instance.TimeRemaining;
            int minutes = Mathf.FloorToInt(time / 60);
            int seconds = Mathf.FloorToInt(time % 60);
            demoTimerLabel.text = $"⏱️ {minutes:D2}:{seconds:D2}";
        }

        if (demoProgressLabel != null)
        {
            int researched = DemoManager.Instance.TechnologiesResearched;
            int max = DemoManager.Instance.MaxTechnologiesToResearch;
            demoProgressLabel.text = $"🔬 Технологий: {researched}/{max}";
        }
        #endif
    }


    // Методы:
    private void OpenNewsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (newsOverlay != null)
        {
            newsOverlay.style.display = DisplayStyle.Flex;
            AnimateWindowOpen(newsOverlay);
            UpdateNewsUI();
        }
    }

    private void CloseNewsWindow()
    {
        if (newsOverlay != null)
            AnimateWindowClose(newsOverlay, () => newsOverlay.style.display = DisplayStyle.None);
    }

    public void UpdateNewsUI()
    {
        if (newsTitleLabel == null) return;
        bool hasNews = NewsManager.Instance != null && NewsManager.Instance.IsNewsActive;
        if (hasNews)
        {
            newsTitleLabel.text = NewsManager.Instance.CurrentTitle;
            newsDescriptionLabel.text = NewsManager.Instance.CurrentDescription;
            float importance = NewsManager.Instance.CurrentImportance;
            newsImportanceLabel.text = $"Влияние: {importance * 100:F0}%";
            if (newsImportanceBar != null)
            {
                newsImportanceBar.style.width = new Length(importance * 100, LengthUnit.Percent);
                Color color = importance > 0.6f ? Color.green : (importance > 0.3f ? Color.yellow : Color.red);
                newsImportanceBar.style.backgroundColor = color;
            }
        }
        else
        {
            newsTitleLabel.text = "Нет активных новостей";
            newsDescriptionLabel.text = "Рынок стабилен.";
            newsImportanceLabel.text = "Влияние: 0%";
            if (newsImportanceBar != null)
                newsImportanceBar.style.width = new Length(0, LengthUnit.Percent);
        }
    }


    public void Initialize(UIDocument document)
    {
        uiDoc = document;
        if (uiDoc == null) { Debug.LogError("UIDocument не передан!"); return; }
        root = uiDoc.rootVisualElement;
        if (root == null) return;

        newsOverlay = root.Q<VisualElement>("NewsOverlay");
        carStockLabel = root.Q<Label>("CarStockLabel");
        newsTitleLabel = root.Q<Label>("NewsTitleLabel");
        newsDescriptionLabel = root.Q<Label>("NewsDescriptionLabel");
        newsImportanceLabel = root.Q<Label>("NewsImportanceLabel");
        newsImportanceBar = root.Q<VisualElement>("NewsImportanceBar");
        closeNewsButton = root.Q<Button>("CloseNewsButton");

        SubscribeButton("OpenNewsButton", OpenNewsWindow);
        if (closeNewsButton != null)
            closeNewsButton.clicked += CloseNewsWindow;

        if (newsOverlay != null)
            newsOverlay.style.display = DisplayStyle.None;

        musicToggle = root.Q<Toggle>("MusicToggle");
        if (musicToggle != null)
        {
            // Загружаем сохранённое состояние и устанавливаем переключатель
            int savedMute = PlayerPrefs.GetInt("MusicMuted", 0);
            musicToggle.value = (savedMute == 0); // true = музыка включена
            musicToggle.RegisterValueChangedCallback(evt =>
            {
                if (MusicManager.Instance != null)
                    MusicManager.Instance.SetMute(!evt.newValue); // если toggle выключен, музыка mute = true
            });
        }

        mainPanel = root.Q<VisualElement>("MainPanel");
        reputationLabel = root.Q<Label>("ReputationLabel");
        notificationContainer = root.Q<VisualElement>("NotificationContainer");
        versionLabel = root.Q<Label>("VersionLabel");
        if (versionLabel != null) versionLabel.text = $"v. {Application.version}";
        moneyLabel = root.Q<Label>("MoneyLabel");
        incomeLabel = root.Q<Label>("IncomeLabel");
        savedDifficultyLabel = root.Q<Label>("SavedDifficultyLabel");
        eventLabel = root.Q<Label>("EventLabel");

        dateLabel = root.Q<Label>("DateLabel");
        inflationLabel = root.Q<Label>("InflationLabel");
        seasonLabel = root.Q<Label>("SeasonLabel");

        achievementsOverlay = root.Q<VisualElement>("AchievementsOverlay");
        achievementsContainer = root.Q<ScrollView>("AchievementsContainer");
        closeAchievementsButton = root.Q<Button>("CloseAchievementsButton");

        competitorsTabButton = root.Q<Button>("CompetitorsTabButton");
        actionLogTabButton = root.Q<Button>("ActionLogTabButton");
        competitorsContent = root.Q<VisualElement>("CompetitorsContent");
        actionLogContent = root.Q<VisualElement>("ActionLogContent");

        hamburgerButton = root.Q<Button>("HamburgerButton");
        menuContainer = root.Q<VisualElement>("MenuContainer");
        carsOverlay = root.Q<VisualElement>("CarsOverlay");
        carsContainer = root.Q<VisualElement>("CarsContainer");
        techOverlay = root.Q<VisualElement>("TechOverlay");
        techScrollView = root.Q<ScrollView>("TechContainer");
        upgradeOverlay = root.Q<VisualElement>("UpgradeOverlay");
        settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        competitorsOverlay = root.Q<VisualElement>("CompetitorsOverlay");
        welcomeOverlay = root.Q<VisualElement>("WelcomeOverlay");
        countLabel = root.Q<Label>("CountLabel");
        decreaseCountBtn = root.Q<Button>("DecreaseCountButton");
        increaseCountBtn = root.Q<Button>("IncreaseCountButton");
        produceButton = root.Q<Button>("ProduceButton");
        conveyorLevelLabel = root.Q<Label>("ConveyorLevelLabel");
        engineerCountLabel = root.Q<Label>("EngineerCountLabel");
        buyConveyorButton = root.Q<Button>("BuyConveyorButton");
        hireEngineerButton = root.Q<Button>("HireEngineerButton");
        buyPartsButton = root.Q<Button>("BuyPartsButton");

        engineLabel = root.Q<Label>("EngineLabel");
        bodyLabel = root.Q<Label>("BodyLabel");
        wheelsLabel = root.Q<Label>("WheelsLabel");
        electronicsLabel = root.Q<Label>("ElectronicsLabel");

        marketingOverlay = root.Q<VisualElement>("MarketingOverlay");
        marketVisOverlay = root.Q<VisualElement>("MarketVisOverlay");
        loansOverlay = root.Q<VisualElement>("LoansOverlay");
        investmentsOverlay = root.Q<VisualElement>("InvestmentsOverlay");
        officesOverlay = root.Q<VisualElement>("OfficesOverlay");
        patentsOverlay = root.Q<VisualElement>("PatentsOverlay");
        demoTimerLabel = root.Q<Label>("DemoTimerLabel");
        demoProgressLabel = root.Q<Label>("DemoProgressLabel");
        SetupLoansUI();
        SetupInvestmentsUI();
        SetupLoansListView();
        SetupInvestmentsListView();

        upgradeTabFactoryButton = root.Q<Button>("UpgradeTabFactoryButton");
        upgradeTabPartsButton = root.Q<Button>("UpgradeTabPartsButton");
        upgradeFactoryContent = root.Q<VisualElement>("UpgradeFactoryContent");
        upgradePartsContent = root.Q<VisualElement>("UpgradePartsContent");

        if (achievementsOverlay != null)
            achievementsOverlay.style.display = DisplayStyle.None;
        if (actionLogContent != null)
            actionLogContent.style.display = DisplayStyle.None;
        if (competitorsContent != null)
            competitorsContent.style.display = DisplayStyle.Flex;

        if (savedDifficultyLabel != null)
            savedDifficultyLabel.AddToClassList("difficulty-display");

        if (hamburgerButton != null)
            hamburgerButton.clicked += OpenMenuWindow;

        // ---- Подписки ----
        SubscribeButton("OpenCarsButton", OpenCarsWindow);
        SubscribeButton("OpenTechButton", OpenTechWindow);
        SubscribeButton("OpenUpgradeButton", OpenUpgradeWindow);
        SubscribeButton("OpenSettingsButton", OpenSettingsWindow);
        SubscribeButton("OpenCompetitorsButton", OpenCompetitorsWindow);
        SubscribeButton("OpenAchievementsButton", OpenAchievementsWindow);
        SubscribeButton("CloseCarsButton", CloseCarsWindow);
        SubscribeButton("CloseTechButton", CloseTechWindow);
        SubscribeButton("CloseUpgradeButton", CloseUpgradeWindow);
        SubscribeButton("CloseSettingsButton", CloseSettingsWindow);
        SubscribeButton("CloseCompetitorsButton", CloseCompetitorsWindow);
        SubscribeButton("OpenMarketingButton", OpenMarketingWindow);
        SubscribeButton("CloseMarketingButton", CloseMarketingWindow);
        SubscribeButton("CloseMenuButton", CloseMenuWindow);
        SubscribeButton("OpenNewsButton", OpenNewsWindow);
        SubscribeButton("CloseNewsButton", CloseNewsWindow);
        SubscribeButton("CloseNewsButton2", CloseNewsWindow);

        SubscribeButton("OpenOfficesButton", OpenOfficesWindow);
        SubscribeButton("CloseOfficesButton", CloseOfficesWindow);
        SubscribeButton("OpenPatentsButton", OpenPatentsWindow);
        SubscribeButton("ClosePatentsButton", ClosePatentsWindow);
        SubscribeButton("RefreshOfficesButton", UpdateOfficesUI);
        SubscribeButton("RefreshPatentsButton", UpdatePatentsUI);

        SubscribeButton("CloseMarketingButton2", CloseMarketingWindow);
        SubscribeButton("RefreshCompetitorsButton", () =>
        {
            ExecuteAllCompetitorActions();
            competitor.RefreshCompetitorsList();
            if (actionLogContent != null && actionLogContent.style.display == DisplayStyle.Flex)
                RefreshActionLogs();
        });

        SubscribeButton("SaveButton", () => CarCompanyManager.Instance.SaveLoadManager.SaveGame());
        SubscribeButton("LoadButton", () => CarCompanyManager.Instance.SaveLoadManager.LoadGame());
        SubscribeButton("NewGameButton", () => CarCompanyManager.Instance.SaveLoadManager.NewGame());
        if (DemoManager.IsDemoBuild)
        {
            // Деактивируем все кнопки, связанные с сохранением
            var saveButton = root.Q<Button>("SaveButton");
            if (saveButton != null) saveButton.SetEnabled(false);
            var loadButton = root.Q<Button>("LoadButton");
            if (loadButton != null) loadButton.SetEnabled(false);
            var newGameButton = root.Q<Button>("NewGameButton");
            if (newGameButton != null) newGameButton.SetEnabled(false);

            // Кнопки слотов
            for (int i = 1; i <= 3; i++)
            {
                var saveSlot = root.Q<Button>($"SaveSlot{i}");
                if (saveSlot != null) saveSlot.SetEnabled(false);
                var loadSlot = root.Q<Button>($"LoadSlot{i}");
                if (loadSlot != null) loadSlot.SetEnabled(false);
            }
        }
        SubscribeButton("ProduceButton", ProduceSingleCar);

        SubscribeButton("EasyButton", () => { SetDifficulty(Difficulty.Easy); CloseSettingsWindow(); });
        SubscribeButton("NormalButton", () => { SetDifficulty(Difficulty.Normal); CloseSettingsWindow(); });
        SubscribeButton("HardButton", () => { SetDifficulty(Difficulty.Hard); CloseSettingsWindow(); });

        SubscribeButton("WelcomeEasyButton", () => OnDifficultySelected(Difficulty.Easy));
        SubscribeButton("WelcomeNormalButton", () => OnDifficultySelected(Difficulty.Normal));
        SubscribeButton("WelcomeHardButton", () => OnDifficultySelected(Difficulty.Hard));

        if (competitorsTabButton != null)
            competitorsTabButton.clicked += () => SwitchCompetitorTab(true);
        if (actionLogTabButton != null)
            actionLogTabButton.clicked += () => SwitchCompetitorTab(false);

        if (closeAchievementsButton != null)
            closeAchievementsButton.clicked += CloseAchievementsWindow;

        if (buyConveyorButton != null)
            buyConveyorButton.clicked += TryBuyConveyorUpgrade;
        if (hireEngineerButton != null)
            hireEngineerButton.clicked += TryHireEngineer;

        if (decreaseCountBtn != null) decreaseCountBtn.clicked += production.DecreaseCount;
        if (increaseCountBtn != null) increaseCountBtn.clicked += production.IncreaseCount;
        if (buyPartsButton != null) buyPartsButton.clicked += TryBuyParts;

        if (upgradeTabFactoryButton != null)
            upgradeTabFactoryButton.clicked += () => SwitchUpgradeTab(true);
        if (upgradeTabPartsButton != null)
            upgradeTabPartsButton.clicked += () => SwitchUpgradeTab(false);

        // ---- ПОДПИСКИ НОВЫХ КНОПОК (релиз 1.13.0) ----
        SubscribeButton("OpenMarketVisualizationButton", OpenMarketVisualizationWindow);
        SubscribeButton("CloseMarketVisButton", CloseMarketVisWindow);
        SubscribeButton("OpenLoansButton", OpenLoansWindow);
        SubscribeButton("CloseLoansButton", CloseLoansWindow);
        SubscribeButton("OpenInvestmentsButton", OpenInvestmentsWindow);
        SubscribeButton("CloseInvestmentsButton", CloseInvestmentsWindow);
        SubscribeButton("TakeLoanButton", OnTakeLoan);
        SubscribeButton("MakeInvestmentButton", OnMakeInvestment);

        SubscribeButton("SaveSlot1", () => CarCompanyManager.Instance.SaveLoadManager.SaveSlot1());
        SubscribeButton("SaveSlot2", () => CarCompanyManager.Instance.SaveLoadManager.SaveSlot2());
        SubscribeButton("SaveSlot3", () => CarCompanyManager.Instance.SaveLoadManager.SaveSlot3());
        SubscribeButton("LoadSlot1", () => CarCompanyManager.Instance.SaveLoadManager.LoadSlot1());
        SubscribeButton("LoadSlot2", () => CarCompanyManager.Instance.SaveLoadManager.LoadSlot2());
        SubscribeButton("LoadSlot3", () => CarCompanyManager.Instance.SaveLoadManager.LoadSlot3());

        // ---- Кнопки продажи и производства деталей ----
        SubscribeButton("SellEngineButton", () => TrySellPart(PartType.Engine));
        SubscribeButton("SellBodyButton", () => TrySellPart(PartType.Body));
        SubscribeButton("SellWheelsButton", () => TrySellPart(PartType.Wheels));
        SubscribeButton("SellElectronicsButton", () => TrySellPart(PartType.Electronics));
        SubscribeButton("ProduceEngineButton", () => TryProducePart(PartType.Engine));
        SubscribeButton("ProduceBodyButton", () => TryProducePart(PartType.Body));
        SubscribeButton("ProduceWheelsButton", () => TryProducePart(PartType.Wheels));
        SubscribeButton("ProduceElectronicsButton", () => TryProducePart(PartType.Electronics));

        HideAllOverlays();
        UpdateMoneyLabels();
        UpdateReputationLabel();
        UpdateUpgradeUI();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();

        // ---- ИНИЦИАЛИЗАЦИЯ КОНТРОЛЛЕРА ВИЗУАЛИЗАЦИИ ----
        var visController = GetComponent<MarketVisualizationController>();
        if (visController != null)
        {
            visController.Initialize();
            Debug.Log("MarketVisualizationController инициализирован");
        }
        else
        {
            Debug.LogWarning("MarketVisualizationController не найден на объекте. Визуализация рынка недоступна.");
        }

        int saved = PlayerPrefs.GetInt("Difficulty", 1);
        currentDifficulty = (Difficulty)saved;
        UpdateDifficultyDisplay();

        if (GameTimeManager.Instance != null)
        {
            GameTimeManager.Instance.OnMonthChanged += UpdateDateTimeDisplay;
            UpdateDateTimeDisplay();
        }
        else
        {
            Debug.LogWarning("GameTimeManager не найден! Дата не будет обновляться.");
        }

        if (marketingOverlay != null) marketingOverlay.style.display = DisplayStyle.None;
        if (marketVisOverlay != null) marketVisOverlay.style.display = DisplayStyle.None;
        if (loansOverlay != null) loansOverlay.style.display = DisplayStyle.None;
        if (investmentsOverlay != null) investmentsOverlay.style.display = DisplayStyle.None;

        // ========== НОВОЕ: СОЗДАНИЕ ДЕМО-ЛЕЙБЛОВ ==========
        #if DEMO_BUILD
        // Ищем или создаём таймер
        demoTimerLabel = root.Q<Label>("DemoTimerLabel");
        if (demoTimerLabel == null)
        {
            demoTimerLabel = new Label();
            demoTimerLabel.name = "DemoTimerLabel";
            demoTimerLabel.style.position = Position.Absolute;
            demoTimerLabel.style.left = 10;
            demoTimerLabel.style.top = 10;
            demoTimerLabel.style.color = Color.yellow;
            demoTimerLabel.style.fontSize = 16;
            demoTimerLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            root.Add(demoTimerLabel);
        }

        demoProgressLabel = root.Q<Label>("DemoProgressLabel");
        if (demoProgressLabel == null)
        {
            demoProgressLabel = new Label();
            demoProgressLabel.name = "DemoProgressLabel";
            demoProgressLabel.style.position = Position.Absolute;
            demoProgressLabel.style.left = 10;
            demoProgressLabel.style.top = 35;
            demoProgressLabel.style.color = Color.cyan;
            demoProgressLabel.style.fontSize = 14;
            root.Add(demoProgressLabel);
        }
        #endif
        
    }

    private void SubscribeButton(string name, Action action)
    {
        var btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += () => action?.Invoke();
        else Debug.LogWarning($"Кнопка '{name}' не найдена");
    }



/// <summary>
/// Производит указанное количество машин указанной модели.
/// </summary>
/// <param name="car">Модель для производства. Если null, используется первая доступная.</param>
/// <param name="count">Количество машин для производства.</param>
    private void ProduceCars(CarBlueprint car, int count)
    {
        if (count <= 0) return;

        // Если модель не указана, берём первую доступную
        if (car == null)
        {
            var availableCars = tech.AvailableCars;
            if (availableCars == null || availableCars.Length == 0)
            {
                ShowNotification("Нет доступных машин для производства!");
                return;
            }
            car = availableCars[0];
            if (car == null) return;
        }

        var economy = CarCompanyManager.Instance.EconomyManager;
        int produced = 0;

        for (int i = 0; i < count; i++)
        {
            // 1. Проверяем наличие деталей
            if (!HasRequiredParts(car))
            {
                ShowNotification($"❌ Недостаточно деталей для {car.GetDisplayName()}!");
                break;
            }

            // 2. Проверяем наличие денег
            int assemblyCost = car.GetModifiedAssemblyCost();
            if (economy.Money < assemblyCost)
            {
                ShowNotification($"❌ Недостаточно денег! Нужно ${assemblyCost} для сборки.");
                break;
            }

            // 3. Производим машину
            bool success = production.ProduceSpecificCar(car);
            if (success)
            {
                produced++;
            }
            else
            {
                // Если производство не удалось по другой причине, прерываем
                break;
            }
        }

        if (produced > 0)
        {
            UpdateWarehouseLabels();
            UpdateProductionButtonsState();
            UpdateMoneyLabels();
            ShowNotification($"✅ {produced} {car.GetDisplayName()} произведено!");

            // ---- Обновление достижений ----
            CarCompanyManager.Instance.EconomyManager.RegisterCarSold(produced);
        }
        else
        {
            // Если не произвели ни одной машины, показываем общее уведомление
            if (count > 0) ShowNotification($"❌ Не удалось произвести ни одной машины.");
        }
    }


    // ---- Производство базовой машины ----
    private void TryProduceBasicCar()
    {
        var availableCars = tech.AvailableCars;
        if (availableCars == null || availableCars.Length == 0)
        {
            ShowNotification("Нет доступных машин для производства!");
            return;
        }
        CarBlueprint car = availableCars[0];
        if (car == null) return;

        // Получаем количество из CountLabel
        int count = 1;
        if (countLabel != null && int.TryParse(countLabel.text, out int parsed))
            count = Mathf.Max(1, parsed);

        ProduceCars(car, count);
    }

    // ---- Производство конкретной машины ----
    private void TryProduceSpecificCar(CarBlueprint car)
    {
        if (car == null) return;

        // Получаем количество из CountLabel
        int count = 1;
        if (countLabel != null && int.TryParse(countLabel.text, out int parsed))
            count = Mathf.Max(1, parsed);

        ProduceCars(car, count);
    }


    /// <summary>
    /// Производит ОДНУ базовую машину (для кнопки в главном меню).
    /// </summary>
    public void ProduceSingleCar()
    {
        var availableCars = tech.AvailableCars;
        if (availableCars == null || availableCars.Length == 0)
        {
            ShowNotification("Нет доступных машин для производства!");
            return;
        }
        CarBlueprint car = availableCars[0];
        if (car == null) return;

        ProduceCars(car, 1);
    }



    private void UpdateProductionButtonsState()
    {
        if (produceButton != null)
        {
            CarRecipe defaultRecipe = ScriptableObject.CreateInstance<CarRecipe>();
            defaultRecipe.engineRequired = 1;
            defaultRecipe.bodyRequired = 1;
            defaultRecipe.wheelsRequired = 1;
            defaultRecipe.electronicsRequired = 1;

            // ИСПРАВЛЕНО: используем CreateInstance вместо new
            CarBlueprint dummy = ScriptableObject.CreateInstance<CarBlueprint>();
            dummy.recipe = defaultRecipe;
            produceButton.SetEnabled(HasRequiredParts(dummy));
        }
    }

    // ---- Покупка деталей ----
    private void TryBuyParts()
    {
        if (warehouse == null)
        {
            ShowNotification("Склад не доступен");
            return;
        }

        int cost = Mathf.RoundToInt(100 * economy.CostMultiplier);
        if (economy.Money < cost)
        {
            ShowNotification($"Недостаточно денег! Нужно ${cost}");
            return;
        }

        economy.Money -= cost;
        warehouse.AddParts(PartType.Engine, 1);
        warehouse.AddParts(PartType.Body, 1);
        warehouse.AddParts(PartType.Wheels, 1);
        warehouse.AddParts(PartType.Electronics, 1);

        UpdateMoneyLabels();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
        UpdateUpgradeUI();

        ShowNotification("✅ Куплена партия деталей (+1 каждой)");
    }

    // ---- Обновление лейблов склада ----
    public void UpdateWarehouseLabels()
    {
        if (carStockLabel != null)
            carStockLabel.text = WarehouseManager.Instance?.GetTotalCarCount().ToString() ?? "0";
        if (warehouse == null) return;
        if (engineLabel != null)
            engineLabel.text = warehouse.GetPartCount(PartType.Engine).ToString();
        if (bodyLabel != null)
            bodyLabel.text = warehouse.GetPartCount(PartType.Body).ToString();
        if (wheelsLabel != null)
            wheelsLabel.text = warehouse.GetPartCount(PartType.Wheels).ToString();
        if (electronicsLabel != null)
            electronicsLabel.text = warehouse.GetPartCount(PartType.Electronics).ToString();
    }

    // ---- Улучшения (без требований деталей) ----
    private bool HasRequiredPartsForUpgrade(int engine, int body, int wheels, int electronics)
    {
        if (warehouse == null) return false;
        return warehouse.GetPartCount(PartType.Engine) >= engine &&
               warehouse.GetPartCount(PartType.Body) >= body &&
               warehouse.GetPartCount(PartType.Wheels) >= wheels &&
               warehouse.GetPartCount(PartType.Electronics) >= electronics;
    }

    private void ConsumePartsForUpgrade(int engine, int body, int wheels, int electronics)
    {
        if (warehouse == null) return;
        warehouse.AddParts(PartType.Engine, -engine);
        warehouse.AddParts(PartType.Body, -body);
        warehouse.AddParts(PartType.Wheels, -wheels);
        warehouse.AddParts(PartType.Electronics, -electronics);
    }

    private void TryBuyConveyorUpgrade()
    {
        economy.BuyConveyorUpgrade();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
        UpdateUpgradeUI();
    }


    private void Update()
    {
        UpdateDemoUI();
    }


    private void TryHireEngineer()
    {
        economy.HireEngineer();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
        UpdateUpgradeUI();
    }

    // ---- Производство деталей ----
    private void TryProducePart(PartType type)
    {
        int count = 1;
        if (WarehouseManager.Instance.ProduceParts(type, count))
        {
            // всё уже обновлено внутри
        }
    }

    // ---- Продажа деталей ----
    private void TrySellPart(PartType type)
    {
        int count = 1;
        float price = GetMarketPrice(type);
        WarehouseManager.Instance.SellParts(type, count, price);
    }

    private float GetMarketPrice(PartType type)
    {
        float basePrice = type switch
        {
            PartType.Engine => 30f,
            PartType.Body => 25f,
            PartType.Wheels => 20f,
            PartType.Electronics => 35f,
            _ => 20f
        };
        return basePrice * economy.TotalPriceModifier;
    }

    // ---- Переключение вкладок ----
    private void SwitchCompetitorTab(bool showCompetitors)
    {
        if (competitorsContent != null)
            competitorsContent.style.display = showCompetitors ? DisplayStyle.Flex : DisplayStyle.None;
        if (actionLogContent != null)
            actionLogContent.style.display = showCompetitors ? DisplayStyle.None : DisplayStyle.Flex;

        if (showCompetitors)
            competitor.RefreshCompetitorsList();
        else
            RefreshActionLogs();
    }

    private void SwitchUpgradeTab(bool showFactory)
    {
        if (upgradeFactoryContent != null)
            upgradeFactoryContent.style.display = showFactory ? DisplayStyle.Flex : DisplayStyle.None;
        if (upgradePartsContent != null)
            upgradePartsContent.style.display = showFactory ? DisplayStyle.None : DisplayStyle.Flex;
    }

    // ---- Достижения ----
    private void OpenAchievementsWindow()
    {
        HideAllOverlays();
        if (achievementsOverlay != null)
        {
            achievementsOverlay.style.display = DisplayStyle.Flex;
            AnimateWindowOpen(achievementsOverlay);
            RefreshAchievements();
        }
    }

    private void CloseAchievementsWindow()
    {
        if (achievementsOverlay != null)
            AnimateWindowClose(achievementsOverlay, () => achievementsOverlay.style.display = DisplayStyle.None);
    }

    private void RefreshAchievements()
    {
        if (achievementsContainer == null) return;
        achievementsContainer.Clear();

        var manager = CarCompanyManager.Instance.AchievementManager;
        if (manager == null)
        {
            achievementsContainer.Add(new Label("Менеджер достижений не найден."));
            return;
        }

        var allAchievements = manager.GetAllAchievements();
        if (allAchievements == null || allAchievements.Length == 0)
        {
            achievementsContainer.Add(new Label("Нет доступных достижений."));
            return;
        }

        foreach (var ach in allAchievements)
        {
            var progress = manager.GetProgress(ach.achievementId);
            if (progress == null) continue;

            VisualElement card = new VisualElement();
            card.style.backgroundColor = progress.isUnlocked
                ? new StyleColor(new Color(0.15f, 0.35f, 0.15f, 0.9f))
                : new StyleColor(new Color(0.25f, 0.25f, 0.25f, 0.9f));
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.paddingLeft = 8;
            card.style.paddingRight = 8;
            card.style.marginBottom = 8;
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;
            card.style.alignItems = Align.Center;
            card.style.flexDirection = FlexDirection.Column;

            if (ach.hiddenUntilUnlocked && !progress.isUnlocked)
            {
                VisualElement placeholder = new VisualElement();
                placeholder.style.width = 64;
                placeholder.style.height = 64;
                placeholder.style.backgroundColor = new StyleColor(Color.gray);
                placeholder.style.borderTopLeftRadius = 8;
                placeholder.style.borderTopRightRadius = 8;
                placeholder.style.borderBottomLeftRadius = 8;
                placeholder.style.borderBottomRightRadius = 8;
                card.Add(placeholder);

                Label secretLabel = new Label("???");
                secretLabel.style.fontSize = 18;
                secretLabel.style.color = Color.white;
                secretLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                secretLabel.style.marginTop = 8;
                card.Add(secretLabel);

                Label secretDesc = new Label("Секретное достижение");
                secretDesc.style.fontSize = 12;
                secretDesc.style.color = new Color(0.7f, 0.7f, 0.7f);
                secretDesc.style.marginTop = 4;
                card.Add(secretDesc);
            }
            else
            {
                if (ach.icon != null)
                {
                    Image icon = new Image();
                    icon.sprite = ach.icon;
                    icon.style.width = 64;
                    icon.style.height = 64;
                    icon.style.marginBottom = 8;
                    card.Add(icon);
                }
                else
                {
                    VisualElement placeholder = new VisualElement();
                    placeholder.style.width = 64;
                    placeholder.style.height = 64;
                    placeholder.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                    placeholder.style.borderTopLeftRadius = 8;
                    placeholder.style.borderTopRightRadius = 8;
                    placeholder.style.borderBottomLeftRadius = 8;
                    placeholder.style.borderBottomRightRadius = 8;
                    placeholder.style.marginBottom = 8;
                    card.Add(placeholder);
                }

                Label titleLabel = new Label(ach.title);
                titleLabel.style.fontSize = 16;
                titleLabel.style.color = progress.isUnlocked ? Color.green : Color.white;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.marginBottom = 2;
                titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                card.Add(titleLabel);

                Label descLabel = new Label(ach.description);
                descLabel.style.fontSize = 12;
                descLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                descLabel.style.marginBottom = 6;
                card.Add(descLabel);

                if (!progress.isUnlocked)
                {
                    Label progressLabel = new Label($"Прогресс: {progress.currentValue} / {ach.targetValue}");
                    progressLabel.style.fontSize = 12;
                    progressLabel.style.color = new Color(0.9f, 0.9f, 0.2f);
                    progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(progressLabel);
                }
                else
                {
                    Label unlockedLabel = new Label("✅ Получено!");
                    unlockedLabel.style.fontSize = 14;
                    unlockedLabel.style.color = Color.green;
                    unlockedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    unlockedLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(unlockedLabel);
                }
            }

            achievementsContainer.Add(card);
        }
    }

    // ---- Журнал действий ----
    private void RefreshActionLogs()
    {
        if (actionLogContent == null) return;
        actionLogContent.Clear();
        actionLogContent.style.display = DisplayStyle.Flex;

        var logManager = CarCompanyManager.Instance.ActionLogManager;
        if (logManager == null)
        {
            actionLogContent.Add(new Label("Менеджер логов не найден.") 
            { 
                style = { color = Color.white, fontSize = 14, unityTextAlign = TextAnchor.MiddleCenter } 
            });
            return;
        }

        var logs = logManager.GetLogsForCurrentYear();
        if (logs == null || logs.Count == 0)
        {
            actionLogContent.Add(new Label("За текущий год не было действий конкурентов.") 
            { 
                style = { color = Color.white, fontSize = 14, unityTextAlign = TextAnchor.MiddleCenter, marginTop = 20 } 
            });
            return;
        }

        // Заголовок
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;
        headerRow.style.paddingLeft = 8;
        headerRow.style.paddingRight = 8;
        headerRow.style.marginBottom = 4;
        headerRow.style.borderBottomWidth = 1;
        headerRow.style.borderBottomColor = Color.gray;

        string[] headers = { "Месяц", "Компания", "Действие", "Результат" };
        float[] widths = { 50, 100, 120, 1f };
        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.fontSize = 14;
            if (i < headers.Length - 1)
                lbl.style.width = widths[i];
            else
                lbl.style.flexGrow = 1;
            headerRow.Add(lbl);
        }
        actionLogContent.Add(headerRow);

        // Строки логов
        foreach (var entry in logs)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 2;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.backgroundColor = entry.success ? new Color(0.2f, 0.3f, 0.2f) : new Color(0.3f, 0.2f, 0.2f);
            // Исправлено: вместо borderRadius используем отдельные свойства
            row.style.borderTopLeftRadius = 3;
            row.style.borderTopRightRadius = 3;
            row.style.borderBottomLeftRadius = 3;
            row.style.borderBottomRightRadius = 3;

            Label dateLabel = new Label($"{entry.gameMonth:D2}/{entry.gameYear}");
            dateLabel.style.width = 50;
            dateLabel.style.color = Color.white;
            dateLabel.style.fontSize = 13;
            row.Add(dateLabel);

            Label compLabel = new Label(entry.competitorName);
            compLabel.style.width = 100;
            compLabel.style.color = Color.white;
            compLabel.style.fontSize = 13;
            row.Add(compLabel);

            Label actionLabel = new Label(entry.actionType);
            actionLabel.style.width = 120;
            actionLabel.style.color = Color.white;
            actionLabel.style.fontSize = 13;
            row.Add(actionLabel);

            Label resultLabel = new Label(entry.resultDescription);
            resultLabel.style.flexGrow = 1;
            resultLabel.style.color = Color.white;
            resultLabel.style.fontSize = 13;
            row.Add(resultLabel);

            actionLogContent.Add(row);
        }
    }

    // ---- Основные методы обновления UI ----
    public void ShowWelcomeScreen()
    {
        if (welcomeOverlay != null)
        {
            welcomeOverlay.style.display = DisplayStyle.Flex;
            AnimateWindowOpen(welcomeOverlay);
        }
    }

    public void CloseWelcomeScreen()
    {
        if (welcomeOverlay != null) welcomeOverlay.style.display = DisplayStyle.None;
    }

    public void UpdateMoneyLabels()
    {
        if (moneyLabel != null) moneyLabel.text = $"Денег: ${economy.Money:F0}";
        if (incomeLabel != null) incomeLabel.text = $"Авто/сек: {economy.PassiveIncome:F1}";
        UpdateReputationLabel();
    }

    public void UpdateSavedDifficultyLabel()
    {
        UpdateDifficultyDisplay();
    }

    public void UpdateUpgradeUI()
    {
        if (conveyorLevelLabel != null) conveyorLevelLabel.text = $"Уровень конвейера: {economy.ConveyorLevel}";
        if (engineerCountLabel != null) engineerCountLabel.text = $"Инженеров: {economy.EngineerCount}";

        if (buyConveyorButton != null)
        {
            int cost = Mathf.RoundToInt((10 + economy.ConveyorLevel * 5) * economy.CostMultiplier);
            buyConveyorButton.text = $"Улучшить конвейер (${cost})";
        }

        if (hireEngineerButton != null)
        {
            int cost = Mathf.RoundToInt((50 + economy.EngineerCount * 20) * economy.CostMultiplier);
            hireEngineerButton.text = $"Нанять инженера (${cost})";
        }

        if (buyPartsButton != null)
        {
            int cost = Mathf.RoundToInt(100 * economy.CostMultiplier);
            buyPartsButton.text = $"Купить детали (${cost})";
        }
    }

    public void UpdateEventUI(string eventText, float multiplier)
    {
        if (eventLabel != null)
        {
            eventLabel.text = eventText;
            if (multiplier < 0.8f) eventLabel.style.color = new StyleColor(Color.red);
            else if (multiplier > 1.2f) eventLabel.style.color = new StyleColor(Color.green);
            else eventLabel.style.color = new StyleColor(Color.white);
        }
    }

    public void UpdateCountLabel(int count)
    {
        if (countLabel != null) countLabel.text = count.ToString();
    }

    public void UpdateProductionButtons(bool canIncrease)
    {
        if (increaseCountBtn != null) increaseCountBtn.SetEnabled(canIncrease);
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
        root.schedule.Execute(() =>
        {
            if (notificationLabel.parent != null) notificationLabel.RemoveFromHierarchy();
        }).ExecuteLater(3000);
    }

    public void UpdateDateTimeDisplay()
    {
        if (dateLabel != null && GameTimeManager.Instance != null)
            dateLabel.text = $"Дата: {GameTimeManager.Instance.GetDateString()}";

        if (inflationLabel != null && economy != null)
            inflationLabel.text = $"Инфляция: {(economy.basePriceMultiplier - 1f) * 100:F1}%";

        if (seasonLabel != null && economy != null)
            seasonLabel.text = $"Сезон: {economy.GetSeasonalDemandModifier():F2}x";

        RefreshTechButtons();
        UpdateReputationLabel();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
        UpdateUpgradeUI();
    }

    private string GetTechPriceInfo(Technology tech)
    {
        if (tech == null) return "";
        if (tech.isResearched) return " (Изучено)";

        int currentYear = GameTimeManager.Instance?.currentYear ?? 2025;
        int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
        bool available = tech.IsAvailable(currentYear, currentMonth);

        int baseCost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier);
        int actualCost = baseCost;
        string penaltyInfo = "";

        if (!available)
        {
            actualCost = Mathf.RoundToInt(baseCost * 2f);
            string dateStr = $"{tech.availableMonth:D2}/{tech.availableYear}";
            penaltyInfo = $"\n(Штраф +100%, доступно без штрафа: {dateStr})";
        }
        else
        {
            penaltyInfo = "\n(без штрафа)";
        }

        return $"\nЦена: ${actualCost} (базовая ${baseCost}){penaltyInfo}";
    }


    // ---- Карточки машин ----
    public void CreateCarCards(CarBlueprint[] availableCars)
    {
        if (carsContainer == null) return;
        carsContainer.Clear();
        carCards.Clear();
        bool upgradeUnlocked = tech.IsCarUpgradeUnlocked();

        foreach (CarBlueprint car in availableCars)
        {
            if (car == null) continue;

            bool hasHigherLevel = availableCars.Any(c => c != car && c.carName == car.carName && c.currentLevel > car.currentLevel);
            bool canUpgrade = !hasHigherLevel
                && (car.levels != null && car.levels.Length > 0
                    && car.currentLevel < car.levels.Length)
                && upgradeUnlocked;

            VisualElement card = new VisualElement();
            card.AddToClassList("car-card");
            card.style.backgroundColor = new StyleColor(new Color(0.25f, 0.25f, 0.25f));
            card.style.borderTopLeftRadius = 6;
            card.style.borderTopRightRadius = 6;
            card.style.borderBottomLeftRadius = 6;
            card.style.borderBottomRightRadius = 6;
            card.style.paddingTop = 8;
            card.style.paddingBottom = 8;
            card.style.paddingLeft = 12;
            card.style.paddingRight = 12;
            card.style.marginBottom = 6;
            card.style.flexDirection = FlexDirection.Column;
            card.style.alignItems = Align.Stretch;

            VisualElement topRow = new VisualElement();
            topRow.style.flexDirection = FlexDirection.Row;
            topRow.style.alignItems = Align.Center;

            Sprite iconSprite = car.carIcon != null ? car.carIcon : LoadCarIcon(car.carName);
            if (iconSprite != null)
            {
                Image iconImage = new Image();
                iconImage.sprite = iconSprite;
                iconImage.style.width = 50;
                iconImage.style.height = 50;
                iconImage.style.marginRight = 12;
                topRow.Add(iconImage);
            }
            else
            {
                VisualElement placeholder = new VisualElement();
                placeholder.style.width = 50;
                placeholder.style.height = 50;
                placeholder.style.marginRight = 12;
                placeholder.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                topRow.Add(placeholder);
            }

            VisualElement textContainer = new VisualElement();
            textContainer.style.flexDirection = FlexDirection.Column;
            textContainer.style.flexGrow = 1;

            CarRecipe currentRecipe = car.recipe;
            var req = GetRequirements(currentRecipe);

            Label partsLabel = new Label($"Требуется: 🔧{req.engine} 🔩{req.body} ⚙️{req.wheels} 💻{req.electronics}");
            partsLabel.style.fontSize = 10;
            partsLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            textContainer.Add(partsLabel);

            Label typeLabel = new Label(car.carType != null ? car.carType.typeName : "Без типа");
            typeLabel.style.fontSize = 11;
            typeLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            textContainer.Add(typeLabel);

            int assemblyCost = car.GetModifiedAssemblyCost();
            Label costLabel = new Label($"Сборка: ${assemblyCost}");
            costLabel.style.fontSize = 10;
            costLabel.style.color = new Color(0.9f, 0.7f, 0.3f);
            textContainer.Add(costLabel);

            // ========== БЛОК ВЫБОРА КОРОБКИ ПЕРЕДАЧ ==========
            VisualElement transmissionRow = new VisualElement();
            transmissionRow.style.flexDirection = FlexDirection.Row;
            transmissionRow.style.alignItems = Align.Center;
            transmissionRow.style.marginTop = 2;

            Label transmissionLabel = new Label("Коробка:");
            transmissionLabel.style.color = Color.white;
            transmissionLabel.style.fontSize = 11;
            transmissionLabel.style.marginRight = 8;

            DropdownField transmissionDropdown = new DropdownField();
            transmissionDropdown.style.width = 100;
            transmissionDropdown.style.fontSize = 11;

            List<string> transmissionOptions = new List<string>();
            string[] allTransmissions = { "АКПП", "МКПП", "Робот", "Вариатор" };
            string[] techNames = { "AKPP", "MKPP", "ROBOT", "VARIATOR" };
            for (int i = 0; i < allTransmissions.Length; i++)
            {
                if (tech.IsTechResearched(techNames[i]))
                    transmissionOptions.Add(allTransmissions[i]);
            }
            if (transmissionOptions.Count == 0)
            {
                transmissionOptions.Add("АКПП");
            }

            transmissionDropdown.choices = transmissionOptions;
            if (!string.IsNullOrEmpty(car.transmissionType) && transmissionOptions.Contains(car.transmissionType))
                transmissionDropdown.value = car.transmissionType;
            else
                transmissionDropdown.value = transmissionOptions[0];

            transmissionDropdown.RegisterValueChangedCallback(evt =>
            {
                car.transmissionType = evt.newValue;
            });

            transmissionRow.Add(transmissionLabel);
            transmissionRow.Add(transmissionDropdown);
            textContainer.Add(transmissionRow);
            // =================================================

            Label nameLabel = new Label(car.GetDisplayName());
            nameLabel.style.fontSize = 15;
            nameLabel.style.color = Color.white;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
            int modCost = Mathf.RoundToInt(car.GetProductionCostWithLevel() * economy.CostMultiplier);
            Label detailsLabel = new Label($"Цена: ${modPrice}  |  Себ: ${modCost}");
            detailsLabel.style.fontSize = 12;
            detailsLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            textContainer.Add(detailsLabel);

            // ========== БЛОК РЕГУЛИРОВКИ ЦЕНЫ ==========
            VisualElement priceControlRow = new VisualElement();
            priceControlRow.style.flexDirection = FlexDirection.Row;
            priceControlRow.style.alignItems = Align.Center;
            priceControlRow.style.marginTop = 4;

            Label priceLabel = new Label($"${car.currentPrice}");
            priceLabel.style.fontSize = 14;
            priceLabel.style.color = Color.yellow;
            priceLabel.style.marginRight = 8;
            priceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            Button decreasePriceBtn = new Button();
            decreasePriceBtn.text = "−";
            decreasePriceBtn.style.width = 24;
            decreasePriceBtn.style.height = 24;
            decreasePriceBtn.style.fontSize = 14;
            decreasePriceBtn.style.unityFontStyleAndWeight = FontStyle.Bold;

            Button increasePriceBtn = new Button();
            increasePriceBtn.text = "+";
            increasePriceBtn.style.width = 24;
            increasePriceBtn.style.height = 24;
            increasePriceBtn.style.fontSize = 14;
            increasePriceBtn.style.unityFontStyleAndWeight = FontStyle.Bold;

            CarBlueprint localCar = car;

            decreasePriceBtn.clicked += () => ChangeCarPrice(localCar, -10);
            increasePriceBtn.clicked += () => ChangeCarPrice(localCar, 10);

            priceControlRow.Add(priceLabel);
            priceControlRow.Add(decreasePriceBtn);
            priceControlRow.Add(increasePriceBtn);
            textContainer.Add(priceControlRow);
            // =========================================

            double profit = modPrice - modCost;
            Label profitLabel = new Label($"Прибыль: {profit:F0}");
            profitLabel.style.fontSize = 13;
            profitLabel.style.color = profit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
            profitLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            Label profitDetailsLabel = new Label();
            profitDetailsLabel.style.fontSize = 10;
            profitDetailsLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
            profitDetailsLabel.style.marginTop = 2;
            profitDetailsLabel.style.display = DisplayStyle.Flex;
            textContainer.Add(profitDetailsLabel);

            Label demandLabel = new Label($"Спрос: {car.CurrentDemandMultiplier:F1}x");
            demandLabel.style.fontSize = 11;
            demandLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
            textContainer.Add(demandLabel);

            Label levelLabel = new Label($"Уровень: {car.currentLevel + 1}");
            levelLabel.style.fontSize = 11;
            levelLabel.style.color = new Color(0.6f, 0.8f, 1f);
            textContainer.Add(levelLabel);

            Label trendLabel = new Label("тренд: ...");
            trendLabel.style.fontSize = 10;
            trendLabel.style.color = Color.gray;
            textContainer.Add(trendLabel);

            Label taxRateLabel = new Label($"Налог: {economy.GetTaxRate(car):P0}");
            taxRateLabel.style.fontSize = 10;
            taxRateLabel.style.color = new Color(0.9f, 0.5f, 0.2f);
            textContainer.Add(taxRateLabel);

            Label modifierLabel = new Label();
            modifierLabel.style.fontSize = 10;
            modifierLabel.style.color = new Color(1f, 0.8f, 0.2f);
            modifierLabel.style.marginTop = 2;
            modifierLabel.style.display = DisplayStyle.None;
            textContainer.Add(modifierLabel);

            // ---- Рейтинг (размещён в textContainer) ----
            Label ratingLabelElement = new Label($"Рейтинг: {car.CalculateRating(economy.TotalPriceModifier):F1}");
            ratingLabelElement.style.fontSize = 11;
            ratingLabelElement.style.color = new Color(1f, 0.8f, 0.2f);
            textContainer.Add(ratingLabelElement);

            textContainer.Add(nameLabel);

            topRow.Add(textContainer);

            Button upgradeButton = new Button();
            upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            upgradeButton.style.width = 70;
            upgradeButton.style.height = 26;
            upgradeButton.style.marginLeft = 8;
            upgradeButton.style.alignSelf = Align.Center;
            upgradeButton.SetEnabled(canUpgrade);
            upgradeButton.clicked += () => tech.UpgradeCar(localCar);
            topRow.Add(upgradeButton);

            // ---- Кнопка "Произвести" ----
            Button produceButton = new Button();
            produceButton.text = "Произвести";
            produceButton.style.width = 70;
            produceButton.style.height = 26;
            produceButton.style.marginLeft = 4;
            produceButton.style.alignSelf = Align.Center;
            produceButton.clicked += () => TryProduceSpecificCar(localCar);
            topRow.Add(produceButton);

            // ---- Кнопка "Продать 1" ----
            Button sellButton = new Button();
            sellButton.text = "Продать 1";
            sellButton.style.width = 70;
            sellButton.style.height = 26;
            sellButton.style.marginLeft = 4;
            sellButton.style.alignSelf = Align.Center;
            sellButton.clicked += () =>
            {
                int sold = WarehouseManager.Instance.SellCars(localCar, 1);
                if (sold > 0)
                {
                    UpdateWarehouseLabels();
                    UpdateMoneyLabels();
                    UpdateCarCards();
                }
                else
                {
                    ShowNotification("Нет машин этой модели на складе!");
                }
            };
            topRow.Add(sellButton);

            // ---- Отображение количества на складе ----
            Label stockLabel = new Label($"На складе: {WarehouseManager.Instance.GetCarCount(localCar)}");
            stockLabel.style.fontSize = 10;
            stockLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            stockLabel.style.marginTop = 2;
            textContainer.Add(stockLabel);

            card.Add(topRow);

            // ---- Панель тюнинга ----
            VisualElement tuningPanel = new VisualElement();
            tuningPanel.style.flexDirection = FlexDirection.Column;
            tuningPanel.style.marginTop = 3;
            tuningPanel.style.marginBottom = 3;
            tuningPanel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
            tuningPanel.style.paddingTop = 3;
            tuningPanel.style.paddingBottom = 3;
            tuningPanel.style.paddingLeft = 3;
            tuningPanel.style.paddingRight = 3;
            tuningPanel.style.borderTopLeftRadius = 4;
            tuningPanel.style.borderTopRightRadius = 4;
            tuningPanel.style.borderBottomLeftRadius = 4;
            tuningPanel.style.borderBottomRightRadius = 4;

            CarCardData cardData = new CarCardData();
            cardData.car = car;
            cardData.card = card;
            cardData.profitLabel = profitLabel;
            cardData.profitDetailsLabel = profitDetailsLabel;
            cardData.demandLabel = demandLabel;
            cardData.trendLabel = trendLabel;
            cardData.levelLabel = levelLabel;
            cardData.upgradeButton = upgradeButton;
            cardData.taxRateLabel = taxRateLabel;
            cardData.modifierLabel = modifierLabel;
            cardData.ratingLabel = ratingLabelElement; // ТОЛЬКО ОДНА СТРОКА

            // Сохраняем ссылки
            cardData.detailsLabel = detailsLabel;
            cardData.priceLabel = priceLabel;
            cardData.decreasePriceBtn = decreasePriceBtn;
            cardData.increasePriceBtn = increasePriceBtn;
            cardData.produceButton = produceButton;
            cardData.sellButton = sellButton;
            cardData.stockLabel = stockLabel;
            cardData.transmissionDropdown = transmissionDropdown;

            // ---- Тюнинг-слайдеры ----
            for (int i = 0; i < tuningParamNames.Length; i++)
            {
                string param = tuningParamNames[i];
                string display = tuningParamDisplay[i];

                VisualElement row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.marginBottom = 1;

                Label nameLabelParam = new Label(display);
                nameLabelParam.style.width = 40;
                nameLabelParam.style.color = Color.white;
                nameLabelParam.style.fontSize = 9;
                row.Add(nameLabelParam);

                SliderInt slider = new SliderInt();
                slider.lowValue = 0;
                slider.highValue = GetMaxTuning(car, param);
                slider.value = GetCurrentTuning(car, param);
                slider.style.flexGrow = 1;
                slider.style.marginLeft = 2;
                slider.style.marginRight = 2;
                row.Add(slider);

                Label valueLabel = new Label(GetCurrentTuning(car, param).ToString());
                valueLabel.style.width = 16;
                valueLabel.style.color = Color.white;
                valueLabel.style.fontSize = 10;
                valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                row.Add(valueLabel);

                Label maxLabel = new Label($"/{GetMaxTuning(car, param)}");
                maxLabel.style.width = 20;
                maxLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
                maxLabel.style.fontSize = 9;
                row.Add(maxLabel);

                Button upgradeBtn = new Button();
                upgradeBtn.text = "↑";
                upgradeBtn.style.width = 20;
                upgradeBtn.style.height = 20;
                upgradeBtn.style.marginLeft = 1;
                upgradeBtn.style.fontSize = 12;
                upgradeBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(upgradeBtn);

                tuningPanel.Add(row);

                switch (i)
                {
                    case 0:
                        cardData.powerSlider = slider;
                        cardData.powerValueLabel = valueLabel;
                        cardData.powerMaxLabel = maxLabel;
                        cardData.powerUpgradeBtn = upgradeBtn;
                        break;
                    case 1:
                        cardData.economySlider = slider;
                        cardData.economyValueLabel = valueLabel;
                        cardData.economyMaxLabel = maxLabel;
                        cardData.economyUpgradeBtn = upgradeBtn;
                        break;
                    case 2:
                        cardData.designSlider = slider;
                        cardData.designValueLabel = valueLabel;
                        cardData.designMaxLabel = maxLabel;
                        cardData.designUpgradeBtn = upgradeBtn;
                        break;
                    case 3:
                        cardData.safetySlider = slider;
                        cardData.safetyValueLabel = valueLabel;
                        cardData.safetyMaxLabel = maxLabel;
                        cardData.safetyUpgradeBtn = upgradeBtn;
                        break;
                }

                CarBlueprint localCarTuning = car;
                string localParam = param;
                slider.RegisterValueChangedCallback(evt =>
                {
                    int newVal = evt.newValue;
                    SetCurrentTuning(localCarTuning, localParam, newVal);
                    valueLabel.text = newVal.ToString();
                    UpdateCarCards();
                    UpdateMoneyLabels();
                });

                upgradeBtn.clicked += () => tech.UpgradeTuning(localCarTuning, localParam);
            }

            card.Add(tuningPanel);

            // ---- Цвета и тонировка ----
            VisualElement colorPanel = new VisualElement();
            colorPanel.style.flexDirection = FlexDirection.Row;
            colorPanel.style.alignItems = Align.Center;
            colorPanel.style.marginTop = 5;
            colorPanel.style.marginBottom = 5;

            Label colorLabel = new Label("Цвет:");
            colorLabel.style.color = Color.white;
            colorLabel.style.marginRight = 8;
            colorLabel.style.fontSize = 12;
            colorPanel.Add(colorLabel);

            List<Button> colorBtns = new List<Button>();
            for (int i = 0; i < carColors.Length; i++)
            {
                Button colorBtn = new Button();
                colorBtn.style.width = 24;
                colorBtn.style.height = 24;
                colorBtn.style.marginRight = 4;
                colorBtn.style.backgroundColor = carColors[i];
                colorBtn.style.borderTopLeftRadius = 4;
                colorBtn.style.borderTopRightRadius = 4;
                colorBtn.style.borderBottomLeftRadius = 4;
                colorBtn.style.borderBottomRightRadius = 4;
                colorBtn.tooltip = colorNames[i];
                int index = i;
                colorBtn.clicked += () =>
                {
                    car.bodyColor = carColors[index];
                    UpdateColorButtons(cardData, index);
                    UpdateCarCards();
                };
                colorPanel.Add(colorBtn);
                colorBtns.Add(colorBtn);
            }
            cardData.colorButtons = colorBtns.ToArray();

            Toggle tintToggle = new Toggle("Тонировка");
            tintToggle.style.color = Color.white;
            tintToggle.style.marginLeft = 10;
            tintToggle.value = car.hasTint;
            tintToggle.RegisterValueChangedCallback(evt =>
            {
                car.hasTint = evt.newValue;
                UpdateCarCards();
            });
            colorPanel.Add(tintToggle);
            cardData.tintToggle = tintToggle;

            card.Add(colorPanel);

            // ---- График спроса ----
            VisualElement graphContainer = new VisualElement();
            graphContainer.style.width = new Length(100, LengthUnit.Percent);
            graphContainer.style.height = new Length(40, LengthUnit.Pixel);
            graphContainer.style.marginTop = 3;
            card.Add(graphContainer);
            cardData.graphContainer = graphContainer;

            // ---- Завершение карточки ----
            carCards.Add(cardData);

            carsContainer.Add(card);
        }

        // Обновляем карточки и цвета
        UpdateCarCards();
        foreach (var cardData in carCards)
        {
            int selectedIndex = Array.IndexOf(carColors, cardData.car.bodyColor);
            UpdateColorButtons(cardData, selectedIndex);
        }
    }

   
   public void RefreshMarketGraphIfOpen()
    {
        if (marketVisOverlay != null && marketVisOverlay.style.display == DisplayStyle.Flex)
        {
            var visController = GetComponent<MarketVisualizationController>();
            if (visController != null)
            {
                visController.DrawIncomeGraph(economy.monthlyIncomeHistory);
            }
        }
    }


    public void UpdateCarCards()
    {
        bool upgradeUnlocked = tech.IsCarUpgradeUnlocked();

        foreach (var cardData in carCards)
        {
            if (cardData.car == null) continue;
            CarBlueprint car = cardData.car;

            // ---- Расчёт чистой прибыли ----
            float partCost = economy.GetPartCostForCar(car);
            float productionCost = car.GetProductionCostWithLevel() + partCost;
            int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
            double profitBeforeTax = modPrice - productionCost;
            float taxRate = economy.GetTaxRate(car);
            double finalProfit = profitBeforeTax * (1f - taxRate);

            // ---- Обновление рейтинга ----
            if (cardData.ratingLabel != null)
            {
                float rating = car.CalculateRating(economy.TotalPriceModifier);
                cardData.ratingLabel.text = $"Рейтинг: {rating:F1}";
                // Цвет в зависимости от оценки
                if (rating >= 8f)
                    cardData.ratingLabel.style.color = new Color(0.2f, 0.9f, 0.2f);
                else if (rating >= 5f)
                    cardData.ratingLabel.style.color = new Color(1f, 0.8f, 0.2f);
                else
                    cardData.ratingLabel.style.color = new Color(1f, 0.3f, 0.3f);
            }

            // Обновляем количество на складе
            if (cardData.stockLabel != null)
                cardData.stockLabel.text = $"На складе: {WarehouseManager.Instance.GetCarCount(car)}";

            // Кнопка продажи активна только если есть машины
            if (cardData.sellButton != null)
                cardData.sellButton.SetEnabled(WarehouseManager.Instance.GetCarCount(car) > 0);

            // Обновляем прибыль
            if (cardData.profitLabel != null)
            {
                cardData.profitLabel.text = $"Прибыль: {finalProfit:F0}";
                cardData.profitLabel.style.color = finalProfit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
            }
            if (cardData.profitDetailsLabel != null)
            {
                cardData.profitDetailsLabel.text = $"Цена: {modPrice:F0}  |  Себ: {productionCost:F0} (вкл. детали)  |  Налог: {(profitBeforeTax * taxRate):F0}  |  Итого: {finalProfit:F0}";
                cardData.profitDetailsLabel.style.display = DisplayStyle.Flex;
            }
            if (cardData.profitLabel != null)
            {
                cardData.profitLabel.tooltip = $"Цена: {modPrice:F0}\nСебестоимость: {productionCost:F0}\nНалог: {profitBeforeTax * taxRate:F0}\nПрибыль: {finalProfit:F0}";
            }

            // Спрос
            float demandValue = car.CurrentDemandMultiplier;
            if (cardData.demandLabel != null)
            {
                cardData.demandLabel.text = $"Спрос: {demandValue:F1}x";
                cardData.demandLabel.style.color = demandValue > 1.2f ? Color.green : (demandValue < 0.8f ? Color.red : Color.yellow);
            }

            // Уровень
            if (cardData.levelLabel != null)
                cardData.levelLabel.text = $"Уровень: {car.currentLevel + 1}";

            // Тренд
            if (cardData.trendLabel != null && MarketSystem.Instance != null)
            {
                string trend = MarketSystem.Instance.GetDemandTrend(car.carName);
                cardData.trendLabel.text = $"Тренд: {trend}";
                cardData.trendLabel.style.color = trend.Contains("растёт") ? Color.green : (trend.Contains("падает") ? Color.red : Color.gray);
            }

            // Кнопка улучшения
            if (cardData.upgradeButton != null)
            {
                bool hasHigherLevel = carCards.Any(cd => cd.car != car && cd.car.carName == car.carName && cd.car.currentLevel > car.currentLevel);
                bool canUpgrade = !hasHigherLevel
                    && (car.levels != null && car.levels.Length > 0
                        && car.currentLevel < car.levels.Length)
                    && upgradeUnlocked;

                cardData.upgradeButton.SetEnabled(canUpgrade);
                cardData.upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            }

            // Налог
            if (cardData.taxRateLabel != null)
                cardData.taxRateLabel.text = $"Налог: {taxRate:P0}";

            // График
            if (cardData.graphContainer != null && MarketSystem.Instance != null)
                MarketSystem.Instance.DrawDemandGraph(cardData.graphContainer, car.carName);

            // Тюнинг слайдеры
            UpdateTuningSliders(cardData, car);

            // Цвет и тонировка
            if (cardData.tintToggle != null)
                cardData.tintToggle.value = car.hasTint;
            if (cardData.colorButtons != null)
            {
                int selectedIndex = Array.IndexOf(carColors, car.bodyColor);
                UpdateColorButtons(cardData, selectedIndex);
            }

            // Модификаторы
            if (cardData.modifierLabel != null)
            {
                string modText = "";
                bool hasMod = false;
                float penalty = 0f;
                if (demand.demandPenalties != null && demand.demandPenalties.TryGetValue(car.carName, out penalty))
                {
                    modText += $"⚔️ Спрос -{(1f - penalty) * 100:F0}% ";
                    hasMod = true;
                }
                if (economy.TemporaryPriceModifier != 1f)
                {
                    float change = (economy.TemporaryPriceModifier - 1f) * 100f;
                    string sign = change > 0 ? "+" : "";
                    modText += $"📊 Цена {sign}{change:F0}% ";
                    hasMod = true;
                }
                if (hasMod)
                {
                    cardData.modifierLabel.text = modText;
                    cardData.modifierLabel.style.display = DisplayStyle.Flex;
                }
                else
                {
                    cardData.modifierLabel.text = "";
                    cardData.modifierLabel.style.display = DisplayStyle.None;
                }
            }

            // ========== ОБНОВЛЕНИЕ ЦЕНЫ ==========
            if (cardData.detailsLabel != null)
            {
                int currentModPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
                int currentModCost = Mathf.RoundToInt(car.GetProductionCostWithLevel() * economy.CostMultiplier);
                cardData.detailsLabel.text = $"Цена: ${currentModPrice}  |  Себ: ${currentModCost}";
            }

            if (cardData.priceLabel != null)
            {
                cardData.priceLabel.text = $"${car.currentPrice}";
            }
            // ======================================

            // ========== СИНХРОНИЗАЦИЯ ВЫПАДАЮЩЕГО СПИСКА КОРОБОК ПЕРЕДАЧ ==========
            if (cardData.transmissionDropdown != null)
            {
                List<string> newOptions = new List<string>();
                string[] allTrans = { "АКПП", "МКПП", "Робот", "Вариатор" };
                string[] techNames = { "AKPP", "MKPP", "ROBOT", "VARIATOR" };
                for (int i = 0; i < allTrans.Length; i++)
                {
                    if (tech.IsTechResearched(techNames[i]))
                        newOptions.Add(allTrans[i]);
                }
                if (newOptions.Count == 0) newOptions.Add("АКПП");
                cardData.transmissionDropdown.choices = newOptions;
                if (!newOptions.Contains(car.transmissionType))
                    car.transmissionType = newOptions[0];
                cardData.transmissionDropdown.value = car.transmissionType;
            }
            // =============================================================
        }

        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
    }


    private void UpdateTuningSliders(CarCardData cardData, CarBlueprint car)
    {
        if (cardData.powerSlider != null)
        {
            cardData.powerSlider.highValue = car.tuningPower;
            cardData.powerSlider.value = car.currentPower;
            cardData.powerValueLabel.text = car.currentPower.ToString();
            cardData.powerMaxLabel.text = $"/{car.tuningPower}";
            bool canUpgrade = car.tuningPower < TUNING_MAX_LEVEL && tech.CanUpgradeTuning(car, "power");
            cardData.powerUpgradeBtn.SetEnabled(canUpgrade);
        }
        if (cardData.economySlider != null)
        {
            cardData.economySlider.highValue = car.tuningEconomy;
            cardData.economySlider.value = car.currentEconomy;
            cardData.economyValueLabel.text = car.currentEconomy.ToString();
            cardData.economyMaxLabel.text = $"/{car.tuningEconomy}";
            bool canUpgrade = car.tuningEconomy < TUNING_MAX_LEVEL && tech.CanUpgradeTuning(car, "economy");
            cardData.economyUpgradeBtn.SetEnabled(canUpgrade);
        }
        if (cardData.designSlider != null)
        {
            cardData.designSlider.highValue = car.tuningDesign;
            cardData.designSlider.value = car.currentDesign;
            cardData.designValueLabel.text = car.currentDesign.ToString();
            cardData.designMaxLabel.text = $"/{car.tuningDesign}";
            bool canUpgrade = car.tuningDesign < TUNING_MAX_LEVEL && tech.CanUpgradeTuning(car, "design");
            cardData.designUpgradeBtn.SetEnabled(canUpgrade);
        }
        if (cardData.safetySlider != null)
        {
            cardData.safetySlider.highValue = car.tuningSafety;
            cardData.safetySlider.value = car.currentSafety;
            cardData.safetyValueLabel.text = car.currentSafety.ToString();
            cardData.safetyMaxLabel.text = $"/{car.tuningSafety}";
            bool canUpgrade = car.tuningSafety < TUNING_MAX_LEVEL && tech.CanUpgradeTuning(car, "safety");
            cardData.safetyUpgradeBtn.SetEnabled(canUpgrade);
        }
    }

    private void UpdateColorButtons(CarCardData cardData, int selectedIndex)
    {
        if (cardData.colorButtons == null) return;
        for (int i = 0; i < cardData.colorButtons.Length; i++)
        {
            if (i == selectedIndex)
            {
                cardData.colorButtons[i].style.borderTopWidth = 2;
                cardData.colorButtons[i].style.borderBottomWidth = 2;
                cardData.colorButtons[i].style.borderLeftWidth = 2;
                cardData.colorButtons[i].style.borderRightWidth = 2;
                cardData.colorButtons[i].style.borderTopColor = Color.white;
                cardData.colorButtons[i].style.borderBottomColor = Color.white;
                cardData.colorButtons[i].style.borderLeftColor = Color.white;
                cardData.colorButtons[i].style.borderRightColor = Color.white;
            }
            else
            {
                cardData.colorButtons[i].style.borderTopWidth = 0;
                cardData.colorButtons[i].style.borderBottomWidth = 0;
                cardData.colorButtons[i].style.borderLeftWidth = 0;
                cardData.colorButtons[i].style.borderRightWidth = 0;
            }
        }
    }

    // ---- Конкуренты ----
    public void RefreshCompetitorsList(List<Competitor> competitors, int playerReputation)
    {
        if (competitorsContent == null)
        {
            Debug.LogWarning("RefreshCompetitorsList: competitorsContent is null");
            return;
        }

        foreach (var row in competitorActionRows)
        {
            if (row.competitor != null && row.dropdown != null)
            {
                selectedActions[row.competitor.companyName] = row.dropdown.index;
            }
        }

        competitorsContent.Clear();
        competitorActionRows.Clear();

        Label playerRepLabel = new Label($"Ваша репутация: {playerReputation}");
        playerRepLabel.style.color = Color.white;
        playerRepLabel.style.fontSize = 14;
        playerRepLabel.style.marginBottom = 10;
        competitorsContent.Add(playerRepLabel);

        if (competitors == null || competitors.Count == 0)
        {
            Label empty = new Label("Нет конкурентов");
            empty.style.color = Color.white;
            empty.style.alignSelf = Align.Center;
            empty.style.marginTop = 20;
            competitorsContent.Add(empty);
            return;
        }

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
        float actionWidth = 25f;
        float otherWidth = (100f - actionWidth) / (headers.Length - 1);

        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            if (i == headers.Length - 1)
                lbl.style.width = new Length(actionWidth, LengthUnit.Percent);
            else
                lbl.style.width = new Length(otherWidth, LengthUnit.Percent);
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(lbl);
        }
        competitorsContent.Add(header);

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
            if (comp == null) continue;

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

            for (int i = 0; i < values.Length; i++)
            {
                Label lbl = new Label(values[i]);
                lbl.style.width = new Length(otherWidth, LengthUnit.Percent);
                lbl.style.color = Color.white;
                lbl.style.fontSize = 12;
                row.Add(lbl);
            }

            VisualElement actionContainer = new VisualElement();
            actionContainer.style.width = new Length(actionWidth, LengthUnit.Percent);
            actionContainer.style.justifyContent = Justify.Center;
            actionContainer.style.alignItems = Align.Center;

            DropdownField actionDropdown = new DropdownField();
            actionDropdown.choices = actionOptions;
            actionDropdown.index = 0;
            actionDropdown.style.width = new Length(100, LengthUnit.Percent);
            actionDropdown.style.height = 28;
            actionDropdown.style.fontSize = 14;
            actionContainer.Add(actionDropdown);
            row.Add(actionContainer);

            competitorsContent.Add(row);
            competitorActionRows.Add((comp, actionDropdown));
        }

        foreach (var row in competitorActionRows)
        {
            if (row.competitor != null && row.dropdown != null)
            {
                string name = row.competitor.companyName;
                if (selectedActions.TryGetValue(name, out int savedIndex))
                {
                    if (savedIndex >= 0 && savedIndex < row.dropdown.choices.Count)
                        row.dropdown.index = savedIndex;
                }
            }
        }
    }

    private void ExecuteAllCompetitorActions()
    {
        int executedCount = 0;
        var rowsCopy = competitorActionRows.ToList();
        foreach (var row in rowsCopy)
        {
            int selectedIndex = row.dropdown.index;
            if (selectedIndex <= 0) continue;

            switch (selectedIndex)
            {
                case 1: competitor.PerformMarketingAttack(row.competitor); break;
                case 2: competitor.PerformBlackPR(row.competitor); break;
                case 3: competitor.ProposeAlliance(row.competitor); break;
                case 4: competitor.StealTechnology(row.competitor); break;
                case 5: competitor.PoachEngineer(row.competitor); break;
            }
            executedCount++;
        }
        if (executedCount == 0)
            ShowNotification("Не выбрано ни одного действия для выполнения.");
        else
            ShowNotification($"Выполнено {executedCount} действий над конкурентами.");
    }

    // ---- Технологии ----
    public void CreateTechTree(List<Technology> technologies, float techCostMultiplier)
    {
        if (techScrollView == null)
        {
            Debug.LogError("techScrollView == null! Проверьте, что в UXML есть элемент с name='TechContainer'");
            return;
        }
        techScrollView.Clear();

        if (technologies == null || technologies.Count == 0)
        {
            Label emptyLabel = new Label("Нет доступных технологий");
            emptyLabel.style.color = Color.white;
            techScrollView.Add(emptyLabel);
            return;
        }

        const float nodeWidth = 220;
        const float nodeHeight = 100;
        const float horizontalGap = 100;
        const float verticalGap = 70;

        Dictionary<Technology, int> techLevels = new Dictionary<Technology, int>();
        foreach (var tech in technologies) if (tech != null) techLevels[tech] = CalculateTechLevel(tech, technologies);
        if (techLevels.Count == 0) return;

        int maxLevel = techLevels.Values.Max();
        Dictionary<int, List<Technology>> levelGroups = new Dictionary<int, List<Technology>>();
        for (int i = 0; i <= maxLevel; i++) levelGroups[i] = new List<Technology>();
        foreach (var kvp in techLevels) levelGroups[kvp.Value].Add(kvp.Key);

        int totalWidth = (maxLevel + 1) * (int)(nodeWidth + horizontalGap) + 50;
        int totalHeight = Mathf.Max(300, technologies.Count * (int)(nodeHeight + verticalGap) + 50);

        techGraphRoot = new VisualElement();
        techGraphRoot.style.width = new Length(totalWidth, LengthUnit.Pixel);
        techGraphRoot.style.position = Position.Relative;
        techGraphRoot.style.overflow = Overflow.Visible;
        techScrollView.Add(techGraphRoot);

        Dictionary<Technology, TechNode> nodeMap = new Dictionary<Technology, TechNode>();
        List<TechNode> techNodes = new List<TechNode>();

        foreach (var tech in technologies)
        {
            if (tech == null) continue;
            TechNode node = new TechNode();
            node.tech = tech;
            node.element = CreateTechNodeElement(tech, techCostMultiplier);
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
                    Technology parentTech = technologies.FirstOrDefault(t => t != null && t.techName == parentName);
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
                float y = i * (nodeHeight + verticalGap) + 5;
                node.position = new Vector2(x, y);
                node.element.style.position = Position.Absolute;
                node.element.style.left = x;
                node.element.style.top = y;
                techGraphRoot.Add(node.element);
            }
        }

        float calculatedMaxY = 0;
        foreach (var node in techNodes)
        {
            if (node == null) continue;
            float bottom = node.position.y + nodeHeight;
            if (bottom > calculatedMaxY) calculatedMaxY = bottom;
        }
        float calculatedHeight = Mathf.Max(calculatedMaxY + 20, 100);
        techGraphRoot.style.height = new Length(calculatedHeight, LengthUnit.Pixel);

        VisualElement lineLayer = new VisualElement();
        lineLayer.style.position = Position.Absolute;
        lineLayer.style.left = 0; lineLayer.style.top = 0; lineLayer.style.right = 0; lineLayer.style.bottom = 0;
        lineLayer.pickingMode = PickingMode.Ignore;
        techGraphRoot.Add(lineLayer);

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

    public void RefreshTechButtons()
    {
        if (techGraphRoot == null) return;
        foreach (var child in techGraphRoot.Children())
        {
            if (child is VisualElement node && node.userData is Technology tech)
            {
                Button btn = node.Q<Button>();
                if (btn != null) UpdateTechButtonState(btn, tech);
            }
        }
    }

    public void CloseAllWindows()
    {
        HideAllOverlays();
        if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
        CloseCarsWindow();
        CloseTechWindow();
        CloseUpgradeWindow();
        CloseSettingsWindow();
        CloseCompetitorsWindow();
        CloseAchievementsWindow();
        CloseMarketingWindow();
        CloseMarketVisWindow();
        CloseLoansWindow();
        CloseInvestmentsWindow();
    }

    // ========== МЕТОДЫ ДЛЯ ОКОН (открытие/закрытие) ==========

    private void OpenCarsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (carsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(carsOverlay);
            demand.UpdateDemand();
            UpdateCarCards();

            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsModuleTutorialCompleted("cars"))
                TutorialManager.Instance.StartModuleTutorial("cars");
        }
    }

    private void CloseCarsWindow()
    {
        if (carsOverlay != null)
            AnimateWindowClose(carsOverlay, () => { carsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenTechWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (techOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(techOverlay);
            RefreshTechButtons();

            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsModuleTutorialCompleted("tech"))
                TutorialManager.Instance.StartModuleTutorial("tech");
        }
    }

    private void CloseTechWindow()
    {
        if (techOverlay != null)
            AnimateWindowClose(techOverlay, () => { techOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenUpgradeWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (upgradeOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(upgradeOverlay);
            UpdateUpgradeUI();
            SwitchUpgradeTab(true);

            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsModuleTutorialCompleted("upgrade"))
                TutorialManager.Instance.StartModuleTutorial("upgrade");
        }
    }

    private void CloseUpgradeWindow()
    {
        if (upgradeOverlay != null)
            AnimateWindowClose(upgradeOverlay, () => { upgradeOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenSettingsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (settingsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(settingsOverlay);
        }
    }

    private void CloseSettingsWindow()
    {
        if (settingsOverlay != null)
            AnimateWindowClose(settingsOverlay, () => { settingsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenCompetitorsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (competitorsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(competitorsOverlay);
            competitor.RefreshCompetitorsList();
            SwitchCompetitorTab(true);

            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsModuleTutorialCompleted("competitors"))
                TutorialManager.Instance.StartModuleTutorial("competitors");
        }
    }

    private void CloseCompetitorsWindow()
    {
        if (competitorsOverlay != null)
            AnimateWindowClose(competitorsOverlay, () => { competitorsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenMarketingWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (marketingOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(marketingOverlay);
            var marketingCtrl = GetComponent<UIMarketingController>();
            if (marketingCtrl != null) marketingCtrl.RefreshUI();

            if (TutorialManager.Instance != null && !TutorialManager.Instance.IsModuleTutorialCompleted("marketing"))
                TutorialManager.Instance.StartModuleTutorial("marketing");
        }
    }

    private void CloseMarketingWindow()
    {
        if (marketingOverlay != null)
            AnimateWindowClose(marketingOverlay, () => 
            { 
                marketingOverlay.style.display = DisplayStyle.None; 
                mainPanel.style.display = DisplayStyle.Flex; 
            });
    }

    // ---- НОВЫЕ ОКНА (релиз 1.13.0) ----
    private void OpenMarketVisualizationWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (marketVisOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(marketVisOverlay);
            var visController = GetComponent<MarketVisualizationController>();
            if (visController != null)
            {
                // Используем столбчатую диаграмму вместо круговой
                visController.UpdateBarChart(competitor.Competitors, competitor.CalculatePlayerMarketShare());
                visController.DrawIncomeGraph(economy.monthlyIncomeHistory);
            }
            else
            {
                Debug.LogWarning("MarketVisualizationController не найден на объекте");
            }
        }
    }

    private void CloseMarketVisWindow()
    {
        if (marketVisOverlay != null)
            AnimateWindowClose(marketVisOverlay, () => 
            { 
                marketVisOverlay.style.display = DisplayStyle.None; 
                mainPanel.style.display = DisplayStyle.Flex; 
            });
    }

    private void OpenLoansWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (loansOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(loansOverlay);
            UpdateLoansUI();
        }
    }

    private void CloseLoansWindow()
    {
        if (loansOverlay != null)
            AnimateWindowClose(loansOverlay, () => 
            { 
                loansOverlay.style.display = DisplayStyle.None; 
                mainPanel.style.display = DisplayStyle.Flex; 
            });
    }

    private void OpenInvestmentsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (investmentsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(investmentsOverlay);
            UpdateInvestmentsUI();
        }
    }

    private void CloseInvestmentsWindow()
    {
        if (investmentsOverlay != null)
            AnimateWindowClose(investmentsOverlay, () => 
            { 
                investmentsOverlay.style.display = DisplayStyle.None; 
                mainPanel.style.display = DisplayStyle.Flex; 
            });
    }

    // ---- Обновление списков ----
    private void UpdateLoansUI()
    {
        var listView = root.Q<ListView>("LoansListView");
        if (listView == null) return;
        var loans = LoanManager.Instance?.GetActiveLoans() ?? new List<LoanManager.Loan>();
        listView.itemsSource = loans;
        listView.Rebuild();
    }

    private void UpdateInvestmentsUI()
    {
        var listView = root.Q<ListView>("InvestmentsListView");
        if (listView == null) return;
        var inv = InvestmentManager.Instance?.GetActiveInvestments() ?? new List<InvestmentManager.Investment>();
        listView.itemsSource = inv;
        listView.Rebuild();
    }


    private void SetupLoansUI()
    {
        var container = loansOverlay?.Q<VisualElement>("LoansContainer");
        if (container == null) return;
        
        // Удаляем старый заголовок, если есть
        var oldHeader = container.Q<VisualElement>("LoansHeader");
        if (oldHeader != null) oldHeader.RemoveFromHierarchy();

        // Создаём заголовок
        var header = new VisualElement { name = "LoansHeader" };
        header.style.flexDirection = FlexDirection.Row;
        header.style.paddingLeft = 5;
        header.style.paddingRight = 5;
        header.style.paddingTop = 4;
        header.style.paddingBottom = 4;
        header.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        header.style.marginBottom = 4;

        string[] headers = { "Сумма", "Ставка", "Осталось", "Платёж" };
        float[] widths = { 80, 60, 70, 80 };
        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            lbl.style.width = widths[i];
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.fontSize = 14;
            header.Add(lbl);
        }
        container.Insert(0, header);

        // Настраиваем ListView
        SetupLoansListView();
    }

    private void SetupInvestmentsUI()
    {
        var container = investmentsOverlay?.Q<VisualElement>("InvestmentsContainer");
        if (container == null) return;
        
        var oldHeader = container.Q<VisualElement>("InvestmentsHeader");
        if (oldHeader != null) oldHeader.RemoveFromHierarchy();

        var header = new VisualElement { name = "InvestmentsHeader" };
        header.style.flexDirection = FlexDirection.Row;
        header.style.paddingLeft = 5;
        header.style.paddingRight = 5;
        header.style.paddingTop = 4;
        header.style.paddingBottom = 4;
        header.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        header.style.marginBottom = 4;

        string[] headers = { "Тип", "Сумма", "Осталось", "Бонус" };
        float[] widths = { 80, 80, 70, 60 };
        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            lbl.style.width = widths[i];
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.fontSize = 14;
            header.Add(lbl);
        }
        container.Insert(0, header);

        SetupInvestmentsListView();
    }


    private void SetupLoansListView()
    {
        var listView = root.Q<ListView>("LoansListView");
        if (listView == null) return;
        listView.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = Color.gray;

            var amountLabel = new Label { name = "LoanAmount" };
            amountLabel.style.width = 80;
            amountLabel.style.color = Color.white;
            amountLabel.style.fontSize = 14;
            var rateLabel = new Label { name = "LoanRate" };
            rateLabel.style.width = 60;
            rateLabel.style.color = Color.white;
            rateLabel.style.fontSize = 14;
            var remainingLabel = new Label { name = "LoanRemaining" };
            remainingLabel.style.width = 70;
            remainingLabel.style.color = Color.white;
            remainingLabel.style.fontSize = 14;
            var paymentLabel = new Label { name = "LoanPayment" };
            paymentLabel.style.width = 80;
            paymentLabel.style.color = Color.white;
            paymentLabel.style.fontSize = 14;

            container.Add(amountLabel);
            container.Add(rateLabel);
            container.Add(remainingLabel);
            container.Add(paymentLabel);
            return container;
        };
        listView.bindItem = (element, index) =>
        {
            var loans = listView.itemsSource as List<LoanManager.Loan>;
            if (loans == null || index >= loans.Count) return;
            var loan = loans[index];
            element.Q<Label>("LoanAmount").text = $"${loan.amount:F0}";
            element.Q<Label>("LoanRate").text = $"{loan.interestRate * 100:F0}%";
            element.Q<Label>("LoanRemaining").text = $"{loan.remainingMonths} мес.";
            element.Q<Label>("LoanPayment").text = $"${loan.monthlyPayment:F2}";
        };
    }

    private void SetupInvestmentsListView()
    {
        var listView = root.Q<ListView>("InvestmentsListView");
        if (listView == null) return;
        listView.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.style.paddingTop = 4;
            container.style.paddingBottom = 4;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = Color.gray;

            var typeLabel = new Label { name = "InvestmentType" };
            typeLabel.style.width = 80;
            typeLabel.style.color = Color.white;
            typeLabel.style.fontSize = 14;
            var amountLabel = new Label { name = "InvestmentAmount" };
            amountLabel.style.width = 80;
            amountLabel.style.color = Color.white;
            amountLabel.style.fontSize = 14;
            var remainingLabel = new Label { name = "InvestmentRemaining" };
            remainingLabel.style.width = 70;
            remainingLabel.style.color = Color.white;
            remainingLabel.style.fontSize = 14;
            var bonusLabel = new Label { name = "InvestmentBonus" };
            bonusLabel.style.width = 60;
            bonusLabel.style.color = Color.white;
            bonusLabel.style.fontSize = 14;

            container.Add(typeLabel);
            container.Add(amountLabel);
            container.Add(remainingLabel);
            container.Add(bonusLabel);
            return container;
        };
        listView.bindItem = (element, index) =>
        {
            var inv = listView.itemsSource as List<InvestmentManager.Investment>;
            if (inv == null || index >= inv.Count) return;
            var investment = inv[index];
            element.Q<Label>("InvestmentType").text = investment.type;
            element.Q<Label>("InvestmentAmount").text = $"${investment.amount:F0}";
            element.Q<Label>("InvestmentRemaining").text = $"{investment.remainingMonths} мес.";
            element.Q<Label>("InvestmentBonus").text = $"+{investment.monthlyBonus * 100:F0}%";
        };
    }


    // ---- Обработчики действий ----
    private void OnTakeLoan()
    {
        var amountField = root.Q<FloatField>("LoanAmountField");
        var monthsField = root.Q<IntegerField>("LoanMonthsField");
        var rateField = root.Q<FloatField>("LoanRateField");

        if (amountField == null || monthsField == null || rateField == null)
        {
            ShowNotification("Ошибка: не найдены поля ввода кредита");
            return;
        }

        float amount = amountField.value;
        int months = monthsField.value;
        float rate = rateField.value / 100f;

        if (amount <= 0 || months <= 0 || rate <= 0)
        {
            ShowNotification("Заполните все поля корректными значениями");
            return;
        }

        bool success = LoanManager.Instance?.TakeLoan(amount, months, rate) ?? false;
        if (success)
        {
            UpdateLoansUI();
            UpdateMoneyLabels();
            ShowNotification($"Кредит на ${amount} взят на {months} мес. под {rate*100:F0}%");
        }
        else
        {
            ShowNotification("Не удалось взять кредит. Проверьте наличие менеджера кредитов.");
        }
    }

    private void OnMakeInvestment()
    {
        var typeDropdown = root.Q<DropdownField>("InvestmentTypeDropdown");
        var amountField = root.Q<FloatField>("InvestmentAmountField");
        var monthsField = root.Q<IntegerField>("InvestmentMonthsField");

        if (typeDropdown == null || amountField == null || monthsField == null)
        {
            ShowNotification("Ошибка: не найдены поля ввода инвестиций");
            return;
        }

        string type = typeDropdown.value;
        float amount = amountField.value;
        int months = monthsField.value;

        if (amount <= 0 || months <= 0)
        {
            ShowNotification("Сумма и срок должны быть больше 0");
            return;
        }

        string invType = type switch
        {
            "Маркетинг" => "Marketing",
            "Исследования" => "Research",
            "Производство" => "Production",
            _ => "Marketing"
        };

        bool success = InvestmentManager.Instance?.MakeInvestment(invType, amount, months) ?? false;
        if (success)
        {
            UpdateInvestmentsUI();
            UpdateMoneyLabels();
            ShowNotification($"Инвестиция в {type} на ${amount} на {months} мес. выполнена");
        }
        else
        {
            ShowNotification("Не удалось сделать инвестицию. Проверьте наличие менеджера инвестиций.");
        }
    }

    // ---- Вспомогательные методы ----
    private void HideAllOverlays()
    {
        if (carsOverlay != null) carsOverlay.style.display = DisplayStyle.None;
        if (techOverlay != null) techOverlay.style.display = DisplayStyle.None;
        if (upgradeOverlay != null) upgradeOverlay.style.display = DisplayStyle.None;
        if (settingsOverlay != null) settingsOverlay.style.display = DisplayStyle.None;
        if (competitorsOverlay != null) competitorsOverlay.style.display = DisplayStyle.None;
        if (welcomeOverlay != null) welcomeOverlay.style.display = DisplayStyle.None;
        if (achievementsOverlay != null) achievementsOverlay.style.display = DisplayStyle.None;
        if (marketingOverlay != null) marketingOverlay.style.display = DisplayStyle.None;
        if (marketVisOverlay != null) marketVisOverlay.style.display = DisplayStyle.None;
        if (loansOverlay != null) loansOverlay.style.display = DisplayStyle.None;
        if (investmentsOverlay != null) investmentsOverlay.style.display = DisplayStyle.None;
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

    private void AnimateWindowClose(VisualElement window, Action onComplete)
    {
        window.style.opacity = 0;
        window.style.scale = new Scale(new Vector3(0.9f, 0.9f, 1f));
        window.schedule.Execute(() =>
        {
            onComplete?.Invoke();
        }).ExecuteLater(150);
    }

    private Sprite LoadCarIcon(string carName)
    {
        if (string.IsNullOrEmpty(carName)) return null;
        string path = $"Images/{carName}";
        Sprite icon = Resources.Load<Sprite>(path);
        if (icon == null)
            Debug.LogWarning($"Иконка для {carName} не найдена по пути {path}");
        return icon;
    }

    private int CalculateTechLevel(Technology tech, List<Technology> allTechs)
    {
        if (tech == null) return 0;
        if (tech.requiredTechNames == null || tech.requiredTechNames.Length == 0)
            return 0;
        int maxLevel = 0;
        foreach (string reqName in tech.requiredTechNames)
        {
            Technology reqTech = allTechs.FirstOrDefault(t => t != null && t.techName == reqName);
            if (reqTech != null)
            {
                int level = CalculateTechLevel(reqTech, allTechs) + 1;
                if (level > maxLevel) maxLevel = level;
            }
        }
        return maxLevel;
    }

    private VisualElement CreateTechNodeElement(Technology tech, float techCostMultiplier)
    {
        VisualElement node = new VisualElement();
        node.style.width = 280;
        node.style.height = 130;
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
        btn.style.width = new Length(100, LengthUnit.Percent);
        btn.style.height = new Length(100, LengthUnit.Percent);
        btn.style.whiteSpace = WhiteSpace.Normal;
        btn.style.unityTextAlign = TextAnchor.MiddleCenter;
        btn.style.fontSize = 11;
        btn.userData = tech;

        UpdateTechButtonState(btn, tech);

        btn.clicked += () =>
        {
            CarCompanyManager.Instance.TechManager.ResearchTechnology(tech);
        };

        node.Add(btn);
        node.userData = tech;
        return node;
    }

    private void UpdateTechButtonState(Button button, Technology tech)
    {
        if (button == null || tech == null) return;

        string baseText = $"{tech.techName}\n{tech.description}";

        if (tech.isResearched)
        {
            button.text = $"{tech.techName} (Изучено)";
            button.SetEnabled(false);
            button.style.backgroundColor = new StyleColor(Color.gray);
            button.style.unityFontStyleAndWeight = FontStyle.Normal;
            return;
        }

        bool requirementsMet = true;
        if (tech.requiredTechNames != null && tech.requiredTechNames.Length > 0)
        {
            foreach (string requiredName in tech.requiredTechNames)
            {
                Technology requiredTech = CarCompanyManager.Instance.TechManager.GetTechnologyByName(requiredName);
                if (requiredTech == null || !requiredTech.isResearched)
                {
                    requirementsMet = false;
                    break;
                }
            }
        }

        string priceInfo = GetTechPriceInfo(tech);

        if (!requirementsMet)
        {
            button.SetEnabled(false);
            button.style.backgroundColor = new StyleColor(Color.red);
            button.text = $"{baseText}\n(Требования не выполнены){priceInfo}";
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
        }
        else
        {
            button.SetEnabled(true);
            button.style.backgroundColor = new StyleColor(Color.green);
            button.text = $"{baseText}{priceInfo}";
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
        }

        int currentYear = GameTimeManager.Instance?.currentYear ?? 2025;
        int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
        if (requirementsMet && !tech.IsAvailable(currentYear, currentMonth))
        {
            button.style.backgroundColor = new StyleColor(new Color(1f, 0.5f, 0f));
        }
    }

    private class TechNode
    {
        public Technology tech;
        public VisualElement element;
        public Vector2 position;
        public List<TechNode> children = new List<TechNode>();
        public List<TechNode> parents = new List<TechNode>();
        public int level;
    }

    // ---- Сложность ----
    public void SetDifficulty(Difficulty newDifficulty)
    {
        currentDifficulty = newDifficulty;
        UpdateDifficultyDisplay();
        PlayerPrefs.SetInt("Difficulty", (int)currentDifficulty);
        PlayerPrefs.Save();
        if (CarCompanyManager.Instance != null && CarCompanyManager.Instance.DifficultyManager != null)
            CarCompanyManager.Instance.DifficultyManager.SetDifficulty((DifficultyManager.DifficultyLevel)(int)newDifficulty);
    }

    private void UpdateDifficultyDisplay()
    {
        if (savedDifficultyLabel == null) return;
        string labelText = currentDifficulty switch
        {
            Difficulty.Easy => "Сложность: Лёгкая",
            Difficulty.Normal => "Сложность: Обычная",
            Difficulty.Hard => "Сложность: Тяжёлая",
            _ => "Сложность: ?"
        };
        savedDifficultyLabel.text = labelText;
        savedDifficultyLabel.tooltip = currentDifficulty switch
        {
            Difficulty.Easy => "Цены на машины снижены, конкуренты слабее",
            Difficulty.Normal => "Стандартные настройки игры",
            Difficulty.Hard => "Цены выше, конкуренты агрессивнее, доход меньше",
            _ => "Неизвестная сложность"
        };
    }

    // ---- ОКНО ЗАРУБЕЖНЫХ РЫНКОВ ----
    private void OpenOfficesWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (officesOverlay != null)
        {
            if (!InternationalManager.Instance.IsOfficeTechUnlocked())
            {
                ShowNotification("Изучите технологию 'Международная экспансия' для доступа к этому окну.");
                return;
            }
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(officesOverlay);
            UpdateOfficesUI();
        }
    }

    private void CloseOfficesWindow()
    {
        if (officesOverlay != null)
            AnimateWindowClose(officesOverlay, () => { officesOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    // ---- ОКНО ПАТЕНТНОГО БЮРО ----
    private void OpenPatentsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (patentsOverlay != null)
        {
            if (!IPManager.Instance.IsPatentTechUnlocked())
            {
                ShowNotification("Изучите технологию 'Патентное право' для доступа к этому окну.");
                return;
            }
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(patentsOverlay);
            UpdatePatentsUI();
        }
    }

    private void ClosePatentsWindow()
    {
        if (patentsOverlay != null)
            AnimateWindowClose(patentsOverlay, () => { patentsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }


    // ---- ОБНОВЛЕНИЕ UI ДЛЯ ОФИСОВ ----
    public void UpdateOfficesUI()
    {
        if (officesOverlay == null) return;
        var container = officesOverlay.Q<VisualElement>("OfficesListContainer");
        if (container == null) return;
        container.Clear();

        var manager = InternationalManager.Instance;
        if (manager == null) return;

        var definitions = manager.regionDefinitions;
        var offices = manager.GetOffices();

        // ---- ЗАГОЛОВКИ (увеличенные ширины) ----
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;
        headerRow.style.paddingLeft = 8;
        headerRow.style.paddingRight = 8;
        headerRow.style.marginBottom = 4;
        headerRow.style.borderBottomWidth = 1;
        headerRow.style.borderBottomColor = Color.gray;

        // Новые ширины колонок (увеличены)
        float[] colWidths = { 150, 120, 70, 120, 200 }; // регион, статус, ур., доход, действия
        string[] headers = { "Регион", "Статус", "Ур.", "Доход", "Действия" };

        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            lbl.style.width = colWidths[i];
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            lbl.style.fontSize = 14;
            lbl.style.flexShrink = 0;
            headerRow.Add(lbl);
        }
        container.Add(headerRow);

        // ---- СТРОКИ ДАННЫХ ----
        foreach (var def in definitions)
        {
            var office = offices.FirstOrDefault(o => o.regionId == def.id);
            bool isOpen = office != null && office.isActive;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

            // Регион
            var nameLabel = new Label(def.displayName);
            nameLabel.style.width = colWidths[0];
            nameLabel.style.color = Color.white;
            nameLabel.style.flexShrink = 0;
            row.Add(nameLabel);

            // Статус
            var statusLabel = new Label(isOpen ? "✅ Открыто" : "🔒 Закрыто");
            statusLabel.style.width = colWidths[1];
            statusLabel.style.color = isOpen ? Color.green : Color.red;
            statusLabel.style.flexShrink = 0;
            row.Add(statusLabel);

            // Уровень или прочерк
            if (isOpen)
            {
                var levelLabel = new Label($"Ур. {office.level + 1}");
                levelLabel.style.width = colWidths[2];
                levelLabel.style.color = Color.white;
                levelLabel.style.flexShrink = 0;
                row.Add(levelLabel);
            }
            else
            {
                var emptyLabel = new Label("-");
                emptyLabel.style.width = colWidths[2];
                emptyLabel.style.color = Color.gray;
                emptyLabel.style.flexShrink = 0;
                row.Add(emptyLabel);
            }

            // Доход
            if (isOpen)
            {
                var revenueLabel = new Label($"${office.monthlyRevenue:F0}");
                revenueLabel.style.width = colWidths[3];
                revenueLabel.style.color = Color.yellow;
                revenueLabel.style.flexShrink = 0;
                row.Add(revenueLabel);
            }
            else
            {
                var emptyRevenue = new Label("-");
                emptyRevenue.style.width = colWidths[3];
                emptyRevenue.style.color = Color.gray;
                emptyRevenue.style.flexShrink = 0;
                row.Add(emptyRevenue);
            }

            // Действия (контейнер с кнопками)
            var buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.style.width = colWidths[4];
            buttonContainer.style.flexShrink = 0;
            buttonContainer.style.alignItems = Align.Center;

            if (!isOpen)
            {
                var openBtn = new Button(() =>
                {
                    float rent = def.baseRent * 1.2f;
                    float salaries = def.baseSalaries * 1.2f;
                    manager.OpenOffice(def.id, rent, salaries);
                    UpdateOfficesUI();
                });
                openBtn.text = "Открыть";
                openBtn.style.width = 70;
                openBtn.style.height = 22;
                openBtn.style.fontSize = 11;
                openBtn.style.marginRight = 4;
                buttonContainer.Add(openBtn);
            }
            else
            {
                var upgradeBtn = new Button(() =>
                {
                    manager.UpgradeOffice(def.id);
                    UpdateOfficesUI();
                });
                upgradeBtn.text = "Улучшить";
                upgradeBtn.style.width = 70;
                upgradeBtn.style.height = 22;
                upgradeBtn.style.fontSize = 11;
                upgradeBtn.style.marginRight = 4;
                buttonContainer.Add(upgradeBtn);

                var closeBtn = new Button(() =>
                {
                    manager.CloseOffice(def.id);
                    UpdateOfficesUI();
                });
                closeBtn.text = "Закрыть";
                closeBtn.style.width = 70;
                closeBtn.style.height = 22;
                closeBtn.style.fontSize = 11;
                closeBtn.style.backgroundColor = new Color(0.6f, 0.1f, 0.1f);
                buttonContainer.Add(closeBtn);
            }

            row.Add(buttonContainer);
            container.Add(row);
        }
    }

    // ---- ОБНОВЛЕНИЕ UI ДЛЯ ПАТЕНТОВ ----
    public void UpdatePatentsUI()
    {
        if (patentsOverlay == null) return;
        var container = patentsOverlay.Q<VisualElement>("PatentsListContainer");
        if (container == null) return;
        container.Clear();

        var manager = IPManager.Instance;
        if (manager == null) return;

        var techManager = CarCompanyManager.Instance.TechManager;
        if (techManager == null) return;

        // Заголовок
        var header = new Label("📜 Патенты и лицензии");
        header.style.color = Color.white;
        header.style.fontSize = 16;
        header.style.unityFontStyleAndWeight = FontStyle.Bold;
        header.style.marginBottom = 8;
        container.Add(header);

        // Список технологий, доступных для патентования
        var techs = techManager.Technologies;
        if (techs != null && techs.Length > 0)
        {
            foreach (var tech in techs)
            {
                if (tech == null || !tech.isResearched) continue;
                bool isPatented = manager.IsTechnologyPatented(tech.techName);
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingTop = 2;
                row.style.paddingBottom = 2;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

                var nameLabel = new Label(tech.techName);
                nameLabel.style.width = 150;
                nameLabel.style.color = Color.white;
                row.Add(nameLabel);

                var statusLabel = new Label(isPatented ? "Запатентовано" : "Не запатентовано");
                statusLabel.style.width = 120;
                statusLabel.style.color = isPatented ? Color.green : Color.gray;
                row.Add(statusLabel);

                var button = new Button(() =>
                {
                    if (!isPatented)
                    {
                        manager.PatentTechnology(tech.techName);
                        UpdatePatentsUI();
                    }
                    else
                    {
                        manager.RenewPatent(tech.techName);
                        UpdatePatentsUI();
                    }
                });
                button.text = isPatented ? "Продлить" : "Запатентовать";
                button.style.width = 100;
                button.style.height = 22;
                button.style.fontSize = 11;
                row.Add(button);

                container.Add(row);
            }
        }

        // Список активных лицензий
        var licenses = manager.GetLicenses();
        if (licenses != null && licenses.Count > 0)
        {
            var licenseHeader = new Label("Активные лицензии");
            licenseHeader.style.color = Color.white;
            licenseHeader.style.fontSize = 14;
            licenseHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            licenseHeader.style.marginTop = 12;
            container.Add(licenseHeader);

            foreach (var license in licenses)
            {
                if (!license.isActive) continue;
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.justifyContent = Justify.SpaceBetween;
                row.style.alignItems = Align.Center;
                row.style.paddingTop = 2;
                row.style.paddingBottom = 2;
                row.style.borderBottomWidth = 1;
                row.style.borderBottomColor = new Color(0.3f, 0.3f, 0.3f);

                var label = new Label($"{license.techName} → {license.licensee}");
                label.style.color = Color.white;
                row.Add(label);

                var royaltyLabel = new Label($"Роялти: {license.royaltyRate * 100:F0}%");
                royaltyLabel.style.color = Color.yellow;
                row.Add(royaltyLabel);

                container.Add(row);
            }
        }
    }

    // ---- ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ ВЫЗОВА ИЗ МЕНЕДЖЕРОВ ----
    public void UpdateInternationalUI()
    {
        UpdateOfficesUI();
    }

    public void UpdatePatentUI()
    {
        UpdatePatentsUI();
    }


    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= UpdateDateTimeDisplay;
    }

    public void UpdateReputationLabel()
    {
        if (reputationLabel != null && economy != null)
            reputationLabel.text = economy.Reputation.ToString();
    }

    private void OnDifficultySelected(Difficulty diff)
    {
        Debug.Log($"OnDifficultySelected: {diff}");
        SetDifficulty(diff);
        CloseWelcomeScreen();

        bool hasSave = CarCompanyManager.Instance.SaveLoadManager.HasSaveFile();
        int tutorialCompleted = PlayerPrefs.GetInt("TutorialCompleted", 0);
        Debug.Log($"hasSave={hasSave}, tutorialCompleted={tutorialCompleted}");

        if (!hasSave && tutorialCompleted == 0)
        {
            Debug.Log("Запускаем туториал");
            if (TutorialManager.Instance != null)
                TutorialManager.Instance.StartTutorial();
            else
                Debug.LogError("TutorialManager.Instance == null!");
        }
        else
        {
            Debug.Log("Туториал не запущен");
        }
    }

    // ---- Вспомогательный метод для получения требований из рецепта ----
    private (int engine, int body, int wheels, int electronics) GetRequirements(CarRecipe recipe)
    {
        if (recipe == null) return (1, 1, 1, 1);
        return (recipe.engineRequired, recipe.bodyRequired, recipe.wheelsRequired, recipe.electronicsRequired);
    }

    // ---- Проверка наличия деталей для машины ----
    private bool HasRequiredParts(CarBlueprint car)
    {
        if (warehouse == null || car == null) return false;
        var recipe = car.GetCurrentRecipe();
        var req = GetRequirements(recipe);
        return warehouse.GetPartCount(PartType.Engine) >= req.engine &&
               warehouse.GetPartCount(PartType.Body) >= req.body &&
               warehouse.GetPartCount(PartType.Wheels) >= req.wheels &&
               warehouse.GetPartCount(PartType.Electronics) >= req.electronics;
    }

    // ---- Списание деталей для машины ----
    private void ConsumeParts(CarBlueprint car)
    {
        if (warehouse == null || car == null) return;
        var recipe = car.GetCurrentRecipe();
        var req = GetRequirements(recipe);
        warehouse.AddParts(PartType.Engine, -req.engine);
        warehouse.AddParts(PartType.Body, -req.body);
        warehouse.AddParts(PartType.Wheels, -req.wheels);
        warehouse.AddParts(PartType.Electronics, -req.electronics);
    }

    private void OpenMenuWindow()
    {
        HideAllOverlays();
        var overlay = root.Q<VisualElement>("MenuOverlay");
        if (overlay != null)
        {
            // Убедимся, что MenuContainer отображается и имеет детей
            var menuContainer = overlay.Q<VisualElement>("MenuContainer");
            if (menuContainer != null)
            {
                menuContainer.style.display = DisplayStyle.Flex;
                menuContainer.style.opacity = 1;
                Debug.Log($"MenuContainer найден, детей: {menuContainer.childCount}");
                if (menuContainer.childCount == 0)
                {
                    Debug.LogError("MenuContainer пуст! Проверьте UXML.");
                }
            }
            else
            {
                Debug.LogError("MenuContainer не найден внутри MenuOverlay!");
            }

            AnimateWindowOpen(overlay);
        }
        else
        {
            Debug.LogError("MenuOverlay не найден!");
        }
    }

    private void CloseMenuWindow()
    {
        var overlay = root.Q<VisualElement>("MenuOverlay");
        if (overlay != null)
            AnimateWindowClose(overlay, () => overlay.style.display = DisplayStyle.None);
    }

    private void ChangeCarPrice(CarBlueprint car, int delta)
    {
        if (car == null) return;

        int newPrice = car.currentPrice + delta;
        newPrice = Mathf.Clamp(newPrice, 10, car.basePrice * 3);

        car.currentPrice = newPrice;

        // Сохраняем цену в LevelData для улучшенных машин
        if (car.currentLevel > 0 && car.levels != null && car.currentLevel - 1 < car.levels.Length)
        {
            car.levels[car.currentLevel - 1].levelPrice = newPrice;
        }

        UpdateCarCards();
        demand.UpdateDemand();
        UpdateMoneyLabels();
    }
}