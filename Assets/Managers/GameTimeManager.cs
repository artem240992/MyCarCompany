using UnityEngine;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    public event Action OnMonthChanged; // событие смены месяца

    [Header("Time Settings")]
    public float monthDuration = 30f; // секунд на один месяц
    public int currentMonth = 1;
    public int currentYear = 2025;

    private float timeSinceMonthStart;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        Debug.Log($"GameTime started: {GetDateString()}");
    }

    private void Update()
    {
        timeSinceMonthStart += Time.deltaTime;
        if (timeSinceMonthStart >= monthDuration)
        {
            timeSinceMonthStart -= monthDuration;
            AdvanceMonth();
        }
    }

    private void AdvanceMonth()
    {
        currentMonth++;
        if (currentMonth > 12)
        {
            currentMonth = 1;
            currentYear++;
        }
        OnMonthChanged?.Invoke();
        Debug.Log($"Месяц: {GetDateString()}");
    }

    public int GetCurrentYear() => currentYear;
    public int GetCurrentMonth() => currentMonth;
    public string GetDateString() => $"{currentMonth:D2}/{currentYear}";
    public float GetMonthProgress() => timeSinceMonthStart / monthDuration;

    // ---- Сохранение/загрузка ----
    public void LoadFromSave(int month, int year)
    {
        currentMonth = month;
        currentYear = year;
        Debug.Log($"GameTime загружен: {GetDateString()}");
    }

    public void FillSaveData(SaveData data)
    {
        data.currentMonth = currentMonth;
        data.currentYear = currentYear;
    }
}