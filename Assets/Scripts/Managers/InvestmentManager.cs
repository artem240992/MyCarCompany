using System.Collections.Generic;
using UnityEngine;

public class InvestmentManager : MonoBehaviour
{
    public static InvestmentManager Instance { get; private set; }

    [System.Serializable]
    public class Investment
    {
        public string type;
        public float amount;
        public int remainingMonths;
        public float monthlyBonus;
    }

    private List<Investment> activeInvestments = new List<Investment>();

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

    public bool MakeInvestment(string type, float amount, int months)
    {
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (economy.Money < amount) return false;
        economy.Money -= amount;
        float bonus = type == "Marketing" ? 0.02f : type == "Research" ? 0.01f : 0.015f;
        activeInvestments.Add(new Investment { type = type, amount = amount, remainingMonths = months, monthlyBonus = bonus });
        UIManager.Instance?.ShowNotification($"Инвестиция в {type} на ${amount} на {months} мес.");
        return true;
    }

    private void OnMonthChanged()
    {
        var demand = CarCompanyManager.Instance.DemandManager;
        var tech = CarCompanyManager.Instance.TechManager;
        var economy = CarCompanyManager.Instance.EconomyManager;
        CarCompanyManager.Instance.DemandManager?.UpdateDemand();

        for (int i = activeInvestments.Count - 1; i >= 0; i--)
        {
            var inv = activeInvestments[i];
            switch (inv.type)
            {
                case "Marketing": demand.ApplyTemporaryBoost(inv.monthlyBonus); break;
                case "Research": tech.ApplyResearchDiscount(0.1f); break;
                case "Production": economy.ApplyProductionDiscount(inv.monthlyBonus); break;
            }
            inv.remainingMonths--;
            if (inv.remainingMonths <= 0)
            {
                activeInvestments.RemoveAt(i);
                UIManager.Instance?.ShowNotification($"Инвестиция в {inv.type} завершена.");
                if (inv.type == "Research") tech.ApplyResearchDiscount(0f);
                if (inv.type == "Production") economy.ApplyProductionDiscount(0f);
            }
        }
    }

    public List<Investment> GetActiveInvestments() => activeInvestments;

    public void FillSaveData(SaveData data) => data.investments = activeInvestments;
    public void LoadFromSave(SaveData data) => activeInvestments = data.investments ?? new List<Investment>();
}