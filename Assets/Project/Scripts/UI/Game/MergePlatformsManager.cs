using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MergePlatformsManager : MonoBehaviour
{
    public Platform[] platforms;

    private void Start()
    {
        for (int i = 0; i < platforms.Length; i++)
            platforms[i].index = i;
    }

    public int GetFreePlatformIndex()
    {
        for (int i = 0; i < platforms.Length; i++)
            if (platforms[i].isFree)
                return i;

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
        return platforms[GetFreePlatformIndex()].transform.position;
    }
}
