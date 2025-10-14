using UnityEngine;

[RequireComponent(typeof(TrailRenderer))]
public class BulletVisual : MonoBehaviour
{
    private TrailRenderer _trail;

    private void Awake()
    {
        _trail = GetComponent<TrailRenderer>();


        _trail.emitting = false;
        _trail.Clear();
    }

    /// <summary>
    /// Очищает трейл при спавне
    /// </summary>
    public void OnSpawn()
    {
        _trail.Clear();
        _trail.emitting = true;
    }

    /// <summary>
    /// Сбрасывает трейл и частицы при деактивации.
    /// </summary>
    public void OnDespawn()
    {
        _trail.emitting = false;
        _trail.Clear();
    }

    /// <summary>
    /// Устанавливает основной цвет трейла.
    /// </summary>
    public void SetBaseColor(Color color)
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] {
                new(color, 0.0f),
                new(color, 1.0f)
            },
            new GradientAlphaKey[] {
                new(1f, 0.0f),
                new(0f, 1.0f)
            }
        );
        _trail.colorGradient = gradient;
    }
}
