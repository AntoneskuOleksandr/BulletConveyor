using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WheelSegmentUI : MaskableGraphic
{
    private List<Vector3> _verts;
    private int _cols;
    private int _rows;
    private Color _segColor = Color.white;

    public void SetColor(Color c) => _segColor = c;

    public void BuildSegmentMesh(
        float startAngle, float endAngle,
        float innerRadius, float outerRadius,
        int arcResolution, int radialResolution,
        float gapAngle, float cutStartRadius
    )
    {
        _cols = Mathf.Max(3, arcResolution) + 1;
        _rows = Mathf.Max(2, radialResolution) + 1;

        _verts = new List<Vector3>(_cols * _rows);

        float fullSpan = endAngle - startAngle;
        float maxHalfGap = Mathf.Min(gapAngle * 0.5f, Mathf.Max(0.0001f, (fullSpan * 0.5f - 0.5f)));

        for (int r = 0; r < _rows; r++)
        {
            float tR = (float)r / (_rows - 1);
            float radius = Mathf.Lerp(innerRadius, outerRadius, tR);

            float halfGap = 0f;
            if (radius > cutStartRadius && gapAngle > 0f)
            {
                float t = Mathf.InverseLerp(cutStartRadius, outerRadius, radius);
                halfGap = Mathf.Min(maxHalfGap, (gapAngle * 0.5f) * t);
            }

            float a0 = startAngle + halfGap;
            float a1 = endAngle - halfGap;
            if (a1 < a0) { float mid = (startAngle + endAngle) * 0.5f; a0 = mid; a1 = mid; }

            float step = (a1 - a0) / (_cols - 1);

            for (int c = 0; c < _cols; c++)
            {
                float angDeg = a0 + step * c;
                float ang = Mathf.Deg2Rad * angDeg;
                _verts.Add(new Vector3(Mathf.Cos(ang) * radius, Mathf.Sin(ang) * radius));
            }
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (_verts == null || _verts.Count == 0 || _cols < 2 || _rows < 2) return;

        for (int i = 0; i < _verts.Count; i++)
            vh.AddVert(_verts[i], _segColor, Vector2.zero);

        int Index(int r, int c) => r * _cols + c;

        for (int r = 0; r < _rows - 1; r++)
        {
            for (int c = 0; c < _cols - 1; c++)
            {
                int v00 = Index(r, c);
                int v01 = Index(r, c + 1);
                int v10 = Index(r + 1, c);
                int v11 = Index(r + 1, c + 1);

                vh.AddTriangle(v00, v10, v01);
                vh.AddTriangle(v01, v10, v11);
            }
        }
    }
}