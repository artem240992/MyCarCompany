using UnityEngine;

[CreateAssetMenu(fileName = "NewCarOption", menuName = "Car Company/Car Option")]
public class CarOption : ScriptableObject
{
    public string optionName;
    [TextArea] public string description;

    [Header("Влияние на характеристики")]
    public float safetyBonus;      // к безопасности
    public float comfortBonus;     // к комфорту
    public float techBonus;        // к технологичности
    public float reliabilityBonus; // к надёжности

    [Header("Экономика")]
    public float additionalCost;   // дополнительная себестоимость
    public float assemblyTimeBonus; // +% ко времени сборки
}