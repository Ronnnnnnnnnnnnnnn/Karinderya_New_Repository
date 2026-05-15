using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToScene : MonoBehaviour
{
    public void GoToScene()
    {
       SceneManager.LoadScene("MainMenuScene");
    }
}
