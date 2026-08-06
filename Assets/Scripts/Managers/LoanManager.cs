using System.Collections.Generic;
using UnityEngine;

public class LoanManager : MonoBehaviour
{
    public static LoanManager Instance { get; private set; }

    [System.Serializable]
    public class Loan
    {
        public float amount;
        public float interestRate;
        public int remainingMonths;
        public float monthlyPayment;
    }

    private List<Loan> activeLoans = new List<Loan>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += OnMonthChanged;
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= OnMonthChanged;
    }

    public bool TakeLoan(float amount, int months, float interestRate)
    {
        var economy = CarCompanyManager.Instance.EconomyManager;
        economy.AddMoney(amount);
        float monthlyPayment = (amount * (1f + interestRate * months)) / months;
        activeLoans.Add(new Loan { amount = amount, interestRate = interestRate, remainingMonths = months, monthlyPayment = monthlyPayment });
        UIManager.Instance?.ShowNotification($"Кредит на ${amount} взят. Ежемесячный платёж: ${monthlyPayment:F2}");
        return true;
    }

    private void OnMonthChanged()
    {
        var economy = CarCompanyManager.Instance.EconomyManager;
        for (int i = activeLoans.Count - 1; i >= 0; i--)
        {
            var loan = activeLoans[i];
            if (economy.Money < loan.monthlyPayment)
            {
                UIManager.Instance?.ShowNotification($"Недостаточно денег для оплаты кредита! Штраф 10%.");
                economy.Money *= 0.9f;
                continue;
            }
            economy.Money -= loan.monthlyPayment;
            loan.remainingMonths--;
            if (loan.remainingMonths <= 0)
            {
                activeLoans.RemoveAt(i);
                UIManager.Instance?.ShowNotification("Кредит полностью погашен!");
            }
        }
    }

    public List<Loan> GetActiveLoans() => activeLoans;

    public void FillSaveData(SaveData data) => data.loans = activeLoans;
    public void LoadFromSave(SaveData data) => activeLoans = data.loans ?? new List<Loan>();
}