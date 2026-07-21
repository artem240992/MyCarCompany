using System;
using System.Collections.Generic;
using System.Linq;   // <-- ДОБАВЛЕНО
using UnityEngine;

[Serializable]
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
    public int engineers;
    public int espionageLevel;
    public int marketingPower;
    public int loyalty;
    public bool isAlly;
    public List<string> stolenTechs = new List<string>();
    public List<string> researchedTechs = new List<string>();

    // ---- НОВЫЕ ПОЛЯ ДЛЯ МАРКЕТИНГА ----
    public float marketingBudget = 100f;
    public float brandQuality = 30f;
    public string strategy = "Conservative";
    public float lastPriceCut = 0f;
    public float lastAdCampaign = 0f;
    public List<MarketingCampaign> activeCampaigns = new List<MarketingCampaign>();

    // ---- МЕТОДЫ ----
    public void LaunchCampaign(string carName, float budget)
    {
        if (money < budget) return;
        money -= budget;
        var campaign = new MarketingCampaign($"Реклама {carName}", carName, "TV", 3, budget);
        activeCampaigns.Add(campaign);
        brandQuality = Mathf.Min(100, brandQuality + 2f);
    }

    public void ApplyDiscount(float discount, int months)
    {
        if (money < 50) return;
        money -= 50;
        priceMultiplier = Mathf.Max(0.6f, priceMultiplier - discount);
        lastPriceCut = Time.time;
        brandQuality = Mathf.Min(100, brandQuality + 1f);
    }

    public void UpdateMonth()
    {
        // Обновление активных кампаний
        foreach (var campaign in activeCampaigns.ToList())
        {
            campaign.AdvanceMonth();
            if (!campaign.isActive)
                activeCampaigns.Remove(campaign);
        }
        // Постепенное снижение бренда, если нет активности
        if (activeCampaigns.Count == 0 && Time.time - lastAdCampaign > 30f)
            brandQuality = Mathf.Max(0, brandQuality - 0.5f);
    }
}