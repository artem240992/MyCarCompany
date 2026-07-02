using System.Collections.Generic;

[System.Serializable]
public class Competitor
{
    public string companyName;
    public double money;
    public int factoryLevel;
    public int researchLevel;
    public int reputation;
    public float priceMultiplier;   // 0.6–1.5
    public float marketShare;       // 0–1
    public List<CarBlueprint> availableCars = new List<CarBlueprint>();
    public List<string> researchedTechs = new List<string>();

    // Вспомогательные поля для AI (не сохраняются)
    public float decisionCooldown; // для отсрочки действий
}