using UnityEngine;
using System.Collections;

public class DifficultyManager : MonoBehaviour
{
    public enum DifficultyLevel { Easy, Normal, Hard }

    private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
    private Coroutine eventCoroutine;

    public DifficultyLevel CurrentDifficulty => currentDifficulty;

    public float CurrentEventMultiplier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return 1.2f;
                case DifficultyLevel.Normal: return 1f;
                case DifficultyLevel.Hard:   return 0.8f;
                default: return 1f;
            }
        }
    }

    // ---- НОВЫЙ МЕТОД ----
    public float GetYearlyTaxRate()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:   return 0.25f;
            case DifficultyLevel.Normal: return 0.35f;
            case DifficultyLevel.Hard:   return 0.50f;
            default: return 0.35f;
        }
    }

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;

    public void Initialize()
    {
        int saved = PlayerPrefs.GetInt("Difficulty", 1);
        currentDifficulty = (DifficultyLevel)saved;
        ApplyDifficultySettings();
        StartEconomicEventsIfHard();
    }

    public void SetDifficulty(DifficultyLevel newDifficulty)
    {
        currentDifficulty = newDifficulty;
        ApplyDifficultySettings();
        PlayerPrefs.SetInt("Difficulty", (int)currentDifficulty);
        PlayerPrefs.Save();
        StartEconomicEventsIfHard();
    }

    private void ApplyDifficultySettings()
    {
        switch (currentDifficulty)
        {
            case DifficultyLevel.Easy:
                economy.StartMoney = 1500f;
                economy.ProfitMultiplier = 1.2f;
                economy.DifficultyTechCostMultiplier = 1f;
                break;
            case DifficultyLevel.Normal:
                economy.StartMoney = 1000f;
                economy.ProfitMultiplier = 1f;
                economy.DifficultyTechCostMultiplier = 1f;
                break;
            case DifficultyLevel.Hard:
                economy.StartMoney = 500f;
                economy.ProfitMultiplier = 0.8f;
                economy.DifficultyTechCostMultiplier = 1.35f;
                break;
        }
        economy.TemporaryPriceModifier = 1f;
        economy.RecalculateModifiers(null);
        ui.UpdateMoneyLabels();
        ui.UpdateSavedDifficultyLabel();
        ui.UpdateCarCards();
    }

    public void StartEconomicEventsIfHard()
    {
        if (eventCoroutine != null)
        {
            StopCoroutine(eventCoroutine);
            eventCoroutine = null;
        }

        if (currentDifficulty == DifficultyLevel.Hard)
        {
            eventCoroutine = StartCoroutine(EventLoop());
            Debug.Log("События для Hard запущены.");
        }
        else
        {
            Debug.Log("События для Hard остановлены.");
        }
    }

    private IEnumerator EventLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(20f, 60f));
            TriggerRandomEvent();
        }
    }

    private void TriggerRandomEvent()
    {
        int eventType = Random.Range(0, 3);
        string message = "";
        switch (eventType)
        {
            case 0:
                economy.TemporaryPriceModifier = 0.8f;
                message = "Кризис! Цены упали на 20%";
                break;
            case 1:
                economy.TemporaryPriceModifier = 1.2f;
                message = "Экономический бум! Цены выросли на 20%";
                break;
            case 2:
                economy.TemporaryPriceModifier = 1f;
                message = "Рынок стабилизировался";
                break;
        }
        economy.RecalculateModifiers(null);
        ui.UpdateMoneyLabels();
        ui.UpdateCarCards();
        ui.ShowNotification(message);
    }
}