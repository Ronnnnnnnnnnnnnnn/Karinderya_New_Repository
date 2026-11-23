using UnityEngine;

[CreateAssetMenu(menuName = "Kitchen/Recipe Book Catalog")]
public class RecipeBookCatalog : ScriptableObject
{
    public RecipeData[] recipes;

    static RecipeBookCatalog cached;

    public static RecipeBookCatalog Find()
    {
        if (cached != null)
            return cached;

        RecipeBookCatalog[] loaded =
            Resources.FindObjectsOfTypeAll<RecipeBookCatalog>();

        if (loaded.Length > 0)
        {
            cached = loaded[0];
            return cached;
        }

        foreach (Pot pot in Object.FindObjectsByType<Pot>(
            FindObjectsSortMode.None))
        {
            if (pot.recipeCatalog != null)
            {
                cached = pot.recipeCatalog;
                return cached;
            }
        }

        return null;
    }
}
