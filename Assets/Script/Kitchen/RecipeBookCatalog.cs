using UnityEngine;

[CreateAssetMenu(menuName = "Kitchen/Recipe Book Catalog")]
public class RecipeBookCatalog : ScriptableObject
{
    public RecipeData[] recipes;

    static RecipeBookCatalog cached;

    // FindObjectOfType can't find ScriptableObject assets reliably,
    // so look through loaded objects and then through the Pots.
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

        foreach (Pot pot in FindObjectsOfType<Pot>())
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
