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
    private bool _scrollStarted = false;

    private float _initialFill; // запомненное значение после заполнени€ левой пустой области

    private void Awake()
    {
        GenerateProgressBar();

        // 1) «аполн€ем левую пустую область перед первой волной
        _waveManager.OnWaveRoutineStarted += (timeBetweenWaves) => StartCoroutine(FillInitialEmptyArea(timeBetweenWaves));

        // 2) ѕодписка на прогресс волны
        _waveManager.OnWaveProgress += (waveProgress, waveIndex) =>
        {
            // ѕрогресс бара: прибавл€ем прогресс волны к уже заполненной левой области
            float filledAmount = _initialFill + (waveIndex - 1 + waveProgress) / (_waveManager.TotalWaves + 2f);
            filledPart.fillAmount = filledAmount;

            // —кролл двигаетс€ только после старта первой волны
            if (_scrollStarted)
            {
                float scrollPos = (waveIndex - 1 + waveProgress) / (float)_waveManager.TotalWaves;
                scrollRect.horizontalNormalizedPosition = scrollPos;
            }
        };

        // 3) ѕри старте волны отмечаем еЄ как начатую
        _waveManager.OnWaveStarted += (waveIndex, _) =>
        {
            MarkWaveAsStarted(waveIndex - 1);
            _scrollStarted = true; // включаем скролл после первой волны
        };
    }

    private IEnumerator FillInitialEmptyArea(float duration)
    {
        float target = 1f / (_waveManager.TotalWaves + 2f); // лева€ пуста€ область
        float start = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            filledPart.fillAmount = Mathf.Lerp(start, target, elapsed / duration);
            yield return null;
        }

        filledPart.fillAmount = target;
        _initialFill = target; // запоминаем заполненное значение
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
