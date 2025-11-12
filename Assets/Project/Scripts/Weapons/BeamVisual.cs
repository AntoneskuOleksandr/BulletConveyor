using UnityEngine;

public class BeamVisual : MonoBehaviour
{
    private LineRenderer _line;
    private Material _material;
    private Color _color;
    private float _width;
    private float _length;
    private float _lifeTime;
    private float _additionalLength = 0.5f;

    public void Configure(Color color, float width, float length, float lifeTime, Transform parent)
    {
        _color = color;
        _width = width;
        _length = length;
        _lifeTime = lifeTime;

        transform.SetParent(parent, worldPositionStays: false);

        CreateLineRenderer();

        float scaledLength = (_length + _additionalLength) / transform.lossyScale.z;
        SetLength(scaledLength);

        Destroy(gameObject, _lifeTime);
    }

    public void UpdateLength(float newLength)
    {
        if (_line == null)
            return;

        float scaledLength = (newLength + _additionalLength) / transform.lossyScale.z;
        SetLength(scaledLength);
    }

    private void SetLength(float scaledLength)
    {
        _line.positionCount = 2;
        _line.SetPosition(0, Vector3.zero);
        _line.SetPosition(1, Vector3.forward * scaledLength);
    }

    private void CreateLineRenderer()
    {
        _line = gameObject.AddComponent<LineRenderer>();

        _material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        _material.color = _color;

        _line.material = _material;
        _line.startWidth = _width;
        _line.endWidth = _width;
        _line.useWorldSpace = false;
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
        _line.alignment = LineAlignment.View;
        _line.positionCount = 2;
    }
}
