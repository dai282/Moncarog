using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [Header("References")]
    public GameObject pauseMenu;
    public GameObject pauseButton;
    public bool isPaused;

    void Start()
    {
        pauseMenu.SetActive(false);
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
}