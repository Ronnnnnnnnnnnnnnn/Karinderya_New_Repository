using UnityEngine;
using UnityEngine.UI;

public class MissionCardView : MonoBehaviour
{
    public Image background;
    public Text titleText;
    public Text requesterText;
    public Text descriptionText;
    public Text progressText;
    public Text metaText;
    public Text statusText;

    static readonly Color Gold =
        new Color(0.96f, 0.76f, 0.28f, 1f);

    static readonly Color CompleteGreen =
        new Color(0.45f, 0.85f, 0.45f, 1f);

    static readonly Color MissedRed =
        new Color(0.9f, 0.42f, 0.34f, 1f);

    public void SetMission(
        MissionInstance mission,
        MissionManager manager)
    {
        if (mission == null)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        MissionDefinition definition = mission.Definition;

        titleText.text = definition.title;
        requesterText.text = "from " + definition.requester;
        descriptionText.text = definition.description;
        progressText.text = manager.GetProgressText(mission);

        metaText.text =
            manager.GetWindowText(definition) +
            "   |   ₱" +
            definition.rewardCoins +
            "   |   +" +
            definition.rewardReputation +
            " trust";

        switch (mission.State)
        {
            case MissionState.Completed:
                statusText.text = "COMPLETE";
                statusText.color = CompleteGreen;
                background.color =
                    new Color(0.10f, 0.18f, 0.10f, 0.98f);
                break;

            case MissionState.Expired:
                statusText.text = "MISSED";
                statusText.color = MissedRed;
                background.color =
                    new Color(0.18f, 0.09f, 0.08f, 0.98f);
                break;

            default:
                statusText.text = "IN PROGRESS";
                statusText.color = Gold;
                background.color =
                    new Color(0.17f, 0.13f, 0.09f, 0.98f);
                break;
        }
    }
}
