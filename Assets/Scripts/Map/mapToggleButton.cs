using UnityEngine;

public class MapToggleButton : MonoBehaviour
{
    public GameObject mapRoot;

    private void Start()
    {
        // Check the panel is hidden on start
        if (mapRoot != null)
        {
            mapRoot.SetActive(false);
        }
    }

    public void OnClickToggle()
    {
        // Toggles the active state of the mapRoot GameObject
        mapRoot.SetActive(!mapRoot.activeSelf);
    }
}