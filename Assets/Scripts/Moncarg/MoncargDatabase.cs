using UnityEngine;

public class MoncargDatabase : MonoBehaviour
{
    public GameObject[] moncargs;

    public void resetMoncargDatabase()
    {
        foreach (GameObject moncarg in moncargs)
        {
            StoredMoncarg storedMoncarg = moncarg.GetComponent<StoredMoncarg>();
            if (storedMoncarg != null)
            {
                storedMoncarg.Details.resetData();
            }
        }
    }
}
