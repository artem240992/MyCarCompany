using UnityEngine;
using System.Collections;
using System.Linq;   // FIX: добавлен using для ToList()

public class DifficultyManager : MonoBehaviour
{
    private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
    private float currentEventMultiplier = 1f;
    private string currentEventText = "";
    private Coroutine eventCoroutine;

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;

    public DifficultyLevel CurrentDifficulty => currentDifficulty;
    public float CurrentEventMultiplier => currentEventMultiplier;

    public void Initialize()
    {
        SetDifficulty(DifficultyLevel.Normal, false);
    }

    public void SetDifficulty(DifficultyLevel level, bool resetGame = true)
    {
        currentDifficulty = level;
        ApplyDifficulty(level);
        ui.UpdateSavedDifficultyLabel();
        if (resetGame)
        {
            ResetGameState();
        }
        CarCompanyManager.Instance.SaveLoadManager.SaveGame();
    }

    private void ApplyDifficulty(DifficultyLevel level)
    {
        switch (level)
        {
            case DifficultyLevel.Easy:
                economy.StartMoney = 200f;
                economy.CostMultiplier = 0.8f;
                economy.ProfitMultiplier = 1.2f;
                economy.TechCostMultiplier = 1f;
                StopEconomicEvents();
                break;
            case DifficultyLevel.Normal:
                economy.StartMoney = 100f;
                economy.CostMultiplier = 1f;
                economy.ProfitMultiplier = 1f;
                economy.TechCostMultiplier = 1f;
                StopEconomicEvents();
                break;
            case DifficultyLevel.Hard:
                economy.StartMoney = 50f;
                economy.CostMultiplier = 1.5f;
                economy.ProfitMultiplier = 0.8f;
                economy.TechCostMultiplier = 5f;
                StartEconomicEvents();
                break;
        }
    }

    private void ResetGameState()
    {
        economy.ResetState();
        CarCompanyManager.Instance.TechManager.ResetTechs();
        CarCompanyManager.Instance.ProductionManager.SetProductionCount(1);
        CarCompanyManager.Instance.CompetitorManager.ResetCompetitors();
        var allCars = CarCompanyManager.Instance.TechManager.AvailableCars;
        foreach (var car in allCars) if (car != null) car.demandMultiplier = 1f;
        currentEventMultiplier = 1f;
        currentEventText = "";
        ui.UpdateEventUI(currentEventText, currentEventMultiplier);
        ui.UpdateUpgradeUI();
        ui.UpdateMoneyLabels();
        ui.CreateCarCards(CarCompanyManager.Instance.TechManager.AvailableCars);
        ui.CreateTechTree(CarCompanyManager.Instance.TechManager.Technologies.ToList(), economy.TechCostMultiplier); // FIX: ToList() работает
        ui.CloseAllWindows();   // FIX: теперь public
        ui.ShowNotification($"Сложность изменена на {currentDifficulty}");
    }

    public void StartEconomicEvents()
    {
        if (eventCoroutine != null) StopCoroutine(eventCoroutine);
        eventCoroutine = StartCoroutine(EconomicEvents());
    }

    public void StopEconomicEvents()
    {
        if (eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }
        currentEventMultiplier = 1f;
        currentEventText = "";
        ui.UpdateEventUI(currentEventText, currentEventMultiplier);
    }

    public void StartEconomicEventsIfHard()
    {
        if (currentDifficulty == DifficultyLevel.Hard)
            StartEconomicEvents();
    }

    private IEnumerator EconomicEvents()
    {
        while (true)
        {
            yield return new WaitForSeconds(15f);

            float multiplier = Random.Range(0.5f, 2.0f);
            multiplier = Mathf.Round(multiplier * 10f) / 10f;

            string text;
            if (multiplier < 0.8f)
                text = $"⚠️ Кризис! Спрос упал на {(1f - multiplier) * 100:F0}%";
            else if (multiplier > 1.2f)
                text = $"🚀 Бум! Спрос вырос на {(multiplier - 1f) * 100:F0}%";
            else
                text = $"📊 Спрос стабилен ({multiplier:F1}x)";

            currentEventMultiplier = multiplier;
            currentEventText = text;
            ui.UpdateEventUI(currentEventText, currentEventMultiplier);
            demand.UpdateDemand();
        }
    }
}