using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Tooltip("Optional. If empty, the label that says \"Play\" is found automatically.")]
    public TMP_Text playButtonLabel;

    [Tooltip("Hidden while the menu is opened from the pause screen.")]
    public GameObject newGameButton;

    void Start()
    {

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (PauseMenu.InMainMenu)
        {

            SetPlayLabel("Resume");

            if (newGameButton != null)
                newGameButton.SetActive(false);

            return;
        }

        Time.timeScale = 1f;
        PauseMenu.Pause = false;

        if (newGameButton != null)
            newGameButton.SetActive(true);

        SetPlayLabel(
            SaveManager.HasSave
                ? "Continue"
                : "Play"
        );
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

    const float ConfirmWindowSeconds = 4f;

    bool awaitingNewGameConfirm;
    float confirmExpiresAt;
    TMP_Text newGameLabel;
    string newGameLabelDefault;

    void Update()
    {
        if (awaitingNewGameConfirm &&
            Time.unscaledTime > confirmExpiresAt)
        {
            CancelNewGameConfirm();
        }
    }

    public void NewGame()
    {
        // New Game is only available from the title screen. It is hidden
        // while the menu is layered over an ongoing pause session.
        if (PauseMenu.InMainMenu)
            return;

        // Starting over erases the single save slot, so ask for a second
        // click first when there is progress to lose.
        if (SaveManager.HasSave && !awaitingNewGameConfirm)
        {
            BeginNewGameConfirm();
            return;
        }

        SaveManager.DeleteSave();

        PauseMenu.Pause = false;
        PauseMenu.InMainMenu = false;
        PauseMenu.GameInProgress = true;

        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    void BeginNewGameConfirm()
    {
        if (newGameLabel == null && newGameButton != null)
            newGameLabel = newGameButton.GetComponentInChildren<TMP_Text>(true);

        if (newGameLabel != null)
        {
            if (string.IsNullOrEmpty(newGameLabelDefault))
                newGameLabelDefault = newGameLabel.text;

            newGameLabel.text = "Erase save? Click again";
        }

        awaitingNewGameConfirm = true;
        confirmExpiresAt = Time.unscaledTime + ConfirmWindowSeconds;
    }

    void CancelNewGameConfirm()
    {
        awaitingNewGameConfirm = false;

        if (newGameLabel != null && !string.IsNullOrEmpty(newGameLabelDefault))
            newGameLabel.text = newGameLabelDefault;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
