using UnityEngine;

public class BeamVisual : MonoBehaviour
{
    private LineRenderer _line;
    private Material _material;
    private Color _color;
    private float _width;
    private float _length;
    private float _lifeTime;

    public void Configure(Color color, float width, float length, float lifeTime)
    {
        _color = color;
        _width = width;
        _length = length;
        _lifeTime = lifeTime;

        CreateLineRenderer();

        // выставляем позиции сразу
        _line.SetPosition(0, Vector3.zero);
        _line.SetPosition(1, Vector3.forward * _length);

        // уничтожаем после lifeTime
        Destroy(gameObject, _lifeTime);
    }

    private void CreateLineRenderer()
    {
        _line = gameObject.AddComponent<LineRenderer>();

        _material = new Material(Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Sprites/Default"));
        _material.color = _color;

        _line.material = _material;
        _line.startWidth = _width;
        _line.endWidth = _width;
        _line.useWorldSpace = false; // важно! локальные координаты относительно FirePoint
        _line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _line.receiveShadows = false;
        _line.alignment = LineAlignment.View;
        _line.positionCount = 2;
    }
}
