using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// IngredientDrawerUIE
// [CustomPropertyDrawer(typeof(CtxSteer.Effector_T))]
public class EffectorDrawerUIE : PropertyDrawer
{
    public override VisualElement CreatePropertyGUI(SerializedProperty property)
    {
        // Create property container element.
        var container = new VisualElement();

        // Create property fields.
        var tag = new PropertyField(property.FindPropertyRelative("tag"));
        var factor = new PropertyField(property.FindPropertyRelative("factor"));
        var minDist = new PropertyField(property.FindPropertyRelative("minDist"));
        var maxDist = new PropertyField(property.FindPropertyRelative("maxDist"));

        // Add fields to the container.
        container.Add(tag);
        container.Add(factor);
        container.Add(minDist);
        container.Add(maxDist);

        return container;
    }
}