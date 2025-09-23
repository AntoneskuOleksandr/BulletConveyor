using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WheelOfFortuneUI : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float minSpinCounts = 3f;
    [SerializeField] private float maxSpinCounts = 6f;
    [SerializeField] private Button spinButton;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 300f;
    [SerializeField, Range(0f, 1f)] private float innerRadiusPercent = 0.2f;

    [Tooltip("Сколько точек на дуге. Чем больше — тем плавнее контуры.")]
    [SerializeField, Min(3)] private int arcResolution = 24;

    [Tooltip("Сколько радиальных слоёв. Нужно ≥ 2, иначе вырез не сформируется.")]
    [SerializeField, Min(2)] private int radialResolution = 8;

    [Header("Segment Cut (Gap)")]
    [Tooltip("Угол выреза на внешнем крае (в градусах). 0 — без выреза.")]
    [SerializeField, Range(0f, 20f)] private float gapAngle = 6f;

    [Tooltip("Глубина старта выреза (0..1 по толщине кольца). 0.5 = старт с середины.")]
    [SerializeField, Range(0f, 1f)] private float gapStartPercent = 0.5f;

    [Header("Content Placement")]
    [Tooltip("На какой доле толщины кольца размещать текст/иконку.")]
    [SerializeField, Range(0f, 1f)] private float contentRadiusLerp = 0.6f;

    [Header("Segment Colors")]
    [SerializeField] private List<Color> segmentColors = new List<Color>() { Color.white, Color.gray };

    [Header("Segments")]
    [SerializeField] private SegmentData[] segments;

    private float currentAngle;

    private void Awake()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(Spin);

        GenerateSegments();
    }

    public void Spin()
    {
        StopAllCoroutines();
        StartCoroutine(SpinWheel());
    }

    private IEnumerator SpinWheel()
    {
        float elapsed = 0f;
        float startAngle = currentAngle;
        float endAngle = startAngle + 360f * UnityEngine.Random.Range(minSpinCounts, maxSpinCounts);

        while (elapsed < spinDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / spinDuration);
            currentAngle = Mathf.Lerp(startAngle, endAngle, spinCurve.Evaluate(t));
            wheelTransform.localEulerAngles = new Vector3(0, 0, currentAngle);
            yield return null;
        }

        currentAngle = endAngle;
        wheelTransform.localEulerAngles = new Vector3(0, 0, currentAngle);
    }

    private void GenerateSegments()
    {
        if (wheelTransform == null) return;

        foreach (Transform child in wheelTransform)
            DestroyImmediate(child.gameObject);

        int numberOfSegments = segments != null ? segments.Length : 0;
        if (numberOfSegments == 0) return;

        float angleStep = 360f / numberOfSegments;
        float innerRadius = wheelRadius * innerRadiusPercent;
        float outerRadius = wheelRadius;

        float cutStartRadius = Mathf.Lerp(innerRadius, outerRadius, Mathf.Clamp01(gapStartPercent));

        for (int i = 0; i < numberOfSegments; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = (i + 1) * angleStep;

            GameObject segGO = new GameObject($"Segment {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentUI));
            segGO.transform.SetParent(wheelTransform, false);

            RectTransform rt = segGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(wheelRadius * 2f, wheelRadius * 2f);

            WheelSegmentUI segment = segGO.GetComponent<WheelSegmentUI>();

            // выбираем цвет из списка по индексу
            Color segColor = (segmentColors != null && segmentColors.Count > 0)
                ? segmentColors[i % segmentColors.Count]
                : Color.white;

            segment.SetColor(segColor);

            segment.BuildSegmentMesh(
                startAngle, endAngle,
                innerRadius, outerRadius,
                arcResolution, radialResolution,
                gapAngle, cutStartRadius
            );

            // позиция текста/иконки
            float midAngle = (startAngle + endAngle) * 0.5f;
            float rad = Mathf.Deg2Rad * midAngle;
            float contentRadius = Mathf.Lerp(innerRadius, outerRadius, Mathf.Clamp01(contentRadiusLerp));
            Vector2 contentPos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * contentRadius;

            if (!string.IsNullOrEmpty(segments[i].label))
            {
                GameObject textGO = new GameObject("Text", typeof(TextMeshProUGUI));
                textGO.transform.SetParent(segGO.transform, false);

                var tmp = textGO.GetComponent<TextMeshProUGUI>();
                tmp.text = segments[i].label;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.fontSize = 32;
                tmp.color = Color.black;

                RectTransform textRT = tmp.GetComponent<RectTransform>();
                textRT.sizeDelta = new Vector2(220, 70);
                textRT.anchoredPosition = contentPos;
                textRT.localRotation = Quaternion.identity;
            }

            if (segments[i].icon != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(Image));
                iconGO.transform.SetParent(segGO.transform, false);

                var img = iconGO.GetComponent<Image>();
                img.sprite = segments[i].icon;
                img.preserveAspect = true;

                RectTransform iconRT = img.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(80, 80);
                iconRT.anchoredPosition = contentPos * 0.9f;
                iconRT.localRotation = Quaternion.identity;
            }
        }
    }

    [Serializable]
    public class SegmentData
    {
        public string label;
        public Sprite icon;
    }
}

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
