using UnityEngine;

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
