using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(WarnNull))]
public class WarnNullDrawer : PropertyDrawer
{
    static IDictionary<string, bool> warns = new Dictionary<string, bool>();
    
    public override void OnGUI(Rect inRect, SerializedProperty inProp, GUIContent label)
    {
        EditorGUI.BeginProperty(inRect, label, inProp);

        string k = label.text.ToString();
        if (inProp.objectReferenceValue == null)
        {
            if (!warns.ContainsKey(label.text) || !warns[k]) 
                Debug.LogWarning("WarnNullAttr: Reference of " + k + " is null");
            label.text = "[!] " + label.text;
            GUI.color = Color.yellow;
            warns[k] = true;
        }
        else warns[k] = false;

        EditorGUI.PropertyField(inRect, inProp, label);
        GUI.color = Color.white;

        EditorGUI.EndProperty();
    }
}
