using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class InternationalManager : MonoBehaviour
{
    public static InternationalManager Instance { get; private set; }

    [Header("Технология для доступа")]
    public string requiredTechForOffices = "Международная экспансия";

    [Header("Регионы")]
    public List<RegionDefinition> regionDefinitions = new List<RegionDefinition>();
    private Dictionary<string, RegionDefinition> _regionLookup;

    [Header("Баланс")]
    [Tooltip("Сколько машин в месяц условно продается в каждом офисе (для расчета выручки)")]
    [SerializeField] private float baseCarsSoldPerMonth = 100f;
    [SerializeField] private float businessTaxRate = 0.1f;
    [SerializeField] private float salaryTaxRate = 0.13f;

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

    // ВАЖНО: Если вы планируете сделать отдельный ChargingManager, 
    // лучше вынести этот класс туда. Но пока оставляем здесь для совместимости.
    [System.Serializable]
    public class ChargingStation
    {
        public string regionId;
        public int level = 0;           // 0..5
        public float coverage = 0f;     // 0..1
    }

    private List<OfficeData> offices = new List<OfficeData>();
    public List<ChargingStation> chargingStations = new List<ChargingStation>();
    private int totalOfficesOpened = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Кэшируем регионы для быстрого поиска O(1)
        _regionLookup = regionDefinitions.ToDictionary(r => r.id);
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

    public bool IsOfficeTechUnlocked()
    {
        return CarCompanyManager.Instance.TechManager.IsTechResearched(requiredTechForOffices);
    }

    public bool CanOpenOffice(string regionId)
    {
        if (!IsOfficeTechUnlocked()) return false;
        if (!_regionLookup.TryGetValue(regionId, out var def)) return false;
        if (offices.Any(o => o.regionId == regionId && o.isActive)) return false;
        
        return CarCompanyManager.Instance.EconomyManager.Money >= def.openCost;
    }

    // УПРОЩЕННАЯ СИГНАТУРА: Больше не нужно передавать rent и salaries из UI
    public void OpenOffice(string regionId)
    {
        if (!CanOpenOffice(regionId)) return;
        var def = _regionLookup[regionId];
        var economy = CarCompanyManager.Instance.EconomyManager;

        if (!economy.SpendMoney(def.openCost)) return;

        var office = new OfficeData
        {
            regionId = regionId,
            regionName = def.displayName,
            rent = def.baseRent,
            salaries = def.baseSalaries,
            level = 0,
            localPriceMultiplier = 1f,
            isActive = true
        };
        offices.Add(office);

        totalOfficesOpened++;
        CarCompanyManager.Instance.AchievementManager?.UpdateProgress("officesOpened", totalOfficesOpened);

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
        
        var def = _regionLookup[regionId];
        var economy = CarCompanyManager.Instance.EconomyManager;
        
        float cost = def.upgradeCost * (office.level + 1);
        if (!economy.SpendMoney(cost)) return;
        
        office.level++;
        UIManager.Instance?.ShowNotification($"Представительство в {office.regionName} улучшено до уровня {office.level+1}");
        UpdateUI();
    }

    public void SetLocalPriceMultiplier(string regionId, float multiplier)
    {
        var office = offices.FirstOrDefault(o => o.regionId == regionId && o.isActive);
        if (office == null) return;
        
        office.localPriceMultiplier = Mathf.Clamp(multiplier, 0.5f, 2f);
        UpdateUI();
    }

    public List<OfficeData> GetOffices() => offices;

    private void OnMonthChanged()
    {
        if (!IsOfficeTechUnlocked() || offices.Count == 0) return;
        var economy = CarCompanyManager.Instance.EconomyManager;

        foreach (var office in offices.Where(o => o.isActive))
        {
            if (!_regionLookup.TryGetValue(office.regionId, out var def)) continue;

            // Расходы
            float rent = office.rent * (1f - office.level * 0.05f);
            float salaries = office.salaries * (1f - office.level * 0.05f);
            float totalCost = rent + salaries + (rent + salaries) * businessTaxRate + salaries * salaryTaxRate;
            office.monthlyCost = totalCost;

            // Доход
            float demand = def.baseDemandMultiplier * (1f + office.level * 0.1f);
            float price = economy.TotalPriceModifier * office.localPriceMultiplier;
            float revenue = demand * price * baseCarsSoldPerMonth; 
            float tax = revenue * def.importTax;
            float netRevenue = revenue - tax;

            office.monthlyRevenue = netRevenue;

            // Применяем к экономике
            economy.Money -= totalCost;
            economy.Money += netRevenue;
        }
        
        CarCompanyManager.Instance.DemandManager?.UpdateDemand();
        UpdateUI();
    }

    private void UpdateUI()
    {
        UIManager.Instance?.UpdateInternationalUI();
    }

    // ---- СОХРАНЕНИЕ / ЗАГРУЗКА ----
    public void FillSaveData(SaveData data)
    {
        data.offices = offices;
        data.totalOfficesOpened = totalOfficesOpened;
        data.chargingStations = chargingStations; // <-- ТЕПЕРЬ СОХРАНЯЕТСЯ
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.offices != null)
            offices = data.offices;
            
        totalOfficesOpened = data.totalOfficesOpened;

        if (data.chargingStations != null)
            chargingStations = data.chargingStations; // <-- ТЕПЕРЬ ЗАГРУЖАЕТСЯ
    }
}