using UnityEngine;

/// <summary>
/// Represents a recipe for a potion
/// </summary>
[CreateAssetMenu(fileName = "PotionRecipe", menuName = "Potion/PotionRecipe")]
public class PotionRecipe : ScriptableObject
{
	public int ID;
	public string potionName;
	public IngredientType[] ingredients;

	/// <summary>
	/// Checks if the potion can be made with the given ingredients
	/// </summary>
	/// <param name="ingredients">The ingredients to test</param>
	/// <returns>True if the potion can be created using the specified ingredients</returns>
	public bool CanMakeWithIngredients(IngredientType[] testIngredients)
	{
		bool good = false;
		foreach (IngredientType ingredient in ingredients)
		{
			good = false;
			foreach (IngredientType test in testIngredients)
			{
				if (test == ingredient) good = true;
			}
			if (!good) return false;
		}
		return true;
	}
}


public enum IngredientType
{
	NOTHING,
	TEST1,
	TEST2,
	TEST3,
	TEST4,

	TEST5,

	TEST6
}
