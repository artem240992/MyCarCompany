using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

public class MarketVisualizationController : MonoBehaviour
{
    private VisualElement root;
    private VisualElement pieChartContainer;
    private VisualElement incomeGraphContainer;
    private Label chartTitle;

    private void Awake()
    {
        var doc = GetComponent<UIDocument>();
        if (doc != null) root = doc.rootVisualElement;
        else Debug.LogError("UIDocument not found");
    }

    public void Initialize()
    {
        pieChartContainer = root.Q<VisualElement>("PieChartContainer");
        incomeGraphContainer = root.Q<VisualElement>("IncomeGraphContainer");
        chartTitle = root.Q<Label>("PieChartTitle");
        if (chartTitle != null) chartTitle.text = "Рыночная доля";
    }

    public void UpdatePieChart(List<Competitor> competitors, float playerShare)
    {
        if (pieChartContainer == null) return;
        pieChartContainer.Clear();

        // Собираем данные
        var slices = new List<(string name, float share, Color color)>();
        slices.Add(("Игрок", playerShare, new Color(0.2f, 0.8f, 0.2f)));
        foreach (var comp in competitors)
        {
            Color color = comp.strategy == "Aggressive" ? Color.red :
                          comp.strategy == "Innovative" ? Color.cyan : Color.yellow;
            slices.Add((comp.companyName, comp.marketShare, color));
        }

        float total = slices.Sum(s => s.share);
        if (total <= 0)
        {
            pieChartContainer.Add(new Label("Нет данных о рыночной доле"));
            return;
        }

        // Строим диаграмму
        float startAngle = -90f;
        foreach (var slice in slices)
        {
            float percent = slice.share / total;
            float angle = percent * 360f;
            var element = CreatePieSlice(startAngle, angle, slice.color, $"{slice.name}\n{slice.share * 100:F1}%");
            pieChartContainer.Add(element);
            startAngle += angle;
        }

        // Легенда
        var legend = root?.Q<VisualElement>("LegendContainer");
        if (legend != null)
        {
            legend.Clear();
            foreach (var slice in slices)
            {
                var item = new VisualElement();
                item.style.flexDirection = FlexDirection.Row;
                item.style.alignItems = Align.Center;
                item.style.marginBottom = 2;

                var colorBox = new VisualElement();
                colorBox.style.width = 16;
                colorBox.style.height = 16;
                colorBox.style.backgroundColor = slice.color;
                colorBox.style.marginRight = 6;
                colorBox.style.borderTopLeftRadius = 3;
                colorBox.style.borderTopRightRadius = 3;
                colorBox.style.borderBottomLeftRadius = 3;
                colorBox.style.borderBottomRightRadius = 3;

                var label = new Label($"{slice.name} ({slice.share * 100:F1}%)");
                label.style.color = Color.white;
                label.style.fontSize = 13;
                label.style.unityFontStyleAndWeight = FontStyle.Bold;

                item.Add(colorBox);
                item.Add(label);
                legend.Add(item);
            }
        }
    }


    public void UpdateBarChart(List<Competitor> competitors, float playerShare)
    {
        var container = pieChartContainer;
        if (container == null) return;
        container.Clear();

        var slices = new List<(string name, float share, Color color)>();
        slices.Add(("Игрок", playerShare, new Color(0.2f, 0.8f, 0.2f)));
        foreach (var comp in competitors)
        {
            Color color = comp.strategy == "Aggressive" ? Color.red :
                        comp.strategy == "Innovative" ? Color.cyan : Color.yellow;
            slices.Add((comp.companyName, comp.marketShare, color));
        }

        float total = slices.Sum(s => s.share);
        if (total <= 0)
        {
            container.Add(new Label("Нет данных о рыночной доле"));
            return;
        }

        // Сортируем по убыванию для наглядности
        slices = slices.OrderByDescending(s => s.share).ToList();

        foreach (var slice in slices)
        {
            float percent = slice.share / total * 100f;
            var row = new VisualElement();
            row.style.flexDirection = FlexDirection.Row;
            row.style.alignItems = Align.Center;
            row.style.marginBottom = 4;
            row.style.height = 22;

            // Название
            var nameLabel = new Label(slice.name);
            nameLabel.style.width = 80;
            nameLabel.style.color = Color.white;
            nameLabel.style.fontSize = 13;
            nameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            row.Add(nameLabel);

            // Цветная полоса
            var bar = new VisualElement();
            bar.style.height = 16;
            bar.style.backgroundColor = slice.color;
            bar.style.borderTopLeftRadius = 4;
            bar.style.borderBottomLeftRadius = 4;
            bar.style.borderTopRightRadius = 4;
            bar.style.borderBottomRightRadius = 4;
            bar.style.marginRight = 6;
            float barWidth = Mathf.Clamp((slice.share / total) * 200, 10, 200);
            bar.style.width = barWidth;

            row.Add(bar);

            // Проценты
            var percentLabel = new Label($"{percent:F1}%");
            percentLabel.style.color = Color.white;
            percentLabel.style.fontSize = 13;
            percentLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            percentLabel.style.minWidth = 45;
            row.Add(percentLabel);

            container.Add(row);
        }
    }


