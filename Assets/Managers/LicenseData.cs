using System;

[Serializable]
public class LicenseData
{
    public string techName;
    public string licensee; // имя конкурента или "other"
    public float royaltyRate; // процент от дохода лицензиата
    public float fixedFee;   // если фиксированная плата, то royaltyRate = 0
    public int monthsRemaining; // -1 = бессрочно
    public bool isActive;
}