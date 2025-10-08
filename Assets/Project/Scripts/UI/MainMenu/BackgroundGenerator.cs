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

    public void SetColor(Color topColor, Color bottomColor)
    {
        if (_backgroundTexture == null)
            CreateTexture();

        _backgroundTexture.SetPixels(new Color[] { bottomColor, topColor });
        _backgroundTexture.Apply();
        _image.texture = _backgroundTexture;
    }

    private void CreateTexture()
    {
        if (_backgroundTexture != null)
            Destroy(_backgroundTexture);

        _backgroundTexture = new Texture2D(1, 2);
        _backgroundTexture.wrapMode = TextureWrapMode.Clamp;
        _backgroundTexture.filterMode = FilterMode.Bilinear;

        _image = GetComponent<RawImage>();
    }
}
