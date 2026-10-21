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

            c.deliveredQuantity++;
            if (c.deliveredQuantity >= c.requiredQuantity)
                CompleteContract(c);

            OnContractsChanged?.Invoke();
            return; // одна машина закрывает один контракт
        }
    }

    // Пока принимает любую машину — расширим позже, когда будет выбор рецепта
    private bool CarMatchesContract(CarBlueprint car, ContractData c)
    {
        if (car == null || car.recipe == null) return false;
        return car.recipe.reliability >= 50f; // или другое условие
    }

    private void CompleteContract(ContractData c)
    {
        c.isCompleted = true;
        activeContracts.Remove(c);
        OnContractCompleted?.Invoke(c);
        OnContractsChanged?.Invoke();
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
                activeContracts.RemoveAt(i);
                OnContractFailed?.Invoke(c);
            }
        }
    }
}