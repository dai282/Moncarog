using UnityEngine;

public class DoorDetector : MonoBehaviour
{
    [Header("Optional Settings")]
    public string doorID;
    public Transform destination;

    public void OnPlayerEnter(GameObject player)
    {
        Debug.Log($"Player entered door {doorID}");

        if (destination != null)
        {
            player.transform.position = destination.position;
        }

    }
}