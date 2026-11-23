using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    [Header("Sun")]
    public Light sunLight;

    [Tooltip("Sun intensity at midday.")]
    public float dayIntensity = 1.8f;

    [Tooltip("Dim moonlight intensity at night.")]
    public float nightIntensity = 0.08f;

    public Color dayColor =
        new Color(1f, 0.96f, 0.86f, 1f);

    public Color duskColor =
        new Color(1f, 0.58f, 0.32f, 1f);

    public Color nightColor =
        new Color(0.32f, 0.44f, 0.72f, 1f);

    public bool rotateSun = true;
    public float sunYaw = -30f;

    [Header("Night Tint")]
    public float maxNightOverlayAlpha = 0.38f;

    public Color nightOverlayColor =
        new Color(0.02f, 0.04f, 0.13f, 1f);

    [Header("Ambient")]
    public float dayAmbientIntensity = 1f;
    public float nightAmbientIntensity = 0.38f;

    Canvas overlayCanvas;
    Image overlayImage;

    void Start()
    {
        FindSunIfNeeded();
        CreateOverlay();
        ApplyLighting();
    }

    void Update()
    {
        if (TimeManager.Instance == null)
            return;

        if (PauseMenu.InMainMenu || PauseMenu.Pause)
            return;

        ApplyLighting();
    }

    void FindSunIfNeeded()
    {
        if (sunLight != null)
            return;

        Light[] lights =
            Object.FindObjectsByType<Light>(
                FindObjectsSortMode.None
            );

        float bestIntensity = -1f;

        foreach (Light light in lights)
        {
            if (light == null ||
                light.type != LightType.Directional)
            {
                continue;
            }

            if (light.intensity > bestIntensity)
            {
                bestIntensity = light.intensity;
                sunLight = light;
            }
        }
    }

    void CreateOverlay()
    {
        if (overlayCanvas != null)
            return;

        GameObject canvasObject =
            new GameObject(
                "DayNightOverlay",
                typeof(RectTransform)
            );

        overlayCanvas =
            canvasObject.AddComponent<Canvas>();

        overlayCanvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        overlayCanvas.sortingOrder = -100;

        CanvasScaler scaler =
            canvasObject.AddComponent<CanvasScaler>();

        scaler.uiScaleMode =
            CanvasScaler.ScaleMode.ScaleWithScreenSize;

        scaler.referenceResolution =
            new Vector2(1600f, 900f);

        scaler.matchWidthOrHeight = 0.5f;

        GameObject tintObject =
            new GameObject(
                "NightTint",
                typeof(RectTransform)
            );

        tintObject.transform.SetParent(
            canvasObject.transform,
            false
        );

        RectTransform rect =
            tintObject.GetComponent<RectTransform>();

        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlayImage = tintObject.AddComponent<Image>();
        overlayImage.raycastTarget = false;
        overlayImage.color =
            new Color(
                nightOverlayColor.r,
                nightOverlayColor.g,
                nightOverlayColor.b,
                0f
            );

        overlayImage.enabled = false;
    }

    void ApplyLighting()
    {
        if (TimeManager.Instance == null)
            return;

        GameTimestamp timestamp =
            TimeManager.Instance.GetGameTimestamp();

        if (timestamp == null)
            return;

        float hour =
            timestamp.hour +
            timestamp.minute / 60f;

        float dayFactor = GetDayFactor(hour);

        if (sunLight != null)
        {
            if (rotateSun)
            {
                float sunX;

                if (hour >= 5f && hour <= 19f)
                {
                    sunX = Mathf.Lerp(
                        10f,
                        170f,
                        Mathf.InverseLerp(
                            5f,
                            19f,
                            hour
                        )
                    );
                }
                else
                {

                    sunX = 55f;
                }

                sunLight.transform.rotation =
                    Quaternion.Euler(
                        sunX,
                        sunYaw,
                        0f
                    );
            }

            sunLight.intensity = Mathf.Lerp(
                nightIntensity,
                dayIntensity,
                dayFactor
            );

            sunLight.color =
                dayFactor >= 0.5f
                    ? Color.Lerp(
                        duskColor,
                        dayColor,
                        (dayFactor - 0.5f) * 2f
                    )
                    : Color.Lerp(
                        nightColor,
                        duskColor,
                        dayFactor * 2f
                    );
        }

        RenderSettings.ambientIntensity =
            Mathf.Lerp(
                nightAmbientIntensity,
                dayAmbientIntensity,
                dayFactor
            );

        if (overlayImage != null)
        {
            float alpha =
                (1f - dayFactor) *
                maxNightOverlayAlpha;

            overlayImage.enabled = alpha > 0.002f;
            overlayImage.color =
                new Color(
                    nightOverlayColor.r,
                    nightOverlayColor.g,
                    nightOverlayColor.b,
                    alpha
                );
        }
    }

    static float GetDayFactor(float hour)
    {

        if (hour < 5f)
            return 0f;

        if (hour < 7f)
        {
            return Mathf.InverseLerp(
                5f,
                7f,
                hour
            );
        }

        if (hour < 17f)
            return 1f;

        if (hour < 19f)
        {
            return 1f - Mathf.InverseLerp(
                17f,
                19f,
                hour
            );
        }

        return 0f;
    }
}
