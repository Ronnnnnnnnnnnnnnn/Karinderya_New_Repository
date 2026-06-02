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

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        EnsurePanelExists();
    }

    void EnsurePanelExists()
    {
        if (timerPanel != null && timerListText != null)
            return;

        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
            return;

        timerPanel = new GameObject("FarmTimerPanel");
        timerPanel.transform.SetParent(canvas.transform, false);
        RectTransform rect = timerPanel.AddComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-16, -16);
        rect.sizeDelta = new Vector2(220, 140);
        Image bg = timerPanel.AddComponent<Image>();
        bg.color = new Color(0.08f, 0.12f, 0.08f, 0.75f);

        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(timerPanel.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8, 8);
        textRect.offsetMax = new Vector2(-8, -8);
        timerListText = textObj.AddComponent<Text>();
        timerListText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerListText.fontSize = 13;
        timerListText.alignment = TextAnchor.UpperLeft;
        timerListText.color = Color.white;
    }

    void Update()
    {
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

        int index = 1;
        foreach (CropBehaviour crop in crops)
        {
            if (crop == null || crop.SeedToGrow == null)
                continue;

            string cropName = crop.SeedToGrow.itemName;
            if (string.IsNullOrEmpty(cropName))
                cropName = crop.SeedToGrow.name;

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

        timerListText.text = sb.ToString();
    }
}
