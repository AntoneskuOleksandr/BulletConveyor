using UnityEngine;

[CreateAssetMenu(fileName = "EffectRulesConfig", menuName = "Effects/RulesConfig")]
public class EffectRulesConfigSO : ScriptableObject
{
    [Header("Freeze -> Ice")]
    public float freezeToIceThreshold = 1f;
    public float iceDurationFromFreeze = 0.5f;

    [Header("Fire melts Freeze/Ice")]
    public float fireMeltMultiplierFreeze = 2f;
    public float fireMeltMultiplierIce = 2f;

}
