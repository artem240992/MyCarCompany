using System.Collections.Generic;

[System.Serializable]
public class Competitor
{
    public string companyName;
    public float money;
    public int factoryLevel;
    public int researchLevel;
    public int reputation;
    public float priceMultiplier;
    public float marketShare;
    public List<CarBlueprint> availableCars = new List<CarBlueprint>();
    public List<string> researchedTechs = new List<string>();
    public bool isAlly;
    public int engineers;
    public float espionageLevel;
    public float marketingPower;
    public float loyalty;
    public List<string> stolenTechs = new List<string>();
}