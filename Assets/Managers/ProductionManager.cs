using UnityEngine;
using System.Collections;
using System.Linq;

public class ProductionManager : MonoBehaviour
{
    private GameObject carDisplay;
    private Transform startPoint;
    private Transform endPoint;
    private int productionCount = 1;
    private bool isProductionInProgress = false;
    private GameObject currentCarInstance;

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;

    public void Initialize(GameObject carDisplayPrefab, Transform start, Transform end)
    {
        if (carDisplayPrefab == null)
        {
            GameObject container = new GameObject("CarDisplay");
            container.transform.parent = CarCompanyManager.Instance.transform;
            carDisplay = container;
        }
        else
        {
            carDisplay = Instantiate(carDisplayPrefab, CarCompanyManager.Instance.transform);
        }

        if (start == null)
        {
            GameObject s = new GameObject("StartPoint");
            s.transform.parent = carDisplay.transform;
            s.transform.localPosition = new Vector3(-3, 0, 0);
            startPoint = s.transform;
        }
        else
        {
            startPoint = start;
        }

        if (end == null)
        {
            GameObject e = new GameObject("EndPoint");
            e.transform.parent = carDisplay.transform;
            e.transform.localPosition = new Vector3(3, 0, 0);
            endPoint = e.transform;
        }
        else
        {
            endPoint = end;
        }

        productionCount = 1;
        ui.UpdateCountLabel(productionCount);
        UpdateButtons();
    }

    public void DecreaseCount()
    {
        if (productionCount > 1) productionCount--;
        ui.UpdateCountLabel(productionCount);
        UpdateButtons();
    }

    public void IncreaseCount()
    {
        int max = tech.IsBulkProductionUnlocked() ? 10 : 1;
        if (productionCount >= max)
        {
            if (max == 1)
                ui.ShowNotification($"Для производства более 1 машины за раз изучите технологию '{CarCompanyManager.Instance.BulkProductionTechName}'");
            else
                ui.ShowNotification("Достигнут максимум (10)");
            return;
        }
        productionCount++;
        ui.UpdateCountLabel(productionCount);
        UpdateButtons();
    }

    public void UpdateButtons()
    {
        int max = tech.IsBulkProductionUnlocked() ? 10 : 1;
        ui.UpdateProductionButtons(productionCount < max);
    }

    public void ClampProductionCount()
    {
        int max = tech.IsBulkProductionUnlocked() ? 10 : 1;
        if (productionCount > max) productionCount = max;
        ui.UpdateCountLabel(productionCount);
        UpdateButtons();
    }

    public void ProduceBasicCar()
    {
        if (isProductionInProgress) { ui.ShowNotification("Производство занято!"); return; }
        var availableCars = tech.AvailableCars;
        if (availableCars == null || availableCars.Length == 0) { ui.ShowNotification("Нет доступных машин!"); return; }
        CarBlueprint car = availableCars[0];
        if (car == null) return;

        // ---- РАСЧЁТ С УЧЁТОМ НАЛОГОВ И ИНФЛЯЦИИ ----
        int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
        int modCost = Mathf.RoundToInt(car.GetProductionCostWithLevel() * economy.CostMultiplier);
        double profitBeforeTax = (modPrice - modCost);
        float taxRate = economy.GetTaxRate(car);
        double profitAfterTax = profitBeforeTax * (1f - taxRate);

        economy.AddMoney(profitAfterTax);
        ui.ShowNotification($"Произведена машина {car.GetDisplayName()}. Прибыль: ${profitAfterTax:F0} (налог {taxRate:P0})");
        SpawnCar(car);
    }

    public void ProduceSpecificCar(CarBlueprint car)
    {
        if (isProductionInProgress) { ui.ShowNotification("Производство занято!"); return; }
        if (car == null) return;

        int modPrice = car.GetModifiedPrice(economy.TotalPriceModifier);
        int modCost = Mathf.RoundToInt(car.GetProductionCostWithLevel() * economy.CostMultiplier);
        double profitBeforeTax = (modPrice - modCost) * productionCount;
        float taxRate = economy.GetTaxRate(car);
        double profitAfterTax = profitBeforeTax * (1f - taxRate);

        economy.AddMoney(profitAfterTax);
        ui.ShowNotification($"Произведено {productionCount} шт. {car.GetDisplayName()}. Прибыль: ${profitAfterTax:F0} (налог {taxRate:P0})");
        SpawnCar(car);
    }

    private void SpawnCar(CarBlueprint car)
    {
        if (car == null) { ui.ShowNotification("Ошибка: нет данных о машине!"); return; }

        GameObject prefabToSpawn = null;
        if (car.levelPrefabs != null && car.levelPrefabs.Length > 0)
        {
            int level = Mathf.Clamp(car.currentLevel, 0, car.levelPrefabs.Length - 1);
            prefabToSpawn = car.levelPrefabs[level];
        }
        if (prefabToSpawn == null) prefabToSpawn = car.carPrefab;
        if (prefabToSpawn == null)
        {
            string prefabPath = $"Prefabs/{car.carName}";
            prefabToSpawn = Resources.Load<GameObject>(prefabPath);
        }
        if (prefabToSpawn == null)
        {
            Debug.LogWarning($"У машины {car.carName} нет префаба! Создаю временный куб.");
            prefabToSpawn = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Destroy(prefabToSpawn.GetComponent<Collider>());
            prefabToSpawn.name = "TempCar";
        }

        if (isProductionInProgress) { ui.ShowNotification("Производство занято!"); return; }

        if (currentCarInstance != null) Destroy(currentCarInstance);
        isProductionInProgress = true;

        currentCarInstance = Instantiate(prefabToSpawn, carDisplay.transform);
        currentCarInstance.transform.localPosition = Vector3.zero;
        currentCarInstance.transform.localRotation = Quaternion.identity;
        currentCarInstance.transform.localScale = Vector3.one;

        // ---- ПРИМЕНЯЕМ ЦВЕТ И ТОНИРОВКУ (уже есть) ----
        ApplyCarColor(currentCarInstance, car);

        CarAnimation anim = currentCarInstance.GetComponent<CarAnimation>();
        if (anim == null) anim = currentCarInstance.AddComponent<CarAnimation>();
        anim.startPoint = startPoint;
        anim.endPoint = endPoint;
        anim.duration = 2f;
        anim.OnProductionComplete += () => { isProductionInProgress = false; };
        anim.PlayProduction();
    }

    // ---- МЕТОД ПРИМЕНЕНИЯ ЦВЕТА И ТОНИРОВКИ (расширенный для версии 2.1.30) ----
    private void ApplyCarColor(GameObject carObject, CarBlueprint car)
    {
        if (carObject == null || car == null) return;

        var renderers = carObject.GetComponentsInChildren<Renderer>();
        foreach (var renderer in renderers)
        {
            if (renderer.material == null) continue;

            // Клонируем материал, чтобы не менять исходный префаб
            Material mat = renderer.material;
            mat.color = car.bodyColor;

            // Тонировка стёкол (если включена)
            if (car.hasTint && mat.shader.name.Contains("Standard"))
            {
                Color tintColor = new Color(0.1f, 0.1f, 0.1f, 0.7f);
                mat.color = Color.Lerp(mat.color, tintColor, 0.5f);
            }
        }
    }

    public int ProductionCount => productionCount;

    public void SetProductionCount(int count)
    {
        productionCount = count;
        ClampProductionCount();
    }
}