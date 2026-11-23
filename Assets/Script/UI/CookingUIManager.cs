using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject recipeBookPanel;
    public GameObject ingredientPanel;

    [Header("Recipe Book")]
    public Transform recipeListParent;

    [Header("Ingredient Panel")]
    public Text dishTitleText;
    public Text ingredientsListText;
    public Button startCookButton;
    public Button closeButton;
    public Button backButton;

    Pot activePot;
    RecipeData selectedRecipe;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        WireButtons();
        HideAll();
    }

    void WireButtons()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseAll);

        if (startCookButton != null)
            startCookButton.onClick.AddListener(OnStartCooking);

        if (backButton != null)
            backButton.onClick.AddListener(ShowRecipeBook);

        if (recipeBookPanel != null)
        {
            Transform closeTransform =
                recipeBookPanel.transform.Find("CloseButton");

            if (closeTransform != null)
            {
                Button recipeClose =
                    closeTransform.GetComponent<Button>();

                if (recipeClose != null)
                    recipeClose.onClick.AddListener(CloseAll);
            }
        }
    }

    public void OpenRecipeBook(Pot pot)
    {
        RecipeData[] recipes = pot != null ? pot.GetRecipes() : null;

        if (recipes == null || recipes.Length == 0)
        {
            NotificationManager.Instance?.ShowMessage("No recipes available.");
            return;
        }

        activePot = pot;
        selectedRecipe = null;
        PopulateRecipeList(recipes);
        recipeBookPanel.SetActive(true);
        ingredientPanel.SetActive(false);
        RefreshCursorState();
    }

    void PopulateRecipeList(RecipeData[] recipes)
    {
        foreach (Transform child in recipeListParent)
            Destroy(child.gameObject);

        foreach (RecipeData recipe in recipes)
        {
            if (recipe == null)
                continue;

            GameObject row = new GameObject(recipe.recipeName);
            row.transform.SetParent(recipeListParent, false);
            LayoutElement le = row.AddComponent<LayoutElement>();
            le.minHeight = 72;

            Image bg = row.AddComponent<Image>();
            bg.color = new Color(0.2f, 0.15f, 0.1f, 0.85f);

            Button btn = row.AddComponent<Button>();
            RecipeData captured = recipe;
            btn.onClick.AddListener(() => SelectRecipe(captured));

            string ingredientLine = FormatIngredientLine(recipe);
            bool unlocked =
                RecipeUnlockManager.Instance == null ||
                RecipeUnlockManager.Instance.IsUnlocked(recipe);

            string lockSuffix = unlocked
                ? ""
                : "  [LOCKED - " + recipe.unlockCost + "]";

            Text label = CreateText(row.transform,
                recipe.recipeName + lockSuffix + "\n" + ingredientLine,
                16, TextAnchor.MiddleLeft, Vector2.zero, new Vector2(360, 64));
            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = new Vector2(12, 4);
            labelRect.offsetMax = new Vector2(-12, -4);
        }
    }

    static string FormatIngredientLine(RecipeData recipe)
    {
        if (recipe.ingredients == null || recipe.ingredients.Length == 0)
            return "";

        var names = new List<string>();
        foreach (ItemData item in recipe.ingredients)
        {
            if (item != null)
                names.Add(GetDisplayName(item));
        }

        return string.Join(" / ", names);
    }

    static string GetDisplayName(ItemData item)
    {
        if (!string.IsNullOrEmpty(item.itemName))
            return item.itemName;

        return item.name.Replace(" Crop", "").Trim();
    }

    void SelectRecipe(RecipeData recipe)
    {
        selectedRecipe = recipe;
        ShowIngredientPanel(recipe);
    }

    void ShowIngredientPanel(RecipeData recipe)
    {
        recipeBookPanel.SetActive(false);
        ingredientPanel.SetActive(true);

        dishTitleText.text = recipe.recipeName;

        var sb = new StringBuilder();
        sb.AppendLine("Required ingredients:\n");

        bool hasAll =
            InventoryManager.Instance != null &&
            InventoryManager.Instance.HasItems(recipe.ingredients);

        bool unlocked =
            RecipeUnlockManager.Instance != null &&
            RecipeUnlockManager.Instance.IsUnlocked(recipe);

        foreach (ItemData ingredient in recipe.ingredients)
        {
            if (ingredient == null)
                continue;

            int have = InventoryManager.Instance.CountItem(ingredient);
            string name = GetDisplayName(ingredient);
            string status = have > 0 ? "OK" : "X";
            sb.AppendLine(status + " " + name + "  (" + have + " in bag)");
        }

        if (!unlocked)
        {
            sb.AppendLine();
            sb.AppendLine(
                "LOCKED - unlock at the " +
                recipe.recipeName +
                " buffet (" + recipe.unlockCost + ")"
            );
        }

        ingredientsListText.text = sb.ToString();
        startCookButton.interactable =
            hasAll &&
            unlocked &&
            activePot != null &&
            activePot.CanStartCooking();
    }

    void OnStartCooking()
    {
        if (activePot == null || selectedRecipe == null)
            return;

        if (!InventoryManager.Instance.HasItems(selectedRecipe.ingredients))
        {
            NotificationManager.Instance.ShowMessage("Missing ingredients!");
            ShowIngredientPanel(selectedRecipe);
            return;
        }

        if (!activePot.TryStartCooking(selectedRecipe))
        {

            return;
        }

        CloseAll();
    }

    void ShowRecipeBook()
    {
        if (activePot != null)
            OpenRecipeBook(activePot);
    }

    public void CloseAll()
    {
        HideAll();
        activePot = null;
        selectedRecipe = null;
        RefreshCursorState();
    }

    public bool IsOpen =>
        (recipeBookPanel != null && recipeBookPanel.activeSelf) ||
        (ingredientPanel != null && ingredientPanel.activeSelf);

    void HideAll()
    {
        if (recipeBookPanel != null)
            recipeBookPanel.SetActive(false);

        if (ingredientPanel != null)
            ingredientPanel.SetActive(false);
    }

    static void RefreshCursorState()
    {
        CameraMovement cameraMovement =
            Object.FindFirstObjectByType<CameraMovement>();
        if (cameraMovement != null)
            cameraMovement.RefreshCursorState();
    }

    static Text CreateText(
        Transform parent,
        string content,
        int fontSize,
        TextAnchor anchor,
        Vector2 pos,
        Vector2 size)
    {
        GameObject go = new GameObject("Text");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        Text text = go.AddComponent<Text>();
        text.text = content;
        text.fontSize = fontSize;
        text.alignment = anchor;
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return text;
    }
}
