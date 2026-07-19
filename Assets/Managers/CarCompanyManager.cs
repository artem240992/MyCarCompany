using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class CarCompanyManager : MonoBehaviour
{
    public static CarCompanyManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private UIDocument uiDoc;

    [Header("Стартовые машины")]
    [SerializeField] private CarBlueprint[] startCars;

    [Header("Визуализация производства")]
    [SerializeField] private GameObject carDisplayPrefab;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Имена технологий (для ссылок)")]
    [SerializeField] private string bulkProductionTechName = "Массовое производство";
    [SerializeField] private string carUpgradeTechName = "Улучшить авто";

    [Header("Дополнительные технологии (ассеты)")]
    [SerializeField] private TechnologyAsset[] additionalTechnologies;

    // ---- Существующие менеджеры ----
    public UIManager UIManager { get; private set; }
    public EconomyManager EconomyManager { get; private set; }
    public ProductionManager ProductionManager { get; private set; }
    public TechManager TechManager { get; private set; }
    public CompetitorManager CompetitorManager { get; private set; }
    public DemandManager DemandManager { get; private set; }
    public SaveLoadManager SaveLoadManager { get; private set; }
    public DifficultyManager DifficultyManager { get; private set; }
    public ActionLogManager ActionLogManager { get; private set; }
    public AchievementManager AchievementManager { get; private set; }

    // ---- НОВЫЕ МЕНЕДЖЕРЫ ДЛЯ ЗАПЧАСТЕЙ ----
    public WarehouseManager WarehouseManager { get; private set; }
    public PartsMarketManager PartsMarketManager { get; private set; }
    public PartsProductionManager PartsProductionManager { get; private set; }

    public CarBlueprint[] StartCars => startCars;
    public string BulkProductionTechName => bulkProductionTechName;
    public string CarUpgradeTechName => carUpgradeTechName;
    public TechnologyAsset[] AdditionalTechnologies => additionalTechnologies;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (GameTimeManager.Instance == null)
            gameObject.AddComponent<GameTimeManager>();

        if (TutorialManager.Instance == null)
            gameObject.AddComponent<TutorialManager>();

        // ---- Инициализация менеджеров ----
        UIManager = GetComponent<UIManager>() ?? gameObject.AddComponent<UIManager>();
        EconomyManager = GetComponent<EconomyManager>() ?? gameObject.AddComponent<EconomyManager>();
        ProductionManager = GetComponent<ProductionManager>() ?? gameObject.AddComponent<ProductionManager>();
        TechManager = GetComponent<TechManager>() ?? gameObject.AddComponent<TechManager>();
        CompetitorManager = GetComponent<CompetitorManager>() ?? gameObject.AddComponent<CompetitorManager>();
        DemandManager = GetComponent<DemandManager>() ?? gameObject.AddComponent<DemandManager>();
        SaveLoadManager = GetComponent<SaveLoadManager>() ?? gameObject.AddComponent<SaveLoadManager>();
        DifficultyManager = GetComponent<DifficultyManager>() ?? gameObject.AddComponent<DifficultyManager>();
        ActionLogManager = GetComponent<ActionLogManager>() ?? gameObject.AddComponent<ActionLogManager>();
        AchievementManager = GetComponent<AchievementManager>() ?? gameObject.AddComponent<AchievementManager>();

        // ---- СОЗДАЁМ НОВЫЕ МЕНЕДЖЕРЫ ----
        WarehouseManager = GetComponent<WarehouseManager>() ?? gameObject.AddComponent<WarehouseManager>();
        PartsMarketManager = GetComponent<PartsMarketManager>() ?? gameObject.AddComponent<PartsMarketManager>();
        PartsProductionManager = GetComponent<PartsProductionManager>() ?? gameObject.AddComponent<PartsProductionManager>();
    }

    private void Start()
    {
        UIManager.Initialize(uiDoc);

        float startMoney = 1000f;
        float profitMultiplier = 1f;
        EconomyManager.Initialize(startMoney, profitMultiplier);

        ProductionManager.Initialize(carDisplayPrefab, startPoint, endPoint);
        TechManager.Initialize(startCars);
        CompetitorManager.Initialize();
        DemandManager.Initialize();
        SaveLoadManager.Initialize();
        DifficultyManager.Initialize();

        AchievementManager.Initialize();

        EconomyManager.OnMoneyChanged += UIManager.UpdateMoneyLabels;
        EconomyManager.OnMoneyChanged += ProductionManager.UpdateButtons;

        if (TutorialManager.Instance == null)
            Debug.LogError("TutorialManager не создан!");

        if (SaveLoadManager.HasSaveFile())
        {
            SaveLoadManager.LoadGame();
            UIManager.CloseWelcomeScreen();
        }
        else
        {
            if (PlayerPrefs.HasKey("TutorialCompleted"))
                PlayerPrefs.DeleteKey("TutorialCompleted");
            UIManager.ShowWelcomeScreen();
        }

        StartCoroutine(EconomyManager.PassiveIncomeLoop());
        CompetitorManager.StartCompetitorAI();
        DifficultyManager.StartEconomicEventsIfHard();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    private void OnApplicationQuit()
    {
        SaveLoadManager.SaveGame();
    }
}