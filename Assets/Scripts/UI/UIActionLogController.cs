// контроллер вкладки логов
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class UIActionLogController : MonoBehaviour
{
    private VisualElement actionLogContent;
    private ScrollView logScrollView;

    public void Initialize(VisualElement root)
    {
        actionLogContent = root.Q<VisualElement>("ActionLogContent");
        if (actionLogContent == null) return;

        logScrollView = new ScrollView();
        logScrollView.style.flexGrow = 1;
        actionLogContent.Add(logScrollView);
    }

    public void RefreshLogs()
    {
        if (logScrollView == null) return;
        logScrollView.Clear();

        var logManager = CarCompanyManager.Instance.ActionLogManager;
        if (logManager == null)
        {
            logScrollView.Add(new Label("Менеджер логов не найден."));
            return;
        }

        var logs = logManager.GetLogsForCurrentYear();
        if (logs.Count == 0)
        {
            logScrollView.Add(new Label("За текущий год не было действий конкурентов."));
            return;
        }

        // Заголовок
        var headerRow = new VisualElement();
        headerRow.style.flexDirection = FlexDirection.Row;
        headerRow.style.backgroundColor = new StyleColor(new Color(0.3f, 0.3f, 0.3f));
        headerRow.style.paddingTop = 4;
        headerRow.style.paddingBottom = 4;
        headerRow.style.paddingLeft = 4;
        headerRow.style.paddingRight = 4;
        headerRow.style.marginBottom = 4;
        headerRow.style.borderBottomWidth = 1;
        headerRow.style.borderBottomColor = new StyleColor(Color.gray);

        string[] headers = { "Месяц", "Компания", "Действие", "Результат" };
        float[] widths = { 50, 100, 120, 1f };
        for (int i = 0; i < headers.Length; i++)
        {
            Label lbl = new Label(headers[i]);
            lbl.style.color = Color.white;
            lbl.style.unityFontStyleAndWeight = FontStyle.Bold;
            if (i < headers.Length - 1)
                lbl.style.width = widths[i];
            else
                lbl.style.flexGrow = 1;
            headerRow.Add(lbl);
        }
        logScrollView.Add(headerRow);

        // Записи
        foreach (var entry in logs)
        {
            VisualElement row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.marginBottom = 2;
            row.style.paddingTop = 4;
            row.style.paddingBottom = 4;
            row.style.paddingLeft = 4;
            row.style.paddingRight = 4;
            row.style.backgroundColor = entry.success ? new StyleColor(new Color(0.2f, 0.3f, 0.2f)) : new StyleColor(new Color(0.3f, 0.2f, 0.2f));

            Label dateLabel = new Label($"{entry.gameMonth:D2}/{entry.gameYear}");
            dateLabel.style.width = 50;
            row.Add(dateLabel);

            Label compLabel = new Label(entry.competitorName);
            compLabel.style.width = 100;
            row.Add(compLabel);

            Label actionLabel = new Label(entry.actionType);
            actionLabel.style.width = 120;
            row.Add(actionLabel);

            Label resultLabel = new Label(entry.resultDescription);
            resultLabel.style.flexGrow = 1;
            row.Add(resultLabel);

            logScrollView.Add(row);
        }
    }
}