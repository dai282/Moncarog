using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Transform target;
    private Vector3 offset;
    public Vector3 lockPointTarget;

    // The different modes for the camera
    private enum CameraMode { Following, Locked }
    private CameraMode currentMode = CameraMode.Following;

    //setting boundary limits for the camera
    public float minX, maxX;
    public float minY, maxY;


    private void Awake()
    {
        offset = transform.position - target.position;
    }

    private void LateUpdate()
    {
        switch (currentMode)
        {
            case CameraMode.Following:
                Vector3 boundPosition = target.position + offset;

                //applying boundaries to camera position
                boundPosition.x = Mathf.Clamp(boundPosition.x, minX, maxX);
                boundPosition.y = Mathf.Clamp(boundPosition.y, minY, maxY);

                transform.position = boundPosition;
               
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
