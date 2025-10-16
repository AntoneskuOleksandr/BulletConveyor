using UnityEngine;

[CreateAssetMenu(fileName = "EffectRulesConfig", menuName = "Effects/RulesConfig")]
public class EffectRulesConfigSO : ScriptableObject
{
    [Header("Freeze -> Ice")]
    public float freezeToIceThreshold = 1f; // Strength, при котором Freeze превращается в Ice
    public float iceDurationFromFreeze = 0.5f; // доля Duration Freeze для продления Ice

    [Header("Fire melts Freeze/Ice")]
    public float fireMeltMultiplierFreeze = 2f;
    public float fireMeltMultiplierIce = 2f;

    // Можно добавить любые новые параметры для других правил
}
