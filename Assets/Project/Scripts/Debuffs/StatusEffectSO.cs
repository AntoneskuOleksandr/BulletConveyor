using System;
using System.Collections.Generic;
using System.Linq;
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
    public Color Color;

    [Header("Effect Parameters")]
    public List<EffectParameter> Parameters = new List<EffectParameter>();

    [Header("Allowed Parameters")]
    public EffectParameterType[] AllowedParameters;

    public float GetParameter(EffectParameterType type, float defaultValue = 0f)
    {
        if (!Array.Exists(AllowedParameters, t => t == type))
        {
            Debug.LogWarning($"StatusEffectSO: Parameter {type} is not signed for effect {Type}. Returning default value.");
            return defaultValue;
        }

        var param = Parameters.Find(p => p.Type == type);
        return param != null ? param.Value : defaultValue;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // ”дал€ем параметры, которых больше нет в AllowedParameters
        Parameters.RemoveAll(p => !Array.Exists(AllowedParameters, t => t == p.Type));

        // ƒобавл€ем типы по пор€дку при наличии свободных AllowedParameters
        var existingTypes = Parameters.Select(p => p.Type).ToList();

        for (int i = 0; i < AllowedParameters.Length; i++)
        {
            var allowed = AllowedParameters[i];

            if (!existingTypes.Contains(allowed))
            {
                // ≈сли список Parameters меньше, чем количество Allowed, добавл€ем новый элемент
                if (Parameters.Count < AllowedParameters.Length)
                {
                    Parameters.Add(new EffectParameter { Type = allowed, Value = 0f });
                    existingTypes.Add(allowed); // чтобы дальше не дублировать
                }
            }
        }

        // ≈сли вдруг Parameters больше AllowedParameters, обрезаем
        Parameters.RemoveAll(p => !AllowedParameters.Contains(p.Type));
    }
#endif
}

