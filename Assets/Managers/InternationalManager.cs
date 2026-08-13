using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InternationalManager : MonoBehaviour
{
    public static InternationalManager Instance { get; private set; }

    [Header("Технология для доступа")]
    public string requiredTechForOffices = "Международная экспансия";

    [Header("Регионы")]
    public List<RegionDefinition> regionDefinitions = new List<RegionDefinition>();

    [System.Serializable]
    public class RegionDefinition
    {
        public string id;
        public string displayName;
        public float baseDemandMultiplier = 1f;
        public float importTax = 0.1f;
        public float baseRent = 50f;
        public float baseSalaries = 30f;
        public float openCost = 500f;
        public float upgradeCost = 300f;
    }

    private List<OfficeData> offices = new List<OfficeData>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += OnMonthChanged;
        // Если есть сохранение, загрузим позже
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= OnMonthChanged;
    }

    public bool IsOfficeTechUnlocked()
    {
        var tech = CarCompanyManager.Instance.TechManager;
        return tech != null && tech.IsTechResearched(requiredTechForOffices);
    }

    public bool CanOpenOffice(string regionId)
    {
        if (!IsOfficeTechUnlocked()) return false;
        var def = regionDefinitions.FirstOrDefault(r => r.id == regionId);
        if (def == null) return false;
        if (offices.Any(o => o.regionId == regionId && o.isActive)) return false;
        var economy = CarCompanyManager.Instance.EconomyManager;
        return economy.Money >= def.openCost;
    }

    public void OpenOffice(string regionId, float rent, float salaries)
    {
        if (!CanOpenOffice(regionId)) return;
        var def = regionDefinitions.First(r => r.id == regionId);
        var economy = CarCompanyManager.Instance.EconomyManager;
        economy.SpendMoney(def.openCost);

        var office = new OfficeData
        {
            regionId = regionId,
            regionName = def.displayName,
            rent = rent,
            salaries = salaries,
            level = 0,
            localPriceMultiplier = 1f,
            isActive = true
        };
        offices.Add(office);
        UIManager.Instance?.ShowNotification($"Представительство в {def.displayName} открыто!");
        UpdateUI();
    }

    public void CloseOffice(string regionId)
    {
        var office = offices.FirstOrDefault(o => o.regionId == regionId && o.isActive);
        if (office == null) return;
        office.isActive = false;
        UIManager.Instance?.ShowNotification($"Представительство в {office.regionName} закрыто.");
        UpdateUI();
    }

    public void UpgradeOffice(string regionId)
    {
        var office = offices.FirstOrDefault(o => o.regionId == regionId && o.isActive);
        if (office == null) return;
        var def = regionDefinitions.First(r => r.id == regionId);
        var economy = CarCompanyManager.Instance.EconomyManager;
        if (!economy.SpendMoney(def.upgradeCost * (office.level + 1))) return;
        office.level++;
        // Снижаем расходы на 5% за уровень, повышаем спрос на 10% за уровень
        UIManager.Instance?.ShowNotification($"Представительство в {office.regionName} улучшено до уровня {office.level+1}");
        UpdateUI();
    }

    public void SetLocalPriceMultiplier(string regionId, float multiplier)
    {
        var office = offices.FirstOrDefault(o => o.regionId == regionId && o.isActive);
        if (office == null) return;
        office.localPriceMultiplier = Mathf.Max(0.5f, Mathf.Min(2f, multiplier));
        UpdateUI();
    }

    public List<OfficeData> GetOffices() => offices;

    private void OnMonthChanged()
    {
        if (!IsOfficeTechUnlocked()) return;
        var economy = CarCompanyManager.Instance.EconomyManager;
        foreach (var office in offices.Where(o => o.isActive))
        {
            var def = regionDefinitions.First(r => r.id == office.regionId);
            // Расходы: аренда + зарплата + налог на бизнес (10% от аренды+зарплаты) + налог на зарплату (13%)
            float rent = office.rent * (1f - office.level * 0.05f);
            float salaries = office.salaries * (1f - office.level * 0.05f);
            float businessTax = (rent + salaries) * 0.1f;
            float salaryTax = salaries * 0.13f;
            float totalCost = rent + salaries + businessTax + salaryTax;
            office.monthlyCost = totalCost;

            // Доход: продажи = базовый спрос * (1 + уровень*0.1) * локальный множитель цены * (1 - импортный налог)
            float demand = def.baseDemandMultiplier * (1f + office.level * 0.1f);
            float price = economy.TotalPriceModifier * office.localPriceMultiplier;
            float revenue = demand * price * 100f; // условно 100 машин в месяц
            float tax = revenue * def.importTax;
            float netRevenue = revenue - tax;

            office.monthlyRevenue = netRevenue;

            // Применяем к экономике
            economy.Money -= totalCost;
            economy.Money += netRevenue;
        }
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance?.UpdateInternationalUI();
    }

    // ---- Сохранение/загрузка ----
    public void FillSaveData(SaveData data)
    {
        data.offices = offices;
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.offices != null)
            offices = data.offices;
    }
}