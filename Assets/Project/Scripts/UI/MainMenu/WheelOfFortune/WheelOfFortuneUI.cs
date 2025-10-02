using System;
using System.Collections;
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
    [SerializeField] private float spinCooldownHours = 1f / 2f; // 30 min

    [Header("UI References")]
    [SerializeField] private Button wheelButton;
    [SerializeField] private Button spinButton;
    [SerializeField] private Button claimButton;
    [SerializeField] private GameObject wheelPanel;
    [SerializeField] private TextMeshProUGUI cooldownText;

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
    [SerializeField] private Color[] segmentColors = { Color.white, Color.gray };
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField, Range(0f, 1f)] private float highlightIntensity = 0.6f;
    [SerializeField] private float highlightScaleFactor = 1.1f;

    [Header("Segments")]
    [SerializeField] private SegmentData[] segments;
    [SerializeField] private GameObject segmentContentPrefab;

    private const string TIMER_KEY = "WheelSpin";

    private float currentAngle;
    private WheelSegmentUI[] segmentUIs;
    private int? currentWinningIndex;

    [Inject] private EconomyManager _economy;
    [Inject] private TimerManager _timerManager;

    private void Awake()
    {
        spinButton.onClick.AddListener(Spin);
        claimButton.onClick.AddListener(ClaimPrize);
        wheelButton.onClick.AddListener(ToggleWheel);

        wheelPanel.SetActive(false);
        claimButton.gameObject.SetActive(false);

        _timerManager.RegisterTimer(TIMER_KEY, spinCooldownHours);
        GenerateSegments();
    }

    private void Start()
    {
        StartCoroutine(UpdateCooldownRoutine());
    }

    private IEnumerator UpdateCooldownRoutine()
    {
        while (true)
        {
            UpdateCooldownUI();
            yield return new WaitForSeconds(1f);
        }
    }

    private void UpdateCooldownUI()
    {
        if (_timerManager.CanTrigger(TIMER_KEY))
        {
            wheelButton.interactable = true;
            cooldownText.text = "Spin Available!";
        }
        else
        {
            wheelButton.interactable = false;
            var remaining = _timerManager.GetRemaining(TIMER_KEY);
            if (remaining.TotalSeconds < 0) remaining = TimeSpan.Zero;

            cooldownText.text = $"{remaining.Hours:D2}:{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }
    }

    private void ToggleWheel()
    {
        bool newState = !wheelPanel.activeSelf;
        wheelPanel.SetActive(newState);

        if (newState && currentWinningIndex != null)
            HighlightWinningSegment();
    }

    public void Spin()
    {
        if (!_timerManager.CanTrigger(TIMER_KEY)) return;

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
        claimButton.interactable = true;
    }

    private void HighlightWinningSegment()
    {
        float normalizedAngle = (360f - (currentAngle % 360f)) % 360f;
        float pointerOffset = 90f;
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
        foreach (var seg in segmentUIs)
            seg.ResetHighlight();
        currentWinningIndex = null;
    }

    private void GenerateSegments()
    {
        foreach (Transform child in wheelTransform)
            DestroyImmediate(child.gameObject);

        int numberOfSegments = segments.Length;
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

            GameObject segGO = new GameObject($"Segment {i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(WheelSegmentUI));
            segGO.transform.SetParent(wheelTransform, false);

            RectTransform segRT = segGO.GetComponent<RectTransform>();
            segRT.sizeDelta = new Vector2(wheelRadius * 2f, wheelRadius * 2f);

            WheelSegmentUI segment = segGO.GetComponent<WheelSegmentUI>();
            segment.SetBaseColor(segmentColors[i % segmentColors.Length]);
            segment.BuildSegmentMesh(startAngle, endAngle, innerRadius, outerRadius,
                arcResolution, radialResolution, gapAngle, cutStartRadius);

            segmentUIs[i] = segment;

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

                float effectiveSpan = (endAngle - startAngle) - gapAngle;
                float width = 2f * rCenter * Mathf.Sin(0.5f * Mathf.Deg2Rad * effectiveSpan);

                Vector2 pos = new Vector2(Mathf.Cos(Mathf.Deg2Rad * midAngle), Mathf.Sin(Mathf.Deg2Rad * midAngle)) * rCenter;
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
        if (currentWinningIndex == null) return;

        int reward = segments[currentWinningIndex.Value].reward;
        _economy.AddCurrency(reward, true, this.GetComponent<RectTransform>());

        ResetHighlights();
        wheelPanel.SetActive(false);

        spinButton.gameObject.SetActive(true);
        claimButton.gameObject.SetActive(false);

        _timerManager.Trigger(TIMER_KEY);

        UpdateCooldownUI();
    }

    [Serializable]
    public class SegmentData
    {
        public string label;
        public Sprite icon;
        public int reward;
    }
}
