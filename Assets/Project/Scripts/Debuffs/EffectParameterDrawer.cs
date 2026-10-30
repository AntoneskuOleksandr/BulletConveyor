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

        SerializedProperty typeProp = property.FindPropertyRelative("Type");
        SerializedProperty valueProp = property.FindPropertyRelative("Value");

        StatusEffectSO so = property.serializedObject.targetObject as StatusEffectSO;

        Rect typeRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        Rect valueRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 2, position.width, EditorGUIUtility.singleLineHeight);

        if (so == null)
        {

            EditorGUI.PropertyField(typeRect, typeProp);
            EditorGUI.PropertyField(valueRect, valueProp);
            return;
        }

        List<EffectParameterType> allowed = new List<EffectParameterType>(so.AllowedParameters);

        if (allowed.Count == 0)
        {
            EditorGUI.LabelField(typeRect, label.text, "No allowed parameters!");
            EditorGUI.PropertyField(valueRect, valueProp);
            return;
        }

        EffectParameterType currentType = (EffectParameterType)typeProp.enumValueIndex;
        int currentIndex = allowed.IndexOf(currentType);
        if (currentIndex < 0) currentIndex = 0;

        string[] options = Array.ConvertAll(allowed.ToArray(), t => t.ToString());

        int newIndex = EditorGUI.Popup(typeRect, label.text, currentIndex, options);
        typeProp.enumValueIndex = (int)allowed[newIndex];

        EditorGUI.PropertyField(valueRect, valueProp, new GUIContent("Value"));
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {

        return EditorGUIUtility.singleLineHeight * 2 + 2;
    }
}
#endif
