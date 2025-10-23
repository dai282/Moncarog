using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    public Image targetImage;
    public Sprite[] tutorialPics;

    private int index = 0;

    [Header("References")]
    public GameObject tutorialPage;
    public GameObject mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tutorialPage.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Tutorial()
    {
        tutorialPage.SetActive(true);
        mainMenu.SetActive(false);
    }

    public void MainMenu()
    {
        tutorialPage.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void Next()
    {
        index = (index + 1) % tutorialPics.Length;
        targetImage.sprite = tutorialPics[index];
    }

    public void Previous()
    {
        if (index > 0)
        {
            index = (index - 1) % tutorialPics.Length;
            targetImage.sprite = tutorialPics[index];
        }
        
    }

}