    private VisualElement CreatePieSlice(float startAngle, float angle, Color color, string label)
    {
        var element = new VisualElement();
        element.style.width = 200;
        element.style.height = 200;
        element.generateVisualContent += (ctx) =>
        {
            var painter = ctx.painter2D;
            painter.BeginPath();
            painter.Arc(new Vector2(100, 100), 100, startAngle * Mathf.Deg2Rad, (startAngle + angle) * Mathf.Deg2Rad);
            painter.LineTo(new Vector2(100, 100));
            painter.ClosePath();
            painter.fillColor = color;
            painter.Fill();
            painter.strokeColor = Color.white;
            painter.lineWidth = 1;
            painter.Stroke();
        };
        // Метка только для больших секторов
        if (angle > 20)
        {
            var labelElement = new Label(label);
            labelElement.style.position = Position.Absolute;
            labelElement.style.left = 100;
            labelElement.style.top = 100;
            labelElement.style.color = Color.white;
            labelElement.style.fontSize = 11;
            labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            labelElement.style.unityTextAlign = TextAnchor.MiddleCenter;
            labelElement.style.maxWidth = 80;
            element.Add(labelElement);
        }
        return element;
    }

    public void DrawIncomeGraph(List<float> data)
    {
        if (incomeGraphContainer == null) return;
        incomeGraphContainer.Clear();

        if (data == null || data.Count == 0)
        {
            incomeGraphContainer.Add(new Label("Нет данных для графика") { style = { color = Color.gray, fontSize = 14, unityTextAlign = TextAnchor.MiddleCenter } });
            return;
        }

        float max = data.Max();
        float min = data.Min();
        float range = max - min;
        if (range < 0.1f) range = 0.1f;

        var graph = new VisualElement();
        graph.style.width = new Length(100, LengthUnit.Percent);
        graph.style.height = 120;
        graph.style.backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        graph.style.borderTopLeftRadius = 4;
        graph.style.borderTopRightRadius = 4;
        graph.style.borderBottomLeftRadius = 4;
        graph.style.borderBottomRightRadius = 4;

        graph.generateVisualContent += (ctx) =>
        {
            var painter = ctx.painter2D;
            float width = graph.contentRect.width;
            float height = graph.contentRect.height;

            if (width <= 1 || height <= 1) return;

            // Сетка (горизонтальные линии)
            painter.strokeColor = new Color(0.3f, 0.3f, 0.3f);
            painter.lineWidth = 1;
            for (int i = 0; i < 4; i++)
            {
                float y = (i / 3f) * height;
                painter.BeginPath();
                painter.MoveTo(new Vector2(0, y));
                painter.LineTo(new Vector2(width, y));
                painter.Stroke();
            }

            // Линия доходов
            painter.strokeColor = Color.green;
            painter.lineWidth = 3;
            painter.BeginPath();
            for (int i = 0; i < data.Count; i++)
            {
                float x = (i / (float)(data.Count - 1)) * width;
                float y = height - ((data[i] - min) / range) * height;
                if (i == 0) painter.MoveTo(new Vector2(x, y));
                else painter.LineTo(new Vector2(x, y));
            }
            painter.Stroke();

            // Заливка под графиком (полупрозрачная)
            painter.fillColor = new Color(0, 1, 0, 0.2f);
            painter.BeginPath();
            painter.MoveTo(new Vector2(0, height));
            for (int i = 0; i < data.Count; i++)
            {
                float x = (i / (float)(data.Count - 1)) * width;
                float y = height - ((data[i] - min) / range) * height;
                painter.LineTo(new Vector2(x, y));
            }
            painter.LineTo(new Vector2(width, height));
            painter.ClosePath();
            painter.Fill();
        };
        incomeGraphContainer.Add(graph);
    }
}