using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager
{
    private class TimerData
    {
        public string Key;              // unique key
        public DateTime? LastTrigger;   // last time it was triggered
        public double CooldownHours;    // cooldown in hours
    }

    private Dictionary<string, TimerData> timers = new Dictionary<string, TimerData>();

    public void RegisterTimer(string key, double cooldownHours)
    {
        if (!timers.ContainsKey(key))
        {
            timers[key] = new TimerData
            {
                Key = key,
                LastTrigger = LoadLastTrigger(key),
                CooldownHours = cooldownHours
            };
        }
    }

    public bool CanTrigger(string key)
    {
        if (!timers.ContainsKey(key)) return false;

        var t = timers[key];
        if (!t.LastTrigger.HasValue) return true;

        var span = DateTime.Now - t.LastTrigger.Value;
        return span.TotalHours >= t.CooldownHours;
    }

    public void Trigger(string key)
    {
        if (!timers.ContainsKey(key)) return;

        timers[key].LastTrigger = DateTime.Now;
        SaveLastTrigger(key, timers[key].LastTrigger.Value);
    }

    public TimeSpan GetRemaining(string key)
    {
        if (!timers.ContainsKey(key)) return TimeSpan.Zero;

        var t = timers[key];
        if (!t.LastTrigger.HasValue) return TimeSpan.Zero;

        var next = t.LastTrigger.Value.AddHours(t.CooldownHours);
        var remaining = next - DateTime.Now;

        if (remaining.TotalSeconds < 0)
            return TimeSpan.Zero;

        return TimeSpan.FromSeconds(Math.Floor(remaining.TotalSeconds));
    }


    public void ResetTimer(string key)
    {
        if (timers.ContainsKey(key))
        {
            timers[key].LastTrigger = null;
            PlayerPrefs.DeleteKey("Timer_" + key);
        }
    }

    private void SaveLastTrigger(string key, DateTime time)
    {
        PlayerPrefs.SetString("Timer_" + key, time.ToString());
    }

    private DateTime? LoadLastTrigger(string key)
    {
        string data = PlayerPrefs.GetString("Timer_" + key, "");
        if (!string.IsNullOrEmpty(data))
            return DateTime.Parse(data);
        return null;
    }
}
