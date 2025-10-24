using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public GameObject mainMenu;
    public bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f; 
        isPaused = false;
    }

    // Called by a UI Button
    public void PauseGame()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        pauseButton.SetActive(false);
    }

    // Called by a UI Button
    public void ResumeGame()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        pauseButton.SetActive(true);
    }

    public void MainMenu()
    {
        pauseMenu.SetActive(false);
        mainMenu.SetActive(true);
        isPaused = false;
        pauseButton.SetActive(true);
    }
}