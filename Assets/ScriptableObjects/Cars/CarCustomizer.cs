using UnityEngine;

public class CarCustomizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Renderer bodyRenderer;   // если не назначен, берётся с этого объекта
    [SerializeField] private Renderer[] wheelRenderers; // колёса (находятся автоматически по имени "Tire")

    // Клонированные материалы (индивидуальные для экземпляра)
    private Material bodyMaterial;
    private Material glassMaterial;
    private Material[] wheelMaterials;

    // Предустановленные цвета (индексы 0..4)
    public static readonly Color[] PresetColors = new Color[]
    {
        Color.green,                    // 0 - Зеленый
        Color.red,                      // 1 - Красный
        Color.black,                    // 2 - Черный
        new Color(0.3f, 0.3f, 0.35f),  // 3 - Мокрый асфальт
        Color.white                     // 4 - Белый
    };

    public static readonly string[] PresetColorNames = new string[]
    {
        "Зеленый", "Красный", "Черный", "Мокрый асфальт", "Белый"
    };

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        // Получаем рендерер кузова
        if (bodyRenderer == null)
            bodyRenderer = GetComponent<Renderer>();

        if (bodyRenderer == null)
        {
            Debug.LogError("CarCustomizer: No Renderer found on car body!");
            return;
        }

        // Клонируем материалы кузова (предполагаем 2 слота: 0 – кузов, 1 – стёкла)
        Material[] mats = bodyRenderer.materials;
        if (mats.Length < 2)
        {
            Debug.LogWarning("CarCustomizer: Car body has less than 2 materials. Creating a dummy glass material.");
            bodyMaterial = new Material(mats[0]);
            glassMaterial = new Material(bodyMaterial); // копия для стёкол
            mats = new Material[] { bodyMaterial, glassMaterial };
        }
        else
        {
            bodyMaterial = new Material(mats[0]);
            glassMaterial = new Material(mats[1]);
            mats[0] = bodyMaterial;
            mats[1] = glassMaterial;
        }
        bodyRenderer.materials = mats;

        // Поиск колёс, если не назначены вручную
        if (wheelRenderers == null || wheelRenderers.Length == 0)
        {
            var allRenderers = GetComponentsInChildren<Renderer>();
            var wheelList = new System.Collections.Generic.List<Renderer>();
            foreach (var r in allRenderers)
            {
                if (r.gameObject != gameObject && r.gameObject.name.Contains("Tire"))
                    wheelList.Add(r);
            }
            wheelRenderers = wheelList.ToArray();
        }

        // Клонируем материалы колёс
        wheelMaterials = new Material[wheelRenderers.Length];
        for (int i = 0; i < wheelRenderers.Length; i++)
        {
            if (wheelRenderers[i] != null)
            {
                wheelMaterials[i] = new Material(wheelRenderers[i].sharedMaterial);
                wheelRenderers[i].material = wheelMaterials[i];
            }
        }

        // Устанавливаем цвета по умолчанию (белый кузов, чёрные колёса, лёгкая тонировка)
        SetBodyColor(Color.white);
        SetWheelColor(Color.black);
        SetTintLevel(0f);
    }

    // --- Публичные методы ---

    /// <summary>Установить цвет кузова</summary>
    public void SetBodyColor(Color color)
    {
        if (bodyMaterial != null)
            bodyMaterial.color = color;
    }

    /// <summary>Установить цвет стёкол (цвет тонировки)</summary>
    public void SetGlassColor(Color color)
    {
        if (glassMaterial != null)
            glassMaterial.color = color;
    }

    /// <summary>Установить цвет всех колёс</summary>
    public void SetWheelColor(Color color)
    {
        if (wheelMaterials == null) return;
        foreach (var mat in wheelMaterials)
            if (mat != null) mat.color = color;
    }

    /// <summary>Установить уровень тонировки (0 – светлые, 1 – тёмные)</summary>
    public void SetTintLevel(float level)
    {
        level = Mathf.Clamp01(level);
        // Альфа от 0.1 до 0.9, цвет – тёмно-серый
        float alpha = 0.1f + level * 0.8f;
        Color tintColor = new Color(0.05f, 0.05f, 0.05f, alpha);
        SetGlassColor(tintColor);
    }

    /// <summary>Применить настройки цвета из CarBlueprint</summary>
    public void ApplyFromBlueprint(CarBlueprint blueprint)
    {
        if (blueprint == null) return;

        int index = blueprint.bodyColorIndex;
        if (index >= 0 && index < PresetColors.Length)
            SetBodyColor(PresetColors[index]);
        else
            SetBodyColor(Color.white);

        SetTintLevel(blueprint.tintLevel);
        SetWheelColor(Color.black); // колёса оставляем чёрными (можно изменить при желании)
    }
}