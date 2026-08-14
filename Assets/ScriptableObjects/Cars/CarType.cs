using UnityEngine;

[CreateAssetMenu(fileName = "NewCarType", menuName = "Car Company/Car Type")]
public class CarType : ScriptableObject
{
    [Header("Название типа")]
    public string typeName;

    [Header("Сезонные множители спроса (по месяцам, январь-декабрь)")]
    public float[] seasonalDemandMultipliers = new float[12]
    {
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f,
        1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f
    };
}