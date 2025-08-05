using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ProgressBar : MonoBehaviour
{
    public Image filledPart;
    public ScrollRect scrollRect;
    [SerializeField] private Image[] waves;
    [SerializeField] private Sprite completedWave;
    [SerializeField] private Sprite upgradeWave;
    [SerializeField] private Sprite upcomingWave;

    private int currentWave = 0;

    private void Start()
    {
        for (int i = 1; i < waves.Length - 1; i++)
        {
            if (i % 2 == 0)
                waves[i].sprite = upgradeWave;
        }
    }

    public void StartProgressBar()
    {
        StartCoroutine(UpdateProgressBar());
    }

    private IEnumerator UpdateProgressBar()
    {
        float duration = Spawner.Instance.waveDuration;
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            filledPart.fillAmount = (t + currentWave) / 10f;
            scrollRect.horizontalNormalizedPosition = (t + currentWave) / 10f * 1 / 0.97712f;
            yield return null;
        }
    }

    public void ChangeWaveSprite()
    {
        currentWave++;
        waves[currentWave].sprite = completedWave;
    }
}
