using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Vector3 offset;
    public Vector3 lockPointTarget;

    // The different modes for the camera
    private enum CameraMode { Following, Locked }
    private CameraMode currentMode = CameraMode.Following;


    private void Awake()
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        switch (currentMode)
        {
            case CameraMode.Following:
                transform.position = target.position + offset;
                break;
            case CameraMode.Locked:
                
                break;
        }
    }

    public void LockToPoint()
    {
        if (lockPointTarget == null) return;

        currentMode = CameraMode.Locked;
        transform.position = lockPointTarget;
    }

    public void ResumeFollowing()
    {
        currentMode = CameraMode.Following;
    }
}
