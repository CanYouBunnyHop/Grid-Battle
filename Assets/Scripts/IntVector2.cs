#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
[System.Serializable]
public struct IntVector2
{
    public int x;
    public int y;
    public IntVector2(int _x, int _y)
    {
        x = _x;
        y =_y;
    }
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(IntVector2))]
public class IntVector2_UIE : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that
        // prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        // Draw label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Calculate rects
        var xRect = new Rect(position.x, position.y, 50, position.height);
        var yRect = new Rect(position.x + 60, position.y, 50, position.height);

        // Label
        EditorGUIUtility.labelWidth = 15;

        EditorGUI.PropertyField(xRect, property.FindPropertyRelative("x"), new GUIContent("X:"));
        EditorGUI.PropertyField(yRect, property.FindPropertyRelative("y"), new GUIContent("Y:"));

        EditorGUI.EndProperty();
    }
}
#endif
