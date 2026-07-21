using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class MarketingManager : MonoBehaviour
{
    public static MarketingManager Instance { get; private set; }

    public List<MarketingCampaign> activeCampaigns = new List<MarketingCampaign>();
    public float brandQuality = 50f; // 0-100

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += OnMonthChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= OnMonthChanged;
    }

    public bool StartCampaign(string carName, string campaignType, int months, float budget)
    {
        if (budget <= 0 || months <= 0) return false;

        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < budget)
        {
            Debug.LogWarning($"Недостаточно денег: {economy.Money} < {budget}");
            return false;
        }

        if (activeCampaigns.Any(c => c.carName == carName && c.isActive))
        {
            Debug.LogWarning($"У машины {carName} уже есть активная кампания.");
            return false;
        }

        // Списание бюджета
        economy.Money -= budget;
        Debug.Log($"Списано {budget}, осталось {economy.Money}");

        // Создание кампании – передаём все параметры
        var campaign = new MarketingCampaign(
            $"Реклама {carName} ({campaignType})",
            carName,
            campaignType,
            months,
            budget
        );

        activeCampaigns.Add(campaign);
        ApplyCampaignEffect(campaign);

        // Обновление UI – деньги
        if (UIManager.Instance != null)
            UIManager.Instance.UpdateMoneyLabels();
        else
            Debug.LogError("UIManager.Instance == null! Деньги не обновлены.");

        return true;
    }

    private void ApplyCampaignEffect(MarketingCampaign campaign)
    {
        var allCars = CarCompanyManager.Instance.TechManager.AvailableCars;
        var car = allCars.FirstOrDefault(c => c != null && c.carName == campaign.carName);
        if (car != null)
        {
            Debug.Log($"Кампания для {campaign.carName} запущена. Модификатор: {campaign.GetCurrentModifier():F2}x");
        }
    }

    private void OnMonthChanged()
    {
        foreach (var campaign in activeCampaigns.ToList())
        {
            campaign.AdvanceMonth();
            if (!campaign.isActive)
            {
                activeCampaigns.Remove(campaign);
                OnCampaignEnded(campaign);
            }
        }
        UpdateBrandQuality();
    }

    private void OnCampaignEnded(MarketingCampaign campaign)
    {
        Debug.Log($"Кампания для {campaign.carName} завершена.");
    }

    public void UpdateBrandQuality()
    {
        float baseQuality = 50f;
        float campaignBoost = activeCampaigns.Count * 2f;
        float reputationBoost = CarCompanyManager.Instance.EconomyManager.Reputation / 5f;
        float techBoost = CarCompanyManager.Instance.TechManager.Technologies.Count(t => t.isResearched) * 0.2f;
        brandQuality = Mathf.Clamp(baseQuality + campaignBoost + reputationBoost + techBoost, 0f, 100f);
    }

    public float GetBrandModifier()
    {
        return 0.5f + (brandQuality / 100f);
    }

    public float GetDemandModifierForCar(string carName)
    {
        var campaign = activeCampaigns.FirstOrDefault(c => c.carName == carName && c.isActive);
        return campaign != null ? campaign.GetCurrentModifier() : 1f;
    }

    // ----- Сохранение / загрузка -----
    public void FillSaveData(SaveData data)
    {
        data.activeCampaigns = activeCampaigns;
        data.brandQuality = brandQuality;
    }

    public void LoadFromSave(SaveData data)
    {
        activeCampaigns = data.activeCampaigns ?? new List<MarketingCampaign>();
        brandQuality = data.brandQuality;
    }
}