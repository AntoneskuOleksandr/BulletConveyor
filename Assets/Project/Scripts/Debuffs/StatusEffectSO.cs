using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EffectParameter
{
    public EffectParameterType Type;
    public float Value;
}

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Effects/StatusEffect")]
public class StatusEffectSO : ScriptableObject
{
    public OnHitEffectType Type;
    public float Duration;

    [Header("Effect Parameters")]
    public List<EffectParameter> Parameters = new List<EffectParameter>();

    [Header("Allowed Parameters")]
    public EffectParameterType[] AllowedParameters;

    /// <summary>
    /// Получение параметра с проверкой на разрешённость
    /// </summary>
    public float GetParameter(EffectParameterType type, float defaultValue = 0f)
    {
        if (!Array.Exists(AllowedParameters, t => t == type))
        {
            Debug.LogWarning($"Parameter {type} is not allowed for effect {Type}");
            return defaultValue;
        }

        var param = Parameters.Find(p => p.Type == type);
        return param != null ? param.Value : defaultValue;
    }

    /// <summary>
    /// Установка параметра с проверкой на разрешённость
    /// </summary>
    public void SetParameter(EffectParameterType type, float value)
    {
        if (!Array.Exists(AllowedParameters, t => t == type))
        {
            Debug.LogWarning($"Cannot set parameter {type} for effect {Type}");
            return;
        }

        var param = Parameters.Find(p => p.Type == type);
        if (param != null) param.Value = value;
        else Parameters.Add(new EffectParameter { Type = type, Value = value });
    }
}