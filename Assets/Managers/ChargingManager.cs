using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ChargingManager : MonoBehaviour
{
    public static ChargingManager Instance { get; private set; }

    // Работаем напрямую с данными InternationalManager
    private List<InternationalManager.ChargingStation> Stations =>
        InternationalManager.Instance != null
            ? InternationalManager.Instance.chargingStations
            : new List<InternationalManager.ChargingStation>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public float GetGlobalCoverage()
    {
        var stations = Stations;
        if (stations == null || stations.Count == 0) return 0f;
        float total = 0f;
        foreach (var s in stations) total += s.coverage;
        return Mathf.Clamp01(total / stations.Count);
    }

    public InternationalManager.ChargingStation GetStation(string regionId)
    {
        return Stations.FirstOrDefault(s => s.regionId == regionId);
    }

    public void BuildOrUpgrade(string regionId)
    {
        if (InternationalManager.Instance == null)
        {
            Debug.LogError("[Charging] InternationalManager.Instance == null!");
            return;
        }

        var stations = InternationalManager.Instance.chargingStations;
        var existing = stations.FirstOrDefault(s => s.regionId == regionId);
        var economy = CarCompanyManager.Instance.EconomyManager;

        if (existing == null)
        {
            int cost = 2000;
            if (economy.Money < cost)
            {
                UIManager.Instance?.ShowNotification($"❌ Не хватает денег! Нужно ${cost} для постройки.");
                return;
            }
            
            if (!economy.SpendMoney(cost)) 
            {
                UIManager.Instance?.ShowNotification("❌ Ошибка списания денег.");
                return;
            }

            stations.Add(new InternationalManager.ChargingStation
            {
                regionId = regionId,
                level = 1,
                coverage = 0.2f
            });
            UIManager.Instance?.ShowNotification($"✅ Зарядная станция в регионе {regionId} построена!");
        }
        else
        {
            int cost = 1000 * existing.level;
            if (economy.Money < cost)
            {
                UIManager.Instance?.ShowNotification($"❌ Не хватает денег! Нужно ${cost} для улучшения.");
                return;
            }
            
            if (!economy.SpendMoney(cost)) 
            {
                UIManager.Instance?.ShowNotification("❌ Ошибка списания денег.");
                return;
            }

            existing.level++;
            existing.coverage = Mathf.Min(1f, existing.level * 0.2f);
            UIManager.Instance?.ShowNotification($"✅ Станция улучшена до ур. {existing.level}");
        }

        // Обновляем спрос и UI
        CarCompanyManager.Instance.DemandManager.UpdateDemand();
    }

    // УДАЛИТЕ отсюда FillSaveData / LoadFromSave — их теперь делает InternationalManager
}