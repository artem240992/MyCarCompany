using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IPManager : MonoBehaviour
{
    public static IPManager Instance { get; private set; }

    [Header("Технология для доступа")]
    public string requiredTechForPatents = "Патентное право";

    [Header("Стоимость патентования (зависит от сложности технологии)")]
    public float basePatentCost = 200f;

    private List<PatentData> patents = new List<PatentData>();
    private List<LicenseData> licenses = new List<LicenseData>();

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

    public bool IsPatentTechUnlocked()
    {
        var tech = CarCompanyManager.Instance.TechManager;
        return tech != null && tech.IsTechResearched(requiredTechForPatents);
    }

    public bool CanPatent(string techName)
    {
        if (!IsPatentTechUnlocked()) return false;
        var techManager = CarCompanyManager.Instance.TechManager;
        var tech = techManager?.GetTechnologyByName(techName);
        if (tech == null || !tech.isResearched) return false;
        if (patents.Any(p => p.techName == techName && p.isActive)) return false;
        return true;
    }

    public void PatentTechnology(string techName)
    {
        if (!CanPatent(techName)) return;
        var techManager = CarCompanyManager.Instance.TechManager;
        var tech = techManager.GetTechnologyByName(techName);
        float cost = basePatentCost * (tech.researchCost / 100f);
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (!economy.SpendMoney(cost)) return;

        patents.Add(new PatentData { techName = techName, monthsRemaining = 12, isActive = true });
        // Бонус репутации
        economy.AddReputation(5);
        UIManager.Instance?.ShowNotification($"Технология '{techName}' запатентована на 12 месяцев!");
        UpdateUI();
    }

    public void RenewPatent(string techName)
    {
        var patent = patents.FirstOrDefault(p => p.techName == techName && p.isActive);
        if (patent == null) return;
        float cost = basePatentCost * 0.5f; // продление дешевле
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (!economy.SpendMoney(cost)) return;
        patent.monthsRemaining += 12;
        UIManager.Instance?.ShowNotification($"Патент на '{techName}' продлён ещё на 12 месяцев!");
        UpdateUI();
    }

    public void IssueLicense(string techName, string licensee, float royaltyRate, float fixedFee = 0)
    {
        var patent = patents.FirstOrDefault(p => p.techName == techName && p.isActive);
        if (patent == null) return;
        // Можно проверить, что лицензия ещё не выдана этому лицензиату
        if (licenses.Any(l => l.techName == techName && l.licensee == licensee && l.isActive)) return;

        licenses.Add(new LicenseData
        {
            techName = techName,
            licensee = licensee,
            royaltyRate = royaltyRate,
            fixedFee = fixedFee,
            monthsRemaining = -1, // бессрочно
            isActive = true
        });
        UIManager.Instance?.ShowNotification($"Лицензия на '{techName}' выдана {licensee}");
        UpdateUI();
    }

    public void RevokeLicense(string techName, string licensee)
    {
        var license = licenses.FirstOrDefault(l => l.techName == techName && l.licensee == licensee && l.isActive);
        if (license == null) return;
        license.isActive = false;
        UIManager.Instance?.ShowNotification($"Лицензия на '{techName}' отозвана у {licensee}");
        UpdateUI();
    }

    public List<PatentData> GetPatents() => patents;
    public List<LicenseData> GetLicenses() => licenses;

    public bool IsTechnologyPatented(string techName)
    {
        return patents.Any(p => p.techName == techName && p.isActive);
    }

    private void OnMonthChanged()
    {
        // Обновление патентов
        foreach (var patent in patents.Where(p => p.isActive))
        {
            patent.monthsRemaining--;
            if (patent.monthsRemaining <= 0)
            {
                patent.isActive = false;
                UIManager.Instance?.ShowNotification($"Патент на '{patent.techName}' истёк!");
            }
        }
        CarCompanyManager.Instance.DemandManager?.UpdateDemand();

        // Сбор роялти
        float totalRoyalty = 0f;
        foreach (var license in licenses.Where(l => l.isActive))
        {
            // Здесь нужно получать доход лицензиата (пока заглушка)
            // В реальности нужно хранить доходы конкурентов
            float licenseeIncome = 1000f; // заглушка
            if (license.royaltyRate > 0)
                totalRoyalty += licenseeIncome * license.royaltyRate;
            if (license.fixedFee > 0)
                totalRoyalty += license.fixedFee;
        }
        if (totalRoyalty > 0)
        {
            CarCompanyManager.Instance.EconomyManager.AddMoney(totalRoyalty);
            UIManager.Instance?.ShowNotification($"Доход от лицензий: ${totalRoyalty:F0}");
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance?.UpdatePatentUI();
    }

    // ---- Сохранение/загрузка ----
    public void FillSaveData(SaveData data)
    {
        data.patents = patents;
        data.licenses = licenses;
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.patents != null)
            patents = data.patents;
        if (data.licenses != null)
            licenses = data.licenses;
    }
}