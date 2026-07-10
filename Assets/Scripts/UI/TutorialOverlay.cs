using UnityEngine;
using UnityEngine.UIElements;

public class TutorialOverlay : MonoBehaviour
{
    [SerializeField] private UIDocument uiDoc;
    private VisualElement root;
    private VisualElement overlayPanel;
    private Label titleLabel;
    private Label descLabel;
    private Button nextButton;
    private Button skipButton;

    private VisualElement highlightedElement;
    private VisualElement highlightBorder;

    private void Awake()
    {
        if (uiDoc == null)
            uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null)
        {
            Debug.LogError("TutorialOverlay: UIDocument не назначен!");
            return;
        }

        root = uiDoc.rootVisualElement;
        overlayPanel = root.Q<VisualElement>("TutorialOverlayPanel");
        titleLabel = root.Q<Label>("TutorialTitle");
        descLabel = root.Q<Label>("TutorialDescription");
        nextButton = root.Q<Button>("TutorialNextButton");
        skipButton = root.Q<Button>("TutorialSkipButton");

        if (overlayPanel != null)
            overlayPanel.style.display = DisplayStyle.None;
        else
            Debug.LogError("TutorialOverlayPanel не найден в UXML!");

        if (nextButton != null)
            nextButton.clicked += OnNextClicked;
        if (skipButton != null)
            skipButton.clicked += OnSkipClicked;
    }

    private void Start()
    {
        if (TutorialManager.Instance != null)
        {
            TutorialManager.Instance.OnStepChanged += OnStepChanged;
            Debug.Log("TutorialOverlay: подписан на OnStepChanged");
        }
        else
        {
            Debug.LogError("TutorialManager.Instance всё ещё null в Start!");
        }
    }

    private void OnDestroy()
    {
        if (TutorialManager.Instance != null)
            TutorialManager.Instance.OnStepChanged -= OnStepChanged;
    }

    private void OnStepChanged(int stepIndex)
    {
        Debug.Log($"TutorialOverlay: получил шаг {stepIndex}");
        if (stepIndex == -1 || stepIndex == -2)
        {
            overlayPanel.style.display = DisplayStyle.None;
            ClearHighlight();
            return;
        }

        var steps = TutorialManager.Instance?.GetSteps();
        if (steps == null || stepIndex >= steps.Count) return;

        var step = steps[stepIndex];
        ShowStep(step);
    }

    private void ShowStep(TutorialStep step)
    {
        if (overlayPanel == null) return;

        titleLabel.text = step.title;
        descLabel.text = step.description;

        if (step.highlightTarget && !string.IsNullOrEmpty(step.targetElementName))
            HighlightElement(step.targetElementName);
        else
            ClearHighlight();

        overlayPanel.style.display = DisplayStyle.Flex;
    }

    private void HighlightElement(string elementName)
    {
        ClearHighlight();

        if (root == null) return;
        var target = root.Q<VisualElement>(elementName);
        if (target == null)
        {
            Debug.LogWarning($"Элемент '{elementName}' не найден для подсветки.");
            return;
        }

        highlightedElement = target;

        highlightBorder = new VisualElement();
        highlightBorder.style.position = Position.Absolute;
        highlightBorder.style.left = target.worldBound.x;
        highlightBorder.style.top = target.worldBound.y;
        highlightBorder.style.width = target.worldBound.width;
        highlightBorder.style.height = target.worldBound.height;
        highlightBorder.style.borderTopWidth = 3;
        highlightBorder.style.borderBottomWidth = 3;
        highlightBorder.style.borderLeftWidth = 3;
        highlightBorder.style.borderRightWidth = 3;
        highlightBorder.style.borderTopColor = Color.yellow;
        highlightBorder.style.borderBottomColor = Color.yellow;
        highlightBorder.style.borderLeftColor = Color.yellow;
        highlightBorder.style.borderRightColor = Color.yellow;
        highlightBorder.pickingMode = PickingMode.Ignore;

        root.Add(highlightBorder);
    }

    private void ClearHighlight()
    {
        if (highlightBorder != null && highlightBorder.parent != null)
            highlightBorder.RemoveFromHierarchy();
        highlightBorder = null;
        highlightedElement = null;
    }

    private void OnNextClicked()
    {
        TutorialManager.Instance?.NextStep();
    }

    private void OnSkipClicked()
    {
        TutorialManager.Instance?.SkipTutorial();
        overlayPanel.style.display = DisplayStyle.None;
        ClearHighlight();
    }
}