using System;
using UnityEngine;

[Serializable]
public class WeaponParameter
{
    public WeaponParameterType Type;
    public WeaponParameterValueType ValueType = WeaponParameterValueType.Float;

    public float FloatValue;
    public int IntValue;
    public bool BoolValue;
    public Color ColorValue = Color.white;
    public Vector3 VectorValue;

    public object GetValue()
    {
        return ValueType switch
        {
            WeaponParameterValueType.Float => FloatValue,
            WeaponParameterValueType.Int => IntValue,
            WeaponParameterValueType.Bool => BoolValue,
            WeaponParameterValueType.Color => ColorValue,
            WeaponParameterValueType.Vector3 => VectorValue,
            _ => null
        };
    }

    public T GetValue<T>()
    {
        object val = GetValue();
        if (val is T casted)
            return casted;

        Debug.LogWarning($"[WeaponParameter] Invalid cast for {Type} to {typeof(T).Name}");
        return default;
    }
}
