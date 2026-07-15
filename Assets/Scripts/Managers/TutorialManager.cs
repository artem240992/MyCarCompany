using UnityEngine;
using System;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    public event Action<int> OnStepChanged;

    private List<TutorialStep> steps = new List<TutorialStep>();
    private int currentStep = -1;
    private bool isActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void StartTutorial()
    {
        Debug.Log("StartTutorial вызван");
        if (steps.Count == 0)
        {
            Debug.Log("Создаём шаги");
            CreateDefaultSteps();
        }
        isActive = true;
        currentStep = 0;
        ShowStep(0);
    }

    public void NextStep()
    {
        if (!isActive) return;
        if (currentStep < steps.Count - 1)
        {
            currentStep++;
            ShowStep(currentStep);
        }
        else
        {
            CompleteTutorial();
        }
    }

    public void SkipTutorial()
    {
        isActive = false;
        OnStepChanged?.Invoke(-1);
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
    }

    public void TriggerAction(string action)
    {
        if (!isActive) return;
        if (currentStep < 0 || currentStep >= steps.Count) return;
        if (steps[currentStep].triggerAction == action)
            NextStep();
    }

    public void LoadProgress(int step)
    {
        if (step >= 0 && step < steps.Count)
        {
            currentStep = step;
            isActive = true;
            ShowStep(step);
        }
        else if (step == -2)
        {
            isActive = false;
            PlayerPrefs.SetInt("TutorialCompleted", 1);
            PlayerPrefs.Save();
        }
        else
        {
            isActive = false;
        }
    }

    public void ResetProgress()
    {
        currentStep = -1;
        isActive = false;
        PlayerPrefs.DeleteKey("TutorialCompleted");
        OnStepChanged?.Invoke(-1);
    }

    private void CreateDefaultSteps()
    {
        // Шаг 0: Приветствие
        steps.Add(new TutorialStep
        {
            title = "🚗 Добро пожаловать в автоимперию!",
            description = "Вы – владелец автозавода. Ваша цель – производить машины, зарабатывать деньги и стать лидером рынка.\n\nНажмите «Далее», чтобы начать.",
            highlightTarget = false,
            triggerAction = ""
        });

        // Шаг 1: Производство
        steps.Add(new TutorialStep
        {
            title = "🏭 Производство машин",
            description = "Нажмите на кнопку «Произвести авто» в центре экрана, чтобы создать первую машину.\n\nЭто принесёт вам пассивный доход (🚗/сек).",
            targetElementName = "ProduceButton",
            highlightTarget = true,
            triggerAction = "CarProduced"
        });

        // Шаг 2: Деньги и доход
        steps.Add(new TutorialStep
        {
            title = "💰 Деньги и доход",
            description = "В левом верхнем углу вы видите ваши деньги (💰) и доход в секунду (🚗/сек).\n\nДоход увеличивается с каждой новой машиной.",
            highlightTarget = false,
            triggerAction = ""
        });

        // Шаг 3: Улучшения
        steps.Add(new TutorialStep
        {
            title = "🔧 Улучшения завода",
            description = "В меню выберите «Улучшения». Здесь вы можете купить конвейер и нанять инженеров, чтобы ускорить производство.",
            targetElementName = "OpenUpgradeButton",
            highlightTarget = true,
            triggerAction = "UpgradeBought"
        });

        // Шаг 4: Технологии
        steps.Add(new TutorialStep
        {
            title = "🧪 Технологии",
            description = "В меню выберите «Технологии». Исследуйте улучшения для машин и открывайте новые модели.",
            targetElementName = "OpenTechButton",
            highlightTarget = true,
            triggerAction = "TechResearched"
        });

        // Шаг 5: Улучшение машины
        steps.Add(new TutorialStep
        {
            title = "🚀 Улучшение машин",
            description = "Когда вы исследуете технологию улучшения, у машин появится кнопка «Улучшить».\n\nСоздавайте новые версии автомобилей!",
            targetElementName = "",
            highlightTarget = false,
            triggerAction = "CarUpgraded"
        });

        // Шаг 6: Конкуренты
        steps.Add(new TutorialStep
        {
            title = "📊 Конкуренты",
            description = "В меню «Конкуренты» вы видите соперников и можете выполнять действия против них.\n\nВ журнале действий отображаются все атаки.",
            targetElementName = "OpenCompetitorsButton",
            highlightTarget = true,
            triggerAction = ""
        });

        // Шаг 7: Финал
        steps.Add(new TutorialStep
        {
            title = "🎉 Отличный старт!",
            description = "Теперь вы знаете основные механики. Исследуйте, улучшайте и становитесь лучшим!\n\nУдачи! 🍀",
            highlightTarget = false,
            triggerAction = ""
        });
    }

    private void ShowStep(int index)
    {
        Debug.Log($"Показываем шаг {index}");
        if (index < 0 || index >= steps.Count) return;
        OnStepChanged?.Invoke(index);
    }

    private void CompleteTutorial()
    {
        isActive = false;
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        OnStepChanged?.Invoke(-2);
    }

    public bool IsActive => isActive;
    public int CurrentStep => currentStep;
    public List<TutorialStep> GetSteps() => steps;
}

[Serializable]
public class TutorialStep
{
    public string title;
    public string description;
    public string targetElementName;
    public bool highlightTarget;
    public string triggerAction;
}