using UnityEngine;

public class VictoryScreen : MonoBehaviour
{

    [Header("References")]
    public GameObject victoryScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        victoryScreen.SetActive(false);
    }

    public void Victory()
    {
        victoryScreen.SetActive(true);
    }
}
