using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ProgressBar : MonoBehaviour
{
    [SerializeField] private Image filledPart;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject waveIconPrefab;

    [SerializeField] private TMP_Text nextWaveTimer;

    [Header("Wave Icons Sprites")]
    [SerializeField] private Sprite startedWave;
    [SerializeField] private Sprite upgradeWave;
    [SerializeField] private Sprite upcomingWave;
    [SerializeField] private Sprite finishWave;

    [Inject] private WaveManager _waveManager;

    private List<Image> _waveIcons = new List<Image>();
    private bool _scrollStarted = false;

    private float _initialFill;
    private float _targetFill;   // целевой fillAmount
    private float _targetScroll; // целевой scroll

    [SerializeField] private float fillSmoothSpeed = 5f;   // чем больше — тем быстрее анимация
    [SerializeField] private float scrollSmoothSpeed = 5f; // отдельная скорость для скролла

    private void Awake()
    {
        GenerateProgressBar();

        _waveManager.OnWaveRoutineStarted += (timeBetweenWaves) => StartCoroutine(FillInitialEmptyArea(timeBetweenWaves));

        _waveManager.OnWaveProgress += (waveProgress, waveIndex) =>
        {
            // целевое значение fillAmount (учитывает левую пустую область)
            _targetFill = _initialFill + (waveIndex - 1 + waveProgress) / (_waveManager.TotalWaves + 2f);

            if (_scrollStarted)
                _targetScroll = (waveIndex - 1 + waveProgress) / (float)_waveManager.TotalWaves;
        };

        _waveManager.OnWaveStarted += (waveIndex, _) =>
        {
            MarkWaveAsStarted(waveIndex - 1);
            _scrollStarted = true;
        };

        _waveManager.OnTimerBetweenWavesStarted += (waveIndex, timeBetweenWaves) =>
        {
            StartCoroutine(WaveCountdownText(timeBetweenWaves));
        };
    }

    private void Update()
    {
        // Плавно приближаем fillAmount к targetFill
        filledPart.fillAmount = Mathf.Lerp(filledPart.fillAmount, _targetFill, Time.deltaTime * fillSmoothSpeed);

        // Плавно приближаем scroll к targetScroll (если скролл активен)
        if (_scrollStarted)
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(scrollRect.horizontalNormalizedPosition, _targetScroll, Time.deltaTime * scrollSmoothSpeed);
    }

    private IEnumerator WaveCountdownText(float countdownTime)
    {
        while (countdownTime > 0)
        {
            nextWaveTimer.text = $"Wave starting in <color=#00ffff>{countdownTime:F1} secs</color>";
            countdownTime -= Time.deltaTime;
            yield return null;
        }

        nextWaveTimer.text = "";
    }

    private IEnumerator FillInitialEmptyArea(float duration)
    {
        float target = 1f / (_waveManager.TotalWaves + 2f);
        float start = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _targetFill = Mathf.Lerp(start, target, elapsed / duration);
            filledPart.fillAmount = _targetFill;
            yield return null;
        }

        _targetFill = target;
        filledPart.fillAmount = target;
        _initialFill = target;
    }

    private void GenerateProgressBar()
    {
        float spaceBetweenIcons = GetComponent<RectTransform>().rect.width / 2f;
        content.sizeDelta = new Vector2(spaceBetweenIcons * (_waveManager.TotalWaves + 2), content.sizeDelta.y);

        for (int i = 1; i <= _waveManager.TotalWaves + 1; i++)
        {
            RectTransform waveIcon = Instantiate(waveIconPrefab, content).GetComponent<RectTransform>();
            _waveIcons.Add(waveIcon.GetComponent<Image>());

            if (i % 2 == 0 && i != _waveManager.TotalWaves + 1)
                _waveIcons[i - 1].sprite = upgradeWave;

            waveIcon.localScale = Vector3.one;
            waveIcon.anchoredPosition = new Vector2(spaceBetweenIcons * i, 0f);
            waveIcon.gameObject.name = $"WaveIcon_{i}";

            TMP_Text text = waveIcon.GetComponentInChildren<TMP_Text>();
            if (i == _waveManager.TotalWaves + 1)
            {
                text.text = "Finish";
                _waveIcons[i - 1].sprite = finishWave;
            }
            else
            {
                text.text = i.ToString();
            }
        }
    }

    private void MarkWaveAsStarted(int waveIndex)
    {
        if (waveIndex >= 0 && waveIndex < _waveIcons.Count)
            _waveIcons[waveIndex].sprite = startedWave;
    }
}
