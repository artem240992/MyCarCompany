using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class UIMarketingController : MonoBehaviour
{
    private VisualElement root;

    private Label brandQualityLabel;
    private Label brandModifierLabel;
    private ListView activeCampaignsList;
    private Button refreshButton;
    private Button closeButton1;
    private Button closeButton2;

    private DropdownField carDropdown;
    private DropdownField campaignTypeDropdown;
    private IntegerField durationField;
    private FloatField budgetField;
    private Button startCampaignButton;
    private Button discountButton;
    private Label errorMessageLabel;

    private DropdownField filterStatusDropdown;
    private DropdownField filterTypeDropdown;
    private DropdownField sortDropdown;

    private Label totalSpentLabel;
    private Label totalProfitLabel;

    private List<string> availableCarNames = new List<string>();

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        if (doc != null)
            root = doc.rootVisualElement;
        else
            Debug.LogError("UIDocument не найден на объекте " + gameObject.name);
    }

    private void OnEnable()
    {
        if (root == null) return;

        brandQualityLabel = root.Q<Label>("BrandQualityLabel");
        brandModifierLabel = root.Q<Label>("BrandModifierLabel");
        activeCampaignsList = root.Q<ListView>("ActiveCampaignsList");
        refreshButton = root.Q<Button>("RefreshMarketingButton");
        closeButton1 = root.Q<Button>("CloseMarketingButton");
        closeButton2 = root.Q<Button>("CloseMarketingButton2");

        carDropdown = root.Q<DropdownField>("CarDropdown");
        campaignTypeDropdown = root.Q<DropdownField>("CampaignTypeDropdown");
        durationField = root.Q<IntegerField>("DurationField");
        budgetField = root.Q<FloatField>("BudgetField");
        startCampaignButton = root.Q<Button>("StartCampaignButton");
        discountButton = root.Q<Button>("ApplyDiscountButton");
        errorMessageLabel = root.Q<Label>("ErrorMessageLabel");

        filterStatusDropdown = root.Q<DropdownField>("FilterStatusDropdown");
        filterTypeDropdown = root.Q<DropdownField>("FilterTypeDropdown");
        sortDropdown = root.Q<DropdownField>("SortDropdown");

        totalSpentLabel = root.Q<Label>("TotalSpentLabel");
        totalProfitLabel = root.Q<Label>("TotalProfitLabel");

        if (filterStatusDropdown != null)
        {
            filterStatusDropdown.choices = new List<string> { "Все", "Активные", "Завершённые" };
            filterStatusDropdown.value = "Все";
            filterStatusDropdown.RegisterValueChangedCallback(_ => RefreshUI());
        }
        if (filterTypeDropdown != null)
        {
            filterTypeDropdown.choices = new List<string> { "Все", "TV", "Internet", "Print", "Social" };
            filterTypeDropdown.value = "Все";
            filterTypeDropdown.RegisterValueChangedCallback(_ => RefreshUI());
        }
        if (sortDropdown != null)
        {
            sortDropdown.choices = new List<string> { "По умолчанию", "Бюджет (возр.)", "Бюджет (убыв.)", "Длительность", "Эффект" };
            sortDropdown.value = "По умолчанию";
            sortDropdown.RegisterValueChangedCallback(_ => RefreshUI());
        }

        PopulateCarDropdown();
        PopulateCampaignTypeDropdown();

        if (refreshButton != null) refreshButton.clicked += RefreshUI;
        if (startCampaignButton != null) startCampaignButton.clicked += OnStartCampaignClicked;
        if (discountButton != null) discountButton.clicked += OnApplyDiscount;

        SetupCampaignsListView();
        RefreshUI();

        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += RefreshUI;
    }

    private void OnDisable()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= RefreshUI;
    }

    private void PopulateCarDropdown()
    {
        // ИСПРАВЛЕНО: проверяем, что TechManager инициализирован и AvailableCars не null
        var techManager = CarCompanyManager.Instance?.TechManager;
        if (techManager != null && techManager.AvailableCars != null)
        {
            var cars = techManager.AvailableCars;
            availableCarNames = cars.Select(c => c.carName).ToList();
        }
        else
        {
            availableCarNames = new List<string>();
            Debug.LogWarning("TechManager.AvailableCars ещё не инициализирован, список машин пуст");
        }

        if (carDropdown != null)
        {
            carDropdown.choices = availableCarNames;
            if (availableCarNames.Count > 0)
                carDropdown.value = availableCarNames[0];
            else
                carDropdown.value = "Нет машин";
        }
    }

    private void PopulateCampaignTypeDropdown()
    {
        if (campaignTypeDropdown == null) return;

        var availableTypes = new List<string>();
        string[] allTypes = { "TV", "Internet", "Print", "Social" };
        var techManager = CarCompanyManager.Instance?.TechManager;
        foreach (string type in allTypes)
        {
            if (techManager != null && techManager.IsAdTypeUnlocked(type))
                availableTypes.Add(type);
        }

        if (availableTypes.Count == 0)
        {
            availableTypes.Add("Нет доступных типов");
            campaignTypeDropdown.SetEnabled(false);
        }
        else
        {
            campaignTypeDropdown.SetEnabled(true);
        }

        campaignTypeDropdown.choices = availableTypes;
        campaignTypeDropdown.value = availableTypes[0];
    }

    private void SetupCampaignsListView()
    {
        if (activeCampaignsList == null) return;

        activeCampaignsList.makeItem = () =>
        {
            var container = new VisualElement();
            container.style.flexDirection = FlexDirection.Row;
            container.style.paddingLeft = 5;
            container.style.paddingRight = 5;
            container.style.paddingTop = 5;
            container.style.paddingBottom = 5;
            container.style.borderBottomWidth = 1;
            container.style.borderBottomColor = Color.gray;

            var nameLabel = new Label { name = "CampaignName" };
            nameLabel.style.width = 100;
            var carLabel = new Label { name = "CampaignCar" };
            carLabel.style.width = 80;
            var typeLabel = new Label { name = "CampaignType" };
            typeLabel.style.width = 60;
            var remainingLabel = new Label { name = "CampaignRemaining" };
            remainingLabel.style.width = 60;
            var modifierLabel = new Label { name = "CampaignModifier" };
            modifierLabel.style.width = 60;

            container.Add(nameLabel);
            container.Add(carLabel);
            container.Add(typeLabel);
            container.Add(remainingLabel);
            container.Add(modifierLabel);

            return container;
        };

        activeCampaignsList.bindItem = (element, index) =>
        {
            var campaigns = activeCampaignsList.itemsSource as List<MarketingCampaign>;
            if (campaigns == null || index >= campaigns.Count) return;

            var campaign = campaigns[index];
            element.Q<Label>("CampaignName").text = campaign.campaignName;
            element.Q<Label>("CampaignCar").text = campaign.carName;
            element.Q<Label>("CampaignType").text = campaign.campaignType;
            element.Q<Label>("CampaignRemaining").text = $"{campaign.monthsRemaining} мес.";
            element.Q<Label>("CampaignModifier").text = $"{campaign.GetCurrentModifier():F2}x";
        };
    }

    public void RefreshUI()
    {
        // Проверяем готовность TechManager перед заполнением списков
        if (CarCompanyManager.Instance?.TechManager != null && CarCompanyManager.Instance.TechManager.AvailableCars != null)
            PopulateCarDropdown();

        PopulateCampaignTypeDropdown();

        if (MarketingManager.Instance == null) return;

        MarketingManager.Instance.UpdateBrandQuality();

        if (brandQualityLabel != null)
            brandQualityLabel.text = $"Качество бренда: {MarketingManager.Instance.brandQuality:F1} / 100";
        if (brandModifierLabel != null)
            brandModifierLabel.text = $"Модификатор спроса: {MarketingManager.Instance.GetBrandModifier():F2}x";

        var allCampaigns = MarketingManager.Instance.activeCampaigns;
        var filtered = allCampaigns.AsEnumerable();

        if (filterStatusDropdown != null && filterStatusDropdown.value != "Все")
        {
            bool active = filterStatusDropdown.value == "Активные";
            filtered = filtered.Where(c => c.isActive == active);
        }

        if (filterTypeDropdown != null && filterTypeDropdown.value != "Все")
        {
            string type = filterTypeDropdown.value;
            filtered = filtered.Where(c => c.campaignType == type);
        }

        if (sortDropdown != null)
        {
            switch (sortDropdown.value)
            {
                case "Бюджет (возр.)": filtered = filtered.OrderBy(c => c.budget); break;
                case "Бюджет (убыв.)": filtered = filtered.OrderByDescending(c => c.budget); break;
                case "Длительность": filtered = filtered.OrderBy(c => c.durationMonths); break;
                case "Эффект": filtered = filtered.OrderByDescending(c => c.GetCurrentModifier()); break;
                default: break;
            }
        }

        var list = filtered.ToList();

        if (activeCampaignsList != null)
        {
            activeCampaignsList.itemsSource = list;
            activeCampaignsList.Rebuild();
        }

        if (totalSpentLabel != null)
        {
            double totalSpent = allCampaigns.Sum(c => c.budget);
            totalSpentLabel.text = $"Всего потрачено: ${totalSpent:F0}";
        }
        if (totalProfitLabel != null)
        {
            double totalProfit = allCampaigns.Sum(c => (c.GetCurrentModifier() - 1f) * 100);
            totalProfitLabel.text = $"Оценочная прибыль: ${totalProfit:F0}";
        }

        if (errorMessageLabel != null)
            errorMessageLabel.text = "";
    }

    private void OnStartCampaignClicked()
    {
        if (errorMessageLabel != null)
            errorMessageLabel.text = "";

        if (string.IsNullOrEmpty(carDropdown?.value) || carDropdown.value == "Нет машин")
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "Выберите машину.";
            return;
        }
        if (durationField == null || durationField.value <= 0)
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "Длительность должна быть > 0.";
            return;
        }
        if (budgetField == null || budgetField.value <= 0)
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "Бюджет должен быть > 0.";
            return;
        }

        if (budgetField.value > CarCompanyManager.Instance.EconomyManager.Money)
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "❌ Недостаточно денег!";
            return;
        }

        if (campaignTypeDropdown == null || campaignTypeDropdown.value == "Нет доступных типов")
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "Нет доступных типов рекламы. Исследуйте технологии!";
            return;
        }

        bool success = MarketingManager.Instance.StartCampaign(
            carDropdown.value,
            campaignTypeDropdown.value,
            durationField.value,
            budgetField.value
        );

        if (success)
        {
            RefreshUI();
            UIManager.Instance?.UpdateMoneyLabels();
        }
        else if (errorMessageLabel != null)
        {
            errorMessageLabel.text = "Не удалось запустить кампанию. Возможно, уже активна для этой машины.";
        }
    }

    private void OnApplyDiscount()
    {
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < 50)
        {
            UIManager.Instance?.ShowNotification("❌ Недостаточно денег для проведения акции (нужно $50)");
            return;
        }
        economy.ApplyDiscount(0.2f, 3);
        UIManager.Instance?.UpdateMoneyLabels();
        UIManager.Instance?.ShowNotification("✅ Скидка 20% применена на 3 мес.");
    }

    private void CloseWindow()
    {
        var overlay = root?.Q<VisualElement>("MarketingOverlay");
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;
    }
}