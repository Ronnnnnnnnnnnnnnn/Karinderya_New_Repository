using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Optional. If empty, the label that says \"Play\" is found automatically.")]
    public TMP_Text playButtonLabel;

    void Start()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PauseMenu.InMainMenu)
        {

            SetPlayLabel("Resume");
            return;
        }

        Time.timeScale = 1f;
        PauseMenu.Pause = false;
    }

    void SetPlayLabel(string text)
    {
        if (playButtonLabel == null)
        {
            foreach (TMP_Text label in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                if (label.gameObject.scene == gameObject.scene &&
                    label.text.Trim().ToLower() == "play")
                {
                    playButtonLabel = label;
                    break;
                }
            }
        }

        if (playButtonLabel != null)
            playButtonLabel.text = text;
    }

    public void Play()
    {

        if (PauseMenu.InMainMenu)
        {
            PauseMenu.ResumeGame(gameObject.scene);
            return;
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
