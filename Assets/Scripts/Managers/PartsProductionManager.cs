using UnityEngine;

public class PartsProductionManager : MonoBehaviour
{
    public static PartsProductionManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public bool CanProducePart(PartType type)
    {
        // Проверяем, изучена ли технология производства этого типа
        string techName = $"Производство {type}";
        Technology tech = CarCompanyManager.Instance.TechManager.GetTechnologyByName(techName);
        return tech != null && tech.isResearched;
    }

    public bool ProduceParts(PartType type, int amount)
    {
        if (!CanProducePart(type))
        {
            CarCompanyManager.Instance.UIManager?.ShowNotification($"Изучите технологию производства {type}!");
            return false;
        }

        float pricePerPart = PartsMarketManager.Instance.GetPrice(type) * 0.6f; // 60% от рыночной
        float totalCost = pricePerPart * amount;

        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(totalCost)) return false;

        if (!WarehouseManager.Instance.AddParts(type, amount))
        {
            CarCompanyManager.Instance.EconomyManager.AddMoney(totalCost);
            return false;
        }

        CarCompanyManager.Instance.UIManager?.ShowNotification($"Произведено {amount} {type} за ${totalCost:F0}");
        return true;
    }
}