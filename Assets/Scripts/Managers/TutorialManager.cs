public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public event Action<int> OnStepChanged;

    [SerializeField] private TutorialStep[] steps;
    private int currentStep = -1;
    private bool isActive = false;

    private void Awake() => Instance = this;

    public void StartTutorial()
    {
        isActive = true;
        currentStep = 0;
        ShowStep(0);
    }

    public void NextStep()
    {
        if (!isActive) return;
        if (currentStep < steps.Length - 1)
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
        // скрыть все подсказки
        OnStepChanged?.Invoke(-1);
    }

    private void ShowStep(int index)
    {
        // логика отображения через TutorialOverlay
        OnStepChanged?.Invoke(index);
    }

    private void CompleteTutorial()
    {
        isActive = false;
        // сохранить флаг завершения
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        OnStepChanged?.Invoke(-1);
    }

    public void LoadProgress(int step)
    {
        if (step >= 0 && step < steps.Length)
        {
            currentStep = step;
            ShowStep(step);
        }
        else if (step == -2) // -2 означает завершено
        {
            isActive = false;
        }
    }
}