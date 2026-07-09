using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class DemandManager : MonoBehaviour
{
    public float DemandUpdateInterval = 5f;

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private CompetitorManager competitor => CarCompanyManager.Instance.CompetitorManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;

    // ---- Штрафы от конкурентов (временные) ----
    private Dictionary<string, float> demandPenalties = new Dictionary<string, float>();
    private Dictionary<string, Coroutine> penaltyCoroutines = new Dictionary<string, Coroutine>();

    public void Initialize() { }

    // ---- Применить штраф к конкретной машине на время ----
    public void ApplyDemandPenalty(string carName, float penalty, float duration)
    {
        if (string.IsNullOrEmpty(carName)) return;
        if (penaltyCoroutines.ContainsKey(carName) && penaltyCoroutines[carName] != null)
            CarCompanyManager.Instance.StopCoroutine(penaltyCoroutines[carName]);

        demandPenalties[carName] = penalty;
        penaltyCoroutines[carName] = CarCompanyManager.Instance.StartCoroutine(ClearPenaltyAfter(carName, duration));
        UpdateDemand();
    }

    private IEnumerator ClearPenaltyAfter(string carName, float duration)
    {
        yield return new WaitForSeconds(duration);
        demandPenalties.Remove(carName);
        penaltyCoroutines.Remove(carName);
        UpdateDemand();
    }

    // ---- Сброс всех штрафов (при новой игре, смене сложности) ----
    public void ResetPenalties()
    {
        foreach (var coroutine in penaltyCoroutines.Values)
            if (coroutine != null)
                CarCompanyManager.Instance.StopCoroutine(coroutine);
        demandPenalties.Clear();
        penaltyCoroutines.Clear();
    }

    public void UpdateDemand()
    {
        if (MarketSystem.Instance == null) return;
        List<CarBlueprint> allCars = GetAllPossibleCars();

        // Репутация влияет на спрос: 50 → 1x, 100 → 1.5x, 0 → 0.5x
        float reputationModifier = Mathf.Clamp(1f + (economy.Reputation - 50) / 100f, 0.5f, 1.5f);

        foreach (CarBlueprint car in allCars)
        {
            if (car == null) continue;

            float min, max;
            // ---- ИСПРАВЛЕНО: используем полное имя DifficultyManager.DifficultyLevel ----
            switch (CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty)
            {
                case DifficultyManager.DifficultyLevel.Easy:   min = 0.9f; max = 1.1f; break;
                case DifficultyManager.DifficultyLevel.Normal: min = 0.7f; max = 1.3f; break;
                case DifficultyManager.DifficultyLevel.Hard:   min = 0.5f; max = 1.8f; break;
                default: min = 0.8f; max = 1.2f; break;
            }
            float baseDemand = Random.Range(min, max);

            // Влияние конкурентов
            float competitorFactor = 1f;
            foreach (var comp in competitor.Competitors)
                if (comp != null && comp.availableCars.Contains(car))
                {
                    competitorFactor *= (1f - comp.marketShare * 0.5f);
                    float priceEffect = 1f - (comp.priceMultiplier - 0.8f) * 0.2f;
                    competitorFactor *= Mathf.Clamp(priceEffect, 0.5f, 1.2f);
                }

            // Технологии игрока и конкурентов
            float playerTechDemandModifier = economy.TotalDemandModifier;
            float competitorTechDemandModifier = 1f;
            foreach (var comp in competitor.Competitors)
                if (comp != null) competitorTechDemandModifier *= (1f - comp.researchLevel * 0.02f);

            // ---- РЕПУТАЦИЯ И СОБЫТИЯ ----
            float baseWithTech = baseDemand
                * CarCompanyManager.Instance.DifficultyManager.CurrentEventMultiplier
                * competitorFactor
                * playerTechDemandModifier
                * competitorTechDemandModifier
                * reputationModifier;

            // Тюнинг
            float tuningDemandModifier = car.GetTuningDemandModifier();

            // Штраф от конкурентов
            float penalty = 1f;
            if (demandPenalties.TryGetValue(car.carName, out float p))
                penalty = p;

            // Итоговый спрос
            float finalDemand = MarketSystem.Instance.GetDemandMultiplier(car, baseWithTech * tuningDemandModifier * penalty);
            car.demandMultiplier = finalDemand;
        }

        ui.UpdateCarCards();
        ui.UpdateMoneyLabels();
    }

    private List<CarBlueprint> GetAllPossibleCars()
    {
        var all = new List<CarBlueprint>();
        all.AddRange(tech.AvailableCars);
        foreach (var t in tech.Technologies)
            if (t != null && t.unlockedCar != null && !all.Contains(t.unlockedCar))
                all.Add(t.unlockedCar);
        return all;
    }
}