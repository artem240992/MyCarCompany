using UnityEngine;
using System.Collections;

public class DifficultyManager : MonoBehaviour
{
    public enum DifficultyLevel { Easy, Normal, Hard }

    private DifficultyLevel currentDifficulty = DifficultyLevel.Normal;
    private Coroutine eventCoroutine;

    [Header("Сложность: множители")]
    public float priceModifierEasy = 0.9f;
    public float priceModifierNormal = 1.0f;
    public float priceModifierHard = 1.25f;

    public float demandModifierEasy = 1.15f;
    public float demandModifierNormal = 1.0f;
    public float demandModifierHard = 0.8f;

    public float taxRateEasy = 0.03f;
    public float taxRateNormal = 0.05f;
    public float taxRateHard = 0.15f;

    public float inflationRateEasy = 0.0001f;
    public float inflationRateNormal = 0.0002f;
    public float inflationRateHard = 0.0005f;

    public float researchCostModifierEasy = 0.75f;
    public float researchCostModifierNormal = 1.0f;
    public float researchCostModifierHard = 1.4f;

    public float productionCostModifierEasy = 0.85f;
    public float productionCostModifierNormal = 1.0f;
    public float productionCostModifierHard = 1.2f;

    public float newsMinEasy = 0.85f, newsMaxEasy = 1.15f;
    public float newsMinNormal = 0.65f, newsMaxNormal = 1.35f;
    public float newsMinHard = 0.45f, newsMaxHard = 1.55f;

    public float seasonalAmplitudeEasy = 0.08f;
    public float seasonalAmplitudeNormal = 0.15f;
    public float seasonalAmplitudeHard = 0.25f;

    public float startMoneyEasy = 300f;
    public float startMoneyNormal = 150f;
    public float startMoneyHard = 80f;

    [Header("Платформы")]
    public float platformCostModifierEasy = 0.8f;
    public float platformCostModifierNormal = 1.0f;
    public float platformCostModifierHard = 1.4f;

    public float platformTimeModifierEasy = 0.7f;
    public float platformTimeModifierNormal = 1.0f;
    public float platformTimeModifierHard = 1.3f;

    public float platformDiscountModifierEasy = 0.1f;
    public float platformDiscountModifierNormal = 0.0f;
    public float platformDiscountModifierHard = -0.05f;

    public DifficultyLevel CurrentDifficulty => currentDifficulty;

    // ============================================================
    // ТЕКУЩИЕ ЗНАЧЕНИЯ (вычисляются на основе сложности)
    // ============================================================

    public float CurrentPriceModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return priceModifierEasy;
                case DifficultyLevel.Normal: return priceModifierNormal;
                case DifficultyLevel.Hard:   return priceModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentDemandModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return demandModifierEasy;
                case DifficultyLevel.Normal: return demandModifierNormal;
                case DifficultyLevel.Hard:   return demandModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentTaxRate
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return taxRateEasy;
                case DifficultyLevel.Normal: return taxRateNormal;
                case DifficultyLevel.Hard:   return taxRateHard;
                default: return 0.05f;
            }
        }
    }

    public float CurrentInflationRate
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return inflationRateEasy;
                case DifficultyLevel.Normal: return inflationRateNormal;
                case DifficultyLevel.Hard:   return inflationRateHard;
                default: return 0.0002f;
            }
        }
    }

    public float CurrentResearchCostModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return researchCostModifierEasy;
                case DifficultyLevel.Normal: return researchCostModifierNormal;
                case DifficultyLevel.Hard:   return researchCostModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentProductionCostModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return productionCostModifierEasy;
                case DifficultyLevel.Normal: return productionCostModifierNormal;
                case DifficultyLevel.Hard:   return productionCostModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentStartMoney
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return startMoneyEasy;
                case DifficultyLevel.Normal: return startMoneyNormal;
                case DifficultyLevel.Hard:   return startMoneyHard;
                default: return 150f;
            }
        }
    }

    public float CurrentNewsMin
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return newsMinEasy;
                case DifficultyLevel.Normal: return newsMinNormal;
                case DifficultyLevel.Hard:   return newsMinHard;
                default: return 0.65f;
            }
        }
    }

    public float CurrentNewsMax
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return newsMaxEasy;
                case DifficultyLevel.Normal: return newsMaxNormal;
                case DifficultyLevel.Hard:   return newsMaxHard;
                default: return 1.35f;
            }
        }
    }

    public float CurrentSeasonalAmplitude
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return seasonalAmplitudeEasy;
                case DifficultyLevel.Normal: return seasonalAmplitudeNormal;
                case DifficultyLevel.Hard:   return seasonalAmplitudeHard;
                default: return 0.15f;
            }
        }
    }

    // ============================================================
    // НОВЫЕ ВЫЧИСЛЯЕМЫЕ СВОЙСТВА ДЛЯ ПЛАТФОРМ
    // ============================================================

    public float CurrentPlatformCostModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return platformCostModifierEasy;
                case DifficultyLevel.Normal: return platformCostModifierNormal;
                case DifficultyLevel.Hard:   return platformCostModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentPlatformTimeModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return platformTimeModifierEasy;
                case DifficultyLevel.Normal: return platformTimeModifierNormal;
                case DifficultyLevel.Hard:   return platformTimeModifierHard;
                default: return 1f;
            }
        }
    }

    public float CurrentPlatformDiscountModifier
    {
        get
        {
            switch (currentDifficulty)
            {
                case DifficultyLevel.Easy:   return platformDiscountModifierEasy;
                case DifficultyLevel.Normal: return platformDiscountModifierNormal;
                case DifficultyLevel.Hard:   return platformDiscountModifierHard;
                default: return 0f;
            }
        }
    }

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
        // Обновляем экономику в соответствии с текущей сложностью
        economy.StartMoney = CurrentStartMoney;
        economy.RecalculateModifiers(null);
        ui.UpdateMoneyLabels();
        ui.UpdateSavedDifficultyLabel();
        ui.UpdateCarCards();

        // Обновляем UI платформ, если они есть
        if (PlatformManager.Instance != null)
            PlatformManager.Instance.SendMessage("UpdateUI", SendMessageOptions.DontRequireReceiver);
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