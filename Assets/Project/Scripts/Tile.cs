using UnityEngine;

public class Tile : MonoBehaviour
{
    public bool isAvailable;
    public Vector2Int indexInMatrix;

    private Material material;
    private Color highLightColor = Color.green;
    private Color startColor;

    private void Awake()
    {
        material = GetComponent<MeshRenderer>().material;
        startColor = material.color;
        isAvailable = true;
    }

    public void HighLight()
    {
        material.color = highLightColor;
    }

    public void SetDefaultColor()
    {
        material.color = startColor;
    }
}
