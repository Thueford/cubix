using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(NotNull))]
public class NotNullDrawer : PropertyDrawer
{
    static IDictionary<string, bool> errs = new Dictionary<string, bool>();

    public override void OnGUI(Rect inRect, SerializedProperty inProp, GUIContent label)
    {
        EditorGUI.BeginProperty(inRect, label, inProp);

        string k = label.text.ToString();
        if (inProp.objectReferenceValue == null)
        {
            if (!errs.ContainsKey(label.text) || !errs[k])
                Debug.LogError("NotNullAttr: Reference of " + k + " is null");
            label.text = "[!] " + label.text;
            GUI.color = Color.red;
            errs[k] = true;
        }
        else errs[k] = false;

        EditorGUI.PropertyField(inRect, inProp, label);
        GUI.color = Color.white;

        EditorGUI.EndProperty();
    }
}
