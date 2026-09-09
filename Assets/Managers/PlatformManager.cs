using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class PlatformManager : MonoBehaviour
{
    public static PlatformManager Instance { get; private set; }

    [Header("Доступные платформы (ссылки на ассеты)")]
    public Platform[] allPlatforms;

    private List<Platform> developedPlatforms = new List<Platform>();
    private List<Platform> developingPlatforms = new List<Platform>(); // в процессе разработки

    private Dictionary<Platform, float> developmentProgress = new Dictionary<Platform, float>(); // оставшиеся месяцы

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Загружаем сохранённые платформы, если есть
        LoadPlatformsFromSave();
        // Подписываемся на смену месяца для обновления прогресса разработки
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += OnMonthChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= OnMonthChanged;
    }

    /// <summary>
    /// Проверяет, разблокирована ли технология для данного типа платформы.
    /// </summary>
    public bool IsPlatformTechUnlocked(PlatformType type)
    {
        string techName = GetTechNameForPlatformType(type);
        return CarCompanyManager.Instance.TechManager.IsTechResearched(techName);
    }

    private string GetTechNameForPlatformType(PlatformType type)
    {
        switch (type)
        {
            case PlatformType.City: return "Разработка Городская платформа";
            case PlatformType.Offroad: return "Разработка Внедорожная платформа";
            case PlatformType.Sport: return "Разработка Спортивная платформа";
            case PlatformType.Truck: return "Разработка Грузовая платформа";
            default: return "";
        }
    }

    /// <summary>
    /// Начинает разработку платформы (если технология изучена и платформа не разработана).
    /// </summary>
    public void DevelopPlatform(Platform platform)
    {
        if (platform == null) return;
        if (platform.isDeveloped)
        {
            UIManager.Instance?.ShowNotification($"Платформа {platform.platformName} уже разработана.");
            return;
        }
        if (developingPlatforms.Contains(platform))
        {
            UIManager.Instance?.ShowNotification($"Разработка {platform.platformName} уже идёт.");
            return;
        }

        if (!IsPlatformTechUnlocked(platform.type))
        {
            UIManager.Instance?.ShowNotification($"Изучите технологию '{GetTechNameForPlatformType(platform.type)}' для разработки этой платформы.");
            return;
        }

        var dm = CarCompanyManager.Instance.DifficultyManager;
        float costModifier = dm.CurrentPlatformCostModifier;
        float timeModifier = dm.CurrentPlatformTimeModifier;

        float finalCost = platform.developmentCost * costModifier;
        float finalTime = platform.developmentTimeMonths * timeModifier;

        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(finalCost))
        {
            UIManager.Instance?.ShowNotification($"Не хватает денег! Нужно ${finalCost:F0} для разработки {platform.platformName}.");
            return;
        }

        // Запускаем разработку
        developingPlatforms.Add(platform);
        developmentProgress[platform] = finalTime;

        UIManager.Instance?.ShowNotification($"Разработка {platform.platformName} начата! Займёт {finalTime:F1} мес.");
        UpdateUI();
    }

    /// <summary>
    /// Обновляет прогресс разработки при смене месяца.
    /// </summary>
    private void OnMonthChanged()
    {
        if (developingPlatforms.Count == 0) return;

        List<Platform> toRemove = new List<Platform>();
        foreach (var platform in developingPlatforms)
        {
            if (!developmentProgress.ContainsKey(platform)) continue;
            developmentProgress[platform] -= 1f;
            if (developmentProgress[platform] <= 0f)
            {
                CompleteDevelopment(platform);
                toRemove.Add(platform);
            }
        }

        foreach (var p in toRemove)
        {
            developingPlatforms.Remove(p);
            developmentProgress.Remove(p);
        }

        UpdateUI();
    }

    private void CompleteDevelopment(Platform platform)
    {
        platform.isDeveloped = true;
        developedPlatforms.Add(platform);
        UIManager.Instance?.ShowNotification($"✅ Платформа {platform.platformName} разработана!");
        SavePlatforms();
        UpdateUI();
    }

    public bool IsPlatformDeveloped(Platform platform)
    {
        return platform != null && platform.isDeveloped;
    }

    public float GetPlatformDiscount(CarBlueprint car)
    {
        if (car == null || car.platform == null) return 0f;
        if (!car.platform.isDeveloped) return 0f;

        float baseDiscount = car.platform.productionCostReduction;
        float extra = CarCompanyManager.Instance.DifficultyManager.CurrentPlatformDiscountModifier;
        // Эффект масштаба: каждая дополнительная модель увеличивает скидку на 2% (но не более 10%)
        float scaleBonus = Mathf.Min(car.platform.usedModelsCount * 0.02f, 0.1f);
        return Mathf.Clamp(baseDiscount + extra + scaleBonus, 0f, 0.5f);
    }

    public void RegisterModelOnPlatform(Platform platform)
    {
        if (platform != null && platform.isDeveloped)
        {
            platform.usedModelsCount++;
            SavePlatforms();
        }
    }

    public List<Platform> GetDevelopedPlatforms() => developedPlatforms;
    public List<Platform> GetDevelopingPlatforms() => developingPlatforms;
    public float GetDevelopmentProgress(Platform platform)
    {
        return developmentProgress.TryGetValue(platform, out float progress) ? progress : 0f;
    }

    // ---- Сохранение/загрузка ----
    public void SavePlatforms()
    {
        // Сохраняем в PlayerPrefs или в SaveData
        // Для простоты используем PlayerPrefs (но лучше в SaveData)
        string developedNames = string.Join(",", developedPlatforms.Select(p => p.platformName));
        PlayerPrefs.SetString("DevelopedPlatforms", developedNames);
        // Также сохраняем прогресс
        string progressData = "";
        foreach (var kvp in developmentProgress)
        {
            progressData += kvp.Key.platformName + ":" + kvp.Value + ";";
        }
        PlayerPrefs.SetString("PlatformProgress", progressData);
        PlayerPrefs.Save();
    }

    public void LoadPlatformsFromSave()
    {
        developedPlatforms.Clear();
        developingPlatforms.Clear();
        developmentProgress.Clear();

        string saved = PlayerPrefs.GetString("DevelopedPlatforms", "");
        if (!string.IsNullOrEmpty(saved))
        {
            var names = saved.Split(',');
            foreach (var name in names)
            {
                var platform = allPlatforms.FirstOrDefault(p => p.platformName == name);
                if (platform != null)
                {
                    platform.isDeveloped = true;
                    developedPlatforms.Add(platform);
                }
            }
        }

        string progressSaved = PlayerPrefs.GetString("PlatformProgress", "");
        if (!string.IsNullOrEmpty(progressSaved))
        {
            var entries = progressSaved.Split(';', System.StringSplitOptions.RemoveEmptyEntries);
            foreach (var entry in entries)
            {
                var parts = entry.Split(':');
                if (parts.Length == 2)
                {
                    var platform = allPlatforms.FirstOrDefault(p => p.platformName == parts[0]);
                    if (platform != null && float.TryParse(parts[1], out float progress))
                    {
                        developingPlatforms.Add(platform);
                        developmentProgress[platform] = progress;
                    }
                }
            }
        }
    }

    private void UpdateUI()
    {
        UIManager.Instance?.UpdatePlatformsUI();
    }
}