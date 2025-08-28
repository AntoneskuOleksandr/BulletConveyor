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

    [SerializeField] private Sprite startedWave;
    [SerializeField] private Sprite upgradeWave;
    [SerializeField] private Sprite upcomingWave;
    [SerializeField] private Sprite finishWave;

    [Inject] private WaveManager _waveManager;

    private List<Image> _waveIcons = new List<Image>();

    private void Awake()
    {
        _waveManager.OnWaveRoutineStarted += (timeBetweenWaves) =>
        {
            StartCoroutine(FillProgressBar(1f / (_waveManager.TotalWaves + 2f), timeBetweenWaves));
        };
        _waveManager.OnWaveStarted += (waveIndex, waveDuration) =>
        {
            StartCoroutine(FillProgressBar((waveIndex + 1f) / (_waveManager.TotalWaves + 2f), waveDuration));
            StartCoroutine(MoveProgressBar((float)waveIndex / _waveManager.TotalWaves, waveDuration));
            MarkWaveAsStarted(waveIndex - 1);
        };

        GenerateProgressBar();
    }
    private void GenerateProgressBar()
    {
        float spaceBetweenIcons = GetComponent<RectTransform>().rect.width / 2f; // Half of ProgressBar width.

        content.sizeDelta = new Vector2(spaceBetweenIcons * (_waveManager.TotalWaves + 2), content.sizeDelta.y);

        for (int i = 1; i <= _waveManager.TotalWaves + 1; i++)
        {
            RectTransform waveIcon = Instantiate(waveIconPrefab, content).GetComponent<RectTransform>();

            _waveIcons.Add(waveIcon.gameObject.GetComponent<Image>());

            if (i % 2 == 0 && i != _waveManager.TotalWaves + 1)
                _waveIcons[i - 1].sprite = upgradeWave;

            waveIcon.localScale = Vector3.one;
            waveIcon.anchoredPosition = new Vector2(spaceBetweenIcons * i, 0f);
            waveIcon.gameObject.name = $"WaveIcon_{i}";

            var text = waveIcon.GetComponentInChildren<TMP_Text>();
            if (i == _waveManager.TotalWaves + 1)
            {
                text.text = "Finish";
                _waveIcons[i - 1].sprite = finishWave;
            }
            else
                text.text = i.ToString();
        }
    }

    private IEnumerator FillProgressBar(float targetFillAmount, float duration)
    {
        float startFillAmount = filledPart.fillAmount;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            filledPart.fillAmount = Mathf.Lerp(startFillAmount, targetFillAmount, elapsed / duration);
            yield return null;
        }
        filledPart.fillAmount = targetFillAmount;
    }

    private IEnumerator MoveProgressBar(float targetPosition, float duration)
    {
        float startPosition = scrollRect.horizontalNormalizedPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPosition, targetPosition, elapsed / duration);
            yield return null;
        }
        scrollRect.horizontalNormalizedPosition = targetPosition;
    }

    private void MarkWaveAsStarted(int wave)
    {
        _waveIcons[wave].sprite = startedWave;
    }
}
