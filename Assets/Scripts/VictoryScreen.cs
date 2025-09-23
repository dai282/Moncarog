using UnityEngine;

public class VictoryScreen : MonoBehaviour
{

    [Header("References")]
    public GameObject victoryScreen;
    public GameObject mainMenu;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        victoryScreen.SetActive(false);
    }

    public void Victory()
    {
        victoryScreen.SetActive(true);
    }

    public void MainMenu()
    {
        victoryScreen.SetActive(false);
        mainMenu.SetActive(true);
    }
}
