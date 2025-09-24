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

    [Header("UI Buttons")]
    [SerializeField] private Button wheelButton; // open wheel panel
    [SerializeField] private Button spinButton; // start spin
    [SerializeField] private Button claimButton; // claim prize

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
    [Tooltip("Где центрировать контейнер контента по толщине кольца (0..1).")]
    [SerializeField, Range(0f, 1f)] private float contentRadiusLerp = 0.5f; // лучше по центру

    [Header("Segment Colors")]
    [SerializeField] private List<Color> segmentColors = new List<Color>() { Color.white, Color.gray };

    [Header("Segments")]
    [SerializeField] private SegmentData[] segments;

    [Header("UI Prefab")]
    [Tooltip("Префаб с TextMeshProUGUI и/или Image. Без фонового Mesh.")]
    [SerializeField] private GameObject segmentContentPrefab;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [Range(0f, 1f)][SerializeField] private float highlightIntensity = 0.6f;
    [SerializeField] private float highlightScaleFactor = 1.1f;

    private float currentAngle;
    private WheelSegmentUI[] segmentUIs;

    private void Awake()
    {
        if (spinButton != null)
            spinButton.onClick.AddListener(Spin);

        if (claimButton != null)
            claimButton.onClick.AddListener(ClaimPrize);

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

        HighlightWinningSegment();
    }

    private void HighlightWinningSegment()
    {
        if (segmentUIs == null || segmentUIs.Length == 0) return;

        // угол в локальных координатах
        float normalizedAngle = (360f - (currentAngle % 360f)) % 360f;

        // сдвигаем так, чтобы 0-й сегмент был сверху
        float pointerOffset = 90f; // "указатель" на 12 часов
        float adjustedAngle = (normalizedAngle + pointerOffset) % 360f;

        float angleStep = 360f / segmentUIs.Length;
        int winningIndex = Mathf.FloorToInt(adjustedAngle / angleStep) % segmentUIs.Length;

        // сброс + подсветка
        for (int i = 0; i < segmentUIs.Length; i++)
        {
            if (i == winningIndex)
                segmentUIs[i].SetHighlight(highlightColor, highlightIntensity, highlightScaleFactor);
            else
                segmentUIs[i].ResetHighlight();
        }

        Debug.Log($"[WheelOfFortune] Победил сегмент {winningIndex}");
    }

    private void GenerateSegments()
    {
        if (wheelTransform == null) return;

        foreach (Transform child in wheelTransform)
            DestroyImmediate(child.gameObject);

        int numberOfSegments = (segments != null) ? segments.Length : 0;
        if (numberOfSegments == 0) return;

        segmentUIs = new WheelSegmentUI[numberOfSegments];

        float angleStep = 360f / numberOfSegments;
        float innerRadius = wheelRadius * innerRadiusPercent;
        float outerRadius = wheelRadius;

        float cutStartRadius = Mathf.Lerp(innerRadius, outerRadius, Mathf.Clamp01(gapStartPercent));

        for (int i = 0; i < numberOfSegments; i++)
        {
            float startAngle = i * angleStep;
            float endAngle = (i + 1) * angleStep;
            float midAngle = (startAngle + endAngle) * 0.5f;

            // === фон (mesh) ===
            GameObject segGO = new GameObject($"Segment {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentUI));
            segGO.transform.SetParent(wheelTransform, false);

            RectTransform segRT = segGO.GetComponent<RectTransform>();
            segRT.sizeDelta = new Vector2(wheelRadius * 2f, wheelRadius * 2f);

            WheelSegmentUI segment = segGO.GetComponent<WheelSegmentUI>();
            Color segColor = (segmentColors != null && segmentColors.Count > 0)
                ? segmentColors[i % segmentColors.Count]
                : Color.white;
            segment.SetBaseColor(segColor);
            segment.BuildSegmentMesh(
                startAngle, endAngle,
                innerRadius, outerRadius,
                arcResolution, radialResolution,
                gapAngle, cutStartRadius
            );
            segmentUIs[i] = segment;

            // === контент ===
            if (segmentContentPrefab != null)
            {
                GameObject contentGO = Instantiate(segmentContentPrefab, segGO.transform, false);
                contentGO.name = "Content";
                RectTransform contentRT = contentGO.GetComponent<RectTransform>();

                contentRT.anchorMin = contentRT.anchorMax = new Vector2(0.5f, 0.5f);
                contentRT.pivot = new Vector2(0.5f, 0.5f);
                contentRT.localScale = Vector3.one;

                float rCenter = Mathf.Lerp(innerRadius, outerRadius, Mathf.Clamp01(contentRadiusLerp));
                float halfHeight = Mathf.Min(rCenter - innerRadius, outerRadius - rCenter);
                float height = Mathf.Max(0f, 2f * halfHeight);

                float t = 0f;
                if (gapAngle > 0f && rCenter > cutStartRadius)
                    t = Mathf.Clamp01((rCenter - cutStartRadius) / (outerRadius - cutStartRadius));
                float effectiveSpan = Mathf.Max(0f, (endAngle - startAngle) - (gapAngle * t));

                float width = 2f * rCenter * Mathf.Sin(0.5f * Mathf.Deg2Rad * effectiveSpan);

                float rad = Mathf.Deg2Rad * midAngle;
                Vector2 pos = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * rCenter;
                float zRot = midAngle - 90f;

                contentRT.sizeDelta = new Vector2(width, height);
                contentRT.anchoredPosition = pos;
                contentRT.localRotation = Quaternion.Euler(0, 0, zRot);

                var tmp = contentGO.GetComponentInChildren<TextMeshProUGUI>();
                if (tmp != null) tmp.text = segments[i].label;

                var img = contentGO.GetComponentInChildren<Image>();
                if (img != null)
                {
                    img.sprite = segments[i].icon;
                    img.preserveAspect = true;
                }
            }
        }
    }

    private void ClaimPrize()
    {
        Debug.Log("[WheelOfFortune] Приз получен!");
    }

    [Serializable]
    public class SegmentData
    {
        public string label;
        public Sprite icon;
    }
}
