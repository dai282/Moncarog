using UnityEngine;

public class MainMenu : MonoBehaviour
{

    [Header("References")]
    public GameObject mainMenu;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainMenu.SetActive(true);
    }

    public void NewGame()
    {

    }

    public void ContinueGame()
    {
        mainMenu.SetActive(false);
        Time.timeScale = 1f;
    }

}
