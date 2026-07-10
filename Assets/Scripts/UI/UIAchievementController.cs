using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIAchievementController : MonoBehaviour
{
    private VisualElement achievementsOverlay;
    private ScrollView achievementsContainer;
    private Button closeButton;

    public void Initialize(VisualElement root)
    {
        achievementsOverlay = root.Q<VisualElement>("AchievementsOverlay");
        achievementsContainer = root.Q<ScrollView>("AchievementsContainer");
        closeButton = root.Q<Button>("CloseAchievementsButton");

        if (closeButton != null)
            closeButton.clicked += Close;

        if (achievementsOverlay != null)
            achievementsOverlay.style.display = DisplayStyle.None;
    }

    public void Open()
    {
        if (achievementsOverlay != null)
        {
            achievementsOverlay.style.display = DisplayStyle.Flex;
            Refresh();
        }
    }

    public void Close()
    {
        if (achievementsOverlay != null)
            achievementsOverlay.style.display = DisplayStyle.None;
    }

    public void Refresh()
    {
        if (achievementsContainer == null) return;
        achievementsContainer.Clear();

        var manager = CarCompanyManager.Instance.AchievementManager;
        if (manager == null)
        {
            achievementsContainer.Add(new Label("Менеджер достижений не найден."));
            return;
        }

        var allAchievements = manager.GetAllAchievements();
        if (allAchievements == null || allAchievements.Length == 0)
        {
            achievementsContainer.Add(new Label("Нет доступных достижений."));
            return;
        }

        foreach (var ach in allAchievements)
        {
            var progress = manager.GetProgress(ach.achievementId);
            if (progress == null) continue;

            VisualElement card = new VisualElement();
            card.style.backgroundColor = progress.isUnlocked
                ? new StyleColor(new Color(0.15f, 0.35f, 0.15f, 0.9f))
                : new StyleColor(new Color(0.25f, 0.25f, 0.25f, 0.9f));
            card.style.paddingTop = 12;
            card.style.paddingBottom = 12;
            card.style.paddingLeft = 8;
            card.style.paddingRight = 8;
            card.style.marginBottom = 8;
            card.style.borderTopLeftRadius = 8;
            card.style.borderTopRightRadius = 8;
            card.style.borderBottomLeftRadius = 8;
            card.style.borderBottomRightRadius = 8;
            card.style.alignItems = Align.Center;
            card.style.flexDirection = FlexDirection.Column;

            if (ach.hiddenUntilUnlocked && !progress.isUnlocked)
            {
                VisualElement placeholder = new VisualElement();
                placeholder.style.width = 64;
                placeholder.style.height = 64;
                placeholder.style.backgroundColor = new StyleColor(Color.gray);
                placeholder.style.borderTopLeftRadius = 8;
                placeholder.style.borderTopRightRadius = 8;
                placeholder.style.borderBottomLeftRadius = 8;
                placeholder.style.borderBottomRightRadius = 8;
                card.Add(placeholder);

                Label secretLabel = new Label("???");
                secretLabel.style.fontSize = 18;
                secretLabel.style.color = Color.white;
                secretLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                secretLabel.style.marginTop = 8;
                card.Add(secretLabel);

                Label secretDesc = new Label("Секретное достижение");
                secretDesc.style.fontSize = 12;
                secretDesc.style.color = new Color(0.7f, 0.7f, 0.7f);
                secretDesc.style.marginTop = 4;
                card.Add(secretDesc);
            }
            else
            {
                if (ach.icon != null)
                {
                    Image icon = new Image();
                    icon.sprite = ach.icon;
                    icon.style.width = 64;
                    icon.style.height = 64;
                    icon.style.marginBottom = 8;
                    card.Add(icon);
                }
                else
                {
                    VisualElement placeholder = new VisualElement();
                    placeholder.style.width = 64;
                    placeholder.style.height = 64;
                    placeholder.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
                    placeholder.style.borderTopLeftRadius = 8;
                    placeholder.style.borderTopRightRadius = 8;
                    placeholder.style.borderBottomLeftRadius = 8;
                    placeholder.style.borderBottomRightRadius = 8;
                    placeholder.style.marginBottom = 8;
                    card.Add(placeholder);
                }

                Label titleLabel = new Label(ach.title);
                titleLabel.style.fontSize = 16;
                titleLabel.style.color = progress.isUnlocked ? Color.green : Color.white;
                titleLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                titleLabel.style.marginBottom = 2;
                titleLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                card.Add(titleLabel);

                Label descLabel = new Label(ach.description);
                descLabel.style.fontSize = 12;
                descLabel.style.color = new Color(0.8f, 0.8f, 0.8f);
                descLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                descLabel.style.marginBottom = 6;
                card.Add(descLabel);

                if (!progress.isUnlocked)
                {
                    Label progressLabel = new Label($"Прогресс: {progress.currentValue} / {ach.targetValue}");
                    progressLabel.style.fontSize = 12;
                    progressLabel.style.color = new Color(0.9f, 0.9f, 0.2f);
                    progressLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(progressLabel);
                }
                else
                {
                    Label unlockedLabel = new Label("✅ Получено!");
                    unlockedLabel.style.fontSize = 14;
                    unlockedLabel.style.color = Color.green;
                    unlockedLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
                    unlockedLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
                    card.Add(unlockedLabel);
                }
            }

            achievementsContainer.Add(card);
        }
    }
}