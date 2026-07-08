using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class UIManager : MonoBehaviour
{
    private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement mainPanel;
    private VisualElement notificationContainer;
    private Label eventLabel;
    private Label moneyLabel;
    private Label incomeLabel;
    private Label savedDifficultyLabel;
    private Label versionLabel;

    private VisualElement carsOverlay;
    private VisualElement carsContainer;
    private VisualElement techOverlay;
    private ScrollView techScrollView;
    private VisualElement upgradeOverlay;
    private VisualElement settingsOverlay;
    private VisualElement competitorsOverlay;
    private VisualElement competitorsContainer;
    private VisualElement welcomeOverlay;

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

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;
    private CompetitorManager competitor => CarCompanyManager.Instance.CompetitorManager;
    private ProductionManager production => CarCompanyManager.Instance.ProductionManager;

    private List<CarCardData> carCards = new List<CarCardData>();
    private List<(Competitor competitor, DropdownField dropdown)> competitorActionRows = new List<(Competitor, DropdownField)>();

    private Dictionary<string, int> selectedActions = new Dictionary<string, int>();
    private class CarCardData
    {
        public CarBlueprint car;
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
    }

    private string[] tuningParamNames = { "power", "economy", "design", "safety" };
    private string[] tuningParamDisplay = { "Мощность", "Экономичность", "Дизайн", "Безопасность" };
    private const int TUNING_MAX_LEVEL = 10;
    private VisualElement techGraphRoot;

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

    public void Initialize(UIDocument document)
    {
        uiDoc = document;
        if (uiDoc == null) { Debug.LogError("UIDocument не передан!"); return; }
        root = uiDoc.rootVisualElement;
        if (root == null) return;

        mainPanel = root.Q<VisualElement>("MainPanel");
        notificationContainer = root.Q<VisualElement>("NotificationContainer");
        versionLabel = root.Q<Label>("VersionLabel");
        if (versionLabel != null) versionLabel.text = $"v. {Application.version}";
        moneyLabel = root.Q<Label>("MoneyLabel");
        incomeLabel = root.Q<Label>("IncomeLabel");
        savedDifficultyLabel = root.Q<Label>("SavedDifficultyLabel");
        eventLabel = root.Q<Label>("EventLabel");
        hamburgerButton = root.Q<Button>("HamburgerButton");
        menuContainer = root.Q<VisualElement>("MenuContainer");
        carsOverlay = root.Q<VisualElement>("CarsOverlay");
        carsContainer = root.Q<VisualElement>("CarsContainer");
        techOverlay = root.Q<VisualElement>("TechOverlay");
        techScrollView = root.Q<ScrollView>("TechContainer");
        upgradeOverlay = root.Q<VisualElement>("UpgradeOverlay");
        settingsOverlay = root.Q<VisualElement>("SettingsOverlay");
        competitorsOverlay = root.Q<VisualElement>("CompetitorsOverlay");
        competitorsContainer = root.Q<VisualElement>("CompetitorsContainer");
        welcomeOverlay = root.Q<VisualElement>("WelcomeOverlay");
        countLabel = root.Q<Label>("CountLabel");
        decreaseCountBtn = root.Q<Button>("DecreaseCountButton");
        increaseCountBtn = root.Q<Button>("IncreaseCountButton");
        produceButton = root.Q<Button>("ProduceButton");
        conveyorLevelLabel = root.Q<Label>("ConveyorLevelLabel");
        engineerCountLabel = root.Q<Label>("EngineerCountLabel");
        buyConveyorButton = root.Q<Button>("BuyConveyorButton");
        hireEngineerButton = root.Q<Button>("HireEngineerButton");

        if (hamburgerButton != null && menuContainer != null)
            hamburgerButton.clicked += () => menuContainer.style.display = (menuContainer.style.display == DisplayStyle.Flex) ? DisplayStyle.None : DisplayStyle.Flex;

        SubscribeButton("OpenCarsButton", OpenCarsWindow);
        SubscribeButton("OpenTechButton", OpenTechWindow);
        SubscribeButton("OpenUpgradeButton", OpenUpgradeWindow);
        SubscribeButton("OpenSettingsButton", OpenSettingsWindow);
        SubscribeButton("OpenCompetitorsButton", OpenCompetitorsWindow);
        SubscribeButton("CloseCarsButton", CloseCarsWindow);
        SubscribeButton("CloseTechButton", CloseTechWindow);
        SubscribeButton("CloseUpgradeButton", CloseUpgradeWindow);
        SubscribeButton("CloseSettingsButton", CloseSettingsWindow);
        SubscribeButton("CloseCompetitorsButton", CloseCompetitorsWindow);
        SubscribeButton("RefreshCompetitorsButton", () =>
        {
            ExecuteAllCompetitorActions();
            competitor.RefreshCompetitorsList();
        });

        SubscribeButton("SaveButton", () => CarCompanyManager.Instance.SaveLoadManager.SaveGame());
        SubscribeButton("LoadButton", () => CarCompanyManager.Instance.SaveLoadManager.LoadGame());
        SubscribeButton("NewGameButton", () => CarCompanyManager.Instance.SaveLoadManager.NewGame());
        SubscribeButton("ProduceButton", () => production.ProduceBasicCar());

        SubscribeButton("EasyButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Easy); CloseSettingsWindow(); });
        SubscribeButton("NormalButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Normal); CloseSettingsWindow(); });
        SubscribeButton("HardButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Hard); CloseSettingsWindow(); });

        SubscribeButton("WelcomeEasyButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Easy); CloseWelcomeScreen(); });
        SubscribeButton("WelcomeNormalButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Normal); CloseWelcomeScreen(); });
        SubscribeButton("WelcomeHardButton", () => { CarCompanyManager.Instance.DifficultyManager.SetDifficulty(DifficultyLevel.Hard); CloseWelcomeScreen(); });

        if (buyConveyorButton != null) buyConveyorButton.clicked += economy.BuyConveyorUpgrade;
        if (hireEngineerButton != null) hireEngineerButton.clicked += economy.HireEngineer;
        if (decreaseCountBtn != null) decreaseCountBtn.clicked += production.DecreaseCount;
        if (increaseCountBtn != null) increaseCountBtn.clicked += production.IncreaseCount;

        HideAllOverlays();
        UpdateMoneyLabels();
        UpdateUpgradeUI();
        UpdateSavedDifficultyLabel();
    }

    private void SubscribeButton(string name, Action action)
    {
        var btn = root.Q<Button>(name);
        if (btn != null) btn.clicked += () => action?.Invoke();
        else Debug.LogWarning($"Кнопка '{name}' не найдена");
    }

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
    }

    public void UpdateSavedDifficultyLabel()
    {
        if (savedDifficultyLabel != null)
            savedDifficultyLabel.text = $"Сложность: {CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty}";
    }

    public void UpdateUpgradeUI()
    {
        if (conveyorLevelLabel != null) conveyorLevelLabel.text = $"Уровень конвейера: {economy.ConveyorLevel}";
        if (engineerCountLabel != null) engineerCountLabel.text = $"Инженеров: {economy.EngineerCount}";
        if (buyConveyorButton != null)
        {
            int cost = Mathf.RoundToInt((10 + economy.ConveyorLevel * 5) * economy.CostMultiplier);
            buyConveyorButton.text = $"Улучшить конвейер ({cost})";
        }
        if (hireEngineerButton != null)
        {
            int cost = Mathf.RoundToInt((50 + economy.EngineerCount * 20) * economy.CostMultiplier);
            hireEngineerButton.text = $"Нанять инженера ({cost})";
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

    public void CreateCarCards(CarBlueprint[] availableCars)
{
    if (carsContainer == null) return;
    carsContainer.Clear();
    carCards.Clear();
    bool upgradeUnlocked = tech.IsCarUpgradeUnlocked();
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
        card.style.paddingTop = 8;          // уменьшено
        card.style.paddingBottom = 8;
        card.style.paddingLeft = 12;        // уменьшено
        card.style.paddingRight = 12;
        card.style.marginBottom = 6;        // уменьшено
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
            iconImage.style.width = 50;      // немного меньше
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
        Label nameLabel = new Label(car.GetDisplayName());
        nameLabel.style.fontSize = 15;      // чуть меньше
        nameLabel.style.color = Color.white;
        nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

        int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
        int modCost = Mathf.RoundToInt(car.GetProductionCostWithLevel() * economy.CostMultiplier);
        Label detailsLabel = new Label($"Цена: ${modPrice}  |  Себ: ${modCost}");
        detailsLabel.style.fontSize = 12;    // уменьшено
        detailsLabel.style.color = new Color(0.8f, 0.8f, 0.8f);

        double profit = modPrice - modCost;
        Label profitLabel = new Label($"Прибыль: {profit:F0}");
        profitLabel.style.fontSize = 13;     // уменьшено
        profitLabel.style.color = profit > 0 ? new Color(0.56f, 0.93f, 0.56f) : new Color(1f, 0.42f, 0.42f);
        profitLabel.style.unityFontStyleAndWeight = FontStyle.Bold;

        Label demandLabel = new Label($"Спрос: {car.demandMultiplier:F1}x");
        demandLabel.style.fontSize = 11;     // уменьшено
        demandLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
        Label levelLabel = new Label($"Уровень: {car.currentLevel + 1}");
        levelLabel.style.fontSize = 11;
        levelLabel.style.color = new Color(0.6f, 0.8f, 1f);
        Label trendLabel = new Label("тренд: ...");
        trendLabel.style.fontSize = 10;      // уменьшено
        trendLabel.style.color = Color.gray;

        textContainer.Add(nameLabel);
        textContainer.Add(detailsLabel);
        textContainer.Add(profitLabel);
        textContainer.Add(demandLabel);
        textContainer.Add(levelLabel);
        textContainer.Add(trendLabel);
        topRow.Add(textContainer);

        Button upgradeButton = new Button();
        upgradeButton.text = "Улучшить";
        upgradeButton.style.width = 70;      // уменьшено
        upgradeButton.style.height = 26;
        upgradeButton.style.marginLeft = 8;
        upgradeButton.style.alignSelf = Align.Center;
        bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
        upgradeButton.SetEnabled(canUpgrade);
        CarBlueprint localCar = car;
        upgradeButton.clicked += () => tech.UpgradeCar(localCar);
        topRow.Add(upgradeButton);
        card.Add(topRow);

        VisualElement tuningPanel = new VisualElement();
        tuningPanel.style.flexDirection = FlexDirection.Column;
        tuningPanel.style.marginTop = 3;     // уменьшено
        tuningPanel.style.marginBottom = 3;
        tuningPanel.style.backgroundColor = new StyleColor(new Color(0.15f, 0.15f, 0.15f));
        tuningPanel.style.paddingTop = 3;    // уменьшено
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
        cardData.demandLabel = demandLabel;
        cardData.trendLabel = trendLabel;
        cardData.levelLabel = levelLabel;
        cardData.upgradeButton = upgradeButton;

        for (int i = 0; i < tuningParamNames.Length; i++)
        {
            string param = tuningParamNames[i];
            string display = tuningParamDisplay[i];

            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 1;      // уменьшено

            Label nameLabelParam = new Label(display);
            nameLabelParam.style.width = 40; // уменьшено
            nameLabelParam.style.color = Color.white;
            nameLabelParam.style.fontSize = 9; // уменьшено
            row.Add(nameLabelParam);

            SliderInt slider = new SliderInt();
            slider.lowValue = 0;
            slider.highValue = GetMaxTuning(car, param);
            slider.value = GetCurrentTuning(car, param);
            slider.style.flexGrow = 1;
            slider.style.marginLeft = 2;     // уменьшено
            slider.style.marginRight = 2;
            row.Add(slider);

            Label valueLabel = new Label(GetCurrentTuning(car, param).ToString());
            valueLabel.style.width = 16;     // уменьшено
            valueLabel.style.color = Color.white;
            valueLabel.style.fontSize = 10;  // уменьшено
            valueLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            row.Add(valueLabel);

            Label maxLabel = new Label($"/{GetMaxTuning(car, param)}");
            maxLabel.style.width = 20;       // уменьшено
            maxLabel.style.color = new Color(0.7f, 0.7f, 0.7f);
            maxLabel.style.fontSize = 9;     // уменьшено
            row.Add(maxLabel);

            Button upgradeBtn = new Button();
            upgradeBtn.text = "↑";
            upgradeBtn.style.width = 20;     // уменьшено
            upgradeBtn.style.height = 20;
            upgradeBtn.style.marginLeft = 1;
            upgradeBtn.style.fontSize = 12;  // уменьшено
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

        VisualElement graphContainer = new VisualElement();
        graphContainer.style.width = new Length(100, LengthUnit.Percent);
        graphContainer.style.height = new Length(40, LengthUnit.Pixel); // уменьшено
        graphContainer.style.marginTop = 3;  // уменьшено
        card.Add(graphContainer);
        cardData.graphContainer = graphContainer;

        carCards.Add(cardData);

        CarBlueprint localCarForProduction = car;
        card.RegisterCallback<ClickEvent>(evt => production.ProduceSpecificCar(localCarForProduction));
        carsContainer.Add(card);
    }
    UpdateCarCards();
}

    public void UpdateCarCards()
    {
        bool upgradeUnlocked = tech.IsCarUpgradeUnlocked();
        foreach (var cardData in carCards)
        {
            if (cardData.car == null) continue;
            CarBlueprint car = cardData.car;

            int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
            int modCost = Mathf.RoundToInt(car.GetModifiedProductionCost(economy.TotalCostModifier) * economy.CostMultiplier);
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

            if (cardData.levelLabel != null)
            {
                cardData.levelLabel.text = $"Уровень: {car.currentLevel + 1}";
            }

            if (cardData.trendLabel != null && MarketSystem.Instance != null)
            {
                string trend = MarketSystem.Instance.GetDemandTrend(car.carName);
                cardData.trendLabel.text = $"Тренд: {trend}";
                cardData.trendLabel.style.color = trend.Contains("растёт") ? Color.green : (trend.Contains("падает") ? Color.red : Color.gray);
            }

            if (cardData.upgradeButton != null)
            {
                bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
                cardData.upgradeButton.SetEnabled(canUpgrade);
                cardData.upgradeButton.text = canUpgrade ? "Улучшить" : "Макс. ур.";
            }

            if (cardData.graphContainer != null && MarketSystem.Instance != null)
                MarketSystem.Instance.DrawDemandGraph(cardData.graphContainer, car.carName);

            UpdateTuningSliders(cardData, car);
        }
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

    // ========== КОНКУРЕНТЫ ==========

public void RefreshCompetitorsList(List<Competitor> competitors, int playerReputation)
{
    if (competitorsContainer == null) return;

    // --- 1. Сохраняем текущие выбранные действия ---
    foreach (var row in competitorActionRows)
    {
        if (row.competitor != null && row.dropdown != null)
        {
            selectedActions[row.competitor.companyName] = row.dropdown.index;
        }
    }

    competitorsContainer.Clear();
    competitorActionRows.Clear();

    // --- Заголовок с репутацией ---
    Label playerRepLabel = new Label($"Ваша репутация: {playerReputation}");
    playerRepLabel.style.color = Color.white;
    playerRepLabel.style.fontSize = 14;
    playerRepLabel.style.marginBottom = 10;
    competitorsContainer.Add(playerRepLabel);

    // --- Заголовки таблицы ---
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
    competitorsContainer.Add(header);

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

        // Данные (первые 8 колонок)
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

        // Контейнер для выпадающего списка (последняя колонка)
        VisualElement actionContainer = new VisualElement();
        actionContainer.style.width = new Length(actionWidth, LengthUnit.Percent);
        actionContainer.style.justifyContent = Justify.Center;
        actionContainer.style.alignItems = Align.Center;

        DropdownField actionDropdown = new DropdownField();
        actionDropdown.choices = actionOptions;
        actionDropdown.index = 0;
        actionDropdown.style.width = new Length(100, LengthUnit.Percent);
        actionDropdown.style.height = 28;          // увеличенная высота
        actionDropdown.style.fontSize = 14;         // увеличенный шрифт
        actionContainer.Add(actionDropdown);
        row.Add(actionContainer);

        competitorsContainer.Add(row);
        competitorActionRows.Add((comp, actionDropdown));
    }

    // --- 2. Восстанавливаем сохранённые индексы ---
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

    if (competitors.Count == 0)
    {
        Label empty = new Label("Нет конкурентов");
        empty.style.color = Color.white;
        empty.style.alignSelf = Align.Center;
        empty.style.marginTop = 20;
        competitorsContainer.Add(empty);
    }
}

    private void ExecuteAllCompetitorActions()
{
    int executedCount = 0;
    // Создаём копию, чтобы избежать ошибки "Collection was modified"
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

    // ========== ТЕХНОЛОГИИ ==========

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

        Dictionary<Technology, int> techLevels = new Dictionary<Technology, int>();
        foreach (var tech in technologies) if (tech != null) techLevels[tech] = CalculateTechLevel(tech, technologies);
        if (techLevels.Count == 0) return;

        int maxLevel = techLevels.Values.Max();
        Dictionary<int, List<Technology>> levelGroups = new Dictionary<int, List<Technology>>();
        for (int i = 0; i <= maxLevel; i++) levelGroups[i] = new List<Technology>();
        foreach (var kvp in techLevels) levelGroups[kvp.Value].Add(kvp.Key);

        int totalWidth = (maxLevel + 1) * 300 + 100;
        int totalHeight = Mathf.Max(600, technologies.Count * 100 + 100);
        techGraphRoot = new VisualElement();
        techGraphRoot.style.width = new Length(totalWidth, LengthUnit.Pixel);
        techGraphRoot.style.height = new Length(totalHeight, LengthUnit.Pixel);
        techGraphRoot.style.position = Position.Relative;
        techScrollView.Add(techGraphRoot);

        const float nodeWidth = 200, nodeHeight = 80, horizontalGap = 100, verticalGap = 60;
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
        lineLayer.style.left = 0; lineLayer.style.top = 0; lineLayer.style.right = 0; lineLayer.style.bottom = 0;
        techGraphRoot.Add(lineLayer);
        foreach (var node in techNodes) if (node != null && node.element != null) techGraphRoot.Add(node.element);

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
        CloseCarsWindow();
        CloseTechWindow();
        CloseUpgradeWindow();
        CloseSettingsWindow();
        CloseCompetitorsWindow();
        if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
    }

    private void OpenCarsWindow()
    {
        if (carsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(carsOverlay);
            demand.UpdateDemand();
            UpdateCarCards();
        }
    }

    private void CloseCarsWindow()
    {
        if (carsOverlay != null)
            AnimateWindowClose(carsOverlay, () => { carsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenTechWindow()
    {
        if (techOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(techOverlay);
            RefreshTechButtons();
        }
    }

    private void CloseTechWindow()
    {
        if (techOverlay != null)
            AnimateWindowClose(techOverlay, () => { techOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenUpgradeWindow()
    {
        if (upgradeOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(upgradeOverlay);
            UpdateUpgradeUI();
        }
    }

    private void CloseUpgradeWindow()
    {
        if (upgradeOverlay != null)
            AnimateWindowClose(upgradeOverlay, () => { upgradeOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
    }

    private void OpenSettingsWindow()
    {
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
        if (competitorsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(competitorsOverlay);
            competitor.RefreshCompetitorsList();
        }
    }

    private void CloseCompetitorsWindow()
    {
        if (competitorsOverlay != null)
            AnimateWindowClose(competitorsOverlay, () => { competitorsOverlay.style.display = DisplayStyle.None; mainPanel.style.display = DisplayStyle.Flex; });
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
            CarCompanyManager.Instance.TechManager.ResearchTechnology(tech);
        };

        node.Add(btn);
        node.userData = tech;
        return node;
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
                Technology requiredTech = CarCompanyManager.Instance.TechManager.GetTechnologyByName(requiredName);
                if (requiredTech == null || !requiredTech.isResearched)
                {
                    requirementsMet = false;
                    break;
                }
            }
        }

        int actualCost = Mathf.RoundToInt(tech.researchCost * economy.TechCostMultiplier);

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

    private class TechNode
    {
        public Technology tech;
        public VisualElement element;
        public Vector2 position;
        public List<TechNode> children = new List<TechNode>();
        public List<TechNode> parents = new List<TechNode>();
        public int level;
    }
}