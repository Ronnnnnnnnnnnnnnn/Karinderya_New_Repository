using UnityEngine;

public class MissionBoard : MonoBehaviour
{
    public void Open()
    {
        if (MissionManager.Instance == null ||
            !MissionManager.Instance.IsReady)
        {
            if (NotificationManager.Instance != null)
            {
                NotificationManager.Instance.ShowMessage(
                    "Requests unlock after the tutorial."
                );
            }

            return;
        }

        MissionNotebookUI.OpenNotebook();
    }
}
