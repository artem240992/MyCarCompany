using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCarRecipe", menuName = "Car Company/Car Recipe")]
public class CarRecipe : ScriptableObject
{
    [Header("Требуемые детали")]
    public int engineRequired;
    public int bodyRequired;
    public int wheelsRequired;
    public int electronicsRequired;

    [Header("Комплектации")]
    public List<CarTrim> availableTrims; // список доступных комплектаций для этого рецепта

    [Header("Стоимость сборки")]
    public int assemblyCost;
    [Header("Характеристики модели (для контрактов и рынка)")]
    [Range(0f, 100f)] public float reliability = 50f;
    [Range(0f, 100f)] public float safety = 50f;
    [Range(0f, 100f)] public float comfort = 50f;
    [Range(0f, 100f)] public float tech = 50f;

    [Header("Индивидуальные цены деталей (если не заданы, берутся из PartsMarketManager)")]
    public float enginePrice = -1f;  // -1 означает использовать глобальную цену
    public float bodyPrice = -1f;
    public float wheelsPrice = -1f;
    public float electronicsPrice = -1f;

    [Header("Сезонные множители стоимости сборки (по месяцам, январь-декабрь)")]
    public float[] seasonalCostMultipliers = new float[12]
    {
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
    };

    // Метод для получения стоимости сборки с учётом текущего сезона
    public int GetModifiedAssemblyCost()
    {
        int currentMonth = GameTimeManager.Instance?.currentMonth ?? 1;
        float seasonalFactor = 1.0f;
        if (seasonalCostMultipliers != null && seasonalCostMultipliers.Length == 12)
            seasonalFactor = seasonalCostMultipliers[currentMonth - 1];
        return Mathf.RoundToInt(assemblyCost * seasonalFactor);
    }
}