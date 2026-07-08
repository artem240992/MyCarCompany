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

    private class CarCardData
    {
        public CarBlueprint car;
        public VisualElement card;
        public Label profitLabel;
        public Label demandLabel;
        public Label trendLabel;
        public Label levelLabel; // <-- ДОБАВЛЕНО
        public VisualElement graphContainer;
        public Button upgradeButton;
        public Label powerLabel, economyLabel, designLabel, safetyLabel;
        public Label powerCostLabel, economyCostLabel, designCostLabel, safetyCostLabel;
        public Button powerUpgradeBtn, economyUpgradeBtn, designUpgradeBtn, safetyUpgradeBtn;
    }

    private string[] tuningParamNames = { "power", "economy", "design", "safety" };
    private string[] tuningParamDisplay = { "Мощность", "Экономичность", "Дизайн", "Безопасность" };
    private const int TUNING_MAX_LEVEL = 10;
    private VisualElement techGraphRoot;

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
        SubscribeButton("RefreshCompetitorsButton", () => competitor.RefreshCompetitorsList());
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

    // ---- Публичные методы ----
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
            card.style.paddingTop = 10;
            card.style.paddingBottom = 10;
            card.style.paddingLeft = 15;
            card.style.paddingRight = 15;
            card.style.marginBottom = 8;
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
                iconImage.style.width = 60;
                iconImage.style.height = 60;
                iconImage.style.marginRight = 15;
                topRow.Add(iconImage);
            }
            else
            {
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

            int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
            int modCost = car.GetModifiedProductionCost(economy.TotalCostModifier);
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

            Button upgradeButton = new Button();
            upgradeButton.text = "Улучшить";
            upgradeButton.style.width = 80;
            upgradeButton.style.height = 30;
            upgradeButton.style.marginLeft = 10;
            upgradeButton.style.alignSelf = Align.Center;
            bool canUpgrade = (car.levelPrefabs != null && car.levelPrefabs.Length > 0 && car.currentLevel < car.levelPrefabs.Length - 1) && upgradeUnlocked;
            upgradeButton.SetEnabled(canUpgrade);
            CarBlueprint localCar = car;
            upgradeButton.clicked += () => tech.UpgradeCar(localCar);
            topRow.Add(upgradeButton);
            card.Add(topRow);

            // Тюнинг
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
                upgradeBtn.clicked += () => tech.UpgradeTuning(localCarTuning, localParam);
            }
            card.Add(tuningPanel);

            // График
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
            cardData.levelLabel = levelLabel; // <-- СОХРАНЯЕМ ССЫЛКУ
            cardData.graphContainer = graphContainer;
            cardData.upgradeButton = upgradeButton;

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
                        Label costLabel2 = labels.Count > 2 ? labels[2] : null;
                        Button btn = buttons[0];
                        switch (i)
                        {
                            case 0: cardData.powerLabel = valLabel; cardData.powerUpgradeBtn = btn; cardData.powerCostLabel = costLabel2; break;
                            case 1: cardData.economyLabel = valLabel; cardData.economyUpgradeBtn = btn; cardData.economyCostLabel = costLabel2; break;
                            case 2: cardData.designLabel = valLabel; cardData.designUpgradeBtn = btn; cardData.designCostLabel = costLabel2; break;
                            case 3: cardData.safetyLabel = valLabel; cardData.safetyUpgradeBtn = btn; cardData.safetyCostLabel = costLabel2; break;
                        }
                    }
                }
            }

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
            int modCost = car.GetModifiedProductionCost(economy.TotalCostModifier);
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

            // ---- ОБНОВЛЕНИЕ УРОВНЯ ----
            if (cardData.levelLabel != null)
            {
                cardData.levelLabel.text = $"Уровень: {car.currentLevel + 1}";
            }

            // ---- ОБНОВЛЕНИЕ ТРЕНДА ----
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
            UpdateTuningUI(cardData);
        }
    }

    public void CreateTechTree(List<Technology> technologies, float techCostMultiplier)
    {
        if (techScrollView == null) return;
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

    public void RefreshCompetitorsList(List<Competitor> competitors, int playerReputation)
    {
        if (competitorsContainer == null) return;
        competitorsContainer.Clear();

        Label playerRepLabel = new Label($"Ваша репутация: {playerReputation}");
        playerRepLabel.style.color = Color.white;
        playerRepLabel.style.fontSize = 14;
        playerRepLabel.style.marginBottom = 10;
        competitorsContainer.Add(playerRepLabel);

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
            foreach (var val in values)
            {
                Label lbl = new Label(val);
                lbl.style.width = new Length(colWidthPercent, LengthUnit.Percent);
                lbl.style.color = Color.white;
                lbl.style.fontSize = 12;
                row.Add(lbl);
            }

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
                if (selectedIndex <= 0) { ShowNotification("Выберите действие!"); return; }
                switch (selectedIndex)
                {
                    case 1: CarCompanyManager.Instance.CompetitorManager.PerformMarketingAttack(localComp); break;
                    case 2: CarCompanyManager.Instance.CompetitorManager.PerformBlackPR(localComp); break;
                    case 3: CarCompanyManager.Instance.CompetitorManager.ProposeAlliance(localComp); break;
                    case 4: CarCompanyManager.Instance.CompetitorManager.StealTechnology(localComp); break;
                    case 5: CarCompanyManager.Instance.CompetitorManager.PoachEngineer(localComp); break;
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

    public void CloseAllWindows()
    {
        CloseCarsWindow();
        CloseTechWindow();
        CloseUpgradeWindow();
        CloseSettingsWindow();
        CloseCompetitorsWindow();
        if (menuContainer != null) menuContainer.style.display = DisplayStyle.None;
    }

    // ---- Приватные методы окон ----
    private void OpenCarsWindow()
    {
        if (carsOverlay != null)
        {
            menuContainer.style.display = DisplayStyle.None;
            mainPanel.style.display = DisplayStyle.None;
            AnimateWindowOpen(carsOverlay);
            demand.UpdateDemand(); // <-- ДОБАВЛЕНО для обновления спроса при открытии окна
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

    private void UpdateTuningUI(CarCardData cardData)
    {
        CarBlueprint car = cardData.car;
        if (car == null) return;

        var paramInfos = new (Label label, Button btn, Label costLabel, string param)[]
        {
            (cardData.powerLabel, cardData.powerUpgradeBtn, cardData.powerCostLabel, "power"),
            (cardData.economyLabel, cardData.economyUpgradeBtn, cardData.economyCostLabel, "economy"),
            (cardData.designLabel, cardData.designUpgradeBtn, cardData.designCostLabel, "design"),
            (cardData.safetyLabel, cardData.safetyUpgradeBtn, cardData.safetyCostLabel, "safety")
        };

        foreach (var p in paramInfos)
        {
            if (p.label != null)
                p.label.text = tech.GetTuningLevel(car, p.param).ToString();
            if (p.btn != null)
            {
                bool can = tech.CanUpgradeTuning(car, p.param);
                p.btn.SetEnabled(can);
                if (p.costLabel != null)
                {
                    int level = tech.GetTuningLevel(car, p.param);
                    if (level < TUNING_MAX_LEVEL)
                    {
                        Technology techObj = tech.GetTuningTech(p.param, level + 1);
                        p.costLabel.text = techObj != null ? $"${Mathf.RoundToInt(techObj.researchCost * economy.TechCostMultiplier)}" : "0";
                    }
                    else
                    {
                        p.costLabel.text = "MAX";
                    }
                }
            }
        }
    }

    // ---- Методы, исправленные для устранения CS0161 ----
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