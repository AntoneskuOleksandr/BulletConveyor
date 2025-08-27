using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class ProgressBar : MonoBehaviour
{
    public Image filledPart;
    public ScrollRect scrollRect;

    [SerializeField] private Image[] waves;
    [SerializeField] private Sprite completedWave;
    [SerializeField] private Sprite upgradeWave;
    [SerializeField] private Sprite upcomingWave;

    [Inject] private WaveManager _waveManager;

    private void Awake()
    {
        for (int i = 1; i < waves.Length - 1; i++)
        {
            if (i % 2 == 0)
                waves[i].sprite = upgradeWave;
        }

        _waveManager.OnWaveRoutineStarted += (timeBetweenWaves) =>
        {
            StartCoroutine(FillProgressBar(1f / (_waveManager.TotalWaves + 1f), timeBetweenWaves));
        };
        _waveManager.OnWaveStarted += (waveIndex, waveDuration) =>
        {
            StartCoroutine(FillProgressBar((waveIndex + 1f) / (_waveManager.TotalWaves + 1f), waveDuration));
            StartCoroutine(MoveProgressBar(waveIndex / (_waveManager.TotalWaves - 1f), waveDuration));
        };
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

    public void MarkWaveAsCompleted(int wave)
    {
        waves[wave].sprite = completedWave;
    }
}
