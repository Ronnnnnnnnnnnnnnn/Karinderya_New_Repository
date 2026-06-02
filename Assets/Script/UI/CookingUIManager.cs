using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class CookingUIManager : MonoBehaviour
{
    public static CookingUIManager Instance { get; private set; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AutoCreate()
    {
        EnsureInstance();
    }

    public static CookingUIManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject("CookingUIManager");
        return go.AddComponent<CookingUIManager>();
    }

    [Header("Panels (optional — built at runtime if empty)")]
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
        EnsurePanelsExist();
        HideAll();
    }

    void EnsurePanelsExist()
    {
        if (recipeBookPanel != null && ingredientPanel != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            GameObject canvasObj = new GameObject("CookingCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        if (recipeBookPanel == null)
            recipeBookPanel = BuildRecipeBookPanel(canvas.transform);

        if (ingredientPanel == null)
            ingredientPanel = BuildIngredientPanel(canvas.transform);
    }

    GameObject BuildRecipeBookPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "RecipeBookPanel", new Vector2(420, 480));

        GameObject scrollObj = new GameObject("Scroll");
        scrollObj.transform.SetParent(panel.transform, false);
        RectTransform scrollRect = scrollObj.AddComponent<RectTransform>();
        scrollRect.anchorMin = new Vector2(0.05f, 0.12f);
        scrollRect.anchorMax = new Vector2(0.95f, 0.82f);
        scrollRect.offsetMin = Vector2.zero;
        scrollRect.offsetMax = Vector2.zero;

        ScrollRect scroll = scrollObj.AddComponent<ScrollRect>();
        GameObject content = new GameObject("Content");
        content.transform.SetParent(scrollObj.transform, false);
        RectTransform contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0.5f, 1);
        contentRect.sizeDelta = new Vector2(0, 0);
        VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
        layout.childControlHeight = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 6;
        ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRect;
        scroll.vertical = true;
        scroll.horizontal = false;

        recipeListParent = content.transform;

        closeButton = CreateButton(panel.transform, "Close", new Vector2(0, -210), () => CloseAll());

        return panel;
    }

    GameObject BuildIngredientPanel(Transform parent)
    {
        GameObject panel = CreatePanel(parent, "IngredientPanel", new Vector2(400, 360));

        dishTitleText = CreateText(panel.transform, "Dish", 20, TextAnchor.UpperCenter,
            new Vector2(0, -24), new Vector2(360, 36));

        ingredientsListText = CreateText(panel.transform, "", 16, TextAnchor.UpperLeft,
            new Vector2(0, -70), new Vector2(340, 160));
        ingredientsListText.alignment = TextAnchor.UpperLeft;

        startCookButton = CreateButton(panel.transform, "Start Cooking", new Vector2(0, -120),
            () => OnStartCooking());

        backButton = CreateButton(panel.transform, "Back", new Vector2(-90, -170),
            () => ShowRecipeBook());

        closeButton = CreateButton(panel.transform, "Close", new Vector2(90, -170),
            () => CloseAll());

        return panel;
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
            Text label = CreateText(row.transform,
                recipe.recipeName + "\n" + ingredientLine,
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

        bool hasAll = true;

        foreach (ItemData ingredient in recipe.ingredients)
        {
            if (ingredient == null)
                continue;

            int have = InventoryManager.Instance.CountItem(ingredient);
            string name = GetDisplayName(ingredient);
            string status = have > 0 ? "OK" : "X";
            sb.AppendLine(status + " " + name + "  (" + have + " in bag)");

            if (have < 1)
                hasAll = false;
        }

        ingredientsListText.text = sb.ToString();
        startCookButton.interactable = hasAll && activePot != null && activePot.CanStartCooking();
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
            NotificationManager.Instance.ShowMessage("Cannot cook right now.");
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
        CameraMovement cameraMovement = Object.FindObjectOfType<CameraMovement>();
        if (cameraMovement != null)
            cameraMovement.RefreshCursorState();
    }

    #region UI builders

    static GameObject CreatePanel(Transform parent, string name, Vector2 size)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);
        RectTransform rect = panel.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = Vector2.zero;
        Image img = panel.AddComponent<Image>();
        img.color = new Color(0.12f, 0.1f, 0.08f, 0.94f);
        return panel;
    }

    static Text CreateText(Transform parent, string content, int fontSize, TextAnchor anchor,
        Vector2 pos, Vector2 size)
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

    static Button CreateButton(Transform parent, string label, Vector2 pos, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(label + "Button");
        go.transform.SetParent(parent, false);
        RectTransform rect = go.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(160, 36);
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = pos;
        Image img = go.AddComponent<Image>();
        img.color = new Color(0.45f, 0.32f, 0.18f, 1f);
        Button btn = go.AddComponent<Button>();
        btn.onClick.AddListener(onClick);
        CreateText(go.transform, label, 14, TextAnchor.MiddleCenter, Vector2.zero, rect.sizeDelta);
        return btn;
    }

    #endregion
}
