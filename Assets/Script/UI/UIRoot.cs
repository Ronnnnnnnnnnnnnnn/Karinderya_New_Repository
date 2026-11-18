using UnityEngine;

/// <summary>
/// Finds the game's main HUD canvas. Runtime UI helpers should use this
/// instead of FindObjectOfType&lt;Canvas&gt;() so they never attach
/// themselves to the tutorial overlay or a world-space customer canvas.
/// </summary>
public static class UIRoot
{
    public static Canvas FindGameCanvas()
    {
        if (UIManager.Instance != null &&
            UIManager.Instance.coinText != null)
        {
            return UIManager.Instance.coinText.canvas;
        }

        Canvas[] canvases =
            Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);

        foreach (Canvas canvas in canvases)
        {
            if (canvas == null ||
                !canvas.isActiveAndEnabled ||
                canvas.name == "TutorialCanvas")
            {
                continue;
            }

            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                return canvas;
        }

        return null;
    }
}
