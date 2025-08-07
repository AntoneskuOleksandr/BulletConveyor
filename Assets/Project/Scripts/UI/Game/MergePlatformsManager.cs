using UnityEngine;

public class MergePlatformsManager : MonoBehaviour
{
    [SerializeField] private Platform[] platforms;

    private void Start()
    {
        for (int i = 0; i < platforms.Length; i++)
            platforms[i].index = i;
    }

    public int GetFreePlatformIndex()
    {
        if (platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("No platforms available.");
            return -1;
        }

        for (int i = 0; i < platforms.Length; i++)
        {
            if (platforms[i].isFree)
            {
                return i;
            }
        }

        return -1;
    }

    public bool IsFreePlatform()
    {
        if (GetFreePlatformIndex() == -1)
            return false;

        return true;
    }

    public Vector3 GetFreePlatformPosition()
    {
        if (platforms == null || platforms.Length == 0)
        {
            Debug.LogWarning("No platforms available to get position.");
            return Vector3.zero;
        }

        return platforms[GetFreePlatformIndex()].transform.position;
    }

    public Vector3 GetPlatformPositionByIndex(int index)
    {
        return platforms[index].transform.position;
    }

    public void SetPlatformBusy(int index)
    {
        if (index < 0 || index >= platforms.Length)
        {
            Debug.LogWarning("Invalid platform index: " + index);
            return;
        }
        platforms[index].isFree = false;
    }

    public void SetPlatformFree(int index)
    {
        if (index < 0 || index >= platforms.Length)
        {
            Debug.LogWarning("Invalid platform index: " + index);
            return;
        }
        platforms[index].isFree = true;
    }
}
