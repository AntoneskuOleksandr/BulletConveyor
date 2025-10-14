#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(EffectParameter))]
public class EffectParameterDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Получаем свойства
        SerializedProperty typeProp = property.FindPropertyRelative("Type");
        SerializedProperty valueProp = property.FindPropertyRelative("Value");

        // Получаем родительский SO
        StatusEffectSO so = property.serializedObject.targetObject as StatusEffectSO;

        // Разделяем позиции для popup и value
        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        if (so == null)
        {
            // Рисуем стандартно
            EditorGUI.PropertyField(typeRect, typeProp);
            EditorGUI.PropertyField(valueRect, valueProp);
            return;
        }

        // Создаём список разрешённых типов
        List<EffectParameterType> allowed = new List<EffectParameterType>(so.AllowedParameters);

        if (allowed.Count == 0)
        {
            EditorGUI.LabelField(typeRect, label.text, "No allowed parameters!");
            EditorGUI.PropertyField(valueRect, valueProp);
            return;
        }

        // Индекс текущего значения в allowed
        EffectParameterType currentType = (EffectParameterType)typeProp.enumValueIndex;
        int currentIndex = allowed.IndexOf(currentType);
        if (currentIndex < 0) currentIndex = 0;

        // Опции popup
        string[] options = Array.ConvertAll(allowed.ToArray(), t => t.ToString());

        // Рисуем popup
        int newIndex = EditorGUI.Popup(typeRect, label.text, currentIndex, options);
        typeProp.enumValueIndex = (int)allowed[newIndex];

        // Поле значения
        EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Value"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Две строки: тип + значение
        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}
#endif
