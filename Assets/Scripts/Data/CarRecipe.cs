using UnityEngine;

[CreateAssetMenu(fileName = "NewCarRecipe", menuName = "Car Company/Car Recipe")]
public class CarRecipe : ScriptableObject
{
    public int engineRequired = 1;
    public int bodyRequired = 1;
    public int wheelsRequired = 4;
    public int electronicsRequired = 2;
    public int assemblyCost = 20; // базовая стоимость сборки (без учёта запчастей)
}

