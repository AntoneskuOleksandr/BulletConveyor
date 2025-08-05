using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public bool isFree;
    public int index;

    private void Awake()
    {
        isFree = true;
    }
}
