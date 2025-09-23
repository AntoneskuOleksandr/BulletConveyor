using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Random = UnityEngine.Random;

public class WheelOfFortuneUI : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private AnimationCurve spinCurve;
    [SerializeField] private float minSpinCounts = 3f;
    [SerializeField] private float maxSpinCounts = 6f;
    [SerializeField] private Button spinButton;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 300f;
    [SerializeField, Range(0f, 1f)] private float innerRadiusPercent = 0.2f;
    [SerializeField] private int arcResolution = 20;

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
        foreach (Transform child in wheelTransform)
            Destroy(child.gameObject);

        int numberOfSegments = segments.Length;
        if (numberOfSegments == 0) return;

        float angleStep = 360f / numberOfSegments;
        float innerRadius = wheelRadius * innerRadiusPercent;
        float outerRadius = wheelRadius;

        for (int i = 0; i < numberOfSegments; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = (i + 1) * angleStep;

            // создаём объект сегмента
            GameObject segGO = new GameObject("Segment " + i, typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentUI));
            segGO.transform.SetParent(wheelTransform, false);

            RectTransform rt = segGO.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(wheelRadius * 2, wheelRadius * 2);

            WheelSegmentUI segment = segGO.GetComponent<WheelSegmentUI>();
            segment.BuildSegmentMesh(startAngle, endAngle, innerRadius, outerRadius, arcResolution);
            segment.SetColor(segments[i].color);

            // вычисляем центр сегмента (для текста и иконки)
            float midAngle = (startAngle + endAngle) / 2f;
            float rad = Mathf.Deg2Rad * midAngle;
            Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * ((innerRadius + outerRadius) / 2);

            // добавляем текст
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
                textRT.sizeDelta = new Vector2(200, 60);
                textRT.anchoredPosition = pos;
                textRT.localRotation = Quaternion.identity; // текст всегда ровный
            }

            // добавляем иконку
            if (segments[i].icon != null)
            {
                GameObject iconGO = new GameObject("Icon", typeof(Image));
                iconGO.transform.SetParent(segGO.transform, false);

                var img = iconGO.GetComponent<Image>();
                img.sprite = segments[i].icon;
                img.preserveAspect = true;

                RectTransform iconRT = img.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(80, 80);
                iconRT.anchoredPosition = pos * 0.7f; // ближе к центру
                iconRT.localRotation = Quaternion.identity;
            }
        }
    }

    /// <summary>
    /// Данные для одного сегмента
    /// </summary>
    [Serializable]
    public class SegmentData
    {
        public string label;
        public Sprite icon;
        public Color color = Color.white;
    }
}

public class WheelSegmentUI : MaskableGraphic
{
    private List<Vector3> verts;
    private Color segColor = Color.white;

    public void SetColor(Color c) => segColor = c;

    public void BuildSegmentMesh(float startAngle, float endAngle, float innerRadius, float outerRadius, int arcResolution)
    {
        verts = new List<Vector3>();
        float step = (endAngle - startAngle) / arcResolution;

        // внутренняя дуга
        for (int i = 0; i <= arcResolution; i++)
        {
            float angle = Mathf.Deg2Rad * (startAngle + step * i);
            verts.Add(new Vector3(Mathf.Cos(angle) * innerRadius, Mathf.Sin(angle) * innerRadius));
        }

        // внешняя дуга
        for (int i = 0; i <= arcResolution; i++)
        {
            float angle = Mathf.Deg2Rad * (startAngle + step * i);
            verts.Add(new Vector3(Mathf.Cos(angle) * outerRadius, Mathf.Sin(angle) * outerRadius));
        }

        SetVerticesDirty();
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (verts == null || verts.Count == 0) return;

        int innerCount = verts.Count / 2;

        for (int i = 0; i < verts.Count; i++)
            vh.AddVert(verts[i], segColor, Vector2.zero);

        for (int i = 0; i < innerCount - 1; i++)
        {
            int i0 = i;
            int i1 = i + 1;
            int i2 = innerCount + i;
            int i3 = innerCount + i + 1;

            vh.AddTriangle(i0, i2, i1);
            vh.AddTriangle(i1, i2, i3);
        }
    }
}
