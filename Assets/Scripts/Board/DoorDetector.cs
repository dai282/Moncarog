using UnityEngine;

public class DoorDetector : MonoBehaviour
{
    [Header("Optional Settings")]
    public int doorIndex; // 0 for left, 1 for right (or single door)
    //public Transform destination;

    public void OnPlayerEnter()
    {
        Debug.Log($"Player entered door ");

        GameManager.Instance.PlayerEnteredDoor(this);

    }
}