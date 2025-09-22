using UnityEngine;
using UnityEngine.EventSystems;

public class MovementUI : MonoBehaviour
{
    public GameObject[] buttons; // drag & drop buttons in Inspector

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