using UnityEngine;
using UnityEngine.EventSystems;

public class MovementUI : MonoBehaviour
{
    public GameObject[] buttons; // drag & drop buttons in Inspector
    public static MovementUI Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern to ensure only one instance of GameManager exists
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void DisableAllButtons()
    {
        foreach (GameObject btn in buttons)
        {
            // Manually fire a pointer up event to stop player from moving
            ExecuteEvents.Execute<IPointerUpHandler>(
                btn,
                new PointerEventData(EventSystem.current),
                (handler, eventData) => handler.OnPointerUp((PointerEventData)eventData)
            );

            btn.SetActive(false);
        }
    }

    public void EnableAllButtons()
    {
        foreach (GameObject btn in buttons)
        {
            btn.SetActive(true);
        }
    }
}