using UnityEngine;

public class LoseScreenUI : MonoBehaviour
{
    [Header("References")]
    public GameObject loseScreen;
    public bool isGameOver;

    void Start()
    {
        loseScreen.SetActive(false);
    }

    // Called by CombatHandler when all Moncargs are defeated
    public void ShowGameOverScreen()
    {
        loseScreen.SetActive(true);
        isGameOver = true;
        Debug.Log("Lose screen shown - Game Over");
    }

    // Called by a UI Button
    public void NewGame()
    {
        Debug.Log("New Game button clicked");
        loseScreen.SetActive(false);
        isGameOver = false;
        
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
            Debug.Log("Starting new game via LoseScreenUI");
        }
        else
        {
            Debug.LogError("GameManager.Instance is null!");
        }
    }

    public void HideGameOverScreen()
    {
        loseScreen.SetActive(false);
        isGameOver = false;
        Debug.Log("Lose screen hidden");
    }
}