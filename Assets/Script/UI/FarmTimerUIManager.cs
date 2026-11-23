using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class FarmTimerUIManager : MonoBehaviour
{
    public static FarmTimerUIManager Instance { get; private set; }

    public GameObject timerPanel;
    public Text timerListText;

    public float refreshInterval = 0.25f;

    float refreshTimer;
    int lastScreenWidth;
    int lastScreenHeight;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        ApplyResponsiveLayout();
    }

    void ApplyResponsiveLayout()
    {
        if (timerPanel == null)
            return;

        RectTransform rect = timerPanel.GetComponent<RectTransform>();

        if (rect == null)
            return;

        float aspect =
            Screen.height > 0
                ? (float)Screen.width / Screen.height
                : 1.777f;

        if (aspect < 1.65f)
        {

            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-16f, 170f);
        }
        else
        {

            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(1f, 1f);
            rect.anchoredPosition = new Vector2(-16f, -140f);
        }

        rect.sizeDelta = new Vector2(360f, 230f);

        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }

    void Update()
    {
        if (Screen.width != lastScreenWidth ||
            Screen.height != lastScreenHeight)
        {
            ApplyResponsiveLayout();
        }

        if (PauseMenu.Pause ||
            (UIManager.Instance != null && UIManager.Instance.IsMenuOpen) ||
            (CookingUIManager.Instance != null && CookingUIManager.Instance.IsOpen))
        {
            if (timerPanel != null)
                timerPanel.SetActive(false);

            return;
        }

        refreshTimer -= Time.deltaTime;

        if (refreshTimer > 0f)
            return;

        refreshTimer = refreshInterval;
        RefreshDisplay();
    }

    void RefreshDisplay()
    {
        if (timerListText == null)
            return;

        if (LandManager.Instance == null)
        {
            timerListText.text = "";
            if (timerPanel != null)
                timerPanel.SetActive(false);
            return;
        }

        List<CropBehaviour> crops = LandManager.Instance.GetActiveCrops();

        if (crops.Count == 0)
        {
            timerListText.text = "";
            if (timerPanel != null)
                timerPanel.SetActive(false);
            return;
        }

        if (timerPanel != null)
            timerPanel.SetActive(true);

        var sb = new StringBuilder();
        sb.AppendLine("Crops:");

        const int maxLines = 5;
        int index = 1;
        int total = 0;

        foreach (CropBehaviour crop in crops)
        {
            if (crop == null || crop.SeedToGrow == null)
                continue;

            total++;

            if (index > maxLines)
                continue;

            string cropName = null;

            if (crop.SeedToGrow.cropToYield != null)
                cropName = crop.SeedToGrow.cropToYield.name;

            if (string.IsNullOrEmpty(cropName))
                cropName = crop.SeedToGrow.itemName;

            if (string.IsNullOrEmpty(cropName))
                cropName = crop.SeedToGrow.name;

            cropName = cropName
                .Replace(" Crop", "")
                .Replace(" Seed", "")
                .Replace(" seed", "")
                .Trim();

            if (crop.cropState == CropBehaviour.CropState.Harvestable)
            {
                sb.AppendLine(index + ". " + cropName + " - Ready!");
            }
            else
            {
                float remaining = crop.GetRemainingGrowSeconds();
                int mins = Mathf.FloorToInt(remaining / 60f);
                int secs = Mathf.FloorToInt(remaining % 60f);
                sb.AppendLine(index + ". " + cropName + " - " + mins + "m " + secs.ToString("00") + "s");
            }

            index++;
        }

        if (total > maxLines)
        {
            sb.AppendLine("+" + (total - maxLines) + " more");
        }

        timerListText.text = sb.ToString();
    }
}
