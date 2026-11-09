using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    // ---- НОВОСТИ (единая страница) ----
    private VisualElement newsOverlay;
    private Toggle musicToggle;
    private Label newsTitleLabel;
    private Toggle themeToggle;
    private bool isDarkTheme = false;
    private Label newsDescriptionLabel;
    private Label newsImportanceLabel;
    private VisualElement newsImportanceBar;
    private VisualElement newsTimeBar;
    private Label newsTimeLabel;
    private VisualElement newsBadge;
    private Label newsBadgeIcon;
    private Label newsBadgeText;
    private VisualElement newsActionsContainer;
    private VisualElement newsHistoryContainer;
    private Button closeNewsButton;

    private VisualElement platformsOverlay;
    private ListView platformsListView;
    private Button developPlatformButton;

    private VisualElement chargingOverlay;
    private ScrollView chargingContainer;

    private VisualElement demandGraphOverlay;
    private DropdownField demandGraphCarDropdown;
    private VisualElement demandGraphContainer;
    private Button closeDemandGraphButton;

    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement notificationContainer;
    private Label eventLabel;

    // ---- Демо-версия ----
    private Label demoTimerLabel;
    private Label demoProgressLabel;

    private VisualElement batteryResourceBlock;
    private Label batteryLabel;
    private Button produceBatteryButton;
    private Button sellBatteryButton;

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

    // ---- Контракты ----
    private VisualElement contractsOverlay;
    private ScrollView contractsContainer;

    // ---- Дилер ----
    private VisualElement dealerOverlay;
    private ScrollView dealerContainer;

    private Button competitorsTabButton;
    private Button actionLogTabButton;
    private VisualElement actionLogContent;

    private Button hamburgerButton;
    private VisualElement menuContainer;
    private IntegerField countInputField;
    private TextField searchCarField;
    private DropdownField filterTypeDropdown;
    private DropdownField sortByDropdown;
    private Button decreaseCountBtn;
    private Button increaseCountBtn;
    private Button produceButton;

    private Label conveyorLevelLabel;
    private Label designerCountLabel;
    private Label marketerCountLabel;
    private Label managerCountLabel;
    private Button buyConveyorButton;
    private Button hireDesignerButton;
    private Button hireMarketerButton;
    private Button hireManagerButton;

    // Поля
    private VisualElement carConfiguratorOverlay;
    private ScrollView carConfiguratorContainer;
    private CarBlueprint currentConfigCar;

    // ---- Персонал ----
    private VisualElement staffOverlay;
    private ScrollView staffContainer;

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
        public Button upgradeButton;
        public Button configButton;
        
        public Label ratingLabel; 
        public Label platformLabel;
        public Label stockLabel;
        public Label contractMatchLabel; 
        
        public Label profitLabel;
        public Label demandLabel;
        public Label taxRateLabel;
        public Label detailsLabel; // Себестоимость
        public Label priceLabel;
        
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
    // Вызывается автоматически, когда NewsManager генерирует новость.
    // НЕ закрывает другие окна — новость появляется поверх.
    public void ShowInteractiveNews(InteractiveNews news)
    {
        if (newsOverlay == null) return;

        newsOverlay.style.display = DisplayStyle.Flex;
        AnimateWindowOpen(newsOverlay);
        UpdateNewsUI();
    }

    // Вызывается по кнопке «Новости» из меню — здесь уместно закрыть всё остальное.
    private void OpenNewsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();

        if (newsOverlay == null) return;

        newsOverlay.style.display = DisplayStyle.Flex;
        AnimateWindowOpen(newsOverlay);
        UpdateNewsUI();
    }

    public void CloseNewsWindow()
    {
        if (newsOverlay != null)
            AnimateWindowClose(newsOverlay, () => newsOverlay.style.display = DisplayStyle.None);
    }


    public void CloseInteractiveNewsWindow()
    {
        CloseNewsWindow();
    }

    public void UpdateNewsUI()
    {
        if (newsTitleLabel == null) return;

        bool hasNews = NewsManager.Instance != null && NewsManager.Instance.IsNewsActive;

        if (hasNews)
        {
            var news = NewsManager.Instance.CurrentInteractiveNews;

            // Бейдж типа
            var (icon, text, color) = GetNewsTypeVisual(news.type);
            if (newsBadgeIcon != null) newsBadgeIcon.text = icon;
            if (newsBadgeText != null)
            {
                newsBadgeText.text = text;
                newsBadgeText.style.color = color;
            }
            if (newsBadge != null)
                newsBadge.style.backgroundColor = new StyleColor(new Color(color.r, color.g, color.b, 0.15f));

            newsTitleLabel.text = news.title;
            newsDescriptionLabel.text = news.description;

            float importance = NewsManager.Instance.CurrentImportance;
            newsImportanceLabel.text = $"{importance * 100:F0}%";
            if (newsImportanceBar != null)
            {
                newsImportanceBar.style.width = new Length(importance * 100, LengthUnit.Percent);
                newsImportanceBar.style.backgroundColor = importance > 0.6f ? new Color(0.56f, 0.93f, 0.56f)
                                                        : importance > 0.3f ? new Color(1f, 0.8f, 0.2f)
                                                                            : new Color(1f, 0.42f, 0.42f);
            }

            if (newsTimeBar != null)
            {
                float remaining = NewsManager.Instance.NewsTimeRemaining;
                float duration = Mathf.Max(0.01f, NewsManager.Instance.NewsDuration);
                newsTimeBar.style.width = new Length((remaining / duration) * 100, LengthUnit.Percent);
            }
            if (newsTimeLabel != null)
                newsTimeLabel.text = $"{NewsManager.Instance.NewsTimeRemaining:F0} сек";

            BuildNewsActions(news);
        }
        else
        {
            if (newsBadgeIcon != null) newsBadgeIcon.text = "🌐";
            if (newsBadgeText != null)
            {
                newsBadgeText.text = "Рынок";
                newsBadgeText.style.color = new Color(0.7f, 0.7f, 0.7f);
            }
            if (newsBadge != null)
                newsBadge.style.backgroundColor = new StyleColor(new Color(0.5f, 0.5f, 0.5f, 0.15f));

            newsTitleLabel.text = "Нет активных новостей";
            newsDescriptionLabel.text = "Рынок стабилен. Следующее событие появится позже.";

            newsImportanceLabel.text = "0%";
            if (newsImportanceBar != null)
                newsImportanceBar.style.width = new Length(0, LengthUnit.Percent);

            if (newsTimeLabel != null) newsTimeLabel.text = "—";
            if (newsTimeBar != null) newsTimeBar.style.width = new Length(0, LengthUnit.Percent);

            if (newsActionsContainer != null)
                newsActionsContainer.Clear();
        }

        RefreshNewsHistory();
    }

    private void BuildNewsActions(InteractiveNews news)
    {
        if (newsActionsContainer == null) return;
        newsActionsContainer.Clear();
        if (news.actions == null || news.actions.Length == 0) return;

        for (int i = 0; i < news.actions.Length; i++)
        {
            int idx = i;
            var action = news.actions[idx];

            // ---- Проверка требования технологии ----
            bool locked = false;
            if (!string.IsNullOrEmpty(action.requiredTechName))
            {
                var techMgr = CarCompanyManager.Instance != null ? CarCompanyManager.Instance.TechManager : null;
                locked = techMgr == null || !techMgr.IsTechResearched(action.requiredTechName);
            }

            var btn = new Button();
            btn.enableRichText = true;
            btn.style.marginBottom = 6;
            btn.style.whiteSpace = WhiteSpace.Normal;
            btn.style.paddingTop = 10;
            btn.style.paddingBottom = 10;
            btn.style.paddingLeft = 14;
            btn.style.paddingRight = 14;
            btn.style.borderTopLeftRadius = 8;
            btn.style.borderTopRightRadius = 8;
            btn.style.borderBottomLeftRadius = 8;
            btn.style.borderBottomRightRadius = 8;
            btn.style.unityTextAlign = TextAnchor.MiddleLeft;
            btn.style.fontSize = 13;

            if (locked)
            {
                // Заблокированная кнопка
                btn.text =
                    $"<b>🔒 {action.actionName}</b>\n" +
                    $"<color=#888888>{action.description}</color>\n" +
                    $"<color=#e0a03a>Требуется технология: {action.requiredTechName}</color>";

                btn.style.backgroundColor = new StyleColor(new Color(0.12f, 0.12f, 0.12f));
                btn.style.color = new Color(0.6f, 0.6f, 0.6f);
                btn.SetEnabled(false);
            }
            else
            {
                // Доступная кнопка
                btn.style.backgroundColor = new StyleColor(new Color(0.18f, 0.18f, 0.18f));
                btn.style.color = Color.white;
                btn.text =
                    $"<b>{action.actionName}</b>\n" +
                    $"<color=#bbbbbb>{action.description}</color>\n" +
                    $"{FormatActionEffects(action)}";

                btn.clicked += () =>
                {
                    NewsManager.Instance.ExecuteAction(idx);
                    UpdateMoneyLabels();
                    UpdateCarCards();
                    RefreshMarketGraphIfOpen();
                    UpdateNewsUI();
                };
            }

            newsActionsContainer.Add(btn);
        }
    }

    private void RefreshNewsHistory()
    {
        if (newsHistoryContainer == null) return;
        newsHistoryContainer.Clear();

        var history = NewsManager.Instance?.History;
        if (history == null || history.Count == 0)
        {
            var empty = new Label("Пока не было новостей.");
            empty.style.color = new Color(0.6f, 0.6f, 0.6f);
            empty.style.fontSize = 13;
            newsHistoryContainer.Add(empty);
            return;
        }

        int shown = 0;
        for (int i = history.Count - 1; i >= 0 && shown < 15; i--, shown++)
        {
            var entry = history[i];
            var (icon, _, color) = GetNewsTypeVisual(entry.type);

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 8;
            row.style.paddingBottom = 8;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.marginBottom = 4;
            row.style.backgroundColor = new StyleColor(new Color(0.16f, 0.16f, 0.16f));
            row.style.borderTopLeftRadius = 6;
            row.style.borderTopRightRadius = 6;
            row.style.borderBottomLeftRadius = 6;
            row.style.borderBottomRightRadius = 6;
            row.style.borderLeftWidth = 3;
            row.style.borderLeftColor = new StyleColor(color);

            var dateLabel = new Label($"{entry.gameMonth:D2}/{entry.gameYear}");
            dateLabel.style.width = 60;
            dateLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            dateLabel.style.fontSize = 12;
            row.Add(dateLabel);

            var iconLabel = new Label(icon);
            iconLabel.style.marginRight = 8;
            iconLabel.style.fontSize = 14;
            row.Add(iconLabel);

            var textCol = new VisualElement();
            textCol.style.flexGrow = 1;

            var title = new Label(entry.title);
            title.style.color = Color.white;
            title.style.fontSize = 13;
            title.style.unityFontStyleAndWeight = FontStyle.Bold;
            textCol.Add(title);

            if (!string.IsNullOrEmpty(entry.resultText))
            {
                var result = new Label(entry.resultText);
                result.style.color = new Color(0.7f, 0.7f, 0.7f);
                result.style.fontSize = 11;
                result.style.marginTop = 2;
                textCol.Add(result);
            }

            row.Add(textCol);
            newsHistoryContainer.Add(row);
        }
    }

    private (string icon, string text, Color color) GetNewsTypeVisual(NewsType type)
    {
        switch (type)
        {
            case NewsType.EconomicBoom:      return ("💰", "Экономический бум", new Color(0.56f, 0.93f, 0.56f));
            case NewsType.OilCrisis:         return ("🛢️", "Нефтяной кризис",   new Color(1f, 0.42f, 0.42f));
            case NewsType.GovernmentSubsidy: return ("🏛️", "Гос. субсидия",     new Color(0.4f, 0.8f, 1f));
            case NewsType.EcoTrend:          return ("🌱", "Эко-тренд",          new Color(0.56f, 0.93f, 0.56f));
            case NewsType.CompetitorScandal: return ("⚔️", "Скандал",            new Color(1f, 0.8f, 0.2f));
            case NewsType.MarketSlowdown:    return ("📉", "Замедление рынка",   new Color(1f, 0.42f, 0.42f));
            case NewsType.EvSubsidy:         return ("🔋", "Субсидии на EV", new Color(0.56f, 0.93f, 0.56f));
            default:                         return ("📰", "Событие",            new Color(0.7f, 0.7f, 0.7f));
        }
    }

    private string FormatActionEffects(NewsAction a)
    {
        string money = Mathf.Approximately(a.moneyImpact, 0f) ? "—"
                    : (a.moneyImpact > 0 ? $"<color=#8de08d>+{a.moneyImpact:0}💰</color>"
                                        : $"<color=#e08d8d>{a.moneyImpact:0}💰</color>");

        string rep = Mathf.Approximately(a.reputationImpact, 0f) ? "—"
                : (a.reputationImpact > 0 ? $"<color=#8de08d>+{a.reputationImpact:0}⭐</color>"
                                            : $"<color=#e08d8d>{a.reputationImpact:0}⭐</color>");

        string demand;
        if (Mathf.Approximately(a.demandImpact, 1f)) demand = "—";
        else
        {
            float pct = (a.demandImpact - 1f) * 100f;
            demand = pct > 0 ? $"<color=#8de08d>+{pct:0}%📈</color>"
                            : $"<color=#e08d8d>{pct:0}%📈</color>";
        }

        return $"{money}   {rep}   {demand}";
    }




    public bool IsDemandGraphOpen()
    {
        return demandGraphOverlay != null && demandGraphOverlay.style.display == DisplayStyle.Flex;
    }

    private void OpenDemandGraph()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (demandGraphOverlay == null) return;
        demandGraphOverlay.style.display = DisplayStyle.Flex;
        AnimateWindowOpen(demandGraphOverlay);
        PopulateDemandGraphDropdown();
        DrawDemandGraph();
    }

    private void CloseDemandGraph()
    {
        if (demandGraphOverlay != null)
            AnimateWindowClose(demandGraphOverlay, () => demandGraphOverlay.style.display = DisplayStyle.None);
    }

    private void PopulateDemandGraphDropdown()
    {
        if (demandGraphCarDropdown == null) return;
        var cars = tech.AvailableCars;
        if (cars == null || cars.Length == 0) return;
        demandGraphCarDropdown.choices = cars.Select(c => c.GetDisplayName()).ToList();
        if (demandGraphCarDropdown.choices.Count > 0)
        {
            demandGraphCarDropdown.value = demandGraphCarDropdown.choices[0];
            demandGraphCarDropdown.RegisterValueChangedCallback(evt => DrawDemandGraph());
        }
    }

    public void DrawDemandGraph()
    {
        if (demandGraphContainer == null || demandGraphCarDropdown == null) return;
        string selectedName = demandGraphCarDropdown.value;
        var car = tech.AvailableCars.FirstOrDefault(c => c.GetDisplayName() == selectedName);
        if (car == null)
        {
            demandGraphContainer.Clear();
            var label = new Label("Модель не найдена");
            label.style.color = Color.white;
            demandGraphContainer.Add(label);
            return;
        }

        demandGraphContainer.Clear();

        var history = car.demandHistory;
        if (history == null || history.Count < 2)
        {
            var label = new Label($"Недостаточно данных для графика (записей: {history?.Count ?? 0}).\nПодождите несколько месяцев.");
            label.style.color = Color.white;
            label.style.whiteSpace = WhiteSpace.Normal;
            demandGraphContainer.Add(label);
            return;
        }

        var graph = new VisualElement();
        graph.style.flexGrow = 1;
        graph.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        graph.style.marginTop = 10;
        graph.style.marginBottom = 10;

        float min = history.Min();
        float max = history.Max();
        if (max - min < 0.01f) { min -= 0.5f; max += 0.5f; }
        float range = max - min;

        graph.generateVisualContent += (ctx) =>
        {
            var rect = graph.contentRect;
            if (rect.width < 10 || rect.height < 10) return;
            var painter = ctx.painter2D;
            painter.lineWidth = 2;
            painter.strokeColor = Color.yellow;

            painter.BeginPath();
            for (int i = 0; i < history.Count; i++)
            {
                float x = (i / (float)(history.Count - 1)) * rect.width;
                float y = (1f - (history[i] - min) / range) * rect.height;
                if (i == 0)
                    painter.MoveTo(new Vector2(x, y));
                else
                    painter.LineTo(new Vector2(x, y));
            }
            painter.Stroke();

            // Рисуем сетку и подписи (опционально)
            painter.lineWidth = 0.5f;
            painter.strokeColor = new Color(0.3f, 0.3f, 0.3f);
            for (int i = 0; i <= 4; i++)
            {
                float yPos = rect.y + (i / 4f) * rect.height;
                painter.BeginPath();
                painter.MoveTo(new Vector2(rect.x, yPos));
                painter.LineTo(new Vector2(rect.x + rect.width, yPos));
                painter.Stroke();
            }
        };

        demandGraphContainer.Add(graph);

        // ---- Обновление прогноза ----
        Label forecastLabel = demandGraphOverlay.Q<Label>("SeasonalForecastLabel");
        if (forecastLabel != null)
        {
            string forecast = GenerateSeasonalForecast(car);
            forecastLabel.text = forecast;
        }
    }




    private string GenerateSeasonalForecast(CarBlueprint car)
    {
        if (car == null || car.seasonalDemandMultipliers == null || car.seasonalDemandMultipliers.Length < 12)
            return "Нет данных для прогноза";

        int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
        float currentFactor = car.seasonalDemandMultipliers[currentMonth - 1];

        // Проверяем следующие 3 месяца
        string forecast = "";
        bool hasChange = false;

        for (int i = 1; i <= 3; i++)
        {
            int nextMonth = (currentMonth + i - 1) % 12 + 1; // цикл 1-12
            float nextFactor = car.seasonalDemandMultipliers[nextMonth - 1];
            float change = ((nextFactor - currentFactor) / currentFactor) * 100f;

            if (Mathf.Abs(change) > 3f) // порог 3%
            {
                string sign = change > 0 ? "▲" : "▼";
                string color = change > 0 ? "зелёный" : "красный";
                string monthName = GetMonthName(nextMonth);
                forecast += $"{sign} {monthName}: {change:F0}% ({color})\n";
                hasChange = true;
            }
        }

        if (!hasChange)
            return "Спрос стабилен в ближайшие 3 месяца.";

        return forecast.TrimEnd('\n');
    }

    private string GetMonthName(int month)
    {
        string[] months = { "Янв", "Фев", "Мар", "Апр", "Май", "Июн", "Июл", "Авг", "Сен", "Окт", "Ноя", "Дек" };
        return months[month - 1];
    }

        public void Initialize(UIDocument document)
    {
        uiDoc = document;
        if (uiDoc == null) { Debug.LogError("UIDocument не передан!"); return; }
        root = uiDoc.rootVisualElement;
        if (root == null) return;

        // ==========================================
        // --- ИНИЦИАЛИЗАЦИЯ БРЕНДА (ЛОГОТИП И НАЗВАНИЕ) ---
        // ==========================================
        // 1. Обновляем UI при старте (подтягиваем сохраненный логотип и имя)
        UpdateLogoUI();
        UpdateCompanyNameUI();

        // 2. Подписка на кнопку "✏️ Бренд" в верхней панели
        var editBrandBtn = root.Q<Button>("EditBrandButton");
        if (editBrandBtn != null)
        {
            editBrandBtn.clicked += () => 
            {
                // При клике открываем окно выбора логотипа
                OpenLogoFileDialog(); 
            };
        }
        else
        {
            Debug.LogWarning("Кнопка EditBrandButton не найдена в UXML!");
        }

        // 3. Включаем редактирование названия по клику на текст
        // (Если логотип на фоне, возможно, вы захотите сделать кликабельным и его, но сейчас он ignore)
        SetupCompanyNameEditing();

        // ---- НОВОСТИ (единая страница) ----
        newsOverlay          = root.Q<VisualElement>("NewsOverlay");
        newsTitleLabel       = root.Q<Label>("NewsTitleLabel");
        newsDescriptionLabel = root.Q<Label>("NewsDescriptionLabel");
        newsImportanceLabel  = root.Q<Label>("NewsImportanceLabel");
        newsImportanceBar    = root.Q<VisualElement>("NewsImportanceBar");
        newsTimeBar          = root.Q<VisualElement>("NewsTimeBar");
        newsTimeLabel        = root.Q<Label>("NewsTimeLabel");
        newsBadge            = root.Q<VisualElement>("NewsBadge");
        newsBadgeIcon        = root.Q<Label>("NewsBadgeIcon");
        newsBadgeText        = root.Q<Label>("NewsBadgeText");
        newsActionsContainer = root.Q<VisualElement>("NewsActionsContainer");
        newsHistoryContainer = root.Q<VisualElement>("NewsHistoryContainer");
        closeNewsButton      = root.Q<Button>("CloseNewsButton");
        carStockLabel        = root.Q<Label>("CarStockLabel");

        // ---- График спроса ----
        demandGraphOverlay = root.Q<VisualElement>("DemandGraphOverlay");
        demandGraphCarDropdown = root.Q<DropdownField>("DemandGraphCarDropdown");
        demandGraphContainer = root.Q<VisualElement>("DemandGraphContainer");
        closeDemandGraphButton = root.Q<Button>("CloseDemandGraphButton");

        platformsOverlay = root.Q<VisualElement>("PlatformsOverlay");
        platformsListView = root.Q<ListView>("PlatformsList");
        developPlatformButton = root.Q<Button>("DevelopPlatformButton");
        chargingOverlay = root.Q<VisualElement>("ChargingOverlay");
        chargingContainer = root.Q<ScrollView>("ChargingContainer");
        if (chargingOverlay != null) chargingOverlay.style.display = DisplayStyle.None;

        SubscribeButton("OpenChargingButton", OpenChargingWindow);
        SubscribeButton("CloseChargingButton", CloseChargingWindow);
        SubscribeButton("RefreshChargingButton", RefreshChargingUI);
        SubscribeButton("OpenPlatformsButton", OpenPlatformsWindow);
        SubscribeButton("ClosePlatformsButton", ClosePlatformsWindow);
        SubscribeButton("SaveConfiguratorButton", ApplyConfigurator);
        if (developPlatformButton != null)
            developPlatformButton.clicked += OnDevelopPlatformClicked;

        if (platformsOverlay != null)
            platformsOverlay.style.display = DisplayStyle.None;

        if (closeDemandGraphButton != null)
            closeDemandGraphButton.clicked += CloseDemandGraph;

        if (demandGraphOverlay != null)
            demandGraphOverlay.style.display = DisplayStyle.None;

        SubscribeButton("OpenDemandGraphButton", OpenDemandGraph);

        SubscribeButton("OpenNewsButton", OpenNewsWindow);
        if (closeNewsButton != null)
            closeNewsButton.clicked += CloseNewsWindow;

        if (newsOverlay != null)
            newsOverlay.style.display = DisplayStyle.None;

        musicToggle = root.Q<Toggle>("MusicToggle");
        if (musicToggle != null)
        {
            int savedMute = PlayerPrefs.GetInt("MusicMuted", 0);
            musicToggle.value = (savedMute == 0); 
            musicToggle.RegisterValueChangedCallback(evt =>
            {
                if (MusicManager.Instance != null)
                    MusicManager.Instance.SetMute(!evt.newValue);
            });
        }

                // Настройка переключателя темы
        themeToggle = root.Q<Toggle>("ThemeToggle");
        isDarkTheme = PlayerPrefs.GetInt("DarkTheme", 0) == 1; // Загружаем сохраненную тему
        
        if (themeToggle != null)
        {
            themeToggle.value = isDarkTheme;
            themeToggle.RegisterValueChangedCallback(evt => ApplyTheme(evt.newValue));
        }
        
        // Применяем тему при запуске
        ApplyTheme(isDarkTheme);

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
        countInputField = root.Q<IntegerField>("CountInputField");
        
        // Подключаем поиск и фильтры
        searchCarField = root.Q<TextField>("SearchCarField");
        filterTypeDropdown = root.Q<DropdownField>("FilterTypeDropdown");
        sortByDropdown = root.Q<DropdownField>("SortByDropdown");

        if (searchCarField != null) 
            searchCarField.RegisterValueChangedCallback(evt => RefreshCarList());
        if (filterTypeDropdown != null) 
            filterTypeDropdown.RegisterValueChangedCallback(evt => RefreshCarList());
        if (sortByDropdown != null) 
            sortByDropdown.RegisterValueChangedCallback(evt => RefreshCarList());
        decreaseCountBtn = root.Q<Button>("DecreaseCountButton");
        increaseCountBtn = root.Q<Button>("IncreaseCountButton");
        produceButton = root.Q<Button>("ProduceButton");
        conveyorLevelLabel = root.Q<Label>("ConveyorLevelLabel");
        designerCountLabel = root.Q<Label>("DesignerCountLabel");
        marketerCountLabel = root.Q<Label>("MarketerCountLabel");
        managerCountLabel  = root.Q<Label>("ManagerCountLabel");
        hireDesignerButton = root.Q<Button>("HireDesignerButton");
        hireMarketerButton = root.Q<Button>("HireMarketerButton");
        hireManagerButton  = root.Q<Button>("HireManagerButton");
        buyConveyorButton = root.Q<Button>("BuyConveyorButton");
        buyPartsButton = root.Q<Button>("BuyPartsButton");

        engineLabel = root.Q<Label>("EngineLabel");
        bodyLabel = root.Q<Label>("BodyLabel");
        wheelsLabel = root.Q<Label>("WheelsLabel");
        electronicsLabel = root.Q<Label>("ElectronicsLabel");

        batteryResourceBlock = root.Q<VisualElement>("BatteryResourceBlock");
        batteryLabel = root.Q<Label>("BatteryLabel");
        produceBatteryButton = root.Q<Button>("ProduceBatteryButton");
        sellBatteryButton = root.Q<Button>("SellBatteryButton");

        marketingOverlay = root.Q<VisualElement>("MarketingOverlay");
        marketVisOverlay = root.Q<VisualElement>("MarketVisOverlay");
        loansOverlay = root.Q<VisualElement>("LoansOverlay");
        investmentsOverlay = root.Q<VisualElement>("InvestmentsOverlay");
        officesOverlay = root.Q<VisualElement>("OfficesOverlay");
        patentsOverlay = root.Q<VisualElement>("PatentsOverlay");

        // ---- Персонал ----
        staffOverlay = root.Q<VisualElement>("StaffOverlay");
        staffContainer = root.Q<ScrollView>("StaffContainer");
        if (staffOverlay != null)
            staffOverlay.style.display = DisplayStyle.None;
        
        // ---- Дилер ----
        dealerOverlay = root.Q<VisualElement>("DealerOverlay");
        dealerContainer = root.Q<ScrollView>("DealerContainer");
        if (dealerOverlay != null)
            dealerOverlay.style.display = DisplayStyle.None;
        
        // ---- Контракты ----
        contractsOverlay = root.Q<VisualElement>("ContractsOverlay");
        contractsContainer = root.Q<ScrollView>("ContractsContainer");
        if (contractsOverlay != null)
            contractsOverlay.style.display = DisplayStyle.None;
        demoTimerLabel = root.Q<Label>("DemoTimerLabel");
        demoProgressLabel = root.Q<Label>("DemoProgressLabel");
        
        SetupLoansUI();
        SetupInvestmentsUI();
        SetupLoansListView();
        SetupInvestmentsListView();
        SetupPlatformsListView();

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


        carConfiguratorOverlay = root.Q<VisualElement>("CarConfiguratorOverlay");
        carConfiguratorContainer = root.Q<ScrollView>("ConfiguratorContainer");
        if (carConfiguratorOverlay != null) carConfiguratorOverlay.style.display = DisplayStyle.None;
        SubscribeButton("CloseConfiguratorButton", CloseConfigurator);
        SubscribeButton("SaveConfiguratorButton", ApplyConfigurator);

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

        SubscribeButton("ProduceBatteryButton", () => TryProducePart(PartType.Battery));
        SubscribeButton("SellBatteryButton", () => TrySellPart(PartType.Battery));

        SubscribeButton("OpenOfficesButton", OpenOfficesWindow);
        SubscribeButton("CloseOfficesButton", CloseOfficesWindow);
        SubscribeButton("OpenPatentsButton", OpenPatentsWindow);
        SubscribeButton("ClosePatentsButton", ClosePatentsWindow);
        SubscribeButton("RefreshOfficesButton", UpdateOfficesUI);
        SubscribeButton("RefreshPatentsButton", UpdatePatentsUI);


        SubscribeButton("OpenDealerButton", OpenDealerWindow);
        SubscribeButton("CloseDealerButton", CloseDealerWindow);
        SubscribeButton("RefreshDealerButton", RefreshDealerUI);

        SubscribeButton("OpenStaffButton", OpenStaffWindow);
        SubscribeButton("CloseStaffButton", CloseStaffWindow);
        SubscribeButton("RefreshStaffButton", RefreshStaffUI);

        // ---- Контракты ----
        SubscribeButton("OpenContractsButton", OpenContractsWindow);
        SubscribeButton("CloseContractsButton", CloseContractsWindow);
        SubscribeButton("RefreshContractsButton", RefreshContractsUI);

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
        // НОВАЯ СТРОКА:
        SubscribeButton("ExitButton", QuitGame);
        
        if (DemoManager.IsDemoBuild)
        {
            var saveButton = root.Q<Button>("SaveButton");
            if (saveButton != null) saveButton.SetEnabled(false);
            var loadButton = root.Q<Button>("LoadButton");
            if (loadButton != null) loadButton.SetEnabled(false);
            var newGameButton = root.Q<Button>("NewGameButton");
            if (newGameButton != null) newGameButton.SetEnabled(false);

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
        if (hireDesignerButton != null) hireDesignerButton.clicked += TryHireDesigner;
        if (hireMarketerButton != null) hireMarketerButton.clicked += TryHireMarketer;
        if (hireManagerButton  != null) hireManagerButton.clicked  += TryHireManager;

        if (decreaseCountBtn != null) 
            decreaseCountBtn.clicked += () => { if (countInputField.value > 1) countInputField.value--; };
        if (increaseCountBtn != null) 
            increaseCountBtn.clicked += () => { countInputField.value++; };
        if (buyPartsButton != null) buyPartsButton.clicked += TryBuyParts;

        if (upgradeTabFactoryButton != null)
            upgradeTabFactoryButton.clicked += () => SwitchUpgradeTab(true);
        if (upgradeTabPartsButton != null)
            upgradeTabPartsButton.clicked += () => SwitchUpgradeTab(false);

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

        #if DEMO_BUILD
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
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged += UpdateNewsUI;
        if (ContractManager.Instance != null)
            ContractManager.Instance.OnContractsChanged += RefreshContractsUI;
    }


    
    private void ApplyConfigurator()
    {
        if (currentConfigCar == null) return;
        
        // Здесь сохраняются изменения (сейчас просто обновляет UI)
        UpdateCarCards();
        demand.UpdateDemand();
        UpdateMoneyLabels();
        
        ShowNotification($"Настройки для {currentConfigCar.GetDisplayName()} применены.");
        CloseConfigurator();
    }
    
    
    private void QuitGame()
    {
        #if UNITY_EDITOR
            // Если мы в редакторе Unity — останавливаем Play Mode
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            // Если игра запущена как отдельный .exe файл — закрываем приложение
            Application.Quit();
        #endif
    }



    private (float cost, float price, float profit) CalculateCurrentStats(CarBlueprint car)
    {
        float partCost = economy.GetPartCostForCar(car);
        float baseCost = car.GetProductionCostWithLevel() + partCost;

        float optionsCost = 0f;
        float trimMultiplier = 1f;

        if (car.recipe != null && car.recipe.availableTrims != null)
        {
            var trim = car.recipe.availableTrims.FirstOrDefault(t => t.trimName == car.selectedTrimName);
            if (trim != null)
            {
                trimMultiplier = trim.priceMultiplier;
                if (trim.availableOptions != null)
                {
                    foreach (var opt in trim.availableOptions)
                    {
                        if (car.selectedOptionNames.Contains(opt.optionName))
                            optionsCost += opt.additionalCost;
                    }
                }
            }
        }

        float costWithOptions = (baseCost + optionsCost) * trimMultiplier;

        float platformDiscount = 0f;
        if (car.platform != null && car.platform.isDeveloped && PlatformManager.Instance != null)
            platformDiscount = PlatformManager.Instance.GetPlatformDiscount(car);

        float productionCost = costWithOptions * (1f - platformDiscount);

        int modPrice = Mathf.RoundToInt(car.GetModifiedPrice(economy.TotalPriceModifier) * trimMultiplier);
        double profitBeforeTax = modPrice - productionCost;
        float taxRate = economy.GetTaxRate(car);
        double finalProfit = profitBeforeTax * (1f - taxRate);

        return (productionCost, modPrice, (float)finalProfit);
    }

    // Метод открытия (вызывать по кнопке "Настроить" из карточки машины)
        public void OpenConfigurator(CarBlueprint car)
    {
        if (car == null || carConfiguratorOverlay == null) return;
        currentConfigCar = car;
        
        CloseMenuWindow();
        HideAllOverlays();
        menuContainer.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.None;
        AnimateWindowOpen(carConfiguratorOverlay);
        
        var title = carConfiguratorOverlay.Q<Label>("ConfiguratorTitle");
        if (title != null) title.text = $"Настройка: {car.GetDisplayName()}";

        carConfiguratorContainer.Clear();

        // Инициализация значений по умолчанию
        if (string.IsNullOrEmpty(car.selectedTransmission)) car.selectedTransmission = car.transmissionType;
        if (car.recipe != null && car.recipe.availableTrims != null && car.recipe.availableTrims.Count > 0)
        {
            if (string.IsNullOrEmpty(car.selectedTrimName) || !car.recipe.availableTrims.Any(t => t.trimName == car.selectedTrimName))
            {
                car.selectedTrimName = car.recipe.availableTrims[0].trimName;
                car.selectedOptionNames.Clear();
                if (car.recipe.availableTrims[0].defaultOptions != null)
                    foreach (var opt in car.recipe.availableTrims[0].defaultOptions) car.selectedOptionNames.Add(opt.optionName);
            }
        }

        // --- БЛОК: Цена ---
        VisualElement priceSection = new VisualElement();
        priceSection.style.flexDirection = FlexDirection.Row;
        priceSection.style.alignItems = Align.Center;
        priceSection.style.marginBottom = 10;
        priceSection.style.backgroundColor = isDarkTheme ? new Color(0.2f, 0.2f, 0.2f) : new Color(0.9f, 0.9f, 0.9f);
        priceSection.style.paddingTop = 5; priceSection.style.paddingBottom = 5;
        priceSection.style.paddingLeft = 10; priceSection.style.paddingRight = 10;
        priceSection.style.borderTopLeftRadius = 4; priceSection.style.borderTopRightRadius = 4;

        Label priceTitle = new Label("Текущая цена:");
        priceTitle.style.color = isDarkTheme ? Color.white : Color.black;
        priceTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        priceTitle.style.marginRight = 10;
        priceSection.Add(priceTitle);

        Label currentPriceLbl = new Label($"${car.currentPrice}");
        currentPriceLbl.style.color = new Color(1f, 0.85f, 0.3f);
        currentPriceLbl.style.fontSize = 16;
        currentPriceLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        currentPriceLbl.style.width = 80;
        currentPriceLbl.style.unityTextAlign = TextAnchor.MiddleCenter;

        Button decBtn = new Button(() => 
        {
            ChangeCarPrice(car, -Mathf.Max(10, car.currentPrice / 20));
            currentPriceLbl.text = $"${car.currentPrice}"; 
            OpenConfigurator(car);
        });
        decBtn.text = "−"; decBtn.style.width = 30; decBtn.style.height = 25;
        
        Button incBtn = new Button(() => 
        {
            ChangeCarPrice(car, Mathf.Max(10, car.currentPrice / 20));
            currentPriceLbl.text = $"${car.currentPrice}"; 
            OpenConfigurator(car);
        });
        incBtn.text = "+"; incBtn.style.width = 30; incBtn.style.height = 25;

        priceSection.Add(decBtn);
        priceSection.Add(currentPriceLbl);
        priceSection.Add(incBtn);
        carConfiguratorContainer.Add(priceSection);

        // --- БЛОК: Коробка и Комплектация ---
        if (car.recipe != null)
        {
            VisualElement configBox = new VisualElement();
            configBox.style.marginBottom = 10;
            
            // Коробка
            VisualElement transRow = new VisualElement();
            transRow.style.flexDirection = FlexDirection.Row;
            transRow.style.alignItems = Align.Center;
            transRow.style.marginBottom = 6;

            Label transLbl = new Label("Коробка передач:");
            transLbl.style.width = 150;
            transLbl.style.color = isDarkTheme ? Color.white : Color.black;
            transRow.Add(transLbl);

            List<string> transOptions = new List<string>();
            string[] allTrans = { "АКПП", "МКПП", "Робот", "Вариатор" };
            string[] techNames = { "AKPP", "MKPP", "ROBOT", "VARIATOR" };
            for (int i = 0; i < allTrans.Length; i++)
                if (tech.IsTechResearched(techNames[i])) transOptions.Add(allTrans[i]);
            if (transOptions.Count == 0) transOptions.Add("АКПП");

            DropdownField transDropdown = new DropdownField(transOptions, Mathf.Max(0, transOptions.IndexOf(car.selectedTransmission)));
            transDropdown.style.flexGrow = 1;
            transDropdown.RegisterValueChangedCallback(evt => {
                car.selectedTransmission = evt.newValue;
            });
            ApplyFieldStyle(transDropdown, isDarkTheme ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.95f, 0.95f, 0.95f), isDarkTheme ? Color.white : Color.black);
            transRow.Add(transDropdown);
            configBox.Add(transRow);

            // Комплектация
            if (car.recipe.availableTrims != null && car.recipe.availableTrims.Count > 0)
            {
                VisualElement trimRow = new VisualElement();
                trimRow.style.flexDirection = FlexDirection.Row;
                trimRow.style.alignItems = Align.Center;
                trimRow.style.marginBottom = 6;

                Label trimLbl = new Label("Комплектация:");
                trimLbl.style.width = 150;
                trimLbl.style.color = isDarkTheme ? Color.white : Color.black;
                trimRow.Add(trimLbl);

                List<string> trimNames = car.recipe.availableTrims.Select(t => t.trimName).ToList();
                int trimIdx = trimNames.IndexOf(car.selectedTrimName);
                if (trimIdx < 0) trimIdx = 0;
                
                DropdownField trimDropdown = new DropdownField(trimNames, trimIdx);
                trimDropdown.style.flexGrow = 1;
                ApplyFieldStyle(trimDropdown, isDarkTheme ? new Color(0.25f, 0.25f, 0.25f) : new Color(0.95f, 0.95f, 0.95f), isDarkTheme ? Color.white : Color.black);
                trimRow.Add(trimDropdown);
                configBox.Add(trimRow);

                VisualElement optionsContainer = new VisualElement();
                configBox.Add(optionsContainer);

                Action updateOptionsUI = () =>
                {
                    optionsContainer.Clear();
                    var selectedTrim = car.recipe.availableTrims[trimDropdown.index];
                    
                    // Если комплектация изменилась, сбрасываем галочки на дефолтные
                    if (car.selectedTrimName != selectedTrim.trimName)
                    {
                        car.selectedTrimName = selectedTrim.trimName;
                        car.selectedOptionNames.Clear();
                        if (selectedTrim.defaultOptions != null)
                            foreach (var opt in selectedTrim.defaultOptions) car.selectedOptionNames.Add(opt.optionName);
                    }

                    if (selectedTrim.availableOptions == null) return;
                    
                    foreach (var option in selectedTrim.availableOptions)
                    {
                        Toggle optToggle = new Toggle(option.optionName);
                        optToggle.style.color = isDarkTheme ? Color.white : Color.black;
                        optToggle.style.marginLeft = 150;
                        optToggle.value = car.selectedOptionNames.Contains(option.optionName);
                        optToggle.RegisterValueChangedCallback(evt => {
                            if (evt.newValue && !car.selectedOptionNames.Contains(option.optionName))
                                car.selectedOptionNames.Add(option.optionName);
                            else if (!evt.newValue)
                                car.selectedOptionNames.Remove(option.optionName);
                            
                            // Обновляем итоговые цифры
                            var s = CalculateCurrentStats(car);
                            var c = carConfiguratorOverlay.Q<Label>("ConfigCostLabel");
                            var p = carConfiguratorOverlay.Q<Label>("ConfigProfitLabel");
                            if (c != null) c.text = $"Себестоимость: ${s.cost:F0}";
                            if (p != null) p.text = $"Чистая прибыль: ${s.profit:F0}";
                        });
                        optionsContainer.Add(optToggle);
                    }
                };
                
                trimDropdown.RegisterValueChangedCallback(evt => {
                    updateOptionsUI();
                    var s = CalculateCurrentStats(car);
                    var c = carConfiguratorOverlay.Q<Label>("ConfigCostLabel");
                    var p = carConfiguratorOverlay.Q<Label>("ConfigProfitLabel");
                    if (c != null) c.text = $"Себестоимость: ${s.cost:F0}";
                    if (p != null) p.text = $"Чистая прибыль: ${s.profit:F0}";
                });
                updateOptionsUI();
            }
            carConfiguratorContainer.Add(configBox);
        }

        // --- БЛОК: Тюнинг ---
        VisualElement tuningSection = new VisualElement();
        tuningSection.style.marginBottom = 10;
        
        Label tuningTitle = new Label("Тюнинг:");
        tuningTitle.style.fontSize = 16;
        tuningTitle.style.color = isDarkTheme ? Color.white : Color.black;
        tuningTitle.style.unityFontStyleAndWeight = FontStyle.Bold;
        tuningTitle.style.marginBottom = 6;
        tuningSection.Add(tuningTitle);

        for (int i = 0; i < tuningParamNames.Length; i++)
        {
            string param = tuningParamNames[i];
            string display = tuningParamDisplay[i];

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 5;

            Label pName = new Label(display);
            pName.style.width = 150;
            pName.style.color = isDarkTheme ? Color.white : Color.black;
            row.Add(pName);

            SliderInt slider = new SliderInt();
            slider.lowValue = 0;
            slider.highValue = GetMaxTuning(car, param);
            slider.value = GetCurrentTuning(car, param);
            slider.style.flexGrow = 1;
            slider.style.marginRight = 10;
            row.Add(slider);

            Label valLbl = new Label(GetCurrentTuning(car, param).ToString());
            valLbl.style.width = 30;
            valLbl.style.color = isDarkTheme ? Color.white : Color.black;
            valLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(valLbl);

            Button upBtn = new Button(() => 
            {
                tech.UpgradeTuning(car, param);
                OpenConfigurator(car);
            });
            upBtn.text = "↑"; upBtn.style.width = 30; upBtn.style.height = 25;
            row.Add(upBtn);

            slider.RegisterValueChangedCallback(evt =>
            {
                SetCurrentTuning(car, param, evt.newValue);
                valLbl.text = evt.newValue.ToString();
                UpdateCarCards(); 
                
                var s = CalculateCurrentStats(car);
                var c = carConfiguratorOverlay.Q<Label>("ConfigCostLabel");
                var p = carConfiguratorOverlay.Q<Label>("ConfigProfitLabel");
                if (c != null) c.text = $"Себестоимость: ${s.cost:F0}";
                if (p != null) p.text = $"Чистая прибыль: ${s.profit:F0}";
            });

            tuningSection.Add(row);
        }
        carConfiguratorContainer.Add(tuningSection);

        // --- БЛОК: Цвет и тонировка ---
        VisualElement colorSection = new VisualElement();
        colorSection.style.flexDirection = FlexDirection.Row;
        colorSection.style.alignItems = Align.Center;
        
        Label cTitle = new Label("Цвет:");
        cTitle.style.width = 150;
        cTitle.style.color = isDarkTheme ? Color.white : Color.black;
        colorSection.Add(cTitle);

        for (int i = 0; i < carColors.Length; i++)
        {
            int index = i;
            Button colorBtn = new Button(() =>
            {
                car.bodyColor = carColors[index];
                UpdateCarCards();
                OpenConfigurator(car);
            });
            colorBtn.style.width = 30; colorBtn.style.height = 30;
            colorBtn.style.marginRight = 5;
            colorBtn.style.backgroundColor = carColors[index];
            if (car.bodyColor == carColors[index])
            {
                colorBtn.style.borderTopWidth = 2; colorBtn.style.borderBottomWidth = 2;
                colorBtn.style.borderLeftWidth = 2; colorBtn.style.borderRightWidth = 2;
                colorBtn.style.borderTopColor = Color.white; colorBtn.style.borderBottomColor = Color.white;
                colorBtn.style.borderLeftColor = Color.white; colorBtn.style.borderRightColor = Color.white;
            }
            colorSection.Add(colorBtn);
        }

        Toggle tintToggle = new Toggle("Тонировка");
        tintToggle.style.marginLeft = 20;
        tintToggle.style.color = isDarkTheme ? Color.white : Color.black;
        tintToggle.value = car.hasTint;
        tintToggle.RegisterValueChangedCallback(evt => { car.hasTint = evt.newValue; UpdateCarCards(); });
        colorSection.Add(tintToggle);

        carConfiguratorContainer.Add(colorSection);

        // --- ИТОГОВЫЙ БЛОК (Себестоимость и Прибыль) ---
        var stats = CalculateCurrentStats(car);
        
        VisualElement totalBox = new VisualElement();
        totalBox.style.backgroundColor = isDarkTheme ? new Color(0.12f, 0.2f, 0.12f) : new Color(0.85f, 0.95f, 0.85f);
        totalBox.style.paddingTop = 10; totalBox.style.paddingBottom = 10;
        totalBox.style.paddingLeft = 15; totalBox.style.paddingRight = 15;
        totalBox.style.borderTopLeftRadius = 6; totalBox.style.borderTopRightRadius = 6;
        totalBox.style.borderBottomLeftRadius = 6; totalBox.style.borderBottomRightRadius = 6;
        totalBox.style.marginTop = 10;
        
        Label costLbl = new Label($"Себестоимость: ${stats.cost:F0}");
        costLbl.name = "ConfigCostLabel";
        costLbl.style.color = isDarkTheme ? Color.white : Color.black;
        costLbl.style.fontSize = 14;
        totalBox.Add(costLbl);

        Label profitLbl = new Label($"Чистая прибыль: ${stats.profit:F0}");
        profitLbl.name = "ConfigProfitLabel";
        profitLbl.style.color = new Color(0.1f, 0.5f, 0.1f);
        profitLbl.style.fontSize = 16;
        profitLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        profitLbl.style.marginTop = 4;
        totalBox.Add(profitLbl);

        carConfiguratorContainer.Add(totalBox);
    }

    private void CloseConfigurator()
    {
        if (carConfiguratorOverlay != null)
            AnimateWindowClose(carConfiguratorOverlay, () => {
                carConfiguratorOverlay.style.display = DisplayStyle.None;
                
                // Возвращаемся в окно Машин, так как мы пришли оттуда
                if (carsOverlay != null)
                {
                    carsOverlay.style.display = DisplayStyle.Flex;
                    RefreshCarList(); // Обновляем карточки с новыми ценами/параметрами
                }
                else
                {
                    mainPanel.style.display = DisplayStyle.Flex;
                }
            });
    }

    // Методы
    private void OpenChargingWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (chargingOverlay == null) return;
        menuContainer.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.None;
        AnimateWindowOpen(chargingOverlay);
        RefreshChargingUI();
    }

    private void CloseChargingWindow()
    {
        if (chargingOverlay != null)
            AnimateWindowClose(chargingOverlay, () => {
                chargingOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
    }

    public void RefreshChargingUI()
    {
        if (chargingContainer == null || InternationalManager.Instance == null) return;
        chargingContainer.Clear();

        var regions = InternationalManager.Instance.regionDefinitions;
        var stations = InternationalManager.Instance.chargingStations;

        // Заголовок таблицы
        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        header.style.paddingTop = 4; header.style.paddingBottom = 4; header.style.paddingLeft = 8; header.style.paddingRight = 8;
        header.style.marginBottom = 4;

        string[] headers = { "Регион", "Уровень", "Покрытие", "Действие" };
        float[] widths = { 200, 80, 100, 150 };
        for (int i = 0; i < headers.Length; i++)
        {
            var lbl = new Label(headers[i]);
            lbl.style.width = widths[i];
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(lbl);
        }
        chargingContainer.Add(header);

        foreach (var region in regions)
        {
            var station = stations.FirstOrDefault(s => s.regionId == region.id);
            int level = station?.level ?? 0;
            float coverage = station?.coverage ?? 0f;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 4; row.style.paddingBottom = 4; row.style.paddingLeft = 8; row.style.paddingRight = 8;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

            var nameLabel = new Label(region.displayName);
            nameLabel.style.width = widths[0];
            nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
            row.Add(nameLabel);

            var levelLabel = new Label(level > 0 ? $"Ур. {level}" : "—");
            levelLabel.style.width = widths[1];
            levelLabel.style.color = level > 0 ? Color.green : Color.gray;
            row.Add(levelLabel);

            var coverageLabel = new Label($"{coverage * 100:F0}%");
            coverageLabel.style.width = widths[2];
            coverageLabel.style.color = Color.yellow;
            row.Add(coverageLabel);

            var btn = new Button(() =>
            {
                ChargingManager.Instance?.BuildOrUpgrade(region.id);
                RefreshChargingUI();
            });
            btn.text = level == 0 ? "Построить" : "Улучшить";
            btn.style.width = widths[3];
            btn.style.height = 22;
            btn.style.backgroundColor = level == 0 ? new Color(0.5f, 0.85f, 0.5f) : new Color(0.8f, 0.6f, 0.2f);
            btn.style.color = Color.black;
            row.Add(btn);

            chargingContainer.Add(row);
        }
    }

    private void ApplyTheme(bool isDark)
    {
        isDarkTheme = isDark;
        PlayerPrefs.SetInt("DarkTheme", isDark ? 1 : 0);
        PlayerPrefs.Save();

        if (root == null) return;

        Color windowBg = isDark ? new Color(0.15f, 0.15f, 0.15f) : new Color(0.94f, 0.94f, 0.94f);
        Color panelBg = isDark ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.88f, 0.88f, 0.88f);
        Color fieldBg = isDark ? new Color(0.25f, 0.25f, 0.25f) : new Color(1f, 1f, 1f);
        Color textMain = isDark ? Color.white : Color.black;
        Color textSub = isDark ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.3f, 0.3f, 0.3f);

        // 1. Перекрашиваем фоны окон
        foreach (var overlay in root.Query<VisualElement>(className: "overlay").ToList())
        {
            if (overlay.childCount > 0)
            {
                var window = overlay[0];
                window.style.backgroundColor = windowBg;
                window.style.borderTopColor = isDark ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                window.style.borderBottomColor = isDark ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                window.style.borderLeftColor = isDark ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
                window.style.borderRightColor = isDark ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            }
        }

        // 2. Перекрашиваем текст
        root.Query<Label>().ForEach(lbl => {
            var c = lbl.style.color.value;
            if (c == Color.black || c == Color.white) lbl.style.color = textMain;
            else if (c == new Color(0.2f, 0.2f, 0.2f) || c == new Color(0.3f, 0.3f, 0.3f)) lbl.style.color = textSub;
        });

        root.Query<Button>().ForEach(btn => {
             if (btn.style.color.value == Color.black || btn.style.color.value == Color.white)
                btn.style.color = textMain;
        });

        // 3. Настраиваем поля ввода с помощью вспомогательного метода
        root.Query<TextField>().ForEach(f => ApplyFieldStyle(f, fieldBg, textMain));
        root.Query<IntegerField>().ForEach(f => ApplyFieldStyle(f, fieldBg, textMain));
        root.Query<FloatField>().ForEach(f => ApplyFieldStyle(f, fieldBg, textMain));
        root.Query<DropdownField>().ForEach(f => ApplyFieldStyle(f, fieldBg, textMain));

        // 4. Перекрашиваем фон внутренних панелей
        var menuContainer = root.Q<VisualElement>("MenuContainer");
        if (menuContainer != null) 
            menuContainer.style.backgroundColor = new Color(panelBg.r, panelBg.g, panelBg.b, 0.5f);

        // 5. Обновляем сгенерированные карточки машин
        if (carCards != null && carCards.Count > 0)
            RefreshCarList(); 
    }

    // Вспомогательный метод для правильной покраски полей
    private void ApplyFieldStyle(VisualElement field, Color bg, Color text)
    {
        if (field == null) return;
        
        field.style.backgroundColor = bg;
        field.style.color = text;

        field.Query<VisualElement>().ForEach(child =>
        {
            child.style.backgroundColor = bg;
        });

        field.Query<TextElement>().ForEach(t =>
        {
            t.style.color = text;
        });
    }



    public void OpenLogoFileDialog()
    {
        #if UNITY_EDITOR
        // В редакторе используем встроенный метод
        string path = UnityEditor.EditorUtility.OpenFilePanel("Выберите логотип (PNG)", "", "png");
        if (!string.IsNullOrEmpty(path))
        {
            BrandManager.Instance.LoadLogoFromFile(path);
        }
        #else
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        // В собранной игре под Windows используем наш диалог
        string filter = "Image Files\0*.png;*.jpg;*.jpeg\0All Files\0*.*\0\0";
        string path = WindowsFileDialog.OpenFileDialog("Выберите логотип", filter, "");
        if (!string.IsNullOrEmpty(path))
        {
            BrandManager.Instance.LoadLogoFromFile(path);
        }
        #else
        ShowNotification("Загрузка логотипа доступна только на Windows или в редакторе.");
        #endif
        #endif
    }

    private void SubscribeButton(string name, Action action)
    {
        var btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += () => action?.Invoke();
        else Debug.LogWarning($"Кнопка '{name}' не найдена");
    }



        private void TestLoadLogo()
    {
        // Application.streamingAssetsPath автоматически подставит путь к папке игры (или к Assets/StreamingAssets в редакторе)
        string pathToTestImage = System.IO.Path.Combine(Application.streamingAssetsPath, "IMG", "logo.png");
        
        if (System.IO.File.Exists(pathToTestImage))
        {
            BrandManager.Instance.LoadLogoFromFile(pathToTestImage);
        }
        else
        {
            ShowNotification("Файл не найден по пути: " + pathToTestImage);
        }
    }





    /// <summary>
    /// Производит указанное количество машин указанной модели.
    /// </summary>
    /// <param name="car">Модель для производства. Если null, используется первая доступная.</param>
    /// <param name="count">Количество машин для производства.</param>
    private void ProduceCars(CarBlueprint car, int count)
        => ProduceCars(car, count, "", "АКПП");

    private void ProduceCars(CarBlueprint car, int count, string trimName, string transmissionType)
    {
        if (count <= 0) return;

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
            if (!HasRequiredParts(car))
            {
                ShowNotification($"❌ Недостаточно деталей для {car.GetDisplayName()}!");
                break;
            }

            int assemblyCost = car.finalAssemblyCostWithOptions > 0
                ? car.finalAssemblyCostWithOptions
                : car.GetModifiedAssemblyCost();
            if (economy.Money < assemblyCost)
            {
                ShowNotification($"❌ Недостаточно денег! Нужно ${assemblyCost} для сборки.");
                break;
            }

            bool success = production.ProduceSpecificCar(car, trimName, transmissionType);
            if (success) produced++;
            else break;
        }

        if (produced > 0)
        {
            UpdateWarehouseLabels();
            UpdateProductionButtonsState();
            UpdateMoneyLabels();
            ShowNotification($"✅ {produced} {car.GetDisplayName()} произведено!");
            CarCompanyManager.Instance.EconomyManager.RegisterCarSold(produced);

            if (dealerOverlay != null && dealerOverlay.style.display == DisplayStyle.Flex)
                RefreshDealerUI();
        }
        else if (count > 0)
        {
            ShowNotification("❌ Не удалось произвести ни одной машины.");
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

        int count = 1;
        if (countInputField != null)
        {
            count = Mathf.Max(1, countInputField.value);
            
            // Проверяем, изучена ли технология "Массовое производство" (замените на ваше название)
            bool hasMassProduction = tech.IsTechResearched("Массовое производство"); 
            int maxAllowed = hasMassProduction ? 100 : 1; // Если изучена - можно 100, иначе только 1

            if (count > maxAllowed)
            {
                count = maxAllowed;
                countInputField.value = maxAllowed; 
                ShowNotification($"❌ Максимум {maxAllowed} авто за раз! Изучите технологию массового производства.");
            }
        }

        ProduceCars(car, count);
    }

    // ---- Производство конкретной машины ----
    private void TryProduceSpecificCar(CarCardData cardData)
    {
        if (cardData == null || cardData.car == null) return;
        cardData.car.finalAssemblyCostWithOptions = -1;
        UpdateCarCards();

        int count = 1;
        if (countInputField != null)
        {
            count = Mathf.Max(1, countInputField.value);
            bool hasMassProduction = tech.IsTechResearched("Массовое производство");
            int maxAllowed = hasMassProduction ? 100 : 1;
            if (count > maxAllowed) { count = maxAllowed; countInputField.value = maxAllowed; }
        }

        // Берем данные из сохранённых полей машины
        string trimName = cardData.car.selectedTrimName;
        string gearbox = cardData.car.selectedTransmission;
        ProduceCars(cardData.car, count, trimName, gearbox);
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
         if (batteryLabel != null && warehouse != null)
            batteryLabel.text = warehouse.GetPartCount(PartType.Battery).ToString(); // <--- ДОБАВИТЬ
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

    private float autoProduceTimer = 0f;
    

    private void Update()
    {
        UpdateDemoUI();

        if (newsOverlay != null && newsOverlay.style.display == DisplayStyle.Flex
            && NewsManager.Instance != null && NewsManager.Instance.IsNewsActive)
        {
            float remaining = NewsManager.Instance.NewsTimeRemaining;
            float duration = Mathf.Max(0.01f, NewsManager.Instance.NewsDuration);
            if (newsTimeBar != null)
                newsTimeBar.style.width = new Length((remaining / duration) * 100, LengthUnit.Percent);
            if (newsTimeLabel != null)
                newsTimeLabel.text = $"{remaining:F0} сек";
        }

        // === АВТОПРОИЗВОДСТВО ОТ МЕНЕДЖЕРОВ ===
        if (economy.HasManager)
        {
            autoProduceTimer += Time.deltaTime;
            if (autoProduceTimer >= 5f) // Раз в 5 секунд
            {
                autoProduceTimer = 0f;
                TryAutoProduceParts();
            }
        }
    }


    private void TryAutoProduceParts()
    {
        // Если двигателей меньше 10, менеджер покупает 1
        if (warehouse.GetPartCount(PartType.Engine) < 10 && economy.Money > 100)
            warehouse.ProduceParts(PartType.Engine, 1);
        
        if (warehouse.GetPartCount(PartType.Body) < 10 && economy.Money > 100)
            warehouse.ProduceParts(PartType.Body, 1);
            
        if (warehouse.GetPartCount(PartType.Wheels) < 10 && economy.Money > 100)
            warehouse.ProduceParts(PartType.Wheels, 1);
            
        if (warehouse.GetPartCount(PartType.Electronics) < 10 && economy.Money > 100)
            warehouse.ProduceParts(PartType.Electronics, 1);
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


    private void UpdateBatteryUIState()
    {
        // Проверяем, изучена ли технология
        bool isUnlocked = tech != null && tech.IsTechResearched("Производство Battery");

        // Показываем/скрываем в шапке
        if (batteryResourceBlock != null)
            batteryResourceBlock.style.display = isUnlocked ? DisplayStyle.Flex : DisplayStyle.None;

        // Показываем/скрываем кнопки в окне Улучшений
        if (produceBatteryButton != null)
            produceBatteryButton.style.display = isUnlocked ? DisplayStyle.Flex : DisplayStyle.None;
            
        if (sellBatteryButton != null)
            sellBatteryButton.style.display = isUnlocked ? DisplayStyle.Flex : DisplayStyle.None;
    }


    public void UpdateUpgradeUI()
    {
        UpdateBatteryUIState();
        if (conveyorLevelLabel != null) conveyorLevelLabel.text = $"Уровень конвейера: {economy.ConveyorLevel}";

        if (buyConveyorButton != null)
        {
            int cost = Mathf.RoundToInt((10 + economy.ConveyorLevel * 5) * economy.CostMultiplier);
            buyConveyorButton.text = $"Улучшить конвейер (${cost})";
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


        public void RefreshCarList()
        {
            if (tech == null || tech.AvailableCars == null) return;

            var cars = tech.AvailableCars.ToList();

            // 1. Поиск по названию
            string searchText = searchCarField?.value?.ToLower() ?? "";
            if (!string.IsNullOrEmpty(searchText))
            {
                cars = cars.Where(c => c.GetDisplayName().ToLower().Contains(searchText)).ToList();
            }

            // 2. Фильтр по типу (Седан, Внедорожник и т.д.)
            if (filterTypeDropdown != null)
            {
                // Если список пуст, заполняем его типами машин
                if (filterTypeDropdown.choices.Count == 0)
                {
                    var types = tech.AvailableCars
                        .Where(c => c.carType != null)
                        .Select(c => c.carType.typeName)
                        .Distinct()
                        .ToList();
                        
                    types.Insert(0, "Все");
                    filterTypeDropdown.choices = types;
                    filterTypeDropdown.value = "Все";
                }

                if (filterTypeDropdown.index > 0)
                {
                    string selectedType = filterTypeDropdown.value;
                    cars = cars.Where(c => c.carType != null && c.carType.typeName == selectedType).ToList();
                }
            }

            // 3. Сортировка
            if (sortByDropdown != null)
            {
                if (sortByDropdown.index == 1) // По цене
                    cars = cars.OrderBy(c => c.GetModifiedPrice(economy.TotalPriceModifier)).ToList();
                else if (sortByDropdown.index == 2) // По прибыли
                    cars = cars.OrderByDescending(c => c.GetModifiedPrice(economy.TotalPriceModifier) - c.GetProductionCostWithLevel()).ToList();
            }

            // Передаем отфильтрованный массив в ваш старый метод
            CreateCarCards(cars.ToArray());
        }

        public void UpdateCountLabel(int count)
        {
            if (countInputField != null) 
                countInputField.value = count;
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
        notificationLabel.style.backgroundColor = new StyleColor(isDarkTheme ? new Color(0.2f, 0.2f, 0.2f, 0.9f) : new Color(0.9f, 0.9f, 0.9f, 0.95f));
        notificationLabel.style.color = isDarkTheme ? Color.white : Color.black;
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


    public void CreateCarCards(CarBlueprint[] availableCars)
    {
        if (carsContainer == null) return;
        carsContainer.Clear();
        carCards.Clear();
        bool upgradeUnlocked = tech.IsCarUpgradeUnlocked();

        foreach (CarBlueprint car in availableCars)
        {
            if (car == null) continue;

            CarCardData cardData = new CarCardData();
            cardData.car = car;

            bool hasHigherLevel = availableCars.Any(c => c != car && c.carName == car.carName && c.currentLevel > car.currentLevel);
            bool canUpgrade = !hasHigherLevel
                && (car.levels != null && car.levels.Length > 0
                    && car.currentLevel < car.levels.Length)
                && upgradeUnlocked;

            // ========== КОРНЕВАЯ КАРТОЧКА ==========
            VisualElement card = new VisualElement();
            card.AddToClassList("car-card");
            card.style.backgroundColor = new StyleColor(isDarkTheme ? new Color(0.18f, 0.18f, 0.18f) : new Color(0.95f, 0.95f, 0.95f));
            card.style.borderTopWidth = 1; card.style.borderBottomWidth = 1;
            card.style.borderLeftWidth = 1; card.style.borderRightWidth = 1;
            card.style.borderTopColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            card.style.borderBottomColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            card.style.borderLeftColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            card.style.borderRightColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
            card.style.borderTopLeftRadius = 8; card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8; card.style.borderBottomRightRadius = 8;
            card.style.paddingTop = 10; card.style.paddingBottom = 10;
            card.style.paddingLeft = 12; card.style.paddingRight = 12;
            card.style.marginBottom = 8;
            card.style.flexDirection = FlexDirection.Column;

            // ========== ШАПКА ==========
            VisualElement header = new VisualElement();
            header.style.flexDirection = FlexDirection.Row;
            header.style.alignItems = Align.Center;
            header.style.marginBottom = 6;

            Sprite iconSprite = car.carIcon != null ? car.carIcon : LoadCarIcon(car.carName);
            if (iconSprite != null)
            {
                Image iconImage = new Image();
                iconImage.sprite = iconSprite;
                iconImage.style.width = 48; iconImage.style.height = 48;
                iconImage.style.marginRight = 10;
                header.Add(iconImage);
            }

            VisualElement info = new VisualElement();
            info.style.flexDirection = FlexDirection.Column;
            info.style.flexGrow = 1;

            VisualElement nameRow = new VisualElement();
            nameRow.style.flexDirection = FlexDirection.Row;
            nameRow.style.alignItems = Align.Center;

            Label nameLabel = new Label(car.GetDisplayName());
            nameLabel.style.fontSize = 16;
            nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            nameRow.Add(nameLabel);

            Label levelBadge = new Label($"ур. {car.currentLevel + 1}");
            levelBadge.style.fontSize = 10;
            levelBadge.style.color = isDarkTheme ? new Color(0.7f, 0.9f, 1f) : new Color(0.1f, 0.3f, 0.6f);
            levelBadge.style.backgroundColor = isDarkTheme ? new Color(0.2f, 0.3f, 0.4f) : new Color(0.85f, 0.92f, 1f);
            levelBadge.style.paddingTop = 1; levelBadge.style.paddingBottom = 1;
            levelBadge.style.paddingLeft = 6; levelBadge.style.paddingRight = 6;
            levelBadge.style.marginLeft = 8;
            levelBadge.style.borderTopLeftRadius = 8; levelBadge.style.borderTopRightRadius = 8;
            levelBadge.style.borderBottomLeftRadius = 8; levelBadge.style.borderBottomRightRadius = 8;
            nameRow.Add(levelBadge);
            info.Add(nameRow);

            VisualElement metaRow = new VisualElement();
            metaRow.style.flexDirection = FlexDirection.Row;
            metaRow.style.alignItems = Align.Center;
            metaRow.style.marginTop = 3;

            Label typeLabel = new Label(car.carType != null ? car.carType.typeName : "Без типа");
            typeLabel.style.fontSize = 11;
            typeLabel.style.color = isDarkTheme ? new Color(0.75f, 0.75f, 0.75f) : new Color(0.35f, 0.35f, 0.35f);
            metaRow.Add(typeLabel);

            Label ratingLabelElement = new Label("★ --");
            ratingLabelElement.style.fontSize = 11;
            ratingLabelElement.style.color = isDarkTheme ? new Color(1f, 0.8f, 0.2f) : new Color(0.6f, 0.4f, 0.0f);
            ratingLabelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            ratingLabelElement.style.marginLeft = 10;
            metaRow.Add(ratingLabelElement);
            cardData.ratingLabel = ratingLabelElement;

            Label stockLabel = new Label("На складе: 0");
            stockLabel.style.fontSize = 11;
            stockLabel.style.color = isDarkTheme ? new Color(0.75f, 0.75f, 0.75f) : new Color(0.35f, 0.35f, 0.35f);
            stockLabel.style.marginLeft = 10;
            metaRow.Add(stockLabel);
            cardData.stockLabel = stockLabel;

            info.Add(metaRow);

            var req = GetRequirements(car.recipe);
            Label partsLabel = new Label($"🔧{req.engine} 🔩{req.body} ⚙️{req.wheels} 💻{req.electronics}");
            partsLabel.style.fontSize = 10;
            partsLabel.style.color = isDarkTheme ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.4f, 0.4f, 0.4f);
            partsLabel.style.marginTop = 3;
            info.Add(partsLabel);

            Label platformLabel = new Label();
            platformLabel.style.fontSize = 10;
            platformLabel.style.marginTop = 2;
            info.Add(platformLabel);
            cardData.platformLabel = platformLabel;

            Label contractMatchLabel = new Label();
            contractMatchLabel.style.fontSize = 11;
            contractMatchLabel.style.marginTop = 4;
            contractMatchLabel.style.whiteSpace = WhiteSpace.Normal;
            contractMatchLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            info.Add(contractMatchLabel);
            cardData.contractMatchLabel = contractMatchLabel;

            // ========== КНОПКИ ДЕЙСТВИЙ ==========
            VisualElement actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Column;
            actions.style.width = 120;
            actions.style.marginLeft = 10;

            Button configBtn = new Button(() => OpenConfigurator(car));
            configBtn.text = "Настроить";
            configBtn.style.height = 26;
            configBtn.style.marginBottom = 5;
            configBtn.style.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
            configBtn.style.color = Color.black;
            configBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            actions.Add(configBtn);
            cardData.configButton = configBtn;

            Button upgradeButton = new Button();
            upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            upgradeButton.style.height = 26;
            upgradeButton.style.marginBottom = 5;
            upgradeButton.style.color = isDarkTheme ? Color.white : Color.black;
            upgradeButton.SetEnabled(canUpgrade);
            upgradeButton.clicked += () => tech.UpgradeCar(car);
            actions.Add(upgradeButton);
            cardData.upgradeButton = upgradeButton;

            Button produceBtn = new Button();
            produceBtn.text = "Произвести";
            produceBtn.style.height = 32;
            produceBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            produceBtn.style.backgroundColor = isDarkTheme ? new Color(0.25f, 0.55f, 0.25f) : new Color(0.55f, 0.85f, 0.55f);
            produceBtn.style.color = Color.black;
            // Передаем пустые строки, так как комплектация будет выбрана в конфигураторе, 
            // либо можно брать car.transmissionType (он там сохраняется)
            produceBtn.clicked += () => TryProduceSpecificCar(cardData);
            actions.Add(produceBtn);
            cardData.produceButton = produceBtn;

            header.Add(info);
            header.Add(actions);
            card.Add(header);

            // ========== СТРОКА СТАТИСТИКИ ==========
            VisualElement statsRow = new VisualElement();
            statsRow.style.flexDirection = FlexDirection.Row;
            statsRow.style.flexWrap = Wrap.Wrap;
            statsRow.style.backgroundColor = isDarkTheme ? new Color(0.13f, 0.13f, 0.15f) : new Color(0.92f, 0.92f, 0.92f);
            statsRow.style.paddingTop = 6; statsRow.style.paddingBottom = 6;
            statsRow.style.paddingLeft = 10; statsRow.style.paddingRight = 10;
            statsRow.style.borderTopLeftRadius = 6; statsRow.style.borderTopRightRadius = 6;
            statsRow.style.borderBottomLeftRadius = 6; statsRow.style.borderBottomRightRadius = 6;
            statsRow.style.marginBottom = 0; // Убрали отступ снизу, так как нет Foldout'ов

            VisualElement priceBlock = new VisualElement();
            priceBlock.style.flexDirection = FlexDirection.Column;
            priceBlock.style.width = 140;
            priceBlock.style.flexShrink = 0;
            priceBlock.style.marginRight = 10;

            Label priceCap = new Label("Цена");
            priceCap.style.fontSize = 9;
            priceCap.style.color = isDarkTheme ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);
            priceBlock.Add(priceCap);

            VisualElement priceRow = new VisualElement();
            priceRow.style.flexDirection = FlexDirection.Row;
            priceRow.style.alignItems = Align.Center;

            Label priceLabel = new Label($"${car.currentPrice}");
            priceLabel.style.fontSize = 14;
            priceLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            priceLabel.style.color = isDarkTheme ? new Color(1f, 0.85f, 0.3f) : new Color(0.6f, 0.4f, 0.0f);
            priceLabel.style.marginRight = 4;
            priceRow.Add(priceLabel);
            cardData.priceLabel = priceLabel;

            priceBlock.Add(priceRow);
            statsRow.Add(priceBlock);

            cardData.detailsLabel = AddStatBlock(statsRow, "Себестоимость", "$--", isDarkTheme ? new Color(0.75f, 0.75f, 0.75f) : new Color(0.4f, 0.4f, 0.4f), 110);
            var profitLabelRef = AddStatBlock(statsRow, "Прибыль", "$--", new Color(0.56f, 0.93f, 0.56f), 110);
            profitLabelRef.style.unityFontStyleAndWeight = FontStyle.Bold;
            cardData.profitLabel = profitLabelRef;
            cardData.demandLabel = AddStatBlock(statsRow, "Спрос", "1.0x", isDarkTheme ? Color.white : Color.black, 80);
            cardData.taxRateLabel = AddStatBlock(statsRow, "Налог", "--", new Color(1f, 0.7f, 0.4f), 80);

            card.Add(statsRow);

            carCards.Add(cardData);
            carsContainer.Add(card);
        }

        UpdateCarCards();
    }

    // ---- Вспомогательный метод: колонка статистики ----
    private Label AddStatBlock(VisualElement container, string caption, string initialValue, Color valueColor, float minWidth)
    {
        VisualElement block = new VisualElement();
        block.style.flexDirection = FlexDirection.Column;
        block.style.flexGrow = 1;         // Блок будет растягиваться
        block.style.minWidth = minWidth;  // Минимальная ширина
        block.style.marginRight = 10;

        Label capLabel = new Label(caption);
        capLabel.style.fontSize = 9;
        capLabel.style.color = isDarkTheme ? new Color(0.6f, 0.6f, 0.6f) : new Color(0.5f, 0.5f, 0.5f);
        block.Add(capLabel);

        Label valueLabel = new Label(initialValue);
        valueLabel.style.fontSize = 13;
        valueLabel.style.color = valueColor;
        valueLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        block.Add(valueLabel);

        container.Add(block);
        return valueLabel;
    }




    public void UpdateCompanyNameUI()
    {
        if (root == null) return;
        var nameLabel = root.Q<Label>("CompanyNameLabel");
        if (nameLabel != null && BrandManager.Instance != null)
        {
            nameLabel.text = BrandManager.Instance.companyName;
        }
    }

    // Логика редактирования названия по клику
    private void SetupCompanyNameEditing()
    {
        var nameLabel = root.Q<Label>("CompanyNameLabel");
        if (nameLabel == null) return;

        // При клике на текст...
        nameLabel.RegisterCallback<ClickEvent>(evt =>
        {
            // Создаем поле ввода на месте лейбла
            var textField = new TextField();
            textField.value = nameLabel.text;
            textField.style.width = 150;
            textField.style.fontSize = 14;
            textField.style.color = Color.black; // чтобы текст было видно на белом фоне инпута

            // Заменяем Label на TextField
            var parent = nameLabel.parent;
            int index = parent.IndexOf(nameLabel);
            parent.Insert(index, textField);
            nameLabel.style.display = DisplayStyle.None; // Скрываем лейбл

            textField.Focus(); // Автоматически ставим курсор

            // Функция завершения редактирования
            void FinishEditing(string newName)
            {
                if (!string.IsNullOrWhiteSpace(newName))
                {
                    BrandManager.Instance.SetCompanyName(newName);
                }
                
                // Возвращаем лейбл обратно
                textField.RemoveFromHierarchy();
                nameLabel.style.display = DisplayStyle.Flex;
                nameLabel.text = BrandManager.Instance.companyName;
            }

            // При нажатии Enter
            textField.RegisterCallback<KeyDownEvent>(keyEvt =>
            {
                if (keyEvt.keyCode == KeyCode.Return || keyEvt.keyCode == KeyCode.KeypadEnter)
                    FinishEditing(textField.value);
            });

            // Если кликнули мимо (потеряли фокус)
            textField.RegisterCallback<BlurEvent>(blurEvt =>
            {
                FinishEditing(textField.value);
            });
        });
    }


    public void UpdateLogoUI()
    {
        if (root == null) return;
        
        var logoElement = root.Q<VisualElement>("CompanyLogoElement");
        if (logoElement != null)
        {
            if (BrandManager.Instance != null && BrandManager.Instance.companyLogo != null)
            {
                // Подставляем текстуру на фон элемента
                logoElement.style.backgroundImage = new StyleBackground(BrandManager.Instance.companyLogo);
            }
            else
            {
                // Если логотипа нет, можно показать дефолтную иконку
                logoElement.style.backgroundImage = null; 
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

           // ---- Расчёт себестоимости ----
            var stats = CalculateCurrentStats(car);
            float productionCost = stats.cost;
            int modPrice = (int)stats.price;
            double finalProfit = stats.profit;
            float taxRate = economy.GetTaxRate(car);
            double profitBeforeTax = modPrice - productionCost; // Добавлено для тултипа

            // Скидка от платформы (только для отображения в интерфейсе)
            float platformDiscount = 0f;
            if (car.platform != null && car.platform.isDeveloped && PlatformManager.Instance != null)
            {
                platformDiscount = PlatformManager.Instance.GetPlatformDiscount(car);
            }
            
            // Записываем итоговую стоимость в сам Blueprint
            car.finalAssemblyCostWithOptions = Mathf.RoundToInt(productionCost);

            // ---- Платформа ----
            if (cardData.platformLabel != null)
            {
                if (car.platform == null)
                {
                    cardData.platformLabel.text = "Платформа: не привязана";
                    cardData.platformLabel.style.color = new Color(0.6f, 0.6f, 0.6f);
                }
                else
                {
                    string platformName = car.platform.platformName;
                    bool isDeveloped = car.platform.isDeveloped;
                    if (isDeveloped)
                    {
                        cardData.platformLabel.text = $"🛠️ Платформа: {platformName} ✅";
                        cardData.platformLabel.style.color = new Color(0.56f, 0.93f, 0.56f);
                    }
                    else
                    {
                        cardData.platformLabel.text = $"🛠️ Платформа: {platformName} (в разработке)";
                        cardData.platformLabel.style.color = Color.yellow;
                    }
                }
            }

            // ---- Обновление информации о контрактах ----
            if (cardData.contractMatchLabel != null)
            {
                if (ContractManager.Instance != null)
                {
                    var matches = ContractManager.Instance.GetMatchingContracts(car);
                    if (matches.Count > 0)
                    {
                        List<string> titles = new List<string>();
                        bool hasActive = false;
                        foreach (var m in matches)
                        {
                            if (m.isAccepted) hasActive = true;
                            titles.Add($"{m.title}{(m.isAccepted ? " (выполняется)" : "")}");
                        }
                        cardData.contractMatchLabel.text = $"📋 Контракты: {string.Join(", ", titles)}";
                        cardData.contractMatchLabel.style.color = hasActive ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.8f, 0.2f); 
                    }
                    else
                    {
                        cardData.contractMatchLabel.text = "📋 Нет подходящих контрактов";
                        cardData.contractMatchLabel.style.color = isDarkTheme ? new Color(0.5f, 0.5f, 0.5f) : new Color(0.6f, 0.6f, 0.6f);
                    }
                }
            }

            // ---- Обновление рейтинга ----
            if (cardData.ratingLabel != null)
            {
                float rating = car.CalculateRating(economy.TotalPriceModifier);
                cardData.ratingLabel.text = $"★ {rating:F1}";
                if (rating >= 8f) cardData.ratingLabel.style.color = new Color(0.2f, 0.9f, 0.2f);
                else if (rating >= 5f) cardData.ratingLabel.style.color = new Color(1f, 0.8f, 0.2f);
                else cardData.ratingLabel.style.color = new Color(1f, 0.3f, 0.3f);
            }

            // Обновляем количество на складе
            if (cardData.stockLabel != null)
                cardData.stockLabel.text = $"На складе: {WarehouseManager.Instance.GetCarCount(car)}";

            // Обновляем прибыль
            if (cardData.profitLabel != null)
            {
                cardData.profitLabel.text = $"{finalProfit:F0}";
                cardData.profitLabel.style.color = finalProfit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
                cardData.profitLabel.tooltip = $"Цена: {modPrice:F0}\nСебестоимость: {productionCost:F0}\nНалог: {profitBeforeTax * taxRate:F0}\nПрибыль: {finalProfit:F0}";
            }

            // Спрос
            float demandValue = car.CurrentDemandMultiplier;
            if (cardData.demandLabel != null)
            {
                cardData.demandLabel.text = $"{demandValue:F1}x";
                cardData.demandLabel.style.color = demandValue > 1.2f ? new Color(0.1f, 0.6f, 0.1f) : (demandValue < 0.8f ? new Color(0.8f, 0.1f, 0.1f) : new Color(0.6f, 0.4f, 0.0f));
            }

            // Кнопка улучшения
            if (cardData.upgradeButton != null)
            {
                bool hasHigherLevel = carCards.Any(cd => cd.car != car && cd.car.carName == car.carName && cd.car.currentLevel > car.currentLevel);
                bool canUpgrade = !hasHigherLevel && (car.levels != null && car.levels.Length > 0 && car.currentLevel < car.levels.Length) && upgradeUnlocked;
                cardData.upgradeButton.SetEnabled(canUpgrade);
                cardData.upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            }

            // Налог
            if (cardData.taxRateLabel != null)
                cardData.taxRateLabel.text = $"{taxRate:P0}";

            // Цена
            if (cardData.detailsLabel != null)
            {
                int currentModCost = Mathf.RoundToInt(productionCost);
                string costSuffix = platformDiscount > 0f ? $" (🛠️-{platformDiscount * 100:F0}%)" : "";
                cardData.detailsLabel.text = $"${currentModCost}{costSuffix}";
            }

            if (cardData.priceLabel != null)
            {
                cardData.priceLabel.text = $"${car.currentPrice}";
            }
        }

        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
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
            Debug.LogError("techScrollView == null!");
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

        // ---- Параметры сетки ----
        const float nodeWidth = 260f;
        const float nodeHeight = 140f;
        const float horizontalGap = 180f; // Увеличили расстояние между колонками
        const float verticalGap = 60f;    // Уменьшили расстояние по вертикали (чтобы дерево не было слишком длинным)

        // ---- Определяем уровни (по зависимостям) ----
        Dictionary<Technology, int> techLevels = new Dictionary<Technology, int>();
        foreach (var tech in technologies)
            if (tech != null) techLevels[tech] = CalculateTechLevel(tech, technologies);
        if (techLevels.Count == 0) return;

        int maxLevel = techLevels.Values.Max();

        // ---- Группируем по уровню ----
        Dictionary<int, List<Technology>> levelGroups = new Dictionary<int, List<Technology>>();
        for (int i = 0; i <= maxLevel; i++) levelGroups[i] = new List<Technology>();
        foreach (var kvp in techLevels) levelGroups[kvp.Value].Add(kvp.Key);


        int totalWidth = (maxLevel + 1) * (int)(nodeWidth + horizontalGap) + 80;
        int totalHeight = Mathf.Max(400, levelGroups.Values.Max(l => l.Count) * (int)(nodeHeight + verticalGap) + 120);

        // ---- Корневой контейнер ----
        techGraphRoot = new VisualElement();
        techGraphRoot.style.width = new Length(totalWidth, LengthUnit.Pixel);
        techGraphRoot.style.height = new Length(totalHeight, LengthUnit.Pixel);
        techGraphRoot.style.position = Position.Relative;
        techGraphRoot.style.overflow = Overflow.Visible;
        techScrollView.Add(techGraphRoot);

        // ---- Слой для линий связей (под узлами) ----
        VisualElement lineLayer = new VisualElement();
        lineLayer.style.position = Position.Absolute;
        lineLayer.style.left = 0;
        lineLayer.style.top = 0;
        lineLayer.style.right = 0;
        lineLayer.style.bottom = 0;
        lineLayer.pickingMode = PickingMode.Ignore;
        techGraphRoot.Add(lineLayer);

        // ---- Создаём узлы ----
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

        // ---- Заполняем связи (parent/child) ----
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

        // ============================================================
        // ЛЕЙАУТ ДЕРЕВА: родитель центрируется относительно своих детей.
        // Цепочки технологий выстраиваются по вертикали — без пересечений.
        // ============================================================

        Dictionary<TechNode, float> nodeYPositions = new Dictionary<TechNode, float>();
        float cursorY = 0f;

        // Рекурсивная функция: расставляет поддерево и возвращает Y-позицию узла
        float LayoutNode(TechNode node)
        {
            if (nodeYPositions.ContainsKey(node))
                return nodeYPositions[node];

            // Лист — занимает следующую свободную позицию
            if (node.children.Count == 0)
            {
                float y = cursorY;
                cursorY += nodeHeight + verticalGap;
                nodeYPositions[node] = y;
                return y;
            }

            // Сначала расставляем всех детей
            float firstChildY = -1f;
            float lastChildY = -1f;

            foreach (var child in node.children)
            {
                float childY;
                if (nodeYPositions.ContainsKey(child))
                    childY = nodeYPositions[child];
                else
                    childY = LayoutNode(child);

                if (firstChildY < 0f) firstChildY = childY;
                lastChildY = childY;
            }

            // Если все дети уже были расставлены ранее — берём их реальные позиции
            if (firstChildY < 0f)
            {
                firstChildY = node.children.Min(c => nodeYPositions[c]);
                lastChildY = node.children.Max(c => nodeYPositions[c]);
            }

            // Родитель — по центру между первым и последним ребёнком
            float parentY = (firstChildY + lastChildY) / 2f;
            nodeYPositions[node] = parentY;
            return parentY;
        }

        // 1. Обрабатываем все корни (узлы без родителей)
        foreach (var root in techNodes.Where(n => n.parents.Count == 0))
            LayoutNode(root);

        // 2. На всякий случай обрабатываем оставшиеся (если граф с циклами или мульти-родителями)
        foreach (var node in techNodes)
        {
            if (!nodeYPositions.ContainsKey(node))
                LayoutNode(node);
        }

        // 3. Применяем позиции к VisualElement'ам
        float maxY = 0f;
        foreach (var node in techNodes)
        {
            if (node == null) continue;

            int level = techLevels[node.tech];
            float x = level * (nodeWidth + horizontalGap) + 40;
            float y = nodeYPositions[node] + 40;

            node.position = new Vector2(x, y);

            node.element.style.position = Position.Absolute;
            node.element.style.left = x;
            node.element.style.top = y;
            techGraphRoot.Add(node.element);

            if (y + nodeHeight > maxY) maxY = y + nodeHeight;
        }

        // 4. Подгоняем высоту контейнера под фактический размер дерева
        techGraphRoot.style.height = new Length(maxY + 60, LengthUnit.Pixel);

        // ---- Рисуем связи ----
        lineLayer.generateVisualContent += (meshGenerationContext) =>
        {
            var rect = lineLayer.contentRect;
            if (rect.width < 1 || rect.height < 1) return;
            var painter = meshGenerationContext.painter2D;

            foreach (var node in techNodes)
            {
                if (node == null) continue;
                foreach (var child in node.children)
                {
                    if (child == null) continue;

                    Vector2 start = node.position + new Vector2(nodeWidth, nodeHeight / 2);
                    Vector2 end = child.position + new Vector2(0, nodeHeight / 2);

                    // Цвет связи: зелёный, если оба исследованы, иначе серый
                    bool bothResearched = node.tech.isResearched && child.tech.isResearched;
                    Color lineColor = bothResearched
                        ? new Color(0.56f, 0.93f, 0.56f, 0.9f)
                        : new Color(0.4f, 0.4f, 0.4f, 0.8f);

                    painter.lineWidth = bothResearched ? 3f : 2f;
                    painter.strokeColor = lineColor;

                    // Красивая кривая Безье между узлами
                    painter.BeginPath();
                    painter.MoveTo(start);

                    float controlOffset = (end.x - start.x) * 0.5f;
                    Vector2 c1 = new Vector2(start.x + controlOffset, start.y);
                    Vector2 c2 = new Vector2(end.x - controlOffset, end.y);
                    painter.BezierCurveTo(c1, c2, end);
                    painter.Stroke();

                    // Стрелка на конце
                    DrawArrow(painter, end, lineColor, bothResearched);
                }
            }
        };
        techGraphRoot.RegisterCallback<GeometryChangedEvent>(evt => lineLayer.MarkDirtyRepaint());

        RefreshTechButtons();
    }

    // ---- Рисует стрелку на конце линии ----
    private void DrawArrow(Painter2D painter, Vector2 tip, Color color, bool bothResearched)
    {
        float arrowSize = bothResearched ? 8f : 6f;
        painter.fillColor = color;
        painter.BeginPath();
        painter.MoveTo(tip);
        painter.LineTo(new Vector2(tip.x - arrowSize, tip.y - arrowSize * 0.6f));
        painter.LineTo(new Vector2(tip.x - arrowSize, tip.y + arrowSize * 0.6f));
        painter.ClosePath();
        painter.Fill();
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
        CloseDealerWindow();
        CloseStaffWindow();
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
            RefreshCarList();

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

    private void OnEnable()
    {
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged += UpdateNewsUI;
    }

    private void OnDisable()
    {
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged -= UpdateNewsUI;
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
        if (platformsOverlay != null) platformsOverlay.style.display = DisplayStyle.None;
        if (contractsOverlay != null) contractsOverlay.style.display = DisplayStyle.None;
        if (dealerOverlay != null) dealerOverlay.style.display = DisplayStyle.None;
        if (staffOverlay != null) staffOverlay.style.display = DisplayStyle.None;
        if (chargingOverlay != null) chargingOverlay.style.display = DisplayStyle.None;
        if (carConfiguratorOverlay != null) carConfiguratorOverlay.style.display = DisplayStyle.None;
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



    private void SetupPlatformsListView()
    {
        if (platformsListView == null) return;

        platformsListView.makeItem = () =>
        {
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.paddingLeft = 8;
            row.style.paddingRight = 8;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f);

            var nameLabel = new Label { name = "PlatformName" };
            nameLabel.style.width = 180;
            nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

            var typeLabel = new Label { name = "PlatformType" };
            typeLabel.style.width = 100;
            typeLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

            var costLabel = new Label { name = "PlatformCost" };
            costLabel.style.width = 100;
            costLabel.style.color = Color.yellow;

            var timeLabel = new Label { name = "PlatformTime" };
            timeLabel.style.width = 80;
            timeLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

            var discountLabel = new Label { name = "PlatformDiscount" };
            discountLabel.style.width = 100;
            discountLabel.style.color = new Color(0.56f, 0.93f, 0.56f);

            var statusLabel = new Label { name = "PlatformStatus" };
            statusLabel.style.flexGrow = 1;
            statusLabel.style.color = Color.white;

            row.Add(nameLabel);
            row.Add(typeLabel);
            row.Add(costLabel);
            row.Add(timeLabel);
            row.Add(discountLabel);
            row.Add(statusLabel);
            return row;
        };

        platformsListView.bindItem = (element, index) =>
        {
            if (platformsListView.itemsSource == null || index >= platformsListView.itemsSource.Count) return;
            var data = (PlatformDisplayData)platformsListView.itemsSource[index];
            var platform = data.platform;
            var dm = CarCompanyManager.Instance.DifficultyManager;

            element.Q<Label>("PlatformName").text = platform.platformName;
            element.Q<Label>("PlatformType").text = platform.type.ToString();
            element.Q<Label>("PlatformCost").text = $"${platform.developmentCost * dm.CurrentPlatformCostModifier:F0}";
            element.Q<Label>("PlatformTime").text = $"{platform.developmentTimeMonths * dm.CurrentPlatformTimeModifier:F1} мес.";
            element.Q<Label>("PlatformDiscount").text = $"-{platform.productionCostReduction * 100:F0}%";

            var status = element.Q<Label>("PlatformStatus");
            if (data.isDeveloped)
            {
                status.text = "✅ Разработана";
                status.style.color = Color.green;
            }
            else if (data.isDeveloping)
            {
                status.text = $"⏳ В разработке ({data.progress:F1} мес.)";
                status.style.color = Color.yellow;
            }
            else if (data.canDevelop)
            {
                status.text = "🔧 Доступна для разработки";
                status.style.color = Color.white;
            }
            else
            {
                status.text = "🔒 Изучите технологию";
                status.style.color = Color.gray;
            }
        };
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
        node.style.width = 260;
        node.style.height = 140;
        node.style.paddingTop = 10;
        node.style.paddingBottom = 10;
        node.style.paddingLeft = 12;
        node.style.paddingRight = 12;
        node.style.borderTopLeftRadius = 10;
        node.style.borderTopRightRadius = 10;
        node.style.borderBottomLeftRadius = 10;
        node.style.borderBottomRightRadius = 10;
        node.style.borderTopWidth = 2;
        node.style.borderBottomWidth = 2;
        node.style.borderLeftWidth = 2;
        node.style.borderRightWidth = 2;
        node.style.flexDirection = FlexDirection.Column;
        node.style.justifyContent = Justify.SpaceBetween;
        node.style.overflow = Overflow.Hidden;
        node.userData = tech;

        // ---- Цветовая схема по категории ----
        Color categoryColor = GetCategoryColor(tech.techName);
        node.style.borderTopColor = categoryColor;
        node.style.borderBottomColor = categoryColor;
        node.style.borderLeftColor = categoryColor;
        node.style.borderRightColor = categoryColor;
        node.style.backgroundColor = isDarkTheme ? new Color(0.14f, 0.14f, 0.16f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);

        // ---- Верхняя строка: иконка + название ----
        VisualElement header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.alignItems = Align.Center;
        header.style.marginBottom = 6;

        Label iconLabel = new Label(GetCategoryIcon(tech.techName));
        iconLabel.style.fontSize = 24;
        iconLabel.style.marginRight = 6;
        header.Add(iconLabel);

        Label nameLabel = new Label(tech.techName);
        nameLabel.style.fontSize = 13;
        nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLabel.style.whiteSpace = WhiteSpace.Normal;
        nameLabel.style.flexShrink = 1;
        header.Add(nameLabel);

        node.Add(header);

        // ---- Описание ----
        Label descLabel = new Label(tech.description);
        descLabel.style.fontSize = 10;
        descLabel.style.color = new Color(0.75f, 0.75f, 0.75f);
        descLabel.style.whiteSpace = WhiteSpace.Normal;
        descLabel.style.flexGrow = 1;
        node.Add(descLabel);

        // ---- Нижняя строка: цена и статус ----
        VisualElement footer = new VisualElement();
        footer.style.flexDirection = FlexDirection.Row;
        footer.style.justifyContent = Justify.SpaceBetween;
        footer.style.alignItems = Align.Center;
        footer.style.marginTop = 6;

        int baseCost = Mathf.RoundToInt(tech.researchCost * techCostMultiplier);
        Label costLabel = new Label($"💰 {baseCost}");
        costLabel.style.fontSize = 12;
        costLabel.style.color = new Color(1f, 0.85f, 0.3f);
        costLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        footer.Add(costLabel);

        Label statusLabel = new Label();
        statusLabel.style.fontSize = 11;
        statusLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
        footer.Add(statusLabel);

        node.Add(footer);

        // ---- Кнопка (на весь узел) ----
        Button btn = new Button();
        btn.style.position = Position.Absolute;
        btn.style.left = 0;
        btn.style.top = 0;
        btn.style.right = 0;
        btn.style.bottom = 0;
        btn.style.backgroundColor = Color.clear;
        btn.style.borderTopWidth = 0;
        btn.style.borderBottomWidth = 0;
        btn.style.borderLeftWidth = 0;
        btn.style.borderRightWidth = 0;
        btn.userData = tech;
        btn.clicked += () => CarCompanyManager.Instance.TechManager.ResearchTechnology(tech);
        node.Add(btn);

        // ---- Hover-эффект ----
        node.RegisterCallback<MouseEnterEvent>(evt =>
        {
            node.style.scale = new Scale(new Vector3(1.03f, 1.03f, 1f));
            node.style.backgroundColor = new Color(0.2f, 0.2f, 0.24f, 1f);
        });
        node.RegisterCallback<MouseLeaveEvent>(evt =>
        {
            node.style.scale = new Scale(new Vector3(1f, 1f, 1f));
            node.style.backgroundColor = isDarkTheme ? new Color(0.14f, 0.14f, 0.16f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
        });

        // ---- Применяем текущее состояние ----
        UpdateTechButtonState(btn, tech, node, statusLabel);

        return node;
    }

    private void UpdateTechButtonState(Button button, Technology tech, VisualElement node = null, Label statusLabel = null)
    {
        if (button == null || tech == null) return;

        // Находим node и statusLabel, если не переданы
        if (node == null) node = button.parent;
        if (statusLabel == null && node != null)
            statusLabel = node.Q<Label>(className: "status-label");

        // ---- Уже исследована ----
        if (tech.isResearched)
        {
            button.SetEnabled(false);
            if (node != null)
            {
                node.style.opacity = 0.6f;
                node.style.backgroundColor = new Color(0.12f, 0.2f, 0.12f, 1f);
            }
            if (statusLabel != null)
            {
                statusLabel.text = "✅ Изучено";
                statusLabel.style.color = new Color(0.56f, 0.93f, 0.56f);
            }
            return;
        }

        // ---- Проверка требований ----
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

        // ---- Проверка доступности по времени ----
        int currentYear = GameTimeManager.Instance?.currentYear ?? 2025;
        int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
        bool timeAvailable = tech.IsAvailable(currentYear, currentMonth);

        // ---- Применяем визуал ----
        if (!requirementsMet)
        {
            button.SetEnabled(false);
            if (node != null)
            {
                node.style.opacity = 0.5f;
                node.style.backgroundColor = new Color(0.18f, 0.12f, 0.12f, 1f);
            }
            if (statusLabel != null)
            {
                statusLabel.text = "🔒 Требования";
                statusLabel.style.color = new Color(0.9f, 0.4f, 0.4f);
            }
        }
        else if (!timeAvailable)
        {
            button.SetEnabled(true);
            if (node != null)
            {
                node.style.opacity = 0.9f;
                node.style.backgroundColor = new Color(0.22f, 0.18f, 0.1f, 1f);
            }
            if (statusLabel != null)
            {
                statusLabel.text = $"⏳ с {tech.availableMonth:D2}/{tech.availableYear}";
                statusLabel.style.color = new Color(1f, 0.7f, 0.3f);
            }
        }
        else
        {
            button.SetEnabled(true);
            if (node != null)
            {
                node.style.opacity = 1f;
                node.style.backgroundColor = isDarkTheme ? new Color(0.14f, 0.14f, 0.16f, 1f) : new Color(0.9f, 0.9f, 0.9f, 1f);
            }
            if (statusLabel != null)
            {
                statusLabel.text = "🟢 Доступно";
                statusLabel.style.color = new Color(0.5f, 0.9f, 0.5f);
            }
        }

        // ---- Цвет границы: жёлтый при доступности, серый при блокировке ----
        if (node != null)
        {
            Color borderColor = requirementsMet
                ? (timeAvailable ? new Color(1f, 0.85f, 0.3f) : new Color(1f, 0.6f, 0.2f))
                : new Color(0.4f, 0.4f, 0.4f);
            node.style.borderTopColor = borderColor;
            node.style.borderBottomColor = borderColor;
            node.style.borderLeftColor = borderColor;
            node.style.borderRightColor = borderColor;
        }
    }




    private string GetCategoryIcon(string techName)
    {
        if (techName.StartsWith("power_")) return "⚡";
        if (techName.StartsWith("economy_")) return "🌿";
        if (techName.StartsWith("design_")) return "🎨";
        if (techName.StartsWith("safety_")) return "🛡️";
        if (techName.StartsWith("Производство")) return "🔧";
        if (techName.StartsWith("Реклама")) return "📢";
        if (techName.StartsWith("Улучшение склада")) return "📦";
        if (techName.StartsWith("Разработка")) return "🛠️";
        if (techName == "Электромобиль") return "🔋";
        if (techName == "Гибридный двигатель") return "🌱";
        if (techName == "Автопилот") return "🤖";
        if (techName == "Массовое производство") return "🏭";
        if (techName == "Улучшить авто") return "🔺";
        if (techName == "Международная экспансия") return "🌍";
        if (techName == "Патентное право") return "📜";
        return "📘";
    }

    private Color GetCategoryColor(string techName)
    {
        if (techName.StartsWith("power_")) return new Color(0.9f, 0.4f, 0.2f);       // оранжевый
        if (techName.StartsWith("economy_")) return new Color(0.3f, 0.8f, 0.4f);     // зелёный
        if (techName.StartsWith("design_")) return new Color(0.8f, 0.4f, 0.9f);      // фиолетовый
        if (techName.StartsWith("safety_")) return new Color(0.3f, 0.6f, 0.9f);      // синий
        if (techName.StartsWith("Производство")) return new Color(0.7f, 0.7f, 0.3f); // жёлтый
        if (techName.StartsWith("Реклама")) return new Color(0.9f, 0.6f, 0.3f);      // оранжевый
        if (techName.StartsWith("Улучшение склада")) return new Color(0.6f, 0.5f, 0.3f);
        if (techName.StartsWith("Разработка")) return new Color(0.4f, 0.8f, 0.9f);   // голубой
        if (techName == "Электромобиль") return new Color(0.3f, 1f, 0.5f);
        if (techName == "Гибридный двигатель") return new Color(0.5f, 0.9f, 0.3f);
        if (techName == "Автопилот") return new Color(0.5f, 0.5f, 1f);
        if (techName == "Международная экспансия") return new Color(0.4f, 0.7f, 1f);
        if (techName == "Патентное право") return new Color(0.8f, 0.7f, 0.4f);
        return new Color(0.5f, 0.5f, 0.5f);
    }

    private int GetCategoryOrder(string techName)
    {
        if (techName.StartsWith("power_")) return 0;
        if (techName.StartsWith("economy_")) return 1;
        if (techName.StartsWith("design_")) return 2;
        if (techName.StartsWith("safety_")) return 3;
        if (techName.StartsWith("Разработка")) return 4;
        if (techName.StartsWith("Производство")) return 5;
        if (techName.StartsWith("Улучшение склада")) return 6;
        if (techName.StartsWith("Реклама")) return 7;
        return 8;
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

        // Применяем сложность через DifficultyManager
        var dm = CarCompanyManager.Instance.DifficultyManager;
        dm.SetDifficulty((DifficultyManager.DifficultyLevel)newDifficulty);

        // Применяем настройки экономики (БЕЗ АРГУМЕНТОВ)
        CarCompanyManager.Instance.EconomyManager.ApplyDifficultySettings();

        // Обновляем спрос
        CarCompanyManager.Instance.DemandManager.UpdateDemand();
        UpdateNewsUI();
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
            nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
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
                    manager.OpenOffice(def.id);
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
                nameLabel.style.color = isDarkTheme ? Color.white : Color.black;
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


    // ========== КОНТРАКТЫ ==========

    private void OpenContractsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (contractsOverlay == null)
        {
            Debug.LogWarning("ContractsOverlay не найден в UXML");
            ShowNotification("Окно контрактов не найдено.");
            return;
        }

        menuContainer.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.None;
        AnimateWindowOpen(contractsOverlay);
        RefreshContractsUI();
    }

    private void CloseContractsWindow()
    {
        if (contractsOverlay != null)
            AnimateWindowClose(contractsOverlay, () =>
            {
                contractsOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
    }

    public void RefreshContractsUI()
    {
        if (contractsContainer == null) return;
        contractsContainer.Clear();

        if (ContractManager.Instance == null)
        {
            var err = new Label("ContractManager не найден в сцене.");
            err.style.color = new Color(1f, 0.4f, 0.4f);
            contractsContainer.Add(err);
            return;
        }

        // ---- Доступные ----
        var availHeader = new Label("📋 Доступные контракты");
        availHeader.style.color = Color.black;
        availHeader.style.fontSize = 16;
        availHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
        availHeader.style.marginBottom = 6;
        contractsContainer.Add(availHeader);

        if (ContractManager.Instance.availableContracts == null
            || ContractManager.Instance.availableContracts.Count == 0)
        {
            var empty = new Label("Нет доступных контрактов. Новые появятся со временем.");
            empty.style.color = new Color(0.4f, 0.4f, 0.4f);
            empty.style.marginBottom = 10;
            contractsContainer.Add(empty);
        }
        else
        {
            foreach (var c in ContractManager.Instance.availableContracts)
                contractsContainer.Add(CreateContractRow(c, isActive: false));
        }

        // ---- Активные ----
        var actHeader = new Label("✅ Активные контракты");
        actHeader.style.color = Color.black;
        actHeader.style.fontSize = 16;
        actHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
        actHeader.style.marginTop = 12;
        actHeader.style.marginBottom = 6;
        contractsContainer.Add(actHeader);

        if (ContractManager.Instance.activeContracts == null
            || ContractManager.Instance.activeContracts.Count == 0)
        {
            var empty = new Label("Нет активных контрактов.");
            empty.style.color = new Color(0.4f, 0.4f, 0.4f);
            contractsContainer.Add(empty);
        }
        else
        {
            foreach (var c in ContractManager.Instance.activeContracts)
                contractsContainer.Add(CreateContractRow(c, isActive: true));
        }
    }

    private VisualElement CreateContractRow(ContractData c, bool isActive)
    {
        var row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.alignItems = Align.Center;
        row.style.paddingTop = 8;
        row.style.paddingBottom = 8;
        row.style.paddingLeft = 10;
        row.style.paddingRight = 10;
        row.style.marginBottom = 4;
        row.style.borderBottomWidth = 1;
        row.style.borderBottomColor = new Color(0.75f, 0.75f, 0.75f);
        row.style.backgroundColor = isDarkTheme
            ? new Color(0.18f, 0.18f, 0.18f)
            : new Color(0.96f, 0.96f, 0.96f);

        // Колонка с названием и требованиями
        var infoCol = new VisualElement();
        infoCol.style.width = 280; // Увеличили ширину с 200, чтобы влез текст требований
        infoCol.style.flexDirection = FlexDirection.Column;
        infoCol.style.justifyContent = Justify.Center;

        var title = new Label(c.title);
        title.style.color = isDarkTheme ? Color.white : Color.black;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.fontSize = 14;
        infoCol.Add(title);

        // НОВЫЙ БЛОК: Предупреждение о конкуренте
        if (!isActive && c.isCompetitorInterested)
        {
            var warningLabel = new Label($"👀 {c.competitorName} присматривается!");
            warningLabel.style.fontSize = 10;
            warningLabel.style.color = new Color(1f, 0.6f, 0.2f); // Оранжевый
            warningLabel.style.marginTop = 2;
            infoCol.Add(warningLabel);
        }

        // Получаем текст требований из менеджера
        string reqText = ContractManager.Instance != null 
            ? ContractManager.Instance.GetContractRequirementsText(c) 
            : "Нет данных о требованиях";

        var reqLabel = new Label(reqText);
        reqLabel.style.fontSize = 11;
        reqLabel.style.color = isDarkTheme ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.4f, 0.4f, 0.4f);
        reqLabel.style.marginTop = 2;
        infoCol.Add(reqLabel);

        row.Add(infoCol);

        var qty = new Label(isActive
            ? $"{c.deliveredQuantity} / {c.requiredQuantity}"
            : $"{c.requiredQuantity} шт.");
        qty.style.width = 80;
        qty.style.color = isActive
            ? new Color(0.6f, 0.4f, 0.0f)
            : (isDarkTheme ? Color.white : Color.black);
        row.Add(qty);

        var deadline = new Label($"{c.monthsRemaining} мес.");
        deadline.style.width = 80;
        deadline.style.color = c.monthsRemaining <= 1
            ? new Color(0.9f, 0.2f, 0.2f)
            : (isDarkTheme ? Color.white : Color.black);
        row.Add(deadline);

        var reward = new Label($"💰 {c.rewardMoney:F0}");
        reward.style.width = 110;
        reward.style.color = new Color(0.1f, 0.55f, 0.1f);
        row.Add(reward);

        var penalty = new Label($"⚠️ {c.penaltyMoney:F0}");
        penalty.style.width = 110;
        penalty.style.color = new Color(0.8f, 0.2f, 0.2f);
        row.Add(penalty);

        if (!isActive)
        {
            var acceptBtn = new Button(() =>
            {
                ContractManager.Instance.AcceptContract(c.contractId);
            });
            acceptBtn.text = "Принять";
            acceptBtn.style.width = 90;
            acceptBtn.style.height = 26;
            acceptBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            acceptBtn.style.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
            acceptBtn.style.color = Color.black;
            row.Add(acceptBtn);
        }
        else
        {
            var bar = new VisualElement();
            bar.style.width = 90;
            bar.style.height = 12;
            bar.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
            bar.style.borderTopLeftRadius = 3;
            bar.style.borderTopRightRadius = 3;
            bar.style.borderBottomLeftRadius = 3;
            bar.style.borderBottomRightRadius = 3;

            var fill = new VisualElement();
            float pct = c.requiredQuantity > 0
                ? Mathf.Clamp01((float)c.deliveredQuantity / c.requiredQuantity)
                : 0f;
            fill.style.width = new Length(pct * 100, LengthUnit.Percent);
            fill.style.height = 12;
            fill.style.backgroundColor = new Color(0.3f, 0.75f, 0.3f);
            fill.style.borderTopLeftRadius = 3;
            fill.style.borderTopRightRadius = 3;
            fill.style.borderBottomLeftRadius = 3;
            fill.style.borderBottomRightRadius = 3;

            bar.Add(fill);
            row.Add(bar);
        }

        return row;
    }


    // ========== ДИЛЕР ==========

    private void OpenDealerWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (dealerOverlay == null)
        {
            ShowNotification("Окно дилера не найдено в UXML.");
            return;
        }

        menuContainer.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.None;
        AnimateWindowOpen(dealerOverlay);
        RefreshDealerUI();
    }

    private void CloseDealerWindow()
    {
        if (dealerOverlay != null)
            AnimateWindowClose(dealerOverlay, () =>
            {
                dealerOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
    }

    
    public void RefreshDealerUI()
    {
        if (dealerContainer == null) return;
        dealerContainer.Clear();

        if (WarehouseManager.Instance == null)
        {
            dealerContainer.Add(new Label("WarehouseManager не найден."));
            return;
        }

        var entries = WarehouseManager.Instance.GetAllEntries();

        if (entries == null || entries.Count == 0)
        {
            var empty = new Label("На складе нет машин.\nПроизведите автомобили, чтобы продать их здесь.");
            empty.style.color = new Color(0.4f, 0.4f, 0.4f);
            empty.style.fontSize = 14;
            empty.style.whiteSpace = WhiteSpace.Normal;
            empty.style.marginTop = 30;
            empty.style.unityTextAlign = TextAnchor.MiddleCenter;
            dealerContainer.Add(empty);
            return;
        }

        // ---- Заголовок ----
        var header = new VisualElement();
        header.style.flexDirection = FlexDirection.Row;
        header.style.paddingTop = 6;
        header.style.paddingBottom = 6;
        header.style.paddingLeft = 10;
        header.style.paddingRight = 10;
        header.style.backgroundColor = new Color(0.3f, 0.3f, 0.3f);
        header.style.marginBottom = 4;

        float[] widths = { 190, 90, 130, 80, 100, 120, 240 };
        string[] headers = { "Модель", "Коробка", "Комплектация", "На складе", "Цена за шт.", "Общая стоимость", "Действия" };
        for (int i = 0; i < headers.Length; i++)
        {
            var lbl = new Label(headers[i]);
            lbl.style.width = widths[i];
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            header.Add(lbl);
        }
        dealerContainer.Add(header);

        // ---- Строки по каждой партии ----
        foreach (var entry in entries)
        {
            if (entry == null || entry.blueprint == null || entry.count <= 0) continue;

            CarBlueprint car = entry.blueprint;
            int qty = entry.count;
            int unitPrice = WarehouseManager.Instance.GetUnitPriceForEntry(entry);
            double totalValue = (double)unitPrice * qty;

            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.paddingTop = 6;
            row.style.paddingBottom = 6;
            row.style.paddingLeft = 10;
            row.style.paddingRight = 10;
            row.style.marginBottom = 4;
            row.style.borderBottomWidth = 1;
            row.style.borderBottomColor = new Color(0.8f, 0.8f, 0.8f);
            row.style.backgroundColor = isDarkTheme
                ? new Color(0.18f, 0.18f, 0.18f)
                : new Color(0.96f, 0.96f, 0.96f);

            var name = new Label(car.GetDisplayName());
            name.style.width = widths[0];
            name.style.color = isDarkTheme ? Color.white : Color.black;
            name.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(name);

            string gearbox = string.IsNullOrEmpty(entry.transmissionType) ? "АКПП" : entry.transmissionType;
            var gearboxLabel = new Label(gearbox);
            gearboxLabel.style.width = widths[1];
            gearboxLabel.style.color = isDarkTheme ? new Color(0.8f, 0.9f, 1f) : new Color(0.1f, 0.3f, 0.6f);
            row.Add(gearboxLabel);

            string trimName = string.IsNullOrEmpty(entry.trimName) ? "Стандарт" : entry.trimName;
            float trimMult = WarehouseManager.GetTrimPriceMultiplier(car, entry.trimName);
            var trimLabel = new Label(trimMult != 1f ? $"{trimName} (×{trimMult:F2})" : trimName);
            trimLabel.style.width = widths[2];
            trimLabel.style.color = isDarkTheme ? new Color(0.9f, 0.8f, 0.6f) : new Color(0.5f, 0.3f, 0.0f);
            trimLabel.tooltip = trimMult != 1f
                ? $"Множитель цены комплектации: ×{trimMult:F2}"
                : "Базовая комплектация (×1.00)";
            row.Add(trimLabel);

            var qtyLabel = new Label(qty.ToString());
            qtyLabel.style.width = widths[3];
            qtyLabel.style.color = isDarkTheme ? Color.white : Color.black;
            row.Add(qtyLabel);

            var priceLabel = new Label($"${unitPrice}");
            priceLabel.style.width = widths[4];
            priceLabel.style.color = new Color(0.6f, 0.4f, 0.0f);
            row.Add(priceLabel);

            var totalLabel = new Label($"${totalValue:F0}");
            totalLabel.style.width = widths[5];
            totalLabel.style.color = new Color(0.1f, 0.55f, 0.1f);
            totalLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(totalLabel);

            var actions = new VisualElement();
            actions.style.flexDirection = FlexDirection.Row;
            actions.style.width = widths[6];
            actions.style.alignItems = Align.Center;

            var entryCopy = entry;

            var sellOneBtn = new Button(() =>
            {
                int sold = WarehouseManager.Instance.SellEntry(entryCopy, 1);
                if (sold > 0) { UpdateMoneyLabels(); UpdateWarehouseLabels(); RefreshDealerUI(); }
            });
            sellOneBtn.text = "Продать 1";
            sellOneBtn.style.width = 90;
            sellOneBtn.style.height = 26;
            sellOneBtn.style.marginRight = 8;
            sellOneBtn.style.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
            sellOneBtn.style.color = Color.black;
            sellOneBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            actions.Add(sellOneBtn);

            var sellAllBtn = new Button(() =>
            {
                int sold = WarehouseManager.Instance.SellEntry(entryCopy, entryCopy.count);
                if (sold > 0) { UpdateMoneyLabels(); UpdateWarehouseLabels(); RefreshDealerUI(); }
            });
            sellAllBtn.text = $"Продать все ({qty})";
            sellAllBtn.style.width = 130;
            sellAllBtn.style.height = 26;
            sellAllBtn.style.backgroundColor = new Color(0.85f, 0.5f, 0.5f);
            sellAllBtn.style.color = Color.black;
            sellAllBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
            actions.Add(sellAllBtn);

            row.Add(actions);
            dealerContainer.Add(row);
        }

          // Блок трейд-ин предложений
        if (CustomerManager.Instance != null && CustomerManager.Instance.tradeInOffers.Count > 0)
        {
            var tradeInHeader = new Label("🔄 Трейд-ин предложения"); // <--- ИСПРАВЛЕНО
            tradeInHeader.style.fontSize = 16;
            tradeInHeader.style.color = isDarkTheme ? Color.white : Color.black;
            tradeInHeader.style.unityFontStyleAndWeight = FontStyle.Bold;
            tradeInHeader.style.marginTop = 20;
            tradeInHeader.style.marginBottom = 10;
            dealerContainer.Add(tradeInHeader);

            foreach (var offer in CustomerManager.Instance.tradeInOffers)
            {
                var row = new VisualElement();
                row.style.flexDirection = FlexDirection.Row;
                row.style.alignItems = Align.Center;
                row.style.paddingTop = 8;
                row.style.paddingBottom = 8;
                row.style.paddingLeft = 10;
                row.style.paddingRight = 10;
                row.style.marginBottom = 4;
                row.style.backgroundColor = isDarkTheme ? new Color(0.22f, 0.22f, 0.22f) : new Color(0.9f, 0.9f, 0.9f);
                row.style.borderTopLeftRadius = 6;
                row.style.borderTopRightRadius = 6;
                row.style.borderBottomLeftRadius = 6;
                row.style.borderBottomRightRadius = 6;

                var label = new Label($"Клиент хочет сдать {offer.oldCarName} (ур. {offer.oldCarLevel + 1}) и купить новый со скидкой {offer.discountPercent * 100:F0}%");
                label.style.flexGrow = 1;
                label.style.color = isDarkTheme ? Color.white : Color.black;
                row.Add(label);

                var btn = new Button(() =>
                {
                    if (CustomerManager.Instance.AcceptTradeIn(offer))
                        RefreshDealerUI();
                });
                btn.text = "Принять";
                btn.style.width = 100;
                btn.style.height = 28;
                btn.style.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
                btn.style.color = Color.black;
                btn.style.unityFontStyleAndWeight = FontStyle.Bold;
                row.Add(btn);

                dealerContainer.Add(row);
            }
        }
    }




    // ========== ПЕРСОНАЛ ==========

    private void OpenStaffWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (staffOverlay == null)
        {
            ShowNotification("Окно персонала не найдено в UXML.");
            return;
        }

        menuContainer.style.display = DisplayStyle.None;
        mainPanel.style.display = DisplayStyle.None;
        AnimateWindowOpen(staffOverlay);
        RefreshStaffUI();
    }

    private void CloseStaffWindow()
    {
        if (staffOverlay != null)
            AnimateWindowClose(staffOverlay, () =>
            {
                staffOverlay.style.display = DisplayStyle.None;
                mainPanel.style.display = DisplayStyle.Flex;
            });
    }

    public void RefreshStaffUI()
    {
        if (staffContainer == null) return;
        staffContainer.Clear();

        if (economy == null)
        {
            staffContainer.Add(new Label("EconomyManager не найден."));
            return;
        }

        // ---- Инженер ----
        int engCost = Mathf.RoundToInt((50 + economy.EngineerCount * 20) * economy.CostMultiplier);
        staffContainer.Add(CreateStaffRow(
            "🔧",
            "Инженер",
            "Ускоряет производство деталей и повышает пассивный доход.",
            $"Пассивный доход: +{economy.EngineerCount * 0.3f:F1}/сек",
            economy.EngineerCount,
            engCost,
            HireEngineerStaff,
            FireEngineerStaff
        ));

        // ---- Дизайнер ----
        int desCost = Mathf.RoundToInt((200 + economy.DesignerCount * 100) * economy.CostMultiplier);
        staffContainer.Add(CreateStaffRow(
            "🎨",
            "Дизайнер",
            "Повышает рейтинг дизайна выпускаемых машин.",
            $"Бонус к дизайну: +{economy.DesignerCount * 5}%",
            economy.DesignerCount,
            desCost,
            HireDesignerStaff,
            FireDesignerStaff
        ));

        // ---- Маркетолог ----
        int mktCost = Mathf.RoundToInt((150 + economy.MarketerCount * 80) * economy.CostMultiplier);
        staffContainer.Add(CreateStaffRow(
            "📢",
            "Маркетолог",
            "Усиливает эффективность рекламных кампаний.",
            $"Бонус к маркетингу: +{economy.MarketerCount * 10}%",
            economy.MarketerCount,
            mktCost,
            HireMarketerStaff,
            FireMarketerStaff
        ));

        // ---- Менеджер ----
        int mgrCost = Mathf.RoundToInt((500 + economy.ManagerCount * 300) * economy.CostMultiplier);
        staffContainer.Add(CreateStaffRow(
            "💼",
            "Менеджер",
            "Автоматизирует закупку деталей (раз в 5 сек).",
            economy.ManagerCount > 0
                ? $"Автозакупка включена ({economy.ManagerCount} шт.)"
                : "Автозакупка выключена",
            economy.ManagerCount,
            mgrCost,
            HireManagerStaff,
            FireManagerStaff
        ));
    }

    private VisualElement CreateStaffRow(
        string icon, string name, string role, string bonus,
        int count, int hireCost,
        System.Action onHire, System.Action onFire)
    {
        var card = new VisualElement();
        card.style.flexDirection = FlexDirection.Row;
        card.style.alignItems = Align.Center;
        card.style.paddingTop = 10;
        card.style.paddingBottom = 10;
        card.style.paddingLeft = 12;
        card.style.paddingRight = 12;
        card.style.marginBottom = 6;
        card.style.borderTopLeftRadius = 6;
        card.style.borderTopRightRadius = 6;
        card.style.borderBottomLeftRadius = 6;
        card.style.borderBottomRightRadius = 6;
        card.style.borderTopWidth = 1;
        card.style.borderBottomWidth = 1;
        card.style.borderLeftWidth = 1;
        card.style.borderRightWidth = 1;
        card.style.borderTopColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
        card.style.borderBottomColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
        card.style.borderLeftColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
        card.style.borderRightColor = isDarkTheme ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f);
        card.style.backgroundColor = isDarkTheme
            ? new Color(0.22f, 0.22f, 0.22f)
            : new Color(0.95f, 0.95f, 0.95f);

        var iconLbl = new Label(icon);
        iconLbl.style.fontSize = 28;
        iconLbl.style.width = 44;
        iconLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
        card.Add(iconLbl);

        var infoCol = new VisualElement();
        infoCol.style.flexDirection = FlexDirection.Column;
        infoCol.style.flexGrow = 1;

        var nameLbl = new Label(name);
        nameLbl.style.fontSize = 15;
        nameLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        nameLbl.style.color = isDarkTheme ? Color.white : Color.black;
        infoCol.Add(nameLbl);

        var roleLbl = new Label(role);
        roleLbl.style.fontSize = 11;
        roleLbl.style.color = isDarkTheme ? new Color(0.7f, 0.7f, 0.7f) : new Color(0.4f, 0.4f, 0.4f);
        roleLbl.style.whiteSpace = WhiteSpace.Normal;
        infoCol.Add(roleLbl);

        var bonusLbl = new Label(bonus);
        bonusLbl.style.fontSize = 11;
        bonusLbl.style.color = isDarkTheme ? new Color(0.6f, 0.8f, 1f) : new Color(0.1f, 0.3f, 0.6f);
        bonusLbl.style.marginTop = 2;
        infoCol.Add(bonusLbl);

        card.Add(infoCol);

        var countLbl = new Label(count.ToString());
        countLbl.style.fontSize = 24;
        countLbl.style.unityFontStyleAndWeight = FontStyle.Bold;
        countLbl.style.color = isDarkTheme ? new Color(1f, 0.9f, 0.4f) : new Color(0.6f, 0.4f, 0.0f);
        countLbl.style.width = 50;
        countLbl.style.unityTextAlign = TextAnchor.MiddleCenter;
        countLbl.style.marginLeft = 8;
        countLbl.style.marginRight = 8;
        card.Add(countLbl);

        var hireBtn = new Button(onHire);
        hireBtn.text = $"Нанять\n${hireCost}";
        hireBtn.style.width = 100;
        hireBtn.style.height = 50;
        hireBtn.style.fontSize = 12;
        hireBtn.style.backgroundColor = new Color(0.5f, 0.85f, 0.5f);
        hireBtn.style.color = Color.black;
        hireBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
        hireBtn.style.marginLeft = 4;
        if (economy.Money < hireCost) hireBtn.SetEnabled(false);
        card.Add(hireBtn);

        var fireBtn = new Button(onFire);
        fireBtn.text = "Уволить";
        fireBtn.style.width = 80;
        fireBtn.style.height = 50;
        fireBtn.style.fontSize = 12;
        fireBtn.style.backgroundColor = new Color(0.9f, 0.5f, 0.5f);
        fireBtn.style.color = Color.black;
        fireBtn.style.unityFontStyleAndWeight = FontStyle.Bold;
        fireBtn.style.marginLeft = 6;
        if (count <= 0) fireBtn.SetEnabled(false);
        card.Add(fireBtn);

        return card;
    }

    private void HireEngineerStaff()
    {
        int before = economy.EngineerCount;
        economy.HireEngineer();
        if (economy.EngineerCount > before)
        {
            UpdateWarehouseLabels();
            UpdateProductionButtonsState();
            UpdateUpgradeUI();
            UpdateMoneyLabels();
            ShowNotification($"✅ Нанят инженер (всего: {economy.EngineerCount})");
        }
        else ShowNotification("❌ Недостаточно денег для найма инженера");
        RefreshStaffUI();
    }

    private void FireEngineerStaff()
    {
        if (economy.EngineerCount <= 0) return;
        economy.FireEngineer();
        UpdateWarehouseLabels();
        UpdateProductionButtonsState();
        UpdateUpgradeUI();
        UpdateMoneyLabels();
        ShowNotification($"Инженер уволен (осталось: {economy.EngineerCount})");
        RefreshStaffUI();
    }

    private void HireDesignerStaff()
    {
        int before = economy.DesignerCount;
        economy.HireDesigner();
        if (economy.DesignerCount > before)
        {
            UpdateMoneyLabels();
            ShowNotification($"✅ Нанят дизайнер (всего: {economy.DesignerCount})");
        }
        else ShowNotification("❌ Недостаточно денег для найма дизайнера");
        RefreshStaffUI();
    }

    private void FireDesignerStaff()
    {
        if (economy.DesignerCount <= 0) return;
        economy.FireDesigner();
        ShowNotification($"Дизайнер уволен (осталось: {economy.DesignerCount})");
        RefreshStaffUI();
    }

    private void HireMarketerStaff()
    {
        int before = economy.MarketerCount;
        economy.HireMarketer();
        if (economy.MarketerCount > before)
        {
            UpdateMoneyLabels();
            ShowNotification($"✅ Нанят маркетолог (всего: {economy.MarketerCount})");
        }
        else ShowNotification("❌ Недостаточно денег для найма маркетолога");
        RefreshStaffUI();
    }

    private void FireMarketerStaff()
    {
        if (economy.MarketerCount <= 0) return;
        economy.FireMarketer();
        ShowNotification($"Маркетолог уволен (осталось: {economy.MarketerCount})");
        RefreshStaffUI();
    }

    private void HireManagerStaff()
    {
        int before = economy.ManagerCount;
        economy.HireManager();
        if (economy.ManagerCount > before)
        {
            UpdateMoneyLabels();
            ShowNotification($"✅ Нанят менеджер (автозакупка деталей включена)");
        }
        else ShowNotification("❌ Недостаточно денег для найма менеджера");
        RefreshStaffUI();
    }

    private void FireManagerStaff()
    {
        if (economy.ManagerCount <= 0) return;
        economy.FireManager();
        ShowNotification($"Менеджер уволен (осталось: {economy.ManagerCount})");
        RefreshStaffUI();
    }


    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= UpdateDateTimeDisplay;
        if (NewsManager.Instance != null)
            NewsManager.Instance.OnNewsChanged -= UpdateNewsUI;
        if (ContractManager.Instance != null)
            ContractManager.Instance.OnContractsChanged -= RefreshContractsUI;
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
        
        // Убираем жесткий потолок basePrice * 3. 
        // Ограничиваем только снизу (10$) и сверху защитным лимитом (1 000 000$), 
        // чтобы игрок не ушел в бесконечность.
        newPrice = Mathf.Clamp(newPrice, 10, 1000000);

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


    private void OpenPlatformsWindow()
    {
        CloseMenuWindow();
        HideAllOverlays();
        if (platformsOverlay == null) return;
        platformsOverlay.style.display = DisplayStyle.Flex;
        AnimateWindowOpen(platformsOverlay);
        UpdatePlatformsUI();
    }

    private void ClosePlatformsWindow()
    {
        if (platformsOverlay != null)
            AnimateWindowClose(platformsOverlay, () => platformsOverlay.style.display = DisplayStyle.None);
    }

    public void UpdatePlatformsUI()
    {
        if (platformsListView == null) return;
        var manager = PlatformManager.Instance;
        if (manager == null) return;

        var list = new List<PlatformDisplayData>();
        var allPlatforms = manager.allPlatforms;
        foreach (var platform in allPlatforms)
        {
            var data = new PlatformDisplayData
            {
                platform = platform,
                isDeveloped = platform.isDeveloped,
                isDeveloping = manager.GetDevelopingPlatforms().Contains(platform),
                progress = manager.GetDevelopmentProgress(platform),
                canDevelop = manager.IsPlatformTechUnlocked(platform.type) && !platform.isDeveloped && !manager.GetDevelopingPlatforms().Contains(platform)
            };
            list.Add(data);
        }

        platformsListView.itemsSource = list;
        platformsListView.Rebuild();
    }

    private void OnDevelopPlatformClicked()
    {
        if (platformsListView.selectedItem == null) return;
        var data = (PlatformDisplayData)platformsListView.selectedItem;
        if (data == null || !data.canDevelop) return;
        PlatformManager.Instance.DevelopPlatform(data.platform);
        UpdatePlatformsUI();
    }

    // ---- Вспомогательный класс для отображения ----
    private class PlatformDisplayData
    {
        public Platform platform;
        public bool isDeveloped;
        public bool isDeveloping;
        public float progress;
        public bool canDevelop;
    }

    private void TryHireDesigner()
    {
        int before = economy.DesignerCount;
        economy.HireDesigner();
        if (economy.DesignerCount > before)
            ShowNotification($"✅ Нанят дизайнер (всего: {economy.DesignerCount})");
        else
            ShowNotification("❌ Недостаточно денег для найма дизайнера");
        UpdateMoneyLabels();
        UpdateUpgradeUI();
    }

    private void TryHireMarketer()
    {
        int before = economy.MarketerCount;
        economy.HireMarketer();
        if (economy.MarketerCount > before)
            ShowNotification($"✅ Нанят маркетолог (всего: {economy.MarketerCount})");
        else
            ShowNotification("❌ Недостаточно денег для найма маркетолога");
        UpdateMoneyLabels();
        UpdateUpgradeUI();
    }

    private void TryHireManager()
    {
        int before = economy.ManagerCount;
        economy.HireManager();
        if (economy.ManagerCount > before)
            ShowNotification($"✅ Нанят менеджер (автозакупка деталей включена)");
        else
            ShowNotification("❌ Недостаточно денег для найма менеджера");
        UpdateMoneyLabels();
        UpdateUpgradeUI();
    }
}