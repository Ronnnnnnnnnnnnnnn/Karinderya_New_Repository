using UnityEngine;
using UnityEngine.UI;

public class TutorialPages : MonoBehaviour
{
    public RawImage tutorialImage;

    public Texture[] pages;

    private int currentPage = 0;

    void Start()
    {
        ShowPage();
    }

    void ShowPage()
    {
        tutorialImage.texture = pages[currentPage];
    }

    public void NextPage()
    {
        if (currentPage < pages.Length - 1)
        {
            currentPage++;
            ShowPage();
        }
    }

    public void PreviousPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            ShowPage();
        }
    }
}