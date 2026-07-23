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
    private Button closeButton1; // крестик
    private Button closeButton2; // кнопка "Закрыть"

    private DropdownField carDropdown;
    private DropdownField campaignTypeDropdown;
    private IntegerField durationField;
    private FloatField budgetField;
    private Button startCampaignButton;
    private Button discountButton; // кнопка скидки
    private Label errorMessageLabel;

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

        // Найти элементы по именам
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

        PopulateCarDropdown();
        PopulateCampaignTypeDropdown();

        // Подписки
        if (refreshButton != null) refreshButton.clicked += RefreshUI;
        if (startCampaignButton != null) startCampaignButton.clicked += OnStartCampaignClicked;
        if (discountButton != null) discountButton.clicked += OnApplyDiscount;

        // Закрытие теперь через UIManager
        // if (closeButton1 != null) closeButton1.clicked += CloseWindow;
        // if (closeButton2 != null) closeButton2.clicked += CloseWindow;

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
        if (CarCompanyManager.Instance?.TechManager != null && CarCompanyManager.Instance.TechManager.AvailableCars != null)
        {
            var cars = CarCompanyManager.Instance.TechManager.AvailableCars;
            availableCarNames = cars.Select(c => c.carName).ToList();
        }
        else
        {
            availableCarNames = new List<string>();
            Debug.LogWarning("TechManager.AvailableCars ещё не инициализирован");
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
        foreach (string type in allTypes)
        {
            if (CarCompanyManager.Instance.TechManager.IsAdTypeUnlocked(type))
                availableTypes.Add(type);
        }
        
        // Если ни один тип не разблокирован, показываем заглушку и сообщение
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
        if (MarketingManager.Instance == null) return;
        if (CarCompanyManager.Instance?.TechManager == null) 
        {
            Debug.LogWarning("TechManager ещё не инициализирован, пропускаем обновление бренда");
            return;
        }
        
        MarketingManager.Instance.UpdateBrandQuality();
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
            var campaigns = MarketingManager.Instance?.activeCampaigns;
            if (campaigns == null || index >= campaigns.Count) return;

            var campaign = campaigns[index];
            element.Q<Label>("CampaignName").text = campaign.campaignName;
            element.Q<Label>("CampaignCar").text = campaign.carName;
            element.Q<Label>("CampaignType").text = campaign.campaignType;
            element.Q<Label>("CampaignRemaining").text = $"{campaign.monthsRemaining} мес.";
            element.Q<Label>("CampaignModifier").text = $"{campaign.GetCurrentModifier():F2}x";
        };

        activeCampaignsList.itemsSource = MarketingManager.Instance?.activeCampaigns ?? new List<MarketingCampaign>();
    }

    public void RefreshUI()
    {
        // Обновить список доступных машин
        PopulateCarDropdown();
        PopulateCampaignTypeDropdown(); // <-- добавить

        if (MarketingManager.Instance == null) return;

        MarketingManager.Instance.UpdateBrandQuality();

        if (brandQualityLabel != null)
            brandQualityLabel.text = $"Качество бренда: {MarketingManager.Instance.brandQuality:F1} / 100";
        if (brandModifierLabel != null)
            brandModifierLabel.text = $"Модификатор спроса: {MarketingManager.Instance.GetBrandModifier():F2}x";

        if (activeCampaignsList != null)
        {
            activeCampaignsList.itemsSource = MarketingManager.Instance.activeCampaigns;
            activeCampaignsList.Rebuild(); // если ошибка, замените на Refresh()
        }

        if (errorMessageLabel != null)
            errorMessageLabel.text = "";
    }

    private void OnStartCampaignClicked()
    {
        if (errorMessageLabel != null)
            errorMessageLabel.text = "";

        if (string.IsNullOrEmpty(carDropdown?.value))
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

        // Проверка денег
        if (budgetField.value > CarCompanyManager.Instance.EconomyManager.Money)
        {
            if (errorMessageLabel != null) errorMessageLabel.text = "❌ Недостаточно денег!";
            return;
        }

        bool success = MarketingManager.Instance.StartCampaign(
            carDropdown.value,
            campaignTypeDropdown?.value ?? "TV",
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
        float discount = 0.2f;
        int months = 3;
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < 50)
        {
            UIManager.Instance?.ShowNotification("❌ Недостаточно денег для проведения акции (нужно $50)");
            return;
        }
        if (campaignTypeDropdown.value == "Нет доступных типов" || string.IsNullOrEmpty(campaignTypeDropdown.value))
        {
            errorMessageLabel.text = "Нет доступных типов рекламы. Исследуйте технологии!";
            return;
        }
    }

    private void CloseWindow()
    {
        var overlay = root?.Q<VisualElement>("MarketingOverlay");
        if (overlay != null)
            overlay.style.display = DisplayStyle.None;
    }
}