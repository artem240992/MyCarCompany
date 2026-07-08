using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MarketSystem : MonoBehaviour
{
    public static MarketSystem Instance { get; private set; }

    [Header("Параметры рынка")]
    [SerializeField] private float seasonAmplitude = 0.3f;
    [SerializeField] private float seasonPeriod = 60f;
    [SerializeField] private float advertisingDuration = 15f;
    [SerializeField] private float advertisingMultiplier = 1.5f;

    [Header("Зависимость от уровня")]
    [SerializeField] private AnimationCurve levelDemandCurve = AnimationCurve.Linear(0, 1, 5, 1.8f);

    private float seasonTime = 0f;
    private Dictionary<string, float> advertisingEndTimes = new Dictionary<string, float>();
    private Dictionary<string, List<float>> demandHistory = new Dictionary<string, List<float>>();
    private const int HISTORY_LENGTH = 30;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (CarCompanyManager.Instance == null)
            Debug.LogError("MarketSystem: CarCompanyManager.Instance не найден!");
    }

    private void Update()
    {
        seasonTime += Time.deltaTime;
    }

    public float GetDemandMultiplier(CarBlueprint car, float baseDemand)
    {
        if (car == null) return 1f;
        float seasonFactor = 1f + seasonAmplitude * Mathf.Sin(seasonTime * (2f * Mathf.PI / seasonPeriod));
        float adFactor = 1f;
        if (advertisingEndTimes.ContainsKey(car.carName) && advertisingEndTimes[car.carName] > Time.time)
            adFactor = advertisingMultiplier;
        float levelFactor = levelDemandCurve.Evaluate(car.currentLevel);
        float volatility = 1f + Random.Range(-0.05f, 0.05f);
        float final = baseDemand * seasonFactor * adFactor * levelFactor * volatility;
        if (!demandHistory.ContainsKey(car.carName))
            demandHistory[car.carName] = new List<float>();
        var hist = demandHistory[car.carName];
        hist.Add(final);
        if (hist.Count > HISTORY_LENGTH) hist.RemoveAt(0);
        return Mathf.Clamp(final, 0.1f, 5f);
    }

    public void StartAdvertising(string carName, float duration = -1f)
    {
        if (duration < 0) duration = advertisingDuration;
        advertisingEndTimes[carName] = Time.time + duration;
        ShowNotification($"Реклама для {carName} запущена на {duration:F0} сек!");
    }

    public List<float> GetDemandHistory(string carName)
    {
        if (demandHistory.ContainsKey(carName))
            return demandHistory[carName];
        return new List<float>();
    }

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

    public void DrawDemandGraph(VisualElement container, string carName)
    {
        container.Clear();
        var history = GetDemandHistory(carName);
        if (history.Count < 2)
        {
            Label noData = new Label("Нет данных");
            noData.style.color = Color.gray;
            container.Add(noData);
            return;
        }
        var graph = new VisualElement();
        graph.style.width = new Length(100, LengthUnit.Percent);
        graph.style.height = new Length(60, LengthUnit.Pixel);
        graph.style.backgroundColor = new StyleColor(new Color(0.1f, 0.1f, 0.1f));
        graph.style.marginTop = 5;
        graph.style.marginBottom = 5;
        container.Add(graph);
        graph.generateVisualContent += (meshGenContext) =>
        {
            var rect = graph.contentRect;
            if (rect.width < 2 || rect.height < 2 || history.Count < 2) return;
            float min = Mathf.Min(history.ToArray());
            float max = Mathf.Max(history.ToArray());
            if (max - min < 0.01f) { max = min + 1f; }
            var painter = meshGenContext.painter2D;
            painter.lineWidth = 2;
            painter.strokeColor = Color.green;
            painter.BeginPath();
            for (int i = 0; i < history.Count; i++)
            {
                float x = (i / (float)(history.Count - 1)) * rect.width;
                float normalized = (history[i] - min) / (max - min);
                float y = rect.height - normalized * rect.height;
                if (i == 0) painter.MoveTo(new Vector2(x, y));
                else painter.LineTo(new Vector2(x, y));
            }
            painter.Stroke();
            float avg = history.Average();
            float avgY = rect.height - ((avg - min) / (max - min)) * rect.height;
            painter.strokeColor = Color.gray;
            painter.lineWidth = 1;
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, avgY));
            painter.LineTo(new Vector2(rect.width, avgY));
            painter.Stroke();
        };
    }

    private void ShowNotification(string msg)
    {
        if (CarCompanyManager.Instance != null && CarCompanyManager.Instance.UIManager != null)
            CarCompanyManager.Instance.UIManager.ShowNotification(msg);
        else
            Debug.Log(msg);
    }
}