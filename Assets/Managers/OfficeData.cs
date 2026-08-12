using System;

[Serializable]
public class OfficeData
{
    public string regionId;
    public string regionName;
    public float rent;          // ежемесячная аренда
    public float salaries;      // зарплата менеджеров
    public int level;           // уровень представительства (0 = базовый)
    public float localPriceMultiplier; // множитель цены для региона
    public bool isActive;
    public float monthlyRevenue; // автоматически рассчитываемые продажи
    public float monthlyCost;    // аренда+зарплата+налоги
}