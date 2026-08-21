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
    [System.NonSerialized]
    public Dictionary<string, float> demandPenalties = new Dictionary<string, float>();
    private Dictionary<string, Coroutine> penaltyCoroutines = new Dictionary<string, Coroutine>();

    // ---- Множитель от инвестиций ----
    private float temporaryBoost = 1f;

    // ---- Настройка бонуса за уровень ----
    [Header("Влияние улучшений на спрос")]
    [SerializeField] private float demandBonusPerLevel = 0.1f; // 10% за уровень

    private float interactiveNewsMultiplier = 1f;

    public void Initialize() { }

    public void ApplyDemandPenalty(string carName, float penalty, float duration)
    {
        if (string.IsNullOrEmpty(carName)) return;
        if (penaltyCoroutines.ContainsKey(carName) && penaltyCoroutines[carName] != null)
            CarCompanyManager.Instance.StopCoroutine(penaltyCoroutines[carName]);

        demandPenalties[carName] = penalty;
        penaltyCoroutines[carName] = CarCompanyManager.Instance.StartCoroutine(ClearPenaltyAfter(carName, duration));
        UpdateDemand();
    }

    public void ApplyNewsDemandMultiplier(float multiplier)
    {
        interactiveNewsMultiplier = Mathf.Clamp(multiplier, 0.5f, 1.5f);
        UpdateDemand();
    }

    private IEnumerator ClearPenaltyAfter(string carName, float duration)
    {
        yield return new WaitForSeconds(duration);
        demandPenalties.Remove(carName);
        penaltyCoroutines.Remove(carName);
        UpdateDemand();
    }

    public void ResetPenalties()
    {
        foreach (var coroutine in penaltyCoroutines.Values)
            if (coroutine != null)
                CarCompanyManager.Instance.StopCoroutine(coroutine);
        demandPenalties.Clear();
        penaltyCoroutines.Clear();
    }

    public void ApplyTemporaryBoost(float bonus)
    {
        temporaryBoost += bonus;
        temporaryBoost = Mathf.Clamp(temporaryBoost, 0.5f, 3f);
        UpdateDemand();
    }

    // ======================== ОСНОВНОЙ МЕТОД ОБНОВЛЕНИЯ СПРОСА ========================

    public void UpdateDemand()
    {
        // Обновляем множители новостей (устанавливаем car.newsMultiplier)
        UpdateNewsMultipliers();

        List<CarBlueprint> allCars = GetAllPossibleCars();
        if (allCars == null || allCars.Count == 0) return;

        // 1. Репутация игрока
        float reputationModifier = Mathf.Clamp(1f + (economy.Reputation - 50) / 100f, 0.5f, 1.5f);

        // 2. Средняя цена по рынку
        float avgPrice = CalculateAverageMarketPrice(allCars);

        // 3. Множитель сложности
        float difficultyModifier = CarCompanyManager.Instance.DifficultyManager.CurrentDemandModifier;

        foreach (CarBlueprint car in allCars)
        {
            if (car == null) continue;

            // ----- БАЗОВЫЙ СПРОС (уже включает сезонность и новости) -----
            float baseDemand = car.CurrentDemandMultiplier;

            // ----- ЦЕНОВОЙ ФАКТОР -----
            float priceFactor = CalculatePriceFactor(car, avgPrice);

            // ----- МАРКЕТИНГ -----
            float marketingBonus = 1f;
            float brandModifier = 1f;
            if (MarketingManager.Instance != null)
            {
                marketingBonus = MarketingManager.Instance.GetDemandModifierForCar(car.carName);
                brandModifier = MarketingManager.Instance.GetBrandModifier();
            }

            // ----- ДАВЛЕНИЕ КОНКУРЕНТОВ (доля рынка) -----
            float competitorPressure = CalculateCompetitorPressure(car);

            // ----- ШТРАФ ОТ КОНКУРЕНТНЫХ ДЕЙСТВИЙ -----
            float penalty = 1f;
            if (demandPenalties.TryGetValue(car.carName, out float p))
                penalty = p;

            // ----- ТЮНИНГ -----
            float tuningModifier = car.GetTuningDemandModifier();

            // ----- ВРЕМЕННЫЙ БОНУС (инвестиции) -----
            float boost = temporaryBoost;

            // ----- БОНУС ЗА УРОВЕНЬ УЛУЧШЕНИЯ -----
            float levelBonus = 1f + car.currentLevel * demandBonusPerLevel;

            // ----- ИТОГОВЫЙ СПРОС -----
            float finalDemand = baseDemand * interactiveNewsMultiplier
                * priceFactor
                * marketingBonus
                * brandModifier
                * (1f - competitorPressure)
                * penalty
                * tuningModifier
                * reputationModifier
                * boost
                * levelBonus
                * difficultyModifier;   // <-- добавлен множитель сложности

            // Ограничиваем разумными пределами
            finalDemand = Mathf.Clamp(finalDemand, 0.3f, 2.5f);

            // Сохраняем спрос в правильное место
            if (car.currentLevel > 0 && car.levels != null && car.currentLevel - 1 < car.levels.Length)
            {
                car.levels[car.currentLevel - 1].demandMultiplier = finalDemand;
            }
            else
            {
                car.demandMultiplier = finalDemand;
            }
        }

        ui.UpdateCarCards();
        ui.UpdateMoneyLabels();
    }

    private void UpdateNewsMultipliers()
    {
        var allCars = GetAllPossibleCars();
        bool newsActive = NewsManager.Instance != null && NewsManager.Instance.IsNewsActive;
        float importance = newsActive ? NewsManager.Instance.CurrentImportance : 0f;
        DifficultyManager.DifficultyLevel difficulty = CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
        int diffIndex = (int)difficulty;

        foreach (var car in allCars)
        {
            if (car == null) continue;
            if (newsActive)
            {
                float min = 0.8f;
                float max = 1.2f;
                if (car.newsDemandRanges != null && car.newsDemandRanges.Length > diffIndex)
                {
                    var range = car.newsDemandRanges[diffIndex];
                    min = range.min;
                    max = range.max;
                }
                car.newsMultiplier = Mathf.Lerp(min, max, importance);
            }
            else
            {
                car.newsMultiplier = 1f;
            }
        }
    }

    // ======================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ========================

    private float CalculateAverageMarketPrice(List<CarBlueprint> cars)
    {
        if (cars == null || cars.Count == 0) return 100f;
        float sum = 0f;
        int count = 0;
        foreach (var car in cars)
        {
            if (car == null) continue;
            sum += car.GetModifiedPrice(economy.TotalPriceModifier);
            count++;
        }
        return count > 0 ? sum / count : 100f;
    }

    private float CalculatePriceFactor(CarBlueprint car, float avgPrice)
    {
        if (car == null || avgPrice <= 0) return 1f;
        float carPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
        float ratio = carPrice / avgPrice;
        float factor = 1f / Mathf.Lerp(0.5f, 1.5f, ratio);
        return Mathf.Clamp(factor, 0.5f, 1.5f);
    }

    private float CalculateCompetitorPressure(CarBlueprint car)
    {
        if (car == null || competitor.Competitors == null) return 0f;

        float totalPressure = 0f;
        foreach (var comp in competitor.Competitors)
        {
            if (comp == null) continue;
            if (comp.availableCars != null && comp.availableCars.Contains(car))
            {
                totalPressure += comp.marketShare * 0.8f;
            }
        }
        return Mathf.Clamp(totalPressure, 0f, 0.8f);
    }

    private List<CarBlueprint> GetAllPossibleCars()
    {
        var all = new List<CarBlueprint>();
        if (tech.AvailableCars != null)
            all.AddRange(tech.AvailableCars);
        if (tech.Technologies != null)
        {
            foreach (var t in tech.Technologies)
                if (t != null && t.unlockedCar != null && !all.Contains(t.unlockedCar))
                    all.Add(t.unlockedCar);
        }
        return all;
    }

    public float GetDemandModifierForCar(string carName)
    {
        var all = GetAllPossibleCars();
        var car = all.FirstOrDefault(c => c.carName == carName);
        if (car != null)
            return car.CurrentDemandMultiplier;
        return 1f;
    }

    public void RecordDemandHistory()
    {
        var allCars = GetAllPossibleCars();
        Debug.Log($"RecordDemandHistory: найдено машин {allCars?.Count ?? 0}");
        if (allCars == null || allCars.Count == 0) return;

        foreach (var car in allCars)
        {
            if (car == null) continue;
            float currentDemand = car.CurrentDemandMultiplier;
            car.demandHistory.Add(currentDemand);
            if (car.demandHistory.Count > 12)
                car.demandHistory.RemoveAt(0);
            Debug.Log($"Машина {car.carName}: добавлен спрос {currentDemand}, теперь {car.demandHistory.Count} записей");
        }
    }
}