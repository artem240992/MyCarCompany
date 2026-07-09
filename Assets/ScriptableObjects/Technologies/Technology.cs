using UnityEngine;

[System.Serializable]
public class Technology
{
    public string techName;
    public string description;
    public int researchCost;
    public bool isResearched;
    public string[] requiredTechNames;
    public CarBlueprint unlockedCar;

    public int availableYear = 2025;
    public int availableMonth = 1;

    public bool IsAvailable(int currentYear, int currentMonth)
    {
        return currentYear > availableYear || (currentYear == availableYear && currentMonth >= availableMonth);
    }

    [Tooltip("Открывать ли машину при изучении технологии")]
    public bool unlockCarOnResearch = true;

    // ---- НОВЫЕ ПОЛЯ ДЛЯ ВЛИЯНИЯ НА ЦЕНУ И СПРОС ----
    [Tooltip("Множитель цены (например, 1.1 = +10%)")]
    public float priceModifier = 1f;

    [Tooltip("Множитель спроса (например, 1.05 = +5%)")]
    public float demandModifier = 1f;
}