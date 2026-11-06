using UnityEngine;
using System.Collections.Generic;

public class ContractManager : MonoBehaviour
{
    public static ContractManager Instance { get; private set; }

    public List<ContractData> availableContracts = new List<ContractData>();
    public List<ContractData> activeContracts    = new List<ContractData>();

    // События — подписывайтесь из CarCompanyManager или UI
    public event System.Action<ContractData> OnContractCompleted; // награда
    public event System.Action<ContractData> OnContractFailed;    // штраф
    public event System.Action OnContractsChanged;                // перерисовать доску

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged += HandleMonthChanged;

        GenerateContracts(3);
        OnContractsChanged?.Invoke();
    }

    private void OnDestroy()
    {
        if (GameTimeManager.Instance != null)
            GameTimeManager.Instance.OnMonthChanged -= HandleMonthChanged;
    }

    // ---------- Генерация ----------

    public void GenerateContracts(int count)
    {
        for (int i = 0; i < count; i++)
            availableContracts.Add(CreateRandomContract());
    }

    private ContractData CreateRandomContract()
    {
        var c = new ContractData
        {
            contractId       = System.Guid.NewGuid().ToString(),
            type             = (ContractType)Random.Range(0, 4),
            requiredQuantity = Random.Range(5, 20),
            monthsRemaining  = 3,
            rewardReputation = 5f,
            isAccepted       = false,
            isCompleted      = false,
            deliveredQuantity = 0
        };

        switch (c.type)
        {
            case ContractType.Police:
                c.title = "Полицейские седаны";
                break;
            case ContractType.Ambulance:
                c.title = "Машины скорой помощи";
                break;
            case ContractType.Mail:
                c.title = "Почтовые фургоны";
                break;
            case ContractType.Taxi:
                c.title = "Городские такси";
                break;
        }

        c.rewardMoney  = c.requiredQuantity * Random.Range(800f, 1500f);
        c.penaltyMoney = c.rewardMoney * 0.3f;
        return c;
    }

    // ---------- Работа с контрактами ----------

    public bool AcceptContract(string contractId)
    {
        var c = availableContracts.Find(x => x.contractId == contractId);
        if (c == null) return false;

        c.isAccepted = true;
        availableContracts.Remove(c);
        activeContracts.Add(c);
        OnContractsChanged?.Invoke();
        return true;
    }

    /// <summary>Вызывайте из WarehouseManager/производства при выпуске машины.</summary>
    public void RegisterDeliveredCar(CarBlueprint car)
    {
        if (car == null) return;

        for (int i = 0; i < activeContracts.Count; i++)
        {
            var c = activeContracts[i];
            if (c.isCompleted) continue;
            if (!CarMatchesContract(car, c)) continue;

            // Списываем машину со склада без начисления денег
            if (WarehouseManager.Instance == null)
            {
                Debug.LogWarning("[Contract] WarehouseManager.Instance == null");
                return;
            }
            if (!WarehouseManager.Instance.RemoveCar(car, 1))
            {
                Debug.LogWarning($"[Contract] Не удалось списать {car.GetDisplayName()} со склада");
                return;
            }

            c.deliveredQuantity++;
            Debug.Log($"[Contract] Доставлено {c.deliveredQuantity}/{c.requiredQuantity}: {c.title}");

            if (c.deliveredQuantity >= c.requiredQuantity)
                CompleteContract(c);

            OnContractsChanged?.Invoke();
            return;
        }
    }

    // Пока принимает любую машину — расширим позже, когда будет выбор рецепта
    public bool CarMatchesContract(CarBlueprint car, ContractData c)
    {
        if (car == null || car.carType == null) return false;

        string typeName = car.carType.typeName;   // ← вот этой строки не хватало

        switch (c.type)
        {
            case ContractType.Police:    return typeName == "Седан"       && car.currentSafety  >= 3;
            case ContractType.Ambulance: return typeName == "Внедорожник" && car.currentSafety  >= 4;
            case ContractType.Mail:      return typeName == "Фургон"      && car.currentEconomy >= 2;
            case ContractType.Taxi:      return typeName == "Седан"       && car.currentEconomy >= 3;
            default:
                return false;
        }
    }




    public string GetContractRequirementsText(ContractData c)
    {
        if (c == null) return "";
        switch (c.type)
        {
            case ContractType.Police:    return "Требуется: Седан | Безопасность ≥ 3";
            case ContractType.Ambulance: return "Требуется: Внедорожник | Безопасность ≥ 4";
            case ContractType.Mail:      return "Требуется: Фургон | Экономичность ≥ 2";
            case ContractType.Taxi:      return "Требуется: Седан | Экономичность ≥ 3";
            default: return "Требования неизвестны";
        }
    }






    private void CompleteContract(ContractData c)
    {
        c.isCompleted = true;
        activeContracts.Remove(c);
        OnContractCompleted?.Invoke(c);
        OnContractsChanged?.Invoke();
    }

    public List<ContractData> GetMatchingContracts(CarBlueprint car)
    {
        List<ContractData> matches = new List<ContractData>();
        if (car == null) return matches;

        // Сначала проверяем активные (они приоритетнее)
        foreach (var c in activeContracts)
        {
            if (CarMatchesContract(car, c)) 
                matches.Add(c);
        }
        
        // Затем доступные
        foreach (var c in availableContracts)
        {
            if (CarMatchesContract(car, c)) 
                matches.Add(c);
        }
        return matches;
    }

    // ---------- Время ----------

    private void HandleMonthChanged()
    {
        TickDeadlines();
        if (availableContracts.Count < 5)
            GenerateContracts(1);
        OnContractsChanged?.Invoke();
    }

    private void TickDeadlines()
    {
        for (int i = activeContracts.Count - 1; i >= 0; i--)
        {
            var c = activeContracts[i];
            c.monthsRemaining--;

            if (c.monthsRemaining < 0 && !c.isCompleted)
            {
                Debug.Log($"[Contract] Провал: {c.title}, monthsRemaining={c.monthsRemaining}");
                activeContracts.RemoveAt(i);
                OnContractFailed?.Invoke(c);
                OnContractsChanged?.Invoke();
            }
        }
    }


    public void FillSaveData(SaveData data)
    {
        data.availableContracts = new List<ContractSaveData>();
        foreach (var c in availableContracts)
            data.availableContracts.Add(ToSave(c));

        data.activeContracts = new List<ContractSaveData>();
        foreach (var c in activeContracts)
            data.activeContracts.Add(ToSave(c));
    }

    public void LoadFromSave(SaveData data)
    {
        availableContracts.Clear();
        activeContracts.Clear();

        if (data.availableContracts != null)
            foreach (var s in data.availableContracts)
                availableContracts.Add(FromSave(s));

        if (data.activeContracts != null)
            foreach (var s in data.activeContracts)
                activeContracts.Add(FromSave(s));

        OnContractsChanged?.Invoke();
    }

    private ContractSaveData ToSave(ContractData c) => new ContractSaveData
    {
        contractId       = c.contractId,
        title            = c.title,
        type             = (int)c.type,
        requiredQuantity = c.requiredQuantity,
        monthsRemaining  = c.monthsRemaining,
        rewardMoney      = c.rewardMoney,
        rewardReputation = c.rewardReputation,
        penaltyMoney     = c.penaltyMoney,
        deliveredQuantity = c.deliveredQuantity,
        isAccepted       = c.isAccepted,
        isCompleted      = c.isCompleted
    };

    private ContractData FromSave(ContractSaveData s) => new ContractData
    {
        contractId       = s.contractId,
        title            = s.title,
        type             = (ContractType)s.type,
        requiredQuantity = s.requiredQuantity,
        monthsRemaining  = s.monthsRemaining,
        rewardMoney      = s.rewardMoney,
        rewardReputation = s.rewardReputation,
        penaltyMoney     = s.penaltyMoney,
        deliveredQuantity = s.deliveredQuantity,
        isAccepted       = s.isAccepted,
        isCompleted      = s.isCompleted
    };
}