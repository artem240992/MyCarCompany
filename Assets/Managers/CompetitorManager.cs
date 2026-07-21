using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CompetitorManager : MonoBehaviour
{
    private List<Competitor> competitors = new List<Competitor>();
    private Coroutine competitorCoroutine;

    private string[] competitorNames = { "АвтоСтар", "ТехноТранс", "ЭкоДрайв", "СпортМотор", "ГородскойАвто" };

    private EconomyManager economy => CarCompanyManager.Instance.EconomyManager;
    private UIManager ui => CarCompanyManager.Instance.UIManager;
    private TechManager tech => CarCompanyManager.Instance.TechManager;
    private DemandManager demand => CarCompanyManager.Instance.DemandManager;
    private ProductionManager production => CarCompanyManager.Instance.ProductionManager;
    private ActionLogManager logManager => CarCompanyManager.Instance.ActionLogManager;

    public List<Competitor> Competitors => competitors;

    public void Initialize()
    {
        InitCompetitors();
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
        StartCompetitorAI();
    }

    private void InitCompetitors()
    {
        competitors.Clear();
        int difficulty = (int)CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty;
        int count = 3 + difficulty;
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
            comp.researchedTechs = new List<string>();

            // ---- Новые поля ----
            comp.marketingBudget = Random.Range(50f, 200f);
            comp.brandQuality = Random.Range(20f, 50f);
            string[] strategies = { "Aggressive", "Conservative", "Innovative" };
            comp.strategy = strategies[Random.Range(0, strategies.Length)];
            comp.activeCampaigns = new List<MarketingCampaign>();

            competitors.Add(comp);
        }
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }

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
            {
                comp.UpdateMonth(); // обновляем кампании
                RunCompetitorDecision(comp);
            }
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

    // ---- Основная логика AI ----
    private void RunCompetitorDecision(Competitor comp)
    {
        if (comp == null) return;

        float aggressionMod = 1f;
        if (CarCompanyManager.Instance.DifficultyManager.CurrentDifficulty == DifficultyManager.DifficultyLevel.Hard)
            aggressionMod = 1.5f;

        float decision = Random.value * aggressionMod;

        // 1. Обновление бренда и маркетингового бюджета
        if (comp.money > 100 && comp.marketingBudget < 200)
            comp.marketingBudget += 10f;

        // 2. Запуск рекламной кампании
        if (comp.availableCars.Count > 0 && comp.marketingBudget > 50 && decision < 0.15f * aggressionMod)
        {
            string carName = comp.availableCars[Random.Range(0, comp.availableCars.Count)].carName;
            float campaignCost = Random.Range(50f, 150f);
            if (comp.money >= campaignCost && comp.marketingBudget >= campaignCost)
            {
                comp.money -= campaignCost;
                comp.marketingBudget -= campaignCost;
                comp.LaunchCampaign(carName, campaignCost);
                comp.lastAdCampaign = Time.time;
                ui.ShowNotification($"{comp.companyName} запускает рекламу для {carName}!");
                logManager.AddLog(comp.companyName, "Рекламная кампания", true, $"Реклама {carName} за ${campaignCost}");
                return;
            }
        }

        // 3. Применение скидки
        if (comp.marketShare < 0.15f && comp.money > 200 && Time.time - comp.lastPriceCut > 20f && decision < 0.2f * aggressionMod)
        {
            float discount = Random.Range(0.05f, 0.15f);
            int months = Random.Range(2, 4);
            comp.ApplyDiscount(discount, months);
            ui.ShowNotification($"{comp.companyName} даёт скидку {discount * 100:F0}% на {months} мес.");
            logManager.AddLog(comp.companyName, "Скидка", true, $"Скидка {discount * 100:F0}%");
            return;
        }

        // 4. Исследование технологий (инновационные чаще)
        if (comp.researchLevel < 5 && comp.money > 400 && (comp.strategy == "Innovative" || decision < 0.3f))
        {
            List<Technology> availableTechs = tech.Technologies.Where(t => t != null && !t.isResearched && !comp.researchedTechs.Contains(t.techName)).ToList();
            if (availableTechs.Count > 0)
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
                    comp.brandQuality = Mathf.Min(100, comp.brandQuality + 2f);
                    ui.ShowNotification($"{comp.companyName} исследовал технологию '{chosen.techName}'!");
                    logManager.AddLog(comp.companyName, "Исследование", true, $"Технология {chosen.techName}");
                    return;
                }
            }
        }

        // 5. Улучшение завода
        if (comp.money > 300 && decision < 0.4f)
        {
            comp.money -= 150;
            comp.factoryLevel++;
            ui.ShowNotification($"{comp.companyName} модернизирует завод!");
            return;
        }

        // 6. Маркетинговая атака (только агрессивные)
        if (!comp.isAlly && comp.money > 150 && comp.strategy == "Aggressive" && decision < 0.2f * aggressionMod)
        {
            float attackSuccess = Random.value;
            float attackChance = 0.2f + (comp.marketingPower * 0.03f);
            if (attackSuccess < attackChance)
            {
                int repLoss = Random.Range(5, 15);
                economy.AddReputation(-repLoss);
                var playerCars = tech.AvailableCars;
                if (playerCars.Length > 0)
                {
                    string carName = playerCars[Random.Range(0, playerCars.Length)].carName;
                    float penalty = Random.Range(0.7f, 0.9f);
                    float duration = Random.Range(8f, 15f);
                    demand.ApplyDemandPenalty(carName, penalty, duration);
                    string desc = $"Спрос на {carName} упал на {(1f - penalty) * 100:F0}% на {duration:F0} сек.";
                    ui.ShowNotification($"{comp.companyName} атакует! {desc} Репутация -{repLoss}");
                    logManager.AddLog(comp.companyName, "Маркетинговая атака", true, desc);
                }
                else
                {
                    string desc = $"Репутация -{repLoss}";
                    ui.ShowNotification($"{comp.companyName} провёл маркетинговую атаку! {desc}");
                    logManager.AddLog(comp.companyName, "Маркетинговая атака", true, desc);
                }
            }
            comp.money -= 80;
            return;
        }

        // 7. Чёрный PR (только агрессивные)
        if (!comp.isAlly && comp.money > 300 && comp.strategy == "Aggressive" && decision < 0.1f * aggressionMod)
        {
            float prSuccess = Random.value;
            float prChance = 0.1f + (comp.marketingPower * 0.02f);
            if (prSuccess < prChance)
            {
                int repLoss = Random.Range(15, 30);
                economy.AddReputation(-repLoss);
                var playerCars = tech.AvailableCars;
                if (playerCars.Length > 0)
                {
                    string carName = playerCars[Random.Range(0, playerCars.Length)].carName;
                    float penalty = Random.Range(0.6f, 0.8f);
                    float duration = Random.Range(10f, 18f);
                    demand.ApplyDemandPenalty(carName, penalty, duration);
                    string desc = $"Спрос на {carName} упал на {(1f - penalty) * 100:F0}% на {duration:F0} сек.";
                    ui.ShowNotification($"{comp.companyName} запустил чёрный PR! {desc} Репутация -{repLoss}");
                    logManager.AddLog(comp.companyName, "Чёрный PR", true, desc);
                }
                else
                {
                    string desc = $"Репутация -{repLoss}";
                    ui.ShowNotification($"{comp.companyName} запустил чёрный PR! {desc}");
                    logManager.AddLog(comp.companyName, "Чёрный PR", true, desc);
                }
            }
            comp.money -= 150;
            return;
        }

        // 8. Кража технологии (не консервативные)
        if (!comp.isAlly && comp.researchLevel > 2 && comp.money > 200 && (comp.strategy != "Conservative") && decision < 0.12f * aggressionMod)
        {
            List<string> playerTechs = tech.Technologies.Where(t => t != null && t.isResearched).Select(t => t.techName).ToList();
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
                    string desc = $"Украдена технология {stolenTech}";
                    ui.ShowNotification($"{comp.companyName} украл технологию {stolenTech}!");
                    logManager.AddLog(comp.companyName, "Кража технологии", true, desc);
                }
                comp.money -= 100;
                return;
            }
        }

        // 9. Переманивание инженера
        if (!comp.isAlly && comp.money > 100 && economy.EngineerCount > 0 && decision < 0.15f * aggressionMod)
        {
            float poachSuccess = Random.value;
            float poachChance = 0.15f + (comp.marketingPower * 0.02f);
            if (poachSuccess < poachChance)
            {
                economy.LoseEngineer();
                comp.engineers++;
                string desc = $"Переманен 1 инженер";
                ui.ShowNotification($"{comp.companyName} переманил одного из ваших инженеров!");
                logManager.AddLog(comp.companyName, "Переманивание инженера", true, desc);
            }
            comp.money -= 50;
            return;
        }
    }

    // ---- Реакция на действия игрока ----
    public void OnPlayerStartsCampaign(string carName, float budget)
    {
        foreach (var comp in competitors)
        {
            if (comp.isAlly) continue;
            float responseChance = 0.3f;
            if (comp.strategy == "Aggressive") responseChance = 0.7f;
            else if (comp.strategy == "Conservative") responseChance = 0.2f;
            if (budget > 200) responseChance += 0.2f;

            if (Random.value < responseChance && comp.money > 100 && comp.availableCars.Count > 0)
            {
                string responseCar = comp.availableCars[Random.Range(0, comp.availableCars.Count)].carName;
                float responseBudget = budget * Random.Range(0.5f, 1.2f);
                if (comp.money >= responseBudget && comp.marketingBudget >= responseBudget * 0.5f)
                {
                    comp.money -= responseBudget;
                    comp.marketingBudget -= responseBudget * 0.5f;
                    comp.LaunchCampaign(responseCar, responseBudget);
                    comp.lastAdCampaign = Time.time;
                    ui.ShowNotification($"{comp.companyName} запускает ответную кампанию для {responseCar}!");
                    logManager.AddLog(comp.companyName, "Рекламная кампания (ответ)", true, $"Ответ на кампанию игрока");
                }
            }
        }
    }

    public void OnPlayerAppliesDiscount(float discount)
    {
        foreach (var comp in competitors)
        {
            if (comp.isAlly) continue;
            float responseChance = 0.4f;
            if (comp.strategy == "Aggressive") responseChance = 0.8f;
            else if (comp.strategy == "Conservative") responseChance = 0.1f;

            if (Random.value < responseChance && comp.money > 150 && Time.time - comp.lastPriceCut > 15f)
            {
                float responseDiscount = discount * Random.Range(0.7f, 1.3f);
                int months = Random.Range(2, 4);
                comp.ApplyDiscount(Mathf.Min(responseDiscount, 0.3f), months);
                ui.ShowNotification($"{comp.companyName} тоже снижает цены на {Mathf.Min(responseDiscount, 0.3f) * 100:F0}%!");
                logManager.AddLog(comp.companyName, "Скидка (ответ)", true, $"Ответная скидка");
            }
        }
    }

    // ---- Действия игрока (оставлены без изменений) ----
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
            economy.AddPassiveIncome(economy.PassiveIncome * 0.2f);
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
        economy.AddPassiveIncome(-economy.PassiveIncome * 0.2f);
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
            economy.AddEngineer();
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

    public void RefreshCompetitorsList()
    {
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }

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
            comp.marketingBudget = Random.Range(50f, 200f);
            comp.brandQuality = Random.Range(20f, 50f);
            string[] strategies = { "Aggressive", "Conservative", "Innovative" };
            comp.strategy = strategies[Random.Range(0, strategies.Length)];
            comp.activeCampaigns.Clear();
        }
        ui.RefreshCompetitorsList(competitors, economy.Reputation);
    }
}