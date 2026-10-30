using System;
using System.Collections.Generic;
using UnityEngine;

public class GameSpeed : MonoBehaviour
{
    private readonly HashSet<object> pauses = new();
    public event Action OnChanged;
    public float Multiplier { get; private set; } = 1f;
    public bool IsPaused => pauses.Count > 0;

    private void Awake() => Apply();

    public void Toggle() => SetMultiplier(Multiplier > 1f ? 1f : 3f);

    public void SetMultiplier(float value)
    {
        Multiplier = Mathf.Clamp(value, 1f, 3f);
        Apply();
    }

    public void Pause(object owner)
    {
        if (owner != null && pauses.Add(owner)) Apply();
    }

    public void Resume(object owner)
    {
        if (owner != null && pauses.Remove(owner)) Apply();
    }

    private void Apply()
    {
        Time.timeScale = IsPaused ? 0f : Multiplier;
        OnChanged?.Invoke();
    }

    private void OnDestroy() => Time.timeScale = 1f;
}
