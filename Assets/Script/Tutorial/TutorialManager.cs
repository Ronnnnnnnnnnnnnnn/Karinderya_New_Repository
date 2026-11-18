using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A lightweight, data-free tutorial for the current prototype.
/// It guides the player through the first sale, the shop, and the
/// first planting cycle without requiring any scene prefab setup.
///
/// The tutorial is created by TutorialBootstrap when the PlayingScene
/// loads. It pauses the automatic customer spawner until the player
/// reaches the serving step, so the first customer can be controlled.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    enum Step
    {
        Welcome,
        Move,
        GetMeat,
        Cook,
        TakeDish,
        EquipDish,
        StockBuffet,
        TakeServing,
        EquipServing,
        ServeCustomer,
        BuySeed,
        Plant,
        Water,
        Complete
    }

    // The visible numbered steps. Welcome and Complete are bookends.
    static readonly Step[] Steps =
    {
        Step.Move,
        Step.GetMeat,
        Step.Cook,
        Step.TakeDish,
        Step.EquipDish,
        Step.StockBuffet,
        Step.TakeServing,
        Step.EquipServing,
        Step.ServeCustomer,
        Step.BuySeed,
        Step.Plant,
        Step.Water
    };

    static readonly Color Gold = new Color(0.96f, 0.76f, 0.28f, 1f);
    static readonly Color Cream = new Color(1f, 0.96f, 0.88f, 1f);
    static readonly Color Muted = new Color(0.78f, 0.72f, 0.58f, 1f);

    [Header("Runtime references")]
    Transform player;
    CustomerSpawner spawner;
    ItemData meatItem;

    [Header("Tutorial state")]
    Step step;
    Vector3 playerStartPosition;
    ItemData cookedDish;
    ItemData servingItem;
    BuffetContainer targetBuffet;
    CustomerOrder tutorialCustomer;
    bool customerServed;
    bool finished;
    bool completionStarted;
    bool uiBlocking;
    float stepStartTime;
    float nextCustomerSpawnTime;
    float scanTimer;

    // UI
    GameObject tutorialCanvas;
    Text titleText;
    Text objectiveText;
    Text hintText;
    Text progressText;

    // World marker
    GameObject markerRoot;
    Transform markerBeam;
    Transform markerTarget;
    float markerHeight = 2f;
    TutorialPathGuide pathGuide;
    bool uiVisible;

    void Start()
    {
        player = FindPlayer();
        spawner = UnityEngine.Object.FindFirstObjectByType<CustomerSpawner>();
        meatItem = FindMeatItem();

        if (player != null)
            playerStartPosition = player.position;

        // The tutorial controls the first customer. Normal spawning
        // resumes when the tutorial ends or is skipped.
        if (spawner != null)
            spawner.SetSpawningEnabled(false);

        CustomerOrder.CustomerFinished += OnCustomerFinished;

        BuildUI();
        BuildMarker();
        BuildPathGuide();

        Debug.Log("[TUTORIAL] Started");
        SetStep(Step.Welcome);
    }

    void OnDestroy()
    {
        CustomerOrder.CustomerFinished -= OnCustomerFinished;

        if (spawner != null)
            spawner.SetSpawningEnabled(true);
    }

    void Update()
    {
        if (finished)
            return;

        // Hide the tutorial while the game is paused or behind the main menu.
        if (PauseMenu.Pause || PauseMenu.InMainMenu)
        {
            SetUiVisible(false);
            return;
        }

        uiBlocking = IsModalOpen();
        SetUiVisible(!uiBlocking);

        if (Input.GetKeyDown(KeyCode.F1))
        {
            SkipTutorial();
            return;
        }

        if (!uiBlocking)
            UpdateMarker();
        else if (markerRoot != null)
            markerRoot.SetActive(false);

        scanTimer -= Time.deltaTime;

        if (scanTimer > 0f)
            return;

        scanTimer = 0.15f;

        RefreshDynamicMarker();
        CheckStepCompletion();
    }

    static bool IsModalOpen()
    {
        if (UIManager.Instance != null && UIManager.Instance.IsMenuOpen)
            return true;

        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen)
            return true;

        return false;
    }

    // =========================================================
    // STEP FLOW
    // =========================================================

    void SetStep(Step newStep)
    {
        step = newStep;
        stepStartTime = Time.time;
        scanTimer = 0f;

        UpdateStepUi();
        InitializeStep();
    }

    void InitializeStep()
    {
        switch (step)
        {
            case Step.Welcome:
                SetMarker(null, 2f);
                break;

            case Step.Move:
                if (player != null)
                    playerStartPosition = player.position;

                SetMarker(null, 2f);
                break;

            case Step.GetMeat:
                SetMarker(FindMeatTarget(), 1.8f);
                break;

            case Step.Cook:
                SetMarker(GetPotMarker(), 2.1f);
                break;

            case Step.TakeDish:
                SetMarker(GetPotMarker(), 2.1f);
                break;

            case Step.EquipDish:
                SetMarker(null, 2f);
                break;

            case Step.StockBuffet:
                targetBuffet = FindMatchingBuffet(cookedDish);
                SetMarker(targetBuffet != null ? targetBuffet.transform : null, 2.1f);
                break;

            case Step.TakeServing:
                if (targetBuffet == null)
                    targetBuffet = FindMatchingBuffet(cookedDish);

                SetMarker(targetBuffet != null ? targetBuffet.transform : null, 2.1f);
                break;

            case Step.EquipServing:
                SetMarker(null, 2f);
                break;

            case Step.ServeCustomer:
                SetMarker(null, 2f);
                nextCustomerSpawnTime = Time.time + 1.25f;
                break;

            case Step.BuySeed:
                SetMarker(null, 2f);
                break;

            case Step.Plant:
                SetMarker(GetPlantMarkerTarget(), 2.1f);
                break;

            case Step.Water:
                SetMarker(GetPlantedLandMarkerTarget(), 2.1f);
                break;

            case Step.Complete:
                SetMarker(null, 2f);
                CompleteTutorial();
                break;
        }
    }

    void Advance()
    {
        if (step == Step.Welcome)
        {
            SetStep(Steps[0]);
            return;
        }

        int index = Array.IndexOf(Steps, step);

        if (index < 0 || index >= Steps.Length - 1)
        {
            SetStep(Step.Complete);
            return;
        }

        SetStep(Steps[index + 1]);
    }

    void CheckStepCompletion()
    {
        switch (step)
        {
            case Step.Welcome:
                if (Time.time - stepStartTime >= 4f)
                    Advance();
                break;

            case Step.Move:
                if (player == null ||
                    Vector3.Distance(player.position, playerStartPosition) >= 3f)
                {
                    Advance();
                }
                break;

            case Step.GetMeat:
                // If the scene has no meat pickup, don't trap the player.
                if (meatItem == null || HasMeat())
                    Advance();
                break;

            case Step.Cook:
                if (AnyPotHasCookedDish() || HasDish())
                {
                    cookedDish = FindDishInInventory();
                    Advance();
                }
                break;

            case Step.TakeDish:
                cookedDish = FindDishInInventory();

                if (cookedDish != null)
                    Advance();
                break;

            case Step.EquipDish:
                ItemData equippedDish = GetEquippedItem();

                if (equippedDish != null && equippedDish.isDish)
                {
                    cookedDish = equippedDish;
                    Advance();
                }
                break;

            case Step.StockBuffet:
                if (targetBuffet == null)
                    targetBuffet = FindMatchingBuffet(cookedDish);

                if (targetBuffet != null &&
                    targetBuffet.servings > 0 &&
                    targetBuffet.storedDish == cookedDish)
                {
                    Advance();
                }
                break;

            case Step.TakeServing:
                if (HasServing())
                    Advance();
                break;

            case Step.EquipServing:
                ItemData equippedServing = GetEquippedItem();

                if (equippedServing != null && equippedServing.isServing)
                {
                    servingItem = equippedServing;
                    Advance();
                }
                break;

            case Step.ServeCustomer:
                if (customerServed)
                {
                    Advance();
                    break;
                }

                if (Time.time >= nextCustomerSpawnTime)
                    TrySpawnTutorialCustomer();

                if (tutorialCustomer != null &&
                    markerTarget != tutorialCustomer.transform)
                {
                    SetMarker(tutorialCustomer.transform, 2.8f);
                }
                break;

            case Step.BuySeed:
                if (HasSeed())
                    Advance();
                break;

            case Step.Plant:
                if (AnyCropPlanted())
                    Advance();
                break;

            case Step.Water:
                if (AnyCropWatered())
                    Advance();
                break;
        }
    }

    // =========================================================
    // CUSTOMER
    // =========================================================

    void OnCustomerFinished(CustomerOrder order, bool served)
    {
        if (step != Step.ServeCustomer)
            return;

        if (order == null || order != tutorialCustomer)
            return;

        tutorialCustomer = null;
        SetMarker(null, 2f);

        if (served)
        {
            customerServed = true;
            return;
        }

        nextCustomerSpawnTime = Time.time + 2.5f;

        if (hintText != null)
        {
            hintText.text =
                "The customer left. Don't worry - take another serving and try again.\n" +
                "[F1] Skip tutorial";
        }
    }

    void TrySpawnTutorialCustomer()
    {
        if (spawner == null)
            return;

        if (UIManager.Instance != null && UIManager.Instance.IsMenuOpen)
        {
            nextCustomerSpawnTime = Time.time + 0.5f;
            return;
        }

        if (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen)
        {
            nextCustomerSpawnTime = Time.time + 0.5f;
            return;
        }

        if (spawner.GetCurrentCustomers() > 0)
            return;

        if (servingItem == null)
            servingItem = FindServingInInventory();

        if (servingItem == null)
        {
            nextCustomerSpawnTime = Time.time + 1f;
            return;
        }

        CustomerOrder order = spawner.ForceSpawn(servingItem);

        if (order != null)
        {
            tutorialCustomer = order;

            // Give the first customer a little extra patience while
            // the player learns the serving controls.
            tutorialCustomer.patienceTime = 90f;

            SetMarker(order.transform, 2.8f);
            nextCustomerSpawnTime = Time.time + 3f;
        }
        else
        {
            nextCustomerSpawnTime = Time.time + 1f;
        }
    }

    // =========================================================
    // COMPLETE / SKIP
    // =========================================================

    void CompleteTutorial()
    {
        if (completionStarted)
            return;

        completionStarted = true;

        if (spawner != null)
            spawner.SetSpawningEnabled(true);

        SetMarker(null, 2f);

        if (pathGuide != null)
            pathGuide.SetVisible(false);

        Debug.Log("[TUTORIAL] Complete");
        StartCoroutine(CompleteRoutine());
    }

    IEnumerator CompleteRoutine()
    {
        yield return new WaitForSeconds(8f);

        if (finished)
            yield break;

        finished = true;

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (markerRoot != null)
            markerRoot.SetActive(false);
    }

    void SkipTutorial()
    {
        if (finished)
            return;

        finished = true;

        if (spawner != null)
            spawner.SetSpawningEnabled(true);

        SetMarker(null, 2f);

        if (pathGuide != null)
            pathGuide.SetVisible(false);

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        if (markerRoot != null)
            markerRoot.SetActive(false);

        if (NotificationManager.Instance != null)
            NotificationManager.Instance.ShowMessage("Tutorial skipped.");

        Debug.Log("[TUTORIAL] Skipped");
    }

    // =========================================================
    // UI
    // =========================================================

    void UpdateStepUi()
    {
        if (titleText != null)
            titleText.text = "Tutorial: " + GetStepTitle(step);

        if (objectiveText != null)
            objectiveText.text = GetStepObjective(step);

        if (hintText != null)
        {
            string hint = GetStepHint(step);

            if (step != Step.Complete)
                hint += "\n[F1] Skip tutorial";

            hintText.text = hint;
        }

        if (progressText != null)
        {
            int index = Array.IndexOf(Steps, step);

            if (index < 0)
            {
                progressText.text = "";
            }
            else
            {
                progressText.text =
                    "STEP " + (index + 1) + " / " + Steps.Length;
            }
        }
    }

    void SetUiVisible(bool visible)
    {
        if (tutorialCanvas != null && tutorialCanvas.activeSelf != visible)
            tutorialCanvas.SetActive(visible);

        if (pathGuide != null && uiVisible != visible)
            pathGuide.SetVisible(visible);

        uiVisible = visible;

        if (!visible && markerRoot != null)
            markerRoot.SetActive(false);
    }

    void BuildUI()
    {
        tutorialCanvas = new GameObject("TutorialCanvas", typeof(RectTransform));
        tutorialCanvas.transform.SetParent(transform, false);

        Canvas canvas = tutorialCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 5000;

        CanvasScaler scaler = tutorialCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        GameObject panel = new GameObject("TutorialPanel", typeof(RectTransform));
        panel.transform.SetParent(tutorialCanvas.transform, false);

        RectTransform panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(24f, -180f);
        panelRect.sizeDelta = new Vector2(680f, 190f);

        Image panelBackground = panel.AddComponent<Image>();
        panelBackground.color = new Color(0.08f, 0.06f, 0.04f, 0.93f);
        panelBackground.raycastTarget = false;

        GameObject accent = new GameObject("Accent", typeof(RectTransform));
        accent.transform.SetParent(panel.transform, false);

        RectTransform accentRect = accent.GetComponent<RectTransform>();
        accentRect.anchorMin = new Vector2(0f, 0f);
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.offsetMin = Vector2.zero;
        accentRect.offsetMax = new Vector2(6f, 0f);

        Image accentImage = accent.AddComponent<Image>();
        accentImage.color = Gold;
        accentImage.raycastTarget = false;

        titleText = CreateText(
            panel.transform,
            "Title",
            24,
            FontStyle.Bold,
            TextAnchor.UpperLeft,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -14f),
            new Vector2(430f, 30f),
            Gold
        );

        progressText = CreateText(
            panel.transform,
            "Progress",
            13,
            FontStyle.Normal,
            TextAnchor.UpperRight,
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-24f, -18f),
            new Vector2(240f, 24f),
            Muted
        );

        objectiveText = CreateText(
            panel.transform,
            "Objective",
            20,
            FontStyle.Normal,
            TextAnchor.UpperLeft,
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(24f, -50f),
            new Vector2(632f, 98f),
            Cream
        );

        hintText = CreateText(
            panel.transform,
            "Hint",
            14,
            FontStyle.Normal,
            TextAnchor.LowerLeft,
            new Vector2(0f, 0f),
            new Vector2(0f, 0f),
            new Vector2(24f, 14f),
            new Vector2(632f, 42f),
            Muted
        );
    }

    static Text CreateText(
        Transform parent,
        string objectName,
        int fontSize,
        FontStyle fontStyle,
        TextAnchor alignment,
        Vector2 anchor,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 size,
        Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        Text text = go.AddComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = fontSize;
        text.fontStyle = fontStyle;
        text.alignment = alignment;
        text.color = color;
        text.lineSpacing = 1.05f;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;

        Shadow shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.75f);
        shadow.effectDistance = new Vector2(1f, -1f);

        return text;
    }

    // =========================================================
    // WORLD MARKER
    // =========================================================

    void BuildPathGuide()
    {
        if (pathGuide != null)
            return;

        GameObject pathObject = new GameObject("TutorialPathGuide");
        pathObject.transform.SetParent(transform, false);

        pathGuide = pathObject.AddComponent<TutorialPathGuide>();
        pathGuide.Initialize(player);
    }

    void BuildMarker()
    {
        markerRoot = new GameObject("TutorialMarker");
        markerRoot.transform.SetParent(transform, false);

        GameObject textObject = new GameObject("MarkerText");
        textObject.transform.SetParent(markerRoot.transform, false);

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = "!";
        textMesh.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textMesh.fontSize = 96;
        textMesh.characterSize = 0.35f;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.fontStyle = FontStyle.Bold;
        textMesh.color = Gold;

        textObject.AddComponent<Billboard>();

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "MarkerBeam";
        beam.transform.SetParent(markerRoot.transform, false);

        Collider beamCollider = beam.GetComponent<Collider>();

        if (beamCollider != null)
            Destroy(beamCollider);

        markerBeam = beam.transform;

        Renderer beamRenderer = beam.GetComponent<Renderer>();

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Standard");

        if (shader != null && beamRenderer != null)
        {
            Material material = new Material(shader);
            Color beamColor = new Color(0.96f, 0.76f, 0.28f, 0.75f);

            material.color = beamColor;
            material.SetColor("_BaseColor", beamColor);
            beamRenderer.material = material;
        }

        markerRoot.SetActive(false);
    }

    void SetMarker(Transform target, float height)
    {
        markerTarget = target;
        markerHeight = Mathf.Max(0.5f, height);

        if (pathGuide != null)
            pathGuide.SetTarget(target);

        if (markerRoot == null)
            return;

        if (target == null)
        {
            markerRoot.SetActive(false);
            return;
        }

        if (markerBeam != null)
        {
            markerBeam.localPosition = new Vector3(0f, -markerHeight * 0.5f, 0f);
            markerBeam.localScale = new Vector3(0.03f, markerHeight * 0.5f, 0.03f);
        }

        if (uiBlocking)
        {
            markerRoot.SetActive(false);
            return;
        }

        markerRoot.SetActive(true);
        markerRoot.transform.position = target.position + Vector3.up * markerHeight;
    }

    void UpdateMarker()
    {
        if (markerRoot == null)
            return;

        if (uiBlocking || markerTarget == null)
        {
            if (markerRoot.activeSelf)
                markerRoot.SetActive(false);

            return;
        }

        if (!markerRoot.activeSelf)
            markerRoot.SetActive(true);

        Vector3 basePosition =
            markerTarget.position + Vector3.up * markerHeight;

        markerRoot.transform.position =
            basePosition +
            Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.12f);
    }

    void RefreshDynamicMarker()
    {
        if (step == Step.Cook || step == Step.TakeDish)
        {
            Transform target = GetPotMarker();

            if (target != markerTarget)
                SetMarker(target, 2.1f);
        }
        else if (step == Step.Plant)
        {
            Transform target = GetPlantMarkerTarget();

            if (target != markerTarget)
                SetMarker(target, 2.1f);
        }
        else if (step == Step.Water)
        {
            Transform target = GetPlantedLandMarkerTarget();

            if (target != markerTarget)
                SetMarker(target, 2.1f);
        }
        else if (step == Step.ServeCustomer &&
                 tutorialCustomer != null)
        {
            Transform target = tutorialCustomer.transform;

            if (target != markerTarget)
                SetMarker(target, 2.8f);
        }
    }

    // =========================================================
    // WORLD LOOKUPS
    // =========================================================

    static Transform FindPlayer()
    {
        Movement movement = UnityEngine.Object.FindFirstObjectByType<Movement>();

        if (movement != null)
            return movement.transform;

        PlayerInteraction interaction =
            UnityEngine.Object.FindFirstObjectByType<PlayerInteraction>();

        if (interaction != null)
            return interaction.transform.root;

        Camera mainCamera = Camera.main;

        return mainCamera != null ? mainCamera.transform : null;
    }

    ItemData FindMeatItem()
    {
        InfiniteIngredient[] pickups =
            UnityEngine.Object.FindObjectsByType<InfiniteIngredient>(
                FindObjectsSortMode.None
            );

        foreach (InfiniteIngredient pickup in pickups)
        {
            if (pickup != null && pickup.item != null)
                return pickup.item;
        }

        return null;
    }

    Transform FindMeatTarget()
    {
        InfiniteIngredient[] pickups =
            UnityEngine.Object.FindObjectsByType<InfiniteIngredient>(
                FindObjectsSortMode.None
            );

        foreach (InfiniteIngredient pickup in pickups)
        {
            if (pickup == null || pickup.item == null)
                continue;

            if (meatItem == null || pickup.item == meatItem)
                return pickup.transform;
        }

        return null;
    }

    Transform GetPotMarker()
    {
        Pot pot = FindCookingPot();

        if (pot == null)
            pot = FindNearestPot();

        return pot != null ? pot.transform : null;
    }

    Pot FindCookingPot()
    {
        Pot nearestCooking = null;
        float nearestDistance = float.MaxValue;
        Vector3 origin = player != null ? player.position : Vector3.zero;

        Pot[] pots = UnityEngine.Object.FindObjectsByType<Pot>(
            FindObjectsSortMode.None
        );

        foreach (Pot pot in pots)
        {
            if (pot == null)
                continue;

            if (pot.HasCookedDish)
                return pot;

            if (pot.IsCooking)
            {
                float distance =
                    Vector3.SqrMagnitude(pot.transform.position - origin);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestCooking = pot;
                }
            }
        }

        return nearestCooking;
    }

    Pot FindNearestPot()
    {
        Pot nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 origin = player != null ? player.position : Vector3.zero;

        Pot[] pots = UnityEngine.Object.FindObjectsByType<Pot>(
            FindObjectsSortMode.None
        );

        foreach (Pot pot in pots)
        {
            if (pot == null)
                continue;

            float distance =
                Vector3.SqrMagnitude(pot.transform.position - origin);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = pot;
            }
        }

        return nearest;
    }

    BuffetContainer FindMatchingBuffet(ItemData dish)
    {
        if (dish == null)
            return null;

        BuffetContainer fallback = null;

        BuffetContainer[] buffets =
            UnityEngine.Object.FindObjectsByType<BuffetContainer>(
                FindObjectsSortMode.None
            );

        foreach (BuffetContainer buffet in buffets)
        {
            if (buffet == null)
                continue;

            if (buffet.buffetRecipe != null &&
                buffet.buffetRecipe.resultDish == dish)
            {
                return buffet;
            }

            if (buffet.storedDish == dish)
                fallback = buffet;
        }

        return fallback;
    }

    Transform GetPlantMarkerTarget()
    {
        Land land = FindPlantTarget();
        return land != null ? land.transform : null;
    }

    Land FindPlantTarget()
    {
        Land nearestTilled = null;
        Land nearestSoil = null;
        float tilledDistance = float.MaxValue;
        float soilDistance = float.MaxValue;
        Vector3 origin = player != null ? player.position : Vector3.zero;

        Land[] lands = UnityEngine.Object.FindObjectsByType<Land>(
            FindObjectsSortMode.None
        );

        foreach (Land land in lands)
        {
            if (land == null || land.GetPlantedCrop() != null)
                continue;

            float distance =
                Vector3.SqrMagnitude(land.transform.position - origin);

            if (land.landStatus == Land.LandStatus.Farmland &&
                distance < tilledDistance)
            {
                tilledDistance = distance;
                nearestTilled = land;
            }
            else if (land.landStatus == Land.LandStatus.Soil &&
                     distance < soilDistance)
            {
                soilDistance = distance;
                nearestSoil = land;
            }
        }

        return nearestTilled != null ? nearestTilled : nearestSoil;
    }

    Transform GetPlantedLandMarkerTarget()
    {
        Land land = FindPlantedLand();
        return land != null ? land.transform : null;
    }

    Land FindPlantedLand()
    {
        Land nearest = null;
        float nearestDistance = float.MaxValue;
        Vector3 origin = player != null ? player.position : Vector3.zero;

        Land[] lands = UnityEngine.Object.FindObjectsByType<Land>(
            FindObjectsSortMode.None
        );

        foreach (Land land in lands)
        {
            if (land == null || land.GetPlantedCrop() == null)
                continue;

            float distance =
                Vector3.SqrMagnitude(land.transform.position - origin);

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = land;
            }
        }

        return nearest;
    }

    // =========================================================
    // INVENTORY / WORLD STATE
    // =========================================================

    ItemData GetEquippedItem()
    {
        if (InventoryManager.Instance == null)
            return null;

        return InventoryManager.Instance.GetEquippedSlotItem(
            InventorySlot.InventoryType.Item
        );
    }

    ItemData FindDishInInventory()
    {
        if (InventoryManager.Instance == null)
            return null;

        ItemData equipped = GetEquippedItem();

        if (equipped != null && equipped.isDish)
            return equipped;

        ItemSlotData[] slots =
            InventoryManager.Instance.GetInventorySlots(
                InventorySlot.InventoryType.Item
            );

        foreach (ItemSlotData slot in slots)
        {
            if (!slot.IsEmpty() &&
                slot.itemData != null &&
                slot.itemData.isDish)
            {
                return slot.itemData;
            }
        }

        return null;
    }

    ItemData FindServingInInventory()
    {
        if (InventoryManager.Instance == null)
            return null;

        ItemData equipped = GetEquippedItem();

        if (equipped != null && equipped.isServing)
            return equipped;

        ItemSlotData[] slots =
            InventoryManager.Instance.GetInventorySlots(
                InventorySlot.InventoryType.Item
            );

        foreach (ItemSlotData slot in slots)
        {
            if (!slot.IsEmpty() &&
                slot.itemData != null &&
                slot.itemData.isServing)
            {
                return slot.itemData;
            }
        }

        return null;
    }

    bool HasDish()
    {
        return FindDishInInventory() != null;
    }

    bool HasServing()
    {
        return FindServingInInventory() != null;
    }

    bool HasMeat()
    {
        if (meatItem == null || InventoryManager.Instance == null)
            return false;

        if (InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Item) == meatItem)
        {
            return true;
        }

        ItemSlotData[] slots =
            InventoryManager.Instance.GetInventorySlots(
                InventorySlot.InventoryType.Item
            );

        foreach (ItemSlotData slot in slots)
        {
            if (!slot.IsEmpty() && slot.itemData == meatItem)
                return true;
        }

        return false;
    }

    bool HasSeed()
    {
        if (InventoryManager.Instance == null)
            return false;

        ItemData equipped =
            InventoryManager.Instance.GetEquippedSlotItem(
                InventorySlot.InventoryType.Tool
            );

        if (equipped is SeedData)
            return true;

        ItemSlotData[] slots =
            InventoryManager.Instance.GetInventorySlots(
                InventorySlot.InventoryType.Tool
            );

        foreach (ItemSlotData slot in slots)
        {
            if (!slot.IsEmpty() && slot.itemData is SeedData)
                return true;
        }

        return false;
    }

    bool AnyPotHasCookedDish()
    {
        Pot[] pots = UnityEngine.Object.FindObjectsByType<Pot>(
            FindObjectsSortMode.None
        );

        foreach (Pot pot in pots)
        {
            if (pot != null && pot.HasCookedDish)
                return true;
        }

        return false;
    }

    bool AnyCropPlanted()
    {
        Land[] lands = UnityEngine.Object.FindObjectsByType<Land>(
            FindObjectsSortMode.None
        );

        foreach (Land land in lands)
        {
            if (land != null && land.GetPlantedCrop() != null)
                return true;
        }

        return false;
    }

    bool AnyCropWatered()
    {
        Land[] lands = UnityEngine.Object.FindObjectsByType<Land>(
            FindObjectsSortMode.None
        );

        foreach (Land land in lands)
        {
            if (land != null &&
                land.GetPlantedCrop() != null &&
                land.landStatus == Land.LandStatus.Watered)
            {
                return true;
            }
        }

        return false;
    }

    // =========================================================
    // TEXT
    // =========================================================

    static string GetStepTitle(Step value)
    {
        switch (value)
        {
            case Step.Welcome: return "Welcome";
            case Step.Move: return "Move Around";
            case Step.GetMeat: return "Get Some Meat";
            case Step.Cook: return "Cook a Dish";
            case Step.TakeDish: return "Take the Dish";
            case Step.EquipDish: return "Hold the Dish";
            case Step.StockBuffet: return "Stock the Buffet";
            case Step.TakeServing: return "Take a Serving";
            case Step.EquipServing: return "Hold the Serving";
            case Step.ServeCustomer: return "Serve the Customer";
            case Step.BuySeed: return "Buy a Seed";
            case Step.Plant: return "Plant Your Seed";
            case Step.Water: return "Water Your Crop";
            case Step.Complete: return "Tutorial Complete";
            default: return "Tutorial";
        }
    }

    string GetStepObjective(Step value)
    {
        switch (value)
        {
            case Step.Welcome:
                return "You're about to run your own karinderya. Let's serve your first customer.";

            case Step.Move:
                return "Move with WASD and look with the mouse.";

            case Step.GetMeat:
                return "Find the meat in the kitchen (follow the ! marker) and press E to pick it up.";

            case Step.Cook:
                return "Look at the pot and press E. Choose Kaldereta or Giniling, then press Start Cooking.";

            case Step.TakeDish:
                return "Left-click the pot to take the cooked dish.";

            case Step.EquipDish:
                return "Press I to open your bag, click the dish to hold it, then press I to close.";

            case Step.StockBuffet:
                return "Look at the " + GetDishName() +
                    " buffet (the ! marker) and press E to put the dish in.";

            case Step.TakeServing:
                return "Click the same buffet again to take one serving.";

            case Step.EquipServing:
                return "Press I, click the serving to hold it, then press I to close.";

            case Step.ServeCustomer:
                return "Wait for the customer to reach the counter, look at them, and left-click to serve.";

            case Step.BuySeed:
                return "Press B to open the shop, buy a Carrot or Potato Seed for 10 coins, then press B to close.";

            case Step.Plant:
                return "Press B to close the shop if it's open. Open your bag (I), equip the Hoe, then left-click a farm plot to till it. Equip the seed, then left-click the tilled plot to plant.";

            case Step.Water:
                return "Open your bag (I), equip the Watering Can, then left-click the planted plot to water it.";

            case Step.Complete:
                return "You've served your first customer and planted your first crop. Keep farming, cooking, and serving - and unlock new recipes at the buffets.";

            default:
                return "";
        }
    }

    static string GetStepHint(Step value)
    {
        switch (value)
        {
            case Step.Welcome:
                return "Take your time. The tutorial waits for you.";

            case Step.Move:
                return "Mouse looks; WASD moves.";

            case Step.GetMeat:
                return "The kitchen meat pickup is reusable.";

            case Step.Cook:
                return "Kaldereta and Giniling are free. You have the vegetables.";

            case Step.TakeDish:
                return "The pot keeps the dish until you take it.";

            case Step.EquipDish:
                return "The dish must be in your hand, not just your bag.";

            case Step.StockBuffet:
                return "Each buffet only accepts its own dish.";

            case Step.TakeServing:
                return "One cooked dish makes several servings.";

            case Step.EquipServing:
                return "The customer wants a serving, not the whole dish.";

            case Step.ServeCustomer:
                return "If they leave, take another serving.";

            case Step.BuySeed:
                return "Seeds cost 10. You should have enough now.";

            case Step.Plant:
                return "Hoe first, then seed.";

            case Step.Water:
                return "Watering keeps the crop healthy.";

            case Step.Complete:
                return "Crops take about 3 minutes. Check the crop list in the top-right, then left-click the ripe crop to harvest it.";

            default:
                return "";
        }
    }

    string GetDishName()
    {
        if (cookedDish != null && !string.IsNullOrEmpty(cookedDish.itemName))
            return cookedDish.itemName;

        return "matching";
    }
}
