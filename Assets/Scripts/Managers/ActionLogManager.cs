// управление логами действий
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ActionLogManager : MonoBehaviour
{
    private List<ActionLogEntry> logs = new List<ActionLogEntry>();

    public void AddLog(string competitorName, string actionType, bool success, string resultDescription)
    {
        int month = GameTimeManager.Instance?.currentMonth ?? 1;
        int year = GameTimeManager.Instance?.currentYear ?? 2025;
        logs.Add(new ActionLogEntry(competitorName, actionType, success, resultDescription, month, year));
        // Ограничим количество логов, чтобы не разрасталось (например, 200)
        if (logs.Count > 200)
            logs.RemoveAt(0);
    }

    public List<ActionLogEntry> GetLogsForCurrentYear()
    {
        int year = GameTimeManager.Instance?.currentYear ?? 2025;
        return logs.Where(log => log.gameYear == year).ToList();
    }

    public void ClearLogs() => logs.Clear();

    public void FillSaveData(SaveData data)
    {
        data.actionLogs = logs;
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.actionLogs != null)
            logs = data.actionLogs;
        else
            logs.Clear();
    }
}