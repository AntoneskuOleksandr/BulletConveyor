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
        muzzlePoint = attachment?.firePoint;

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
        Debug.Log($"[WeaponVisual] Покраска {colorWeights.Count} цветов");

        int maxParticles = _muzzleInstance.main.maxParticles;
        if (_particles == null || _particles.Length < maxParticles)
            _particles = new ParticleSystem.Particle[maxParticles];

        int aliveCount = _muzzleInstance.GetParticles(_particles);
        Debug.Log($"[WeaponVisual] Частиц активно: {aliveCount}");

        if (aliveCount == 0) return;

        List<Color> palette = BuildColorPalette(colorWeights, aliveCount);
        Debug.Log($"[WeaponVisual] Палитра собрана: {palette.Count} элементов");

        Shuffle(palette);

        for (int i = 0; i < aliveCount; i++)
        {
            _particles[i].startColor = palette[i];
        }

        _muzzleInstance.SetParticles(_particles, aliveCount);
        Debug.Log($"[WeaponVisual] Частицы перекрашены.");
    }


    private List<Color> BuildColorPalette(Dictionary<Color, float> weights, int count)
    {
        var palette = new List<Color>(count);
        float totalWeight = weights.Values.Sum();

        foreach (var kv in weights)
        {
            int num = Mathf.RoundToInt(count * (kv.Value / totalWeight));
            for (int i = 0; i < num; i++)
                palette.Add(kv.Key);
        }

        // добиваем, если не хватило из-за округления
        while (palette.Count < count)
            palette.Add(weights.Keys.First());

        return palette;
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    /// <summary>
    /// Скрыть эффект (например, при выключении пушки).
    /// </summary>
    public void StopEffect()
    {
        if (_muzzleInstance == null) return;
        _muzzleInstance.Stop();
    }
}
