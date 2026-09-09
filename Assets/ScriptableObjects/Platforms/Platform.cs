using UnityEngine;

[CreateAssetMenu(fileName = "NewPlatform", menuName = "Car Company/Platform")]
public class Platform : ScriptableObject
{
    [Header("Основные параметры")]
    public string platformName;
    [TextArea] public string description;
    public PlatformType type;

    [Header("Разработка")]
    public float developmentCost = 500f;
    public float developmentTimeMonths = 6f;

    [Header("Производственные бонусы")]
    [Range(0f, 0.5f)]
    public float productionCostReduction = 0.15f; // 15% скидка

    [Header("Статус")]
    public bool isDeveloped = false;
    public int usedModelsCount = 0; // сколько моделей уже используют платформу

    [Header("Технические характеристики")]
    public float weight = 1000f;
    public float stiffness = 1f;
}

public enum PlatformType
{
    City,
    Offroad,
    Sport,
    Truck
}