using UnityEngine;

public enum ContractType { Police, Ambulance, Mail, Taxi }

[System.Serializable]
public class ContractData
{
    public string contractId;
    public string title;
    public ContractType type;

    public int requiredQuantity;
    public int deliveredQuantity;

    // Дедлайн в игровых месяцах от момента взятия (не абсолютный месяц!)
    public int monthsRemaining;

    public float rewardMoney;
    public float penaltyMoney;
    public float rewardReputation;

    public bool isAccepted;
    public bool isCompleted;

    public float ProgressNormalized =>
        requiredQuantity > 0 ? Mathf.Clamp01((float)deliveredQuantity / requiredQuantity) : 0f;
}