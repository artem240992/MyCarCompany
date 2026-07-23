using System;

[Serializable]
public class MarketingCampaign
{
    public string campaignName;
    public string carName;
    public string campaignType;
    public int durationMonths;
    public int monthsRemaining;
    public float budget;
    public float monthlyCost;
    public float demandModifier;
    public bool isActive;

    // Пустой конструктор – обязателен для сериализации
    public MarketingCampaign() { }

    public MarketingCampaign(string name, string car, string type, int months, float budget)
    {
        campaignName = name;
        carName = car;
        campaignType = type;
        durationMonths = months;
        monthsRemaining = months;
        this.budget = budget;
        monthlyCost = budget / months;
        demandModifier = 1f + (type == "TV" ? 0.15f : type == "Internet" ? 0.12f : type == "Social" ? 0.10f : 0.08f);
        isActive = true;
    }

    public void AdvanceMonth()
    {
        if (!isActive) return;
        monthsRemaining--;
        if (monthsRemaining <= 0) isActive = false;
    }

    public float GetCurrentModifier()
    {
        if (!isActive) return 1f;
        float progress = 1f - (monthsRemaining / (float)durationMonths);
        return 1f + (demandModifier - 1f) * (1f - progress * 0.3f);
    }
}