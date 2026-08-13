using UnityEngine;
using UnityEngine.UIElements;
using System;

public class DemoManager : MonoBehaviour
{
    public static DemoManager Instance { get; private set; }

    [Header("Демо-ограничения")]
    public float demoTimeLimit = 1800f;
    public int maxTechnologiesToResearch = 5;

    private UIDocument uiDoc;
    private float timeRemaining;
    private int technologiesResearched = 0;
    private bool isDemoActive = true;

    public event Action OnDemoEnd;

    public float TimeRemaining => timeRemaining;
    public int TechnologiesResearched => technologiesResearched;
    public int MaxTechnologiesToResearch => maxTechnologiesToResearch;

    public static bool IsDemoBuild
    {
        get
        {
            #if DEMO_BUILD
                return true;
            #else
                return false;
            #endif
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        timeRemaining = demoTimeLimit;
        technologiesResearched = 0;
        isDemoActive = true;

        uiDoc = FindAnyObjectByType<UIDocument>();
        if (uiDoc == null)
            Debug.LogWarning("UIDocument не найден! Экран завершения демо не будет показан.");
    }

    private void Update()
    {
        if (!isDemoActive) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0)
        {
            EndDemo("⏰ Время вышло! Спасибо за игру. Приобретите полную версию!");
        }
    }

    public bool CanResearchTechnology()
    {
        if (!isDemoActive) return false;
        if (technologiesResearched >= maxTechnologiesToResearch)
        {
            UIManager.Instance?.ShowNotification("Достигнут лимит технологий в демо-версии. Купите полную игру!");
            return false;
        }
        return true;
    }

    public void RegisterTechnologyResearched()
    {
        if (!isDemoActive) return;
        technologiesResearched++;
        Debug.Log($"Исследовано технологий: {technologiesResearched}/{maxTechnologiesToResearch}");

        if (technologiesResearched >= maxTechnologiesToResearch)
        {
            EndDemo("🧪 Вы исследовали все доступные технологии в демо-версии! Купите полную игру, чтобы продолжить.");
        }
    }

    private void EndDemo(string message)
    {
        if (!isDemoActive) return;
        isDemoActive = false;
        OnDemoEnd?.Invoke();

        ShowDemoEndScreen(message);
    }

    private void ShowDemoEndScreen(string message)
    {
        if (uiDoc == null)
        {
            uiDoc = FindAnyObjectByType<UIDocument>();
            if (uiDoc == null)
            {
                Debug.LogError("UIDocument не найден для показа экрана завершения демо!");
                return;
            }
        }

        var root = uiDoc.rootVisualElement;
        var overlay = new VisualElement();
        overlay.style.position = Position.Absolute;
        overlay.style.left = 0;
        overlay.style.top = 0;
        overlay.style.right = 0;
        overlay.style.bottom = 0;
        overlay.style.backgroundColor = new Color(0, 0, 0, 0.85f);
        overlay.style.alignItems = Align.Center;
        overlay.style.justifyContent = Justify.Center;
        overlay.style.display = DisplayStyle.Flex;

        var panel = new VisualElement();
        panel.style.backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        panel.style.paddingTop = 30;
        panel.style.paddingBottom = 30;
        panel.style.paddingLeft = 40;
        panel.style.paddingRight = 40;
        panel.style.borderTopLeftRadius = 12;
        panel.style.borderTopRightRadius = 12;
        panel.style.borderBottomLeftRadius = 12;
        panel.style.borderBottomRightRadius = 12;
        panel.style.maxWidth = 500;
        panel.style.alignItems = Align.Center;
        panel.style.flexDirection = FlexDirection.Column;

        var title = new Label("🎯 Демо-версия завершена");
        title.style.fontSize = 24;
        title.style.color = Color.white;
        title.style.unityFontStyleAndWeight = FontStyle.Bold;
        title.style.marginBottom = 12;
        panel.Add(title);

        var desc = new Label(message);
        desc.style.fontSize = 16;
        desc.style.color = new Color(0.9f, 0.9f, 0.9f);
        desc.style.whiteSpace = WhiteSpace.Normal;
        desc.style.marginBottom = 20;
        desc.style.unityTextAlign = TextAnchor.MiddleCenter;
        panel.Add(desc);

        var buyButton = new Button(() => Application.OpenURL("https://vkplay.ru/play/game/my-car-company-49107/"));
        buyButton.text = "🎮 Купить полную версию";
        buyButton.style.backgroundColor = new Color(0.2f, 0.8f, 0.2f);
        buyButton.style.color = Color.white;
        buyButton.style.fontSize = 18;
        buyButton.style.paddingTop = 10;
        buyButton.style.paddingBottom = 10;
        buyButton.style.paddingLeft = 20;
        buyButton.style.paddingRight = 20;
        buyButton.style.borderTopLeftRadius = 8;
        buyButton.style.borderTopRightRadius = 8;
        buyButton.style.borderBottomLeftRadius = 8;
        buyButton.style.borderBottomRightRadius = 8;
        buyButton.style.marginBottom = 10;
        panel.Add(buyButton);

        var closeButton = new Button(() =>
        {
            Application.Quit();
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        });
        closeButton.text = "Закрыть";
        closeButton.style.backgroundColor = new Color(0.4f, 0.4f, 0.4f);
        closeButton.style.color = Color.white;
        closeButton.style.fontSize = 14;
        closeButton.style.paddingTop = 6;
        closeButton.style.paddingBottom = 6;
        closeButton.style.paddingLeft = 16;
        closeButton.style.paddingRight = 16;
        closeButton.style.borderTopLeftRadius = 6;
        closeButton.style.borderTopRightRadius = 6;
        closeButton.style.borderBottomLeftRadius = 6;
        closeButton.style.borderBottomRightRadius = 6;
        panel.Add(closeButton);

        overlay.Add(panel);
        root.Add(overlay);
    }

    public void FillSaveData(SaveData data)
    {
        data.demoTimeRemaining = timeRemaining;
        data.demoTechsResearched = technologiesResearched;
    }

    public void LoadFromSave(SaveData data)
    {
        if (data != null && isDemoActive)
        {
            timeRemaining = data.demoTimeRemaining;
            technologiesResearched = data.demoTechsResearched;
        }
    }
}