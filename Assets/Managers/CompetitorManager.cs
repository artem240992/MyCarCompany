using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CompetitorManager : MonoBehaviour
{
    private List<Competitor> competitors = new List<Competitor>();
    private Coroutine competitorCoroutine;

    private string[] competitorNames = { "АвтоСтар", "ТехноТранс", "ЭкоДрайв", "СпортМотор", "ГородскойАвто" };

    // Ссылки
    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;
    private ProductionManager production => CarCompanyManager.Instance.ProductionManager;

    public List<Competitor> Competitors => competitors;

    public void Initialize()
    {
        InitCompetitors();
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }

    private void InitCompetitors()
    {
        competitors.Clear();
        int count = 3 + (int)CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
        for (int i = 0; i < count && i < competitorNames.Length; i++)
        {
            Competitor comp = new Competitor();
            comp.companyName = competitorNames[i];
            comp.money = Random.Range(200f, 500f);
            comp.factoryLevel = Random.Range(1, 3);
            comp.researchLevel = Random.Range(1, 3);
            comp.reputation = Random.Range(30, 70);
            comp.priceMultiplier = Random.Range(0.8f, 1.2f);
            comp.marketShare = Random.Range(0.05f, 0.15f);
            int carCount = Mathf.Min(Random.Range(1, 3), tech.AvailableCars.Length);
            for (int j = 0; j < carCount; j++)
                if (tech.AvailableCars[j] != null) comp.availableCars.Add(tech.AvailableCars[j]);
            comp.engineers = Random.Range(1, 5);
            comp.espionageLevel = Random.Range(1, 6);
            comp.marketingPower = Random.Range(1, 6);
            comp.loyalty = Random.Range(50, 100);
            comp.isAlly = false;
            comp.stolenTechs = new List<string>();
            competitors.Add(comp);
        }
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }

    // ---- AI ----
    public void StartCompetitorAI()
    {
        if (competitorCoroutine != null) StopCoroutine(competitorCoroutine);
        competitorCoroutine = StartCoroutine(CompetitorAILoop());
    }

    private IEnumerator CompetitorAILoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(GetDecisionInterval());
            foreach (var comp in competitors)
                RunCompetitorDecision(comp);
            demand.UpdateDemand();
            ui.RefreshCompetitorsList(competitors, economy.Reputation);
        }
    }

    private float GetDecisionInterval()
    {
        var diff = CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
        switch (diff)
        {
            case DifficultyManager.DifficultyLevel.Easy:   return 10f;
            case DifficultyManager.DifficultyLevel.Normal: return 7f;
            case DifficultyManager.DifficultyLevel.Hard:   return 5f;
            default: return 8f;
        }
    }

    // ---- Полная логика AI конкурента ----
    private void RunCompetitorDecision(Competitor comp)
    {
        if (comp == null) return;
        float decision = Random.value;

        // 1. Если у конкурента мало денег – поднимает цены
        if (comp.money < 200 && decision < 0.4f)
        {
            comp.priceMultiplier = Mathf.Min(comp.priceMultiplier + 0.1f, 1.5f);
            ui.ShowNotification($"{comp.companyName} поднял цены!");
            return;
        }

        // 2. Если доля рынка мала – снижает цены
        if (comp.marketShare < 0.2f && comp.money > 300 && decision < 0.5f)
        {
            comp.priceMultiplier = Mathf.Max(comp.priceMultiplier - 0.05f, 0.6f);
            comp.reputation += 10;
            comp.money -= 50;
            ui.ShowNotification($"{comp.companyName} снизил цены для захвата рынка!");
            return;
        }

        // 3. Исследование технологий
        if (comp.researchLevel < 5 && comp.money > 400 && decision < 0.6f)
        {
            List<Technology> availableTechs = tech.Technologies.Where(t => t != null && !t.isResearched && !comp.researchedTechs.Contains(t.techName) && t.priceModifier > 1f).ToList();
            if (availableTechs.Count > 0 && comp.money > 300)
            {
                Technology chosen = availableTechs[Random.Range(0, availableTechs.Count)];
                int cost = Mathf.RoundToInt(chosen.researchCost * 0.8f);
                if (comp.money >= cost)
                {
                    comp.money -= cost;
                    comp.researchedTechs.Add(chosen.techName);
                    comp.researchLevel++;
                    comp.reputation += 5;
                    comp.marketShare += 0.02f;
                    ui.ShowNotification($"{comp.companyName} исследовал технологию '{chosen.techName}'!");
                    return;
                }
            }
            if (comp.money > 300)
            {
                comp.money -= 150;
                comp.factoryLevel++;
                ui.ShowNotification($"{comp.companyName} модернизирует завод!");
                return;
            }
        }

        // 4. Модернизация завода
        if (comp.money > 300 && decision < 0.7f)
        {
            comp.money -= 150;
            comp.factoryLevel++;
            ui.ShowNotification($"{comp.companyName} модернизирует завод!");
            return;
        }

        // 5. Маркетинговая атака на игрока
        float actionChance = Random.value;
        if (!comp.isAlly && comp.money > 150 && actionChance < 0.15f)
        {
            float attackSuccess = Random.value;
            float attackChance = 0.2f + (comp.marketingPower * 0.03f);
            if (attackSuccess < attackChance)
            {
                int repLoss = Random.Range(5, 15);
                economy.AddReputation(-repLoss);
                // Штрафуем спрос на случайную машину игрока
                var playerCars = tech.AvailableCars;
                if (playerCars.Length > 0)
                {
                    string carName = playerCars[Random.Range(0, playerCars.Length)].carName;
                    float penalty = Random.Range(0.7f, 0.9f); // -10..-30%
                    float duration = Random.Range(8f, 15f);
                    demand.ApplyDemandPenalty(carName, penalty, duration);
                    ui.ShowNotification($"{comp.companyName} атакует! Спрос на {carName} упал на {(1f - penalty) * 100:F0}% на {duration:F0} сек.");
                }
                else
                {
                    ui.ShowNotification($"{comp.companyName} провёл маркетинговую атаку! Ваша репутация -{repLoss}");
                }
            }
            comp.money -= 80;
            return;
        }

        // 6. Чёрный PR
        if (!comp.isAlly && comp.money > 300 && actionChance < 0.08f)
        {
            float prSuccess = Random.value;
            float prChance = 0.1f + (comp.marketingPower * 0.02f);
            if (prSuccess < prChance)
            {
                int repLoss = Random.Range(15, 30);
                economy.AddReputation(-repLoss);
                // Штрафуем спрос на случайную машину (сильнее)
                var playerCars = tech.AvailableCars;
                if (playerCars.Length > 0)
                {
                    string carName = playerCars[Random.Range(0, playerCars.Length)].carName;
                    float penalty = Random.Range(0.6f, 0.8f); // -20..-40%
                    float duration = Random.Range(10f, 18f);
                    demand.ApplyDemandPenalty(carName, penalty, duration);
                    ui.ShowNotification($"{comp.companyName} запустил чёрный PR! Спрос на {carName} упал на {(1f - penalty) * 100:F0}% на {duration:F0} сек. Репутация -{repLoss}");
                }
                else
                {
                    ui.ShowNotification($"{comp.companyName} запустил чёрный PR! Ваша репутация -{repLoss}");
                }
            }
            comp.money -= 150;
            return;
        }

        // 7. Кража технологии у игрока
        if (!comp.isAlly && comp.researchLevel > 2 && comp.money > 200 && actionChance < 0.10f)
        {
            List<string> playerTechs = new List<string>();
            foreach (var t in tech.Technologies)
                if (t != null && t.isResearched) playerTechs.Add(t.techName);
            if (playerTechs.Count > 0)
            {
                float stealSuccess = Random.value;
                float stealChance = 0.1f + (comp.espionageLevel * 0.02f);
                if (stealSuccess < stealChance)
                {
                    string stolenTech = playerTechs[Random.Range(0, playerTechs.Count)];
                    if (!comp.researchedTechs.Contains(stolenTech))
                        comp.researchedTechs.Add(stolenTech);
                    comp.researchLevel++;
                    ui.ShowNotification($"{comp.companyName} украл технологию {stolenTech}!");
                }
                comp.money -= 100;
                return;
            }
        }

        // 8. Переманивание инженера у игрока
        if (!comp.isAlly && comp.money > 100 && economy.EngineerCount > 0 && actionChance < 0.12f)
        {
            float poachSuccess = Random.value;
            float poachChance = 0.15f + (comp.marketingPower * 0.02f);
            if (poachSuccess < poachChance)
            {
                // Уменьшаем инженеров у игрока
                // Придётся использовать рефлексию или добавить метод в EconomyManager
                // Добавим метод LoseEngineer в EconomyManager (см. ниже)
                // Пока просто вызовем через рефлексию, но лучше добавить публичный метод
                // Временно используем прямой доступ к полю (если нет метода)
                // Вместо этого вызовем новый метод
                CarCompanyManager.Instance.EconomyManager.LoseEngineer(); // нужно добавить
                comp.engineers++;
                ui.ShowNotification($"{comp.companyName} переманил одного из ваших инженеров!");
            }
            comp.money -= 50;
            return;
        }
    }

    // ---- Действия игрока ----
    public void PerformMarketingAttack(Competitor target)
    {
        if (target == null || target.isAlly) { ui.ShowNotification("Нельзя атаковать союзника!"); return; }
        int cost = 100;
        if (!economy.SpendMoney(cost)) { ui.ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }
        float success = Random.Range(0f, 1f);
        float chance = 0.3f + (economy.EngineerCount * 0.02f);
        if (success < chance)
        {
            int repLoss = Random.Range(10, 25);
            target.reputation = Mathf.Max(0, target.reputation - repLoss);
            target.marketShare *= (1f - 0.05f);
            ui.ShowNotification($"Маркетинговая атака на {target.companyName} удалась! Репутация -{repLoss}");
            demand.UpdateDemand();
        }
        else ui.ShowNotification($"Маркетинговая атака на {target.companyName} провалилась.");
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    public void PerformBlackPR(Competitor target)
    {
        if (target == null || target.isAlly) { ui.ShowNotification("Нельзя атаковать союзника!"); return; }
        int cost = 200;
        if (!economy.SpendMoney(cost)) { ui.ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }
        float success = Random.Range(0f, 1f);
        float chance = 0.2f + (economy.EngineerCount * 0.01f);
        if (success < chance)
        {
            int repLoss = Random.Range(20, 40);
            target.reputation = Mathf.Max(0, target.reputation - repLoss);
            target.marketShare *= (1f - 0.1f);
            ui.ShowNotification($"Чёрный PR против {target.companyName} удался! Репутация -{repLoss}");
            if (Random.value < 0.3f)
            {
                int retaliation = Random.Range(10, 20);
                economy.AddReputation(-retaliation);
                ui.ShowNotification($"{target.companyName} ответил! Ваша репутация -{retaliation}");
            }
            demand.UpdateDemand();
        }
        else
        {
            ui.ShowNotification($"Чёрный PR против {target.companyName} провалился.");
            if (Random.value < 0.2f)
            {
                int retaliation = Random.Range(5, 15);
                economy.AddReputation(-retaliation);
                ui.ShowNotification($"{target.companyName} заметил и ответил! Ваша репутация -{retaliation}");
            }
        }
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    public void ProposeAlliance(Competitor target)
    {
        if (target == null || target.isAlly) { ui.ShowNotification("Уже в союзе!"); return; }
        int cost = 150;
        if (!economy.SpendMoney(cost)) { ui.ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }
        float chance = 0.4f + (economy.Reputation / 200f);
        if (Random.value < chance)
        {
            target.isAlly = true;
            economy.AddPassiveIncome(economy.PassiveIncome * 0.2f); // +20% к доходу
            target.money *= 1.1f;
            ui.ShowNotification($"Союз с {target.companyName} заключён! Доход увеличен на 20%.");
        }
        else ui.ShowNotification($"{target.companyName} отклонил предложение о союзе.");
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    public void BreakAlliance(Competitor target)
    {
        if (target == null || !target.isAlly) { ui.ShowNotification("Нет союза для разрыва."); return; }
        target.isAlly = false;
        economy.AddPassiveIncome(-economy.PassiveIncome * 0.2f); // убираем бонус
        ui.ShowNotification($"Союз с {target.companyName} разорван.");
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    public void StealTechnology(Competitor target)
    {
        if (target == null || target.isAlly) { ui.ShowNotification("Нельзя красть у союзника!"); return; }
        if (target.researchedTechs.Count == 0) { ui.ShowNotification($"У {target.companyName} нет технологий для кражи."); return; }
        int cost = 250;
        if (!economy.SpendMoney(cost)) { ui.ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }
        float success = Random.Range(0f, 1f);
        float chance = 0.2f + (economy.EngineerCount * 0.015f) + (target.espionageLevel * 0.01f);
        if (success < chance)
        {
            string techToSteal = target.researchedTechs[Random.Range(0, target.researchedTechs.Count)];
            Technology techToAdd = tech.Technologies.FirstOrDefault(t => t != null && t.techName == techToSteal);
            if (techToAdd != null && !techToAdd.isResearched)
            {
                techToAdd.isResearched = true;
                economy.RecalculateModifiers(tech.Technologies);
                ui.UpdateCarCards();
                ui.ShowNotification($"Украдена технология {techToSteal} у {target.companyName}!");
            }
            else
            {
                double bonus = 500;
                economy.AddMoney(bonus);
                ui.ShowNotification($"Кража не дала технологии, но вы нашли {bonus} денег.");
            }
            target.reputation = Mathf.Max(0, target.reputation - 10);
        }
        else
        {
            ui.ShowNotification($"Попытка кражи технологии у {target.companyName} провалилась.");
            if (Random.value < 0.2f)
            {
                int repLoss = Random.Range(5, 15);
                economy.AddReputation(-repLoss);
                ui.ShowNotification($"{target.companyName} обнаружил кражу! Ваша репутация -{repLoss}");
            }
        }
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    public void PoachEngineer(Competitor target)
    {
        if (target == null || target.isAlly) { ui.ShowNotification("Нельзя переманивать инженеров у союзника!"); return; }
        if (target.engineers <= 0) { ui.ShowNotification($"У {target.companyName} нет инженеров для переманивания."); return; }
        int cost = 150;
        if (!economy.SpendMoney(cost)) { ui.ShowNotification($"Не хватает денег! Нужно ${cost}"); return; }
        float success = Random.Range(0f, 1f);
        float chance = 0.3f + (economy.Reputation * 0.002f) + (target.loyalty * 0.001f);
        if (success < chance)
        {
            target.engineers--;
            // Добавляем инженера игроку
            // Нужен метод в EconomyManager, например, AddEngineer()
            economy.AddEngineer(); // добавим
            ui.ShowNotification($"Инженер переманен у {target.companyName}! Теперь у вас {economy.EngineerCount} инженеров.");
        }
        else
        {
            ui.ShowNotification($"Не удалось переманить инженера у {target.companyName}.");
            if (Random.value < 0.2f)
            {
                int repLoss = Random.Range(5, 15);
                economy.AddReputation(-repLoss);
                ui.ShowNotification($"{target.companyName} разгневан! Ваша репутация -{repLoss}");
            }
        }
        ui.UpdateMoneyLabels();
        RefreshCompetitorsList();
    }

    // ---- Обновление списка ----
    public void RefreshCompetitorsList()
    {
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }

    // ---- Сброс ----
    public void ResetCompetitors()
    {
        foreach (var comp in competitors)
        {
            comp.isAlly = false;
            comp.engineers = Random.Range(1, 5);
            comp.espionageLevel = Random.Range(1, 6);
            comp.marketingPower = Random.Range(1, 6);
            comp.loyalty = Random.Range(50, 100);
            comp.stolenTechs.Clear();
            comp.money = Random.Range(200f, 500f);
            comp.factoryLevel = Random.Range(1, 3);
            comp.researchLevel = Random.Range(1, 3);
            comp.reputation = Random.Range(30, 70);
            comp.priceMultiplier = Random.Range(0.8f, 1.2f);
            comp.marketShare = Random.Range(0.05f, 0.15f);
            comp.availableCars.Clear();
            int carCount = Mathf.Min(Random.Range(1, 3), tech.AvailableCars.Length);
            for (int j = 0; j < carCount; j++)
                if (tech.AvailableCars[j] != null) comp.availableCars.Add(tech.AvailableCars[j]);
        }
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }
}