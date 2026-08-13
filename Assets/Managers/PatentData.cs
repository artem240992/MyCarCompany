using System;

[Serializable]
public class PatentData
{
    public string techName;
    public int monthsRemaining; // сколько месяцев осталось до истечения
    public bool isActive;
}