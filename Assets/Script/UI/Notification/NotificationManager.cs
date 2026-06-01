using System.Collections;
using TMPro;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
    public static NotificationManager Instance;

    [Header("UI")]
    public TMP_Text notificationText;

    [Header("Settings")]
    public float displayTime = 3f;

    Coroutine currentRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if(notificationText != null)
        {
            notificationText.text = "";
            notificationText.gameObject.SetActive(false);
        }
    }

    public void ShowMessage(string message)
    {
        if(notificationText == null)
        {
            Debug.LogWarning("[NOTIFICATION] Missing notification text reference.");
            return;
        }

        if(currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(
            ShowRoutine(message)
        );
    }

    IEnumerator ShowRoutine(string message)
    {
        notificationText.gameObject.SetActive(true);
        notificationText.text = message;

        yield return new WaitForSeconds(displayTime);

        notificationText.text = "";
        notificationText.gameObject.SetActive(false);

        currentRoutine = null;
    }
}