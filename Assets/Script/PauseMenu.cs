using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool Pause = false;

    // True once the player has gone back to the main menu from the game.
    // The game scene is kept (frozen and hidden) so the menu can Resume it.
    public static bool GameInProgress = false;

    // True while the main menu is shown on top of the frozen game
    public static bool InMainMenu = false;

    static Scene gameScene;
    static PauseMenu instance;

    static readonly List<Behaviour> hiddenBehaviours = new List<Behaviour>();
    static readonly List<AudioSource> pausedAudio = new List<AudioSource>();

    public GameObject PauseMenuCanvas;

    // Static fields survive "Enter Play Mode without domain reload"
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        Pause = false;
        GameInProgress = false;
        InMainMenu = false;
        instance = null;
        hiddenBehaviours.Clear();
        pausedAudio.Clear();
    }

    void Start()
    {
        instance = this;

        Time.timeScale = 1f;
        Pause = false;
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(Pause)
            {
                Play();
            }
            else{
                Stop();
            }
        }
    }

    void Stop()
    {
        PauseMenuCanvas.SetActive(true);
            Time.timeScale = 0f;
        Pause = true;
    }

    public void Play()
    {
        PauseMenuCanvas.SetActive(false);
            Time.timeScale = 1f;
        Pause = false;
    }

    // =========================================================
    // MAIN MENU (keeps the game so it can be resumed)
    // =========================================================

    public void MainMenuButton()
    {
        int menuIndex = SceneManager.GetActiveScene().buildIndex - 1;

        gameScene = gameObject.scene;

        PauseMenuCanvas.SetActive(false);

        // Freeze the game: scaled time stops timers, crops,
        // customers and cooking. Pause blocks player input.
        Time.timeScale = 0f;
        Pause = true;

        HideGame(gameScene);

        GameInProgress = true;
        InMainMenu = true;

        // Don't let Escape toggle the in-game pause while in the menu
        enabled = false;

        // The game locks and hides the cursor; the menu needs it back
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        AsyncOperation load =
            SceneManager.LoadSceneAsync(menuIndex, LoadSceneMode.Additive);

        load.completed += _ =>
        {
            Scene menu = SceneManager.GetSceneByBuildIndex(menuIndex);

            if (menu.IsValid())
                SceneManager.SetActiveScene(menu);
        };
    }

    // Called by the main menu's Resume button
    public static void ResumeGame(Scene menuScene)
    {
        foreach (Behaviour b in hiddenBehaviours)
        {
            if (b != null)
                b.enabled = true;
        }
        hiddenBehaviours.Clear();

        foreach (AudioSource s in pausedAudio)
        {
            if (s != null)
                s.UnPause();
        }
        pausedAudio.Clear();

        if (instance != null)
            instance.enabled = true;

        InMainMenu = false;
        Pause = false;
        Time.timeScale = 1f;

        if (gameScene.IsValid())
            SceneManager.SetActiveScene(gameScene);

        SceneManager.UnloadSceneAsync(menuScene);
    }

    // Hide everything the player sees and hears, without deactivating
    // objects (that would kill running coroutines such as the clock)
    static void HideGame(Scene scene)
    {
        hiddenBehaviours.Clear();
        pausedAudio.Clear();

        foreach (GameObject root in scene.GetRootGameObjects())
        {
            foreach (Camera c in root.GetComponentsInChildren<Camera>(true))
                Hide(c);

            foreach (AudioListener l in root.GetComponentsInChildren<AudioListener>(true))
                Hide(l);

            foreach (Canvas c in root.GetComponentsInChildren<Canvas>(true))
                Hide(c);

            foreach (EventSystem e in root.GetComponentsInChildren<EventSystem>(true))
                Hide(e);

            foreach (AudioSource s in root.GetComponentsInChildren<AudioSource>(true))
            {
                if (s.isPlaying)
                {
                    s.Pause();
                    pausedAudio.Add(s);
                }
            }
        }
    }

    static void Hide(Behaviour b)
    {
        if (b != null && b.enabled)
        {
            b.enabled = false;
            hiddenBehaviours.Add(b);
        }
    }
}
