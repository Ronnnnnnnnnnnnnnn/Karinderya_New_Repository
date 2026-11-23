using System.Collections;
using System.Collections.Generic;
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

    readonly Queue<string> pendingMessages =
        new Queue<string>();

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
        pendingMessages.Clear();

        if(notificationText != null)
        {
            RectTransform rect =
                notificationText.rectTransform;

            rect.sizeDelta = new Vector2(620f, 92f);
            rect.anchoredPosition = new Vector2(0f, -300f);

            notificationText.fontSize = 24f;
            notificationText.alignment =
                TextAlignmentOptions.Center;
            notificationText.textWrappingMode =
                TextWrappingModes.Normal;
            notificationText.overflowMode =
                TextOverflowModes.Overflow;
            notificationText.outlineWidth = 0.18f;
            notificationText.outlineColor =
                new Color(0.03f, 0.02f, 0.01f, 1f);
            notificationText.raycastTarget = false;

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

        if (string.IsNullOrEmpty(message))
            return;

        pendingMessages.Enqueue(message);

        if (currentRoutine == null)
        {
            currentRoutine = StartCoroutine(ShowRoutine());
        }
    }

    IEnumerator ShowRoutine()
    {
        while (pendingMessages.Count > 0)
        {
            string message = pendingMessages.Dequeue();

            notificationText.gameObject.SetActive(true);
            notificationText.text = message;

            yield return new WaitForSeconds(displayTime);
        }

        notificationText.text = "";
        notificationText.gameObject.SetActive(false);

        currentRoutine = null;

        if (pendingMessages.Count > 0)
        {
            currentRoutine = StartCoroutine(ShowRoutine());
        }
    }
}
