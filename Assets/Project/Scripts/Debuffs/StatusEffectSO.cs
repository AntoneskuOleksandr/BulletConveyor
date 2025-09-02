using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewStatusEffect", menuName = "Effects/StatusEffect")]
public class StatusEffectSO : ScriptableObject
{
    public OnHitEffectType Type;
    public float Duration;

    [SerializeField]
    private List<EffectParameter> parameters = new List<EffectParameter>();

    // Получение значения параметра по ключу
    public float GetParameter(string key, float defaultValue = 0f)
    {
        var param = parameters.Find(p => p.Key == key);
        return param != null ? param.Value : defaultValue;
    }

    // Установка значения параметра
    public void SetParameter(string key, float value)
    {
        var param = parameters.Find(p => p.Key == key);
        if (param != null)
            param.Value = value;
        else
            parameters.Add(new EffectParameter { Key = key, Value = value });
    }

    [Serializable]
    public class EffectParameter
    {
        public string Key;
        public float Value;
    }
}
