using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private AchievementData[] allAchievements;

    private Dictionary<string, AchievementProgress> progressMap = new Dictionary<string, AchievementProgress>();

    public event System.Action<AchievementData> OnAchievementUnlocked;

    public void Initialize()
    {
        // Если массив не задан, создаём тестовые достижения
        if (allAchievements == null || allAchievements.Length == 0)
        {
            Debug.LogWarning("AchievementManager: allAchievements не задан. Создаются тестовые достижения.");
            GenerateTestAchievements();
        }

        progressMap.Clear();
        foreach (var ach in allAchievements)
        {
            if (!progressMap.ContainsKey(ach.achievementId))
            {
                progressMap[ach.achievementId] = new AchievementProgress
                {
                    achievementId = ach.achievementId,
                    currentValue = 0,
                    isUnlocked = false
                };
            }
        }
    }

    private void GenerateTestAchievements()
    {
        // Создаём простые достижения в коде
        var list = new List<AchievementData>();

        // Достижение "Первые деньги"
        var ach1 = ScriptableObject.CreateInstance<AchievementData>();
        ach1.achievementId = "money_100";
        ach1.title = "Первые $100";
        ach1.description = "Заработайте $100";
        ach1.targetValue = 100;
        ach1.trackedMetric = "money";
        ach1.hiddenUntilUnlocked = false;
        list.Add(ach1);

        // Достижение "Первая машина"
        var ach2 = ScriptableObject.CreateInstance<AchievementData>();
        ach2.achievementId = "cars_1";
        ach2.title = "Первая машина";
        ach2.description = "Произведите 1 машину";
        ach2.targetValue = 1;
        ach2.trackedMetric = "carsProduced";
        ach2.hiddenUntilUnlocked = false;
        list.Add(ach2);

        allAchievements = list.ToArray();
    }

    public void UpdateProgress(string metric, int value)
    {
        if (allAchievements == null || allAchievements.Length == 0)
            return;

        var targets = allAchievements.Where(a => a.trackedMetric == metric).ToList();
        foreach (var ach in targets)
        {
            if (!progressMap.TryGetValue(ach.achievementId, out var progress))
                continue;
            if (progress.isUnlocked) continue;

            if (value > progress.currentValue)
                progress.currentValue = value;

            if (progress.currentValue >= ach.targetValue)
            {
                progress.isUnlocked = true;
                OnAchievementUnlocked?.Invoke(ach);
                CarCompanyManager.Instance.UIManager?.ShowNotification($"🏆 Достижение: {ach.title}!");
            }
        }
    }

    public void SetProgressDirectly(string achievementId, int value, bool unlocked)
    {
        if (progressMap.ContainsKey(achievementId))
        {
            progressMap[achievementId].currentValue = value;
            progressMap[achievementId].isUnlocked = unlocked;
        }
    }

    public AchievementProgress GetProgress(string achievementId)
    {
        progressMap.TryGetValue(achievementId, out var prog);
        return prog;
    }

    public AchievementData[] GetAllAchievements() => allAchievements;

    public void FillSaveData(SaveData data)
    {
        data.achievementProgress = progressMap.Values.ToList();
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.achievementProgress != null)
        {
            progressMap.Clear();
            foreach (var prog in data.achievementProgress)
                progressMap[prog.achievementId] = prog;
        }
        // Синхронизируем недостающие достижения
        Initialize();
    }

    public void ResetProgress()
    {
        progressMap.Clear();
        Initialize();
    }
}