using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DemandManager : MonoBehaviour
{
    public float DemandUpdateInterval = 5f;

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private CompetitorManager competitor => CarCompanyManager.Instance.CompetitorManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;

    public void Initialize() { }

    public void UpdateDemand()
    {
        if (MarketSystem.Instance == null) return;
        List<CarBlueprint> allCars = GetAllPossibleCars();
        foreach (CarBlueprint car in allCars)
        {
            if (car == null) continue;
            float min, max;
            switch (CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty)
            {
                case DifficultyLevel.Easy: min = 0.9f; max = 1.1f; break;
                case DifficultyLevel.Normal: min = 0.7f; max = 1.3f; break;
                case DifficultyLevel.Hard: min = 0.5f; max = 1.8f; break;
                default: min = 0.8f; max = 1.2f; break;
            }
            float baseDemand = Random.Range(min, max);
            float competitorFactor = 1f;
            foreach (var comp in competitor.Competitors)
                if (comp != null && comp.availableCars.Contains(car))
                {
                    competitorFactor *= (1f - comp.marketShare * 0.5f);
                    float priceEffect = 1f - (comp.priceMultiplier - 0.8f) * 0.2f;
                    competitorFactor *= Mathf.Clamp(priceEffect, 0.5f, 1.2f);
                }
            float playerTechDemandModifier = economy.TotalDemandModifier;
            float competitorTechDemandModifier = 1f;
            foreach (var comp in competitor.Competitors)
                if (comp != null) competitorTechDemandModifier *= (1f - comp.researchLevel * 0.02f);
            float baseWithTech = baseDemand * CarCompanyManager.Instance.DifficultyManager.CurrentEventMultiplier * competitorFactor * playerTechDemandModifier * competitorTechDemandModifier;
            float tuningDemandModifier = car.GetTuningDemandModifier();
            float finalDemand = MarketSystem.Instance.GetDemandMultiplier(car, baseWithTech * tuningDemandModifier);
            car.demandMultiplier = finalDemand;
        }
        ui.UpdateCarCards();
    }

    private List<CarBlueprint> GetAllPossibleCars()
    {
        // Получаем все машины из TechManager
        var all = new List<CarBlueprint>();
        all.AddRange(tech.AvailableCars);
        // Также добавить машины, которые могут быть открыты (заблокированы)
        foreach (var t in tech.Technologies)
            if (t != null && t.unlockedCar != null && !all.Contains(t.unlockedCar))
                all.Add(t.unlockedCar);
        return all;
    }
}