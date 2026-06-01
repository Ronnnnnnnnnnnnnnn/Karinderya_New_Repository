using UnityEngine;

public class RecipeBookUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject recipeBookPanel;

    void Start()
    {
        if(recipeBookPanel != null)
        {
            recipeBookPanel.SetActive(false);
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            ToggleRecipeBook();
        }
    }

    void ToggleRecipeBook()
    {
        if(recipeBookPanel == null)
        {
            Debug.LogError("Recipe Book Panel Missing!");
            return;
        }

        recipeBookPanel.SetActive(
            !recipeBookPanel.activeSelf
        );
    }
}
