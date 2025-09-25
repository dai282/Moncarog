using UnityEngine;

public class MapToggleButton : MonoBehaviour
{
    public GameObject mapRoot; // Assign this in the Inspector

    public void OnClickToggle()
    {
        if (mapRoot != null)
        {
            // Toggles the active state of the mapRoot GameObject
            mapRoot.SetActive(!mapRoot.activeSelf);
        }
    }
}