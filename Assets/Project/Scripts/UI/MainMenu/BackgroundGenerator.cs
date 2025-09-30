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

    public void SetColor(Color color1, Color color2)
    {
        _backgroundTexture.SetPixels(new Color[] { color1, color2 });
        _backgroundTexture.Apply();
        _image.texture = _backgroundTexture;
    }
}
