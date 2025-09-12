using System;
using System.Collections.Generic;
using UnityEngine;
using UnityUtility.SerializedDictionary;

public class IngredientManager : MonoBehaviour
{
    public IngredientType[] CurrentIngredients;

    [SerializeField] private SerializedDictionary<IngredientType, VirtualInput> m_ingredientsEquivalent;

    [NonSerialized] private Dictionary<IngredientType, Action<bool>> m_actions;

    private List<IngredientType> m_ingredients;

    private void Awake()
    {
        m_ingredients = new List<IngredientType>(3);
        m_actions = new Dictionary<IngredientType, Action<bool>>();
        foreach (KeyValuePair<IngredientType, VirtualInput> pair in m_ingredientsEquivalent)
        {
            Action<bool> action = (bool state) => OnIngredientUdated(pair.Key, state);
            pair.Value.OnInputValueChanged += action;
            m_actions.Add(pair.Key, action);
        }

        CurrentIngredients = GetCurrentIngredients();
    }

    private void OnDestroy()
    {
        foreach (KeyValuePair<IngredientType, VirtualInput> pair in m_ingredientsEquivalent)
        {
            pair.Value.OnInputValueChanged -= m_actions[pair.Key];
        }
    }

    private IngredientType[] GetCurrentIngredients()
    {
        IngredientType[] result = new IngredientType[3];

        for (int i = 0; i < 3; ++i)
        {
            result[i] = (i < m_ingredients.Count ? m_ingredients[i] : IngredientType.NOTHING);
        }

        return result;
    }

    private void OnIngredientUdated(IngredientType ingredient, bool insert)
    {
        string action = insert ? "Insert" : "Delete";
        Debug.Log($"{action} ingredient {ingredient}");
        if (insert)
        {
            if (m_ingredients.Count >= 3)
            {
                Debug.LogError("Ingredient List full !!!");
                return;
            }

            m_ingredients.Add(ingredient);
        }
        else
        {
            m_ingredients.Remove(ingredient);
        }

        CurrentIngredients = GetCurrentIngredients();

        Debug.LogWarning($"All Ingredients : [{CurrentIngredients[0]}, {CurrentIngredients[1]}, {CurrentIngredients[2]}]");
    }
}
