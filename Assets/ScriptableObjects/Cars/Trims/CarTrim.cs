using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCarTrim", menuName = "Car Company/Car Trim")]
public class CarTrim : ScriptableObject
{
    public string trimName; // "Базовая", "Комфорт", "Премиум"
    public float priceMultiplier = 1f; // 1.0, 1.15, 1.35
    public List<CarOption> availableOptions; // какие опции можно ставить
    public List<CarOption> defaultOptions;   // что уже включено
}