using UnityEngine;

[CreateAssetMenu(fileName = "NewCarRecipe", menuName = "Car Company/Car Recipe")]
public class CarRecipe : ScriptableObject
{
    public int engineRequired;
    public int bodyRequired;
    public int wheelsRequired;
    public int electronicsRequired;
    public int assemblyCost;
    // ---- Индивидуальные цены деталей (если не заданы, берутся из PartsMarketManager) ----
    public float enginePrice = -1f;  // -1 означает использовать глобальную цену
    public float bodyPrice = -1f;
    public float wheelsPrice = -1f;
    public float electronicsPrice = -1f;
}

