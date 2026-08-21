using UnityEngine;
using System;
using System.Collections.Generic;

public enum NewsType
{
    EconomicBoom,
    OilCrisis,
    GovernmentSubsidy,
    EcoTrend,
    CompetitorScandal,
    MarketSlowdown
}

[System.Serializable]
public struct NewsAction
{
    public string actionName;
    public string description;
    public float demandImpact;    // множитель спроса после действия
    public float reputationImpact; // изменение репутации
    public float moneyImpact;     // изменение денег (отрицательное = трата)
}

[System.Serializable]
public struct InteractiveNews
{
    public string title;
    public string description;
    public NewsType type;
    public NewsAction[] actions; // 3-4 варианта
}

public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance { get; private set; }

    [Header("Настройки новостей")]
    public float newsInterval = 25f;
    public float newsDuration = 45f;
    public InteractiveNews[] interactiveNews; // массив интерактивных новостей

    private float timer;
    private float durationTimer;
    private bool isNewsActive = false;
    private float currentImportance = 0f;
    private string currentTitle = "";
    private string currentDescription = "";

    private bool isInteractive = false;
    private InteractiveNews currentInteractive;

    public event Action OnNewsChanged;
    public event Action<InteractiveNews> OnInteractiveNews;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        timer = newsInterval * 0.5f;
    }

    private void Update()
    {
        if (!isNewsActive)
        {
            timer += Time.deltaTime;
            if (timer >= newsInterval)
            {
                timer = 0;
                GenerateInteractiveNews();
            }
        }
        else
        {
            durationTimer += Time.deltaTime;
            if (durationTimer >= newsDuration)
            {
                durationTimer = 0;
                EndInteractiveNews();
            }
        }
    }

    private void GenerateInteractiveNews()
    {
        if (interactiveNews == null || interactiveNews.Length == 0)
        {
            // Если нет интерактивных новостей, генерируем обычную
            GenerateSimpleNews();
            return;
        }

        int index = UnityEngine.Random.Range(0, interactiveNews.Length);
        currentInteractive = interactiveNews[index];
        isInteractive = true;
        isNewsActive = true;
        durationTimer = 0;

        currentImportance = UnityEngine.Random.Range(0.3f, 0.9f);
        currentTitle = currentInteractive.title;
        currentDescription = currentInteractive.description;

        OnInteractiveNews?.Invoke(currentInteractive);
        UIManager.Instance?.ShowInteractiveNews(currentInteractive);
    }

    private void GenerateSimpleNews()
    {
        // Существующая логика обычных новостей (если нужна)
    }

    public void ExecuteAction(int actionIndex)
    {
        if (!isInteractive || actionIndex < 0 || actionIndex >= currentInteractive.actions.Length) return;

        var action = currentInteractive.actions[actionIndex];
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (action.moneyImpact != 0)
        {
            if (action.moneyImpact > 0)
                economy.AddMoney(action.moneyImpact);
            else
                economy.SpendMoney(-action.moneyImpact);
        }
        if (action.reputationImpact != 0)
            economy.AddReputation((int)action.reputationImpact);

        // Применяем влияние на спрос
        CarCompanyManager.Instance.DemandManager.ApplyNewsDemandMultiplier(action.demandImpact);

        EndInteractiveNews();
    }

    private void EndInteractiveNews()
    {
        isNewsActive = false;
        isInteractive = false;
        currentInteractive = default;
        currentImportance = 0f;
        currentTitle = "";
        currentDescription = "";

        CarCompanyManager.Instance.DemandManager.ApplyNewsDemandMultiplier(1f);
        UIManager.Instance?.CloseInteractiveNewsWindow(); // <-- исправлено
        OnNewsChanged?.Invoke();
    }

    public bool IsNewsActive => isNewsActive;
    public float CurrentImportance => currentImportance;
    public string CurrentTitle => currentTitle;
    public string CurrentDescription => currentDescription;
}