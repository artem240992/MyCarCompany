using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Расширенная система рыночного спроса.
/// Учитывает сезонность, рекламу, уровень машин, конкурентов и технологии.
/// </summary>
public class MarketSystem : MonoBehaviour
{
    public static MarketSystem Instance { get; private set; }

    [Header("Параметры рынка")]
    [SerializeField] private float seasonAmplitude = 0.3f;   // амплитуда сезонных колебаний (0.3 = ±30%)
    [SerializeField] private float seasonPeriod = 60f;       // период в секундах (полный цикл)
    [SerializeField] private float advertisingDuration = 15f;
    [SerializeField] private float advertisingMultiplier = 1.5f;

    [Header("Зависимость от уровня")]
    [SerializeField] private AnimationCurve levelDemandCurve = AnimationCurve.Linear(0, 1, 5, 1.8f); // уровень -> множитель спроса

    // --- Состояние ---
    private float seasonTime = 0f;
    private Dictionary<string, float> advertisingEndTimes = new Dictionary<string, float>();
    private Dictionary<string, List<float>> demandHistory = new Dictionary<string, List<float>>();
    private const int HISTORY_LENGTH = 30; // храним 30 последних значений

    // --- Ссылки ---
    private CarCompanyManager manager;
    private CarBlueprint[] allCars;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        manager = FindObjectOfType<CarCompanyManager>();
        if (manager == null) Debug.LogError("MarketSystem: CarCompanyManager не найден!");

        // Получаем все машины из менеджера (нужно будет добавить публичный метод)
        allCars = manager.GetAllCars();

        // Инициализируем историю
        foreach (var car in allCars)
        {
            if (!demandHistory.ContainsKey(car.carName))
                demandHistory[car.carName] = new List<float>();
        }
    }

    private void Update()
    {
        seasonTime += Time.deltaTime;
    }

    /// <summary>
    /// Получить итоговый множитель спроса для конкретной машины с учётом всех факторов.
    /// </summary>
    public float GetDemandMultiplier(CarBlueprint car, float baseDemand)
    {
        if (car == null) return 1f;

        // 1. Базовый спрос (от сложности и случайности) уже передан как baseDemand

        // 2. Сезонность
        float seasonFactor = 1f + seasonAmplitude * Mathf.Sin(seasonTime * (2f * Mathf.PI / seasonPeriod));

        // 3. Реклама (если активна для этой машины)
        float adFactor = 1f;
        if (advertisingEndTimes.ContainsKey(car.carName) && advertisingEndTimes[car.carName] > Time.time)
        {
            adFactor = advertisingMultiplier;
        }

        // 4. Уровень машины (чем выше уровень, тем выше спрос, но не линейно)
        float levelFactor = levelDemandCurve.Evaluate(car.currentLevel);

        // 5. Влияние технологий игрока (totalDemandModifier) и конкурентов уже в базовом спросе,
        //    но мы можем дополнительно учесть их здесь, если нужно.

        // 6. Случайные колебания (волатильность рынка)
        float volatility = 1f + Random.Range(-0.05f, 0.05f);

        float final = baseDemand * seasonFactor * adFactor * levelFactor * volatility;

        // 7. Сохраняем историю
        if (demandHistory.ContainsKey(car.carName))
        {
            var hist = demandHistory[car.carName];
            hist.Add(final);
            if (hist.Count > HISTORY_LENGTH) hist.RemoveAt(0);
        }

        return Mathf.Clamp(final, 0.1f, 5f);
    }

    /// <summary>
    /// Запустить рекламную кампанию для конкретной машины.
    /// </summary>
    public void StartAdvertising(string carName, float duration = -1f)
    {
        if (duration < 0) duration = advertisingDuration;
        advertisingEndTimes[carName] = Time.time + duration;
        ShowNotification($"Реклама для {carName} запущена на {duration:F0} сек!");
    }

    /// <summary>
    /// Получить историю спроса для машины (для графика).
    /// </summary>
    public List<float> GetDemandHistory(string carName)
    {
        if (demandHistory.ContainsKey(carName))
            return demandHistory[carName];
        return new List<float>();
    }

    /// <summary>
    /// Получить тренд спроса (возрастает/убывает/стабилен).
    /// </summary>
    public string GetDemandTrend(string carName)
    {
        var hist = GetDemandHistory(carName);
        if (hist.Count < 3) return "стабилен";

        float recent = hist[hist.Count - 1];
        float older = hist[hist.Count - 3];
        float diff = recent - older;
        if (diff > 0.05f) return "растёт 📈";
        if (diff < -0.05f) return "падает 📉";
        return "стабилен ➡️";
    }

    private void ShowNotification(string msg)
    {
        if (manager != null)
            manager.ShowNotification(msg); // нужно добавить публичный метод ShowNotification
        else
            Debug.Log(msg);
    }
}