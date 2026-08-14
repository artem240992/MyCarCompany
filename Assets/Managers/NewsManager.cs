using UnityEngine;
using System;

public class NewsManager : MonoBehaviour
{
    public static NewsManager Instance { get; private set; }

    [Header("Настройки новостей")]
    public float newsInterval = 20f;
    public float newsDuration = 40f;
    public string[] newsTitles;
    public string[] newsDescriptions;

    private float timer;
    private float durationTimer;
    private bool isNewsActive = false;
    private float currentImportance = 0f;
    private string currentTitle = "";
    private string currentDescription = "";

    public event Action OnNewsChanged;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
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
                GenerateNews();
            }
        }
        else
        {
            durationTimer += Time.deltaTime;
            if (durationTimer >= newsDuration)
            {
                durationTimer = 0;
                EndNews();
            }
        }
    }

    private void GenerateNews()
    {
        currentImportance = UnityEngine.Random.Range(0f, 1f);
        int index = UnityEngine.Random.Range(0, Mathf.Max(1, newsTitles.Length));
        currentTitle = newsTitles.Length > 0 ? newsTitles[index % newsTitles.Length] : "Новость";
        currentDescription = newsDescriptions.Length > 0 ? newsDescriptions[index % newsDescriptions.Length] : "Влияние на рынок.";

        isNewsActive = true;
        durationTimer = 0;

        OnNewsChanged?.Invoke();
        CarCompanyManager.Instance?.DemandManager?.UpdateDemand();
        UIManager.Instance?.UpdateNewsUI();
    }

    private void EndNews()
    {
        isNewsActive = false;
        currentImportance = 0f;
        currentTitle = "";
        currentDescription = "";

        OnNewsChanged?.Invoke();
        CarCompanyManager.Instance?.DemandManager?.UpdateDemand();
        UIManager.Instance?.UpdateNewsUI();
    }

    public bool IsNewsActive => isNewsActive;
    public float CurrentImportance => currentImportance;
    public string CurrentTitle => currentTitle;
    public string CurrentDescription => currentDescription;

    public void ForceNews(float importance = -1f)
    {
        if (importance < 0)
            importance = UnityEngine.Random.Range(0f, 1f);
        currentImportance = Mathf.Clamp01(importance);
        int index = UnityEngine.Random.Range(0, Mathf.Max(1, newsTitles.Length));
        currentTitle = newsTitles.Length > 0 ? newsTitles[index % newsTitles.Length] : "Срочная новость";
        currentDescription = newsDescriptions.Length > 0 ? newsDescriptions[index % newsDescriptions.Length] : "Рынок отреагировал.";

        isNewsActive = true;
        durationTimer = 0;
        OnNewsChanged?.Invoke();
        CarCompanyManager.Instance?.DemandManager?.UpdateDemand();
        UIManager.Instance?.UpdateNewsUI();
    }
}