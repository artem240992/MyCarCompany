using UnityEngine;
using System.Collections.Generic;

public static class CarConfigurationCalculator
{
    public static float GetTotalOptionCost(CarTrim trim, List<CarOption> selectedOptions)
    {
        float cost = 0f;
        foreach (var opt in selectedOptions)
            cost += opt.additionalCost;
        return cost;
    }

    public static float GetFinalAssemblyCost(CarRecipe recipe, CarTrim trim, List<CarOption> selectedOptions)
    {
        float baseCost = recipe.GetModifiedAssemblyCost();
        float optionCost = GetTotalOptionCost(trim, selectedOptions);
        return (baseCost + optionCost) * trim.priceMultiplier;
    }

    public static float GetDemandMultiplier(List<CarOption> selectedOptions)
    {
        float safety = 0f, comfort = 0f, tech = 0f, reliability = 0f;
        foreach (var opt in selectedOptions)
        {
            safety += opt.safetyBonus;
            comfort += opt.comfortBonus;
            tech += opt.techBonus;
            reliability += opt.reliabilityBonus;
        }
        // Простая формула: каждая опция даёт небольшой бонус к спросу
        float totalBonus = (safety + comfort + tech + reliability) * 0.01f;
        return 1f + Mathf.Clamp(totalBonus, 0f, 0.5f); // максимум +50% к спросу
    }
}