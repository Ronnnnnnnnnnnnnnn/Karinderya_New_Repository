using TMPro;
using UnityEngine;
using System.Collections;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI")]
    public TextMeshProUGUI notificationText;

    public float messageDuration = 2f;

    Coroutine currentRoutine;

    void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public void ShowMessage(string message)
    {
        if(currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(
                ShowRoutine(message)
            );
    }

    IEnumerator ShowRoutine(string message)
    {
        notificationText.gameObject.SetActive(true);

        notificationText.text = message;

        yield return new WaitForSeconds(
            messageDuration
        );

        notificationText.gameObject.SetActive(false);
    }
}