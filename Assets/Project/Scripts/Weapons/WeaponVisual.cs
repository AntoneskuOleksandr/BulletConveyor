using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponVisual : MonoBehaviour
{
    [Header("Muzzle Flash")]
    [SerializeField] private ParticleSystem muzzleFlashPrefab;

    private Transform muzzlePoint;
    private ParticleSystem _muzzleInstance;
    private ParticleSystem.Particle[] _particles;

    private void Awake()
    {
        var attachment = GetComponent<WeaponAttachmentPoints>();
        muzzlePoint = attachment?.Get(WeaponAttachmentType.FirePoint);

        if (muzzleFlashPrefab != null && muzzlePoint != null)
        {
            _muzzleInstance = Instantiate(muzzleFlashPrefab, muzzlePoint.position, muzzlePoint.rotation, muzzlePoint);
            _muzzleInstance.Stop();
        }
    }

    /// <summary>
    /// Проигрывает вспышку с распределением цветов по шансам.
    /// Если словарь пуст — используется дефолтный цвет.
    /// </summary>
    public void PlayShotEffect(Dictionary<Color, float> colorWeights)
    {
        if (_muzzleInstance == null)
            return;

        _muzzleInstance.transform.position = muzzlePoint.position;
        _muzzleInstance.transform.rotation = muzzlePoint.rotation;

        _muzzleInstance.Play();

        if (colorWeights != null && colorWeights.Count > 0)
            StartCoroutine(DelayedColorize(colorWeights));
    }


    private System.Collections.IEnumerator DelayedColorize(Dictionary<Color, float> colorWeights)
    {
        yield return null; // ждём 1 кадр
        ColorizeParticles(colorWeights);
    }

    /// <summary>
    /// Красим частицы в пропорциях, зависящих от весов цветов.
    /// </summary>
    private void ColorizeParticles(Dictionary<Color, float> colorWeights)
    {
        int maxParticles = _muzzleInstance.main.maxParticles;
        if (_particles == null || _particles.Length < maxParticles)
            _particles = new ParticleSystem.Particle[maxParticles];

        int aliveCount = _muzzleInstance.GetParticles(_particles);

        if (aliveCount == 0) return;

        List<int> indices = Enumerable.Range(0, aliveCount).ToList();
        Shuffle(indices);

        int usedCount = 0;

        foreach (var kv in colorWeights)
        {
            int countForColor = Mathf.RoundToInt(aliveCount * kv.Value);
            for (int i = 0; i < countForColor && usedCount < aliveCount; i++)
            {
                int particleIndex = indices[usedCount];
                _particles[particleIndex].startColor = kv.Key;
                usedCount++;
            }
        }

        _muzzleInstance.SetParticles(_particles, aliveCount);
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
