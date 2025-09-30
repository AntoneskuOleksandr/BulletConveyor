using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class BackgroundGenerator : MonoBehaviour
{
    [Header("Default Colors")]
    [SerializeField] private Color topColor;
    [SerializeField] private Color bottomColor;

    private RawImage _image;
    private Texture2D _backgroundTexture;

    private void Awake()
    {
        _image = GetComponent<RawImage>();

        _backgroundTexture = new Texture2D(1, 2);
        _backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        _backgroundTexture.filterMode = FilterMode.Bilinear;

        SetColor(topColor, bottomColor);
    }

    public void SetColor(Color topColor, Color bottomColor)
    {
        _backgroundTexture.SetPixels(new Color[] { bottomColor, topColor });
        _backgroundTexture.Apply();
        _image.texture = _backgroundTexture;
    }
}
