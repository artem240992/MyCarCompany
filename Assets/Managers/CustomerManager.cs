using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class CustomerRecord
{
    public string customerId;
    public string carName;
    public int carLevel;
    public string trimName;
    public int monthsOwned;
    public bool hasServiceContract;
    public int serviceMonthsLeft;
    public double monthlyServiceFee;
}

[System.Serializable]
public class TradeInOffer
{
    public string customerId;
    public string oldCarName;
    public int oldCarLevel;
    public string newTrimName;
    public float discountPercent;
    public int expiresInMonths;
}

public class CustomerManager : MonoBehaviour
{
    public static CustomerManager Instance { get; private set; }

    public List<CustomerRecord> customers = new List<CustomerRecord>();
    public List<TradeInOffer> tradeInOffers = new List<TradeInOffer>();

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

    // Вызывается из WarehouseManager при продаже машины
    public void RegisterSale(CarBlueprint car, string trimName, int count)
    {
        for (int i = 0; i < count; i++)
        {
            if (Random.value < 0.8f) // 80% покупателей становятся нашими клиентами
            {
                customers.Add(new CustomerRecord
                {
                    customerId = System.Guid.NewGuid().ToString(),
                    carName = car.carName,
                    carLevel = car.currentLevel,
                    trimName = trimName,
                    monthsOwned = 0,
                    hasServiceContract = false,
                    serviceMonthsLeft = 0,
                    monthlyServiceFee = 0
                });
            }
        }
    }

    private void OnMonthChanged()
    {
        foreach (var c in customers) c.monthsOwned++;

        // Генерируем предложения трейд-ин
        foreach (var c in customers.ToList())
        {
            if (Random.value < 0.05f && c.monthsOwned >= 3)
            {
                var car = FindCarByName(c.carName);
                if (car != null)
                {
                    tradeInOffers.Add(new TradeInOffer
                    {
                        customerId = c.customerId,
                        oldCarName = c.carName,
                        oldCarLevel = c.carLevel,
                        newTrimName = car.recipe?.availableTrims?.FirstOrDefault()?.trimName ?? "",
                        discountPercent = Random.Range(0.10f, 0.20f),
                        expiresInMonths = 2
                    });
                }
            }
        }

        // Таймер предложений
        for (int i = tradeInOffers.Count - 1; i >= 0; i--)
        {
            tradeInOffers[i].expiresInMonths--;
            if (tradeInOffers[i].expiresInMonths <= 0) tradeInOffers.RemoveAt(i);
        }

        // Доход от сервисных контрактов
        double totalIncome = 0;
        foreach (var c in customers)
        {
            if (c.hasServiceContract && c.serviceMonthsLeft > 0)
            {
                totalIncome += c.monthlyServiceFee;
                c.serviceMonthsLeft--;
            }
        }
        if (totalIncome > 0)
        {
            CarCompanyManager.Instance.EconomyManager.AddMoney(totalIncome);
            UIManager.Instance?.ShowNotification($"💰 Доход от сервиса: ${totalIncome:F0}");
        }
    }

    public bool AcceptTradeIn(TradeInOffer offer)
    {
        if (offer == null || !tradeInOffers.Contains(offer)) return false;
        var car = FindCarByName(offer.oldCarName);
        if (car == null) return false;

        int price = car.GetModifiedPrice(CarCompanyManager.Instance.EconomyManager.TotalPriceModifier);
        int discountedPrice = Mathf.RoundToInt(price * (1f - offer.discountPercent));

        if (!CarCompanyManager.Instance.EconomyManager.SpendMoney(discountedPrice))
        {
            UIManager.Instance?.ShowNotification("Не хватает денег для трейд-ина!");
            return false;
        }

        // Возвращаем старую машину на склад (помечаем как б/у комплектацию)
        WarehouseManager.Instance.AddCar(car, "б/у", "АКПП", 1);
        
        tradeInOffers.Remove(offer);
        customers.RemoveAll(c => c.customerId == offer.customerId);
        UIManager.Instance?.ShowNotification($"✅ Трейд-ин принят! Скидка {offer.discountPercent * 100:F0}%.");
        return true;
    }

    public void OfferServiceContract(CustomerRecord customer, double fee, int months)
    {
        if (customer == null || customer.hasServiceContract) return;
        customer.hasServiceContract = true;
        customer.monthlyServiceFee = fee;
        customer.serviceMonthsLeft = months;
    }

    private CarBlueprint FindCarByName(string name)
    {
        return CarCompanyManager.Instance.TechManager.AvailableCars?.FirstOrDefault(c => c.carName == name);
    }

    public void FillSaveData(SaveData data)
    {
        data.customers = customers;
        data.tradeInOffers = tradeInOffers;
    }

    public void LoadFromSave(SaveData data)
    {
        if (data.customers != null) customers = data.customers;
        if (data.tradeInOffers != null) tradeInOffers = data.tradeInOffers;
    }
}