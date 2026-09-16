using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

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
    [TextArea] public string description;
    public float demandImpact;
    public float reputationImpact;
    public float moneyImpact;

    [Tooltip("Если true — для действия автоматически подбирается подходящая технология, если requiredTechName пусто.")]
    public bool requiresTech;

    [Tooltip("Точное имя технологии из TechManager. Показывается в UI как «Требуется технология: X».")]
    public string requiredTechName;

    [Header("Рекламная кампания (опционально)")]
    [Tooltip("Тип рекламы: TV / Internet / Social / Print. Пусто — кампания не создаётся.")]
    public string marketingCampaignType;

    [Tooltip("Длительность кампании в месяцах.")]
    public int marketingDurationMonths;

    [Range(0f, 1f)]
    [Tooltip("Доля от базового бюджета, которую платит игрок. 0.3 = 30%.")]
    public float marketingBudgetShare;
}

[System.Serializable]
public struct InteractiveNews
{
    public string title;
    [TextArea] public string description;
    public NewsType type;
    public NewsAction[] actions;
}

public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance { get; private set; }

    [Header("Настройки расписания")]
    public float minNewsInterval = 45f;
    public float maxNewsInterval = 90f;
    public float minNewsDuration = 30f;
    public float maxNewsDuration = 60f;

    [Header("База новостей")]
    public InteractiveNews[] interactiveNews;

    [System.Serializable]
    public struct NewsHistoryEntry
    {
        public string title;
        public NewsType type;
        public string resultText;
        public int gameMonth;
        public int gameYear;
    }

    private List<NewsHistoryEntry> history = new List<NewsHistoryEntry>();
    public IReadOnlyList<NewsHistoryEntry> History => history;

    public float NewsTimeRemaining => isNewsActive ? Mathf.Max(0f, currentDuration - durationTimer) : 0f;
    public float NewsDuration => currentDuration;

    private float timer;
    private float currentInterval;
    private float durationTimer;
    private float currentDuration;
    private bool isNewsActive = false;
    private float currentImportance = 0f;
    private string currentTitle = "";
    private string currentDescription = "";

    private bool isInteractive = false;
    private InteractiveNews currentInteractive;
    private InteractiveNews currentInteractiveResolved;
    private int lastNewsIndex = -1;

    public event Action OnNewsChanged;
    public event Action<InteractiveNews> OnInteractiveNews;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (interactiveNews == null || interactiveNews.Length == 0)
            GenerateDefaultNewsDatabase();

        SetRandomInterval();
    }

    private void Update()
    {
        if (!isNewsActive)
        {
            timer += Time.deltaTime;
            if (timer >= currentInterval)
            {
                timer = 0;
                GenerateInteractiveNews();
            }
        }
        else
        {
            durationTimer += Time.deltaTime;
            if (durationTimer >= currentDuration)
            {
                durationTimer = 0;
                EndInteractiveNews();
            }
        }
    }

    private void SetRandomInterval()
    {
        currentInterval = UnityEngine.Random.Range(minNewsInterval, maxNewsInterval);
        timer = 0;
    }

    private void GenerateInteractiveNews()
    {
        if (interactiveNews == null || interactiveNews.Length == 0) return;

        int index = UnityEngine.Random.Range(0, interactiveNews.Length);

        if (interactiveNews.Length > 1)
        {
            while (index == lastNewsIndex)
                index = UnityEngine.Random.Range(0, interactiveNews.Length);
        }
        lastNewsIndex = index;

        currentInteractive = interactiveNews[index];
        currentInteractiveResolved = ResolveTechnologies(currentInteractive);
        isInteractive = true;
        isNewsActive = true;
        durationTimer = 0;
        currentDuration = UnityEngine.Random.Range(minNewsDuration, maxNewsDuration);

        currentImportance = UnityEngine.Random.Range(0.3f, 0.9f);
        currentTitle = currentInteractive.title;
        currentDescription = currentInteractive.description;

        OnInteractiveNews?.Invoke(currentInteractiveResolved);
        UIManager.Instance?.ShowInteractiveNews(currentInteractiveResolved);
    }

    private InteractiveNews ResolveTechnologies(InteractiveNews news)
    {
        var result = news;
        if (result.actions == null) return result;

        for (int i = 0; i < result.actions.Length; i++)
        {
            var a = result.actions[i];
            if (!string.IsNullOrEmpty(a.requiredTechName)) continue;
            if (!a.requiresTech) continue;

            a.requiredTechName = PickTechForAction(a);
            result.actions[i] = a;
        }
        return result;
    }

    private string PickTechForAction(NewsAction action)
    {
        var tm = CarCompanyManager.Instance != null ? CarCompanyManager.Instance.TechManager : null;
        if (tm == null) return null;
        var all = tm.Technologies;
        if (all == null || all.Length == 0) return null;

        var candidates = new List<Technology>();
        foreach (var t in all)
            if (t != null && !t.isResearched) candidates.Add(t);

        if (candidates.Count == 0)
            foreach (var t in all)
                if (t != null) candidates.Add(t);

        if (candidates.Count == 0) return null;

        if (action.moneyImpact < 0 && action.demandImpact >= 1f)
        {
            var pref = candidates.Find(t =>
                t.techName.ToLower().Contains("logistics") ||
                t.techName.ToLower().Contains("efficien") ||
                t.techName.ToLower().Contains("production"));
            if (pref != null) return pref.techName;
        }
        if (action.reputationImpact > 0)
        {
            var pref = candidates.Find(t =>
                t.techName.ToLower().Contains("eco") ||
                t.techName.ToLower().Contains("marketing"));
            if (pref != null) return pref.techName;
        }

        return candidates[UnityEngine.Random.Range(0, candidates.Count)].techName;
    }

    public void ExecuteAction(int actionIndex)
    {
        if (!isInteractive || actionIndex < 0 || actionIndex >= currentInteractiveResolved.actions.Length) return;

        var action = currentInteractiveResolved.actions[actionIndex];

        if (!string.IsNullOrEmpty(action.requiredTechName))
        {
            var techMgr = CarCompanyManager.Instance != null ? CarCompanyManager.Instance.TechManager : null;
            if (techMgr == null || !techMgr.IsTechResearched(action.requiredTechName))
            {
                Debug.LogWarning($"[News] Действие '{action.actionName}' заблокировано — нужна технология '{action.requiredTechName}'.");
                return;
            }
        }

        var economy = CarCompanyManager.Instance.EconomyManager;

        if (action.moneyImpact != 0)
        {
            if (action.moneyImpact > 0) economy.AddMoney(action.moneyImpact);
            else economy.SpendMoney(-action.moneyImpact);
        }

        if (action.reputationImpact != 0)
            economy.AddReputation((int)action.reputationImpact);

        CarCompanyManager.Instance.DemandManager.ApplyNewsDemandMultiplier(action.demandImpact);

        // Рекламная кампания (если задана в действии)
        if (!string.IsNullOrEmpty(action.marketingCampaignType))
            ApplyMarketingCampaign(action);

        history.Add(new NewsHistoryEntry
        {
            title = currentInteractive.title,
            type = currentInteractive.type,
            resultText = $"{action.actionName}  ·  💰{(action.moneyImpact >= 0 ? "+" : "")}{action.moneyImpact:0}  ⭐{(action.reputationImpact >= 0 ? "+" : "")}{action.reputationImpact:0}  📈{((action.demandImpact - 1f) * 100f):+0;-0;0}%",
            gameMonth = GameTimeManager.Instance != null ? GameTimeManager.Instance.currentMonth : 1,
            gameYear = GameTimeManager.Instance != null ? GameTimeManager.Instance.currentYear : 2025
        });

        if (history.Count > 30) history.RemoveAt(0);

        EndInteractiveNews();
    }

    // ============================================================
    // Создание рекламной кампании со скидкой 30% от базового бюджета
    // ============================================================
    private void ApplyMarketingCampaign(NewsAction action)
    {
        var mm = MarketingManager.Instance;
        if (mm == null)
        {
            Debug.LogWarning("[News] MarketingManager.Instance == null — кампания не создана.");
            return;
        }

        // 1. Базовый бюджет по типу рекламы
        float baseBudget = GetBaseBudgetForType(action.marketingCampaignType);
        float budget = Mathf.Max(1f, baseBudget * Mathf.Clamp01(action.marketingBudgetShare));
        // Clamp01 защищает от случайного 0. Если share=0 — берём 30% по умолчанию
        if (action.marketingBudgetShare <= 0f) budget = baseBudget * 0.3f;

        // 2. Длительность
        int duration = action.marketingDurationMonths > 0 ? action.marketingDurationMonths : 3;

        // 3. Машина — по умолчанию первая доступная
        string carName = PickDefaultCarName();
        if (string.IsNullOrEmpty(carName))
        {
            Debug.LogWarning("[News] Нет доступных машин для рекламной кампании.");
            return;
        }

        // 4. Запуск кампании (StartCampaign сам спишет budget из экономики)
        bool ok = mm.StartCampaign(carName, action.marketingCampaignType, duration, budget);

        if (ok)
        {
            Debug.Log($"[News] Рекламная кампания: {action.marketingCampaignType} на {duration} мес., " +
                      $"машина '{carName}', бюджет {budget:F0} ({action.marketingBudgetShare:P0} от базового {baseBudget:F0}).");
        }
        else
        {
            Debug.LogWarning($"[News] StartCampaign вернул false — кампания {action.marketingCampaignType} не создана. " +
                             "Проверь: нет ли уже активной кампании у этой машины, хватает ли денег, разблокирован ли тип рекламы.");
        }

        // 5. Обновляем UI маркетинга, если он есть на том же объекте, что UIManager
        if (UIManager.Instance != null)
        {
            var uiMarketing = UIManager.Instance.GetComponent<UIMarketingController>();
            if (uiMarketing != null) uiMarketing.RefreshUI();
        }
    }

    private float GetBaseBudgetForType(string type)
    {
        switch (type)
        {
            case "TV":       return 1000f;
            case "Internet": return 600f;
            case "Social":   return 400f;
            case "Print":    return 500f;
            default:         return 500f;
        }
    }

    private string PickDefaultCarName()
    {
        var tm = CarCompanyManager.Instance != null ? CarCompanyManager.Instance.TechManager : null;
        if (tm == null) return null;
        var cars = tm.AvailableCars;
        if (cars == null || cars.Length == 0) return null;
        // Первая машина без активной кампании
        var mm = MarketingManager.Instance;
        foreach (var c in cars)
        {
            if (c == null) continue;
            string name = c.GetDisplayName();
            if (mm == null || !mm.activeCampaigns.Any(camp => camp.carName == name && camp.isActive))
                return name;
        }
        // Если у всех уже есть кампании — берём первую
        return cars[0].GetDisplayName();
    }

    private void EndInteractiveNews()
    {
        isNewsActive = false;
        isInteractive = false;
        currentInteractive = default;
        currentInteractiveResolved = default;
        currentImportance = 0f;
        currentTitle = "";
        currentDescription = "";

        CarCompanyManager.Instance.DemandManager.ApplyNewsDemandMultiplier(1f);
        UIManager.Instance?.CloseInteractiveNewsWindow();

        OnNewsChanged?.Invoke();
        SetRandomInterval();
    }

    public bool IsNewsActive => isNewsActive;
    public float CurrentImportance => currentImportance;
    public string CurrentTitle => currentTitle;
    public string CurrentDescription => currentDescription;
    public bool HasActiveInteractiveNews => isInteractive;
    public InteractiveNews CurrentInteractiveNews => currentInteractiveResolved;

    private void GenerateDefaultNewsDatabase()
    {
        interactiveNews = new InteractiveNews[]
        {
            // 1. ЭКОНОМИЧЕСКИЙ БУМ
            new InteractiveNews
            {
                title = "Экономический подъем",
                description = "Доходы населения растут. Люди готовы покупать новые автомобили.",
                type = NewsType.EconomicBoom,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Разработать электромобиль",
                        description = "Запустить производство машин на электротяге — большой спрос.",
                        demandImpact = 1.6f, reputationImpact = 20f, moneyImpact = -3000f,
                        requiresTech = true, requiredTechName = "Электромобиль"
                    },
                    new NewsAction {
                        actionName = "Выпустить гибрид",
                        description = "Экономичные гибридные модели привлекут новых клиентов.",
                        demandImpact = 1.35f, reputationImpact = 10f, moneyImpact = -1500f,
                        requiresTech = true, requiredTechName = "Гибридный двигатель"
                    },
                    new NewsAction {
                        actionName = "Поднять цены",
                        description = "Быстрая прибыль, но репутация может пострадать.",
                        demandImpact = 0.9f, reputationImpact = -10f, moneyImpact = 3000f
                    },
                    new NewsAction {
                        actionName = "Ничего не делать",
                        description = "Оставить как есть.",
                        demandImpact = 1.0f, reputationImpact = 0f, moneyImpact = 0f
                    }
                }
            },

            // 2. НЕФТЯНОЙ КРИЗИС
            new InteractiveNews
            {
                title = "Рост цен на топливо",
                description = "Цены на топливо выросли. Расходы на логистику и производство увеличены.",
                type = NewsType.OilCrisis,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Поднять цены на авто",
                        description = "Переложить расходы на клиентов.",
                        demandImpact = 0.8f, reputationImpact = -15f, moneyImpact = 2000f
                    },
                    new NewsAction {
                        actionName = "Взять расходы на себя",
                        description = "Сохранить лояльность клиентов.",
                        demandImpact = 1.1f, reputationImpact = 15f, moneyImpact = -2500f
                    },
                    new NewsAction {
                        actionName = "Перейти на гибриды",
                        description = "Гибридные модели меньше зависят от топлива.",
                        demandImpact = 1.2f, reputationImpact = 15f, moneyImpact = -2000f,
                        requiresTech = true, requiredTechName = "Гибридный двигатель"
                    }
                }
            },

            // 3. ГОСУДАРСТВЕННАЯ СУБСИДИЯ
            new InteractiveNews
            {
                title = "Государственная поддержка",
                description = "Правительство выделило субсидии для развития автопрома.",
                type = NewsType.GovernmentSubsidy,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Взять субсидию",
                        description = "Получить деньги на развитие.",
                        demandImpact = 1.1f, reputationImpact = 5f, moneyImpact = 5000f
                    },
                    new NewsAction {
                        actionName = "Направить на экспансию",
                        description = "Выйти на зарубежные рынки за счёт субсидии.",
                        demandImpact = 1.3f, reputationImpact = 15f, moneyImpact = 2500f,
                        requiresTech = true, requiredTechName = "Международная экспансия"
                    },
                    new NewsAction {
                        actionName = "Отказаться",
                        description = "Сохранить независимость.",
                        demandImpact = 1.0f, reputationImpact = 10f, moneyImpact = 0f
                    }
                }
            },

            // 4. ЭКО-ТРЕНД
            new InteractiveNews
            {
                title = "Экологический тренд",
                description = "Общество требует экологически чистые автомобили.",
                type = NewsType.EcoTrend,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Запустить электромобили",
                        description = "Полностью электрическая линейка — то, что ждёт рынок.",
                        demandImpact = 1.5f, reputationImpact = 30f, moneyImpact = -4000f,
                        requiresTech = true, requiredTechName = "Электромобиль"
                    },
                    new NewsAction {
                        actionName = "Предложить гибриды",
                        description = "Компромиссный вариант — экология и запас хода.",
                        demandImpact = 1.25f, reputationImpact = 20f, moneyImpact = -2000f,
                        requiresTech = true, requiredTechName = "Гибридный двигатель"
                    },
                    new NewsAction {
                        actionName = "Игнорировать тренд",
                        description = "Сэкономить деньги, но потерять клиентов.",
                        demandImpact = 0.8f, reputationImpact = -10f, moneyImpact = 0f
                    }
                }
            },

            // 5. СКАНДАЛ У КОНКУРЕНТОВ
            new InteractiveNews
            {
                title = "Скандал у конкурента",
                description = "У главного конкурента отозвали лицензию. Клиенты переходят к вам.",
                type = NewsType.CompetitorScandal,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Запустить ТВ-рекламу",
                        description = "Привлечь максимум клиентов через телевидение. Кампания на 3 мес. за 30% бюджета.",
                        demandImpact = 1.5f, reputationImpact = 10f, moneyImpact = 0f,
                        requiresTech = true, requiredTechName = "Реклама TV",
                        marketingCampaignType = "TV",
                        marketingDurationMonths = 3,
                        marketingBudgetShare = 0.3f
                    },
                    new NewsAction {
                        actionName = "Запустить интернет-рекламу",
                        description = "Дешевле, но чуть меньший охват. Кампания на 3 мес. за 30% бюджета.",
                        demandImpact = 1.35f, reputationImpact = 8f, moneyImpact = 0f,
                        requiresTech = true, requiredTechName = "Реклама Internet",
                        marketingCampaignType = "Internet",
                        marketingDurationMonths = 3,
                        marketingBudgetShare = 0.3f
                    },
                    new NewsAction {
                        actionName = "Просто ждать",
                        description = "Клиенты придут сами.",
                        demandImpact = 1.2f, reputationImpact = 0f, moneyImpact = 0f
                    }
                }
            },

            // 6. ЗАМЕДЛЕНИЕ РЫНКА
            new InteractiveNews
            {
                title = "Кризис перепроизводства",
                description = "Покупательская способность падает. Рынок перенасыщен.",
                type = NewsType.MarketSlowdown,
                actions = new NewsAction[]
                {
                    new NewsAction {
                        actionName = "Запустить автопилот",
                        description = "Технологичное преимущество поднимет привлекательность.",
                        demandImpact = 1.4f, reputationImpact = 15f, moneyImpact = -1500f,
                        requiresTech = true, requiredTechName = "Автопилот"
                    },
                    new NewsAction {
                        actionName = "Запустить рекламу в соцсетях",
                        description = "Дешёвое продвижение в интернете. Кампания на 2 мес. за 30% бюджета.",
                        demandImpact = 1.15f, reputationImpact = 5f, moneyImpact = 0f,
                        requiresTech = true, requiredTechName = "Реклама Social",
                        marketingCampaignType = "Social",
                        marketingDurationMonths = 2,
                        marketingBudgetShare = 0.3f
                    },
                    new NewsAction {
                        actionName = "Сократить производство",
                        description = "Переждать кризис.",
                        demandImpact = 0.9f, reputationImpact = -5f, moneyImpact = -500f
                    },
                    new NewsAction {
                        actionName = "Уволить часть персонала",
                        description = "Жесткая экономия.",
                        demandImpact = 1.0f, reputationImpact = -20f, moneyImpact = 1500f
                    }
                }
            }
        };
    }
}