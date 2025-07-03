using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FoodDatabase", menuName = "Database/FoodDatabase")]
public class FoodDatabaseSO : ScriptableObject
{
    public List<FoodDataSO> allFoods;

    public FoodDataSO GetFoodByName(string name)
    {
        return allFoods.Find(food => food.foodName == name);
    }
}
