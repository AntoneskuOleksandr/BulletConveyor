using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zenject;

public class WheelOfFortuneUI : MonoBehaviour
{
    [Header("Spin Settings")]
    [SerializeField] private RectTransform wheelTransform;
    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float minSpinCounts = 3f;
    [SerializeField] private float maxSpinCounts = 6f;

    [Header("UI Buttons")]
    [SerializeField] private Button wheelButton; // toggle wheel panel
    [SerializeField] private Button spinButton;  // start spin
    [SerializeField] private Button claimButton; // claim prize

    [Header("Wheel Panel")]
    [SerializeField] private GameObject wheelPanel;

    [Header("Wheel Settings")]
    [SerializeField] private float wheelRadius = 300f;
    [SerializeField, Range(0f, 1f)] private float innerRadiusPercent = 0.2f;
    [SerializeField, Min(3)] private int arcResolution = 24;
    [SerializeField, Min(2)] private int radialResolution = 8;

    [Header("Segment Cut (Gap)")]
    [SerializeField, Range(0f, 20f)] private float gapAngle = 6f;
    [SerializeField, Range(0f, 1f)] private float gapStartPercent = 0.5f;

    [Header("Content Placement")]
    [SerializeField, Range(0f, 1f)] private float contentRadiusLerp = 0.5f;

    [Header("Segment Colors")]
    [SerializeField] private List<Color> segmentColors = new List<Color>() { Color.white, Color.gray };

    [Header("Segments")]
    [SerializeField] private SegmentData[] segments;

    [Header("UI Prefab")]
    [SerializeField] private GameObject segmentContentPrefab;

    [Header("Highlight Settings")]
    [SerializeField] private Color highlightColor = Color.yellow;
    [Range(0f, 1f)][SerializeField] private float highlightIntensity = 0.6f;
    [SerializeField] private float highlightScaleFactor = 1.1f;

    private float currentAngle;
    private WheelSegmentUI[] segmentUIs;
    private int? currentWinningIndex;

    [Inject] private EconomyManager _economy;

    private void Awake()
    {
        if (spinButton != null) spinButton.onClick.AddListener(Spin);
        if (claimButton != null) claimButton.onClick.AddListener(ClaimPrize);
        if (wheelButton != null) wheelButton.onClick.AddListener(ToggleWheel);

        if (wheelPanel != null)
            wheelPanel.SetActive(false);

        if (claimButton != null)
            claimButton.gameObject.SetActive(false); // изначально скрыта

        GenerateSegments();
    }

    private void ToggleWheel()
    {
        if (wheelPanel == null) return;

        bool newState = !wheelPanel.activeSelf;
        wheelPanel.SetActive(newState);

        if (newState) // открыли панель
        {
            if (currentWinningIndex != null)
                HighlightWinningSegment(); // восстановить подсветку
        }
    }


    public void Spin()
    {
        StopAllCoroutines();

        // состояния кнопок
        spinButton.gameObject.SetActive(false);
        claimButton.gameObject.SetActive(true);
        claimButton.interactable = false;

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

        // теперь можно забирать приз
        claimButton.interactable = true;
    }

    private void HighlightWinningSegment()
    {
        if (segmentUIs == null || segmentUIs.Length == 0) return;

        float normalizedAngle = (360f - (currentAngle % 360f)) % 360f;
        float pointerOffset = 90f; // верх
        float adjustedAngle = (normalizedAngle + pointerOffset) % 360f;

        float angleStep = 360f / segmentUIs.Length;
        int winningIndex = Mathf.FloorToInt(adjustedAngle / angleStep) % segmentUIs.Length;

        currentWinningIndex = winningIndex;

        for (int i = 0; i < segmentUIs.Length; i++)
        {
            if (i == winningIndex)
                segmentUIs[i].SetHighlight(highlightColor, highlightIntensity, highlightScaleFactor);
            else
                segmentUIs[i].ResetHighlight();
        }
    }

    private void ResetHighlights()
    {
        if (segmentUIs == null) return;
        foreach (var seg in segmentUIs)
            seg.ResetHighlight();
        currentWinningIndex = null;
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

            // фон
            GameObject segGO = new GameObject($"Segment {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentUI));
            segGO.transform.SetParent(wheelTransform, false);

            RectTransform segRT = segGO.GetComponent<RectTransform>();
            segRT.sizeDelta = new Vector2(wheelRadius * 2f, wheelRadius * 2f);

            WheelSegmentUI segment = segGO.GetComponent<WheelSegmentUI>();
            Color segColor = (segmentColors != null && segmentColors.Count > 0)
                ? segmentColors[i % segmentColors.Count]
                : Color.white;
            segment.SetBaseColor(segColor);
            segment.BuildSegmentMesh(startAngle, endAngle, innerRadius, outerRadius,
                arcResolution, radialResolution, gapAngle, cutStartRadius);

            segmentUIs[i] = segment;

            // контент
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
        if (currentWinningIndex == null)
        {
            Debug.Log("[WheelOfFortune] Приз не выбран!");
            return;
        }

        int reward = segments[currentWinningIndex.Value].reward;
        _economy.AddMoney(reward);

        Debug.Log($"[WheelOfFortune] Получена награда {reward} из сегмента {currentWinningIndex}");

        ResetHighlights();

        // закрыть меню
        if (wheelPanel != null)
            wheelPanel.SetActive(false);

        // восстановить кнопки
        spinButton.gameObject.SetActive(true);
        claimButton.gameObject.SetActive(false);
    }

    [Serializable]
    public class SegmentData
    {
        public string label;
        public Sprite icon;
        public int reward;
    }
}
