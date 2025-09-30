using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class BackgroundGenerator : MonoBehaviour
{
    private RawImage _image;
    private Texture2D _backgroundTexture;

    private void Awake()
    {
        _image = GetComponent<RawImage>();

        _backgroundTexture = new Texture2D(1, 2);
        _backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        _backgroundTexture.filterMode = FilterMode.Bilinear;
    }

    public void SetColor(Color topColor, Color bottomColor)
    {
        _backgroundTexture.SetPixels(new Color[] { topColor, bottomColor });
        _backgroundTexture.Apply();
        _image.texture = _backgroundTexture;
    }
}
