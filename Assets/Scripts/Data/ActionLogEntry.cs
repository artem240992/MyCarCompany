// класс записи лога
using System;

[Serializable]
public class ActionLogEntry
{
    public string competitorName;
    public string actionType;
    public bool success;
    public string resultDescription;
    public int gameMonth;
    public int gameYear;

    public ActionLogEntry(string compName, string action, bool isSuccess, string desc, int month, int year)
    {
        competitorName = compName;
        actionType = action;
        success = isSuccess;
        resultDescription = desc;
        gameMonth = month;
        gameYear = year;
    }
}