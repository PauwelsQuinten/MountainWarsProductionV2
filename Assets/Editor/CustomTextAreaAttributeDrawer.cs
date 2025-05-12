using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CustomTextAreaAttribute))]
public class CustomTextAreaAttributeDrawer : PropertyDrawer
{
    private float _textHeight;
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // make a rect of fixed height for text area.
        Rect textareaPos = position;
        textareaPos.height = _textHeight;
        EditorGUI.BeginProperty(textareaPos, label, prop);

        // add word wrap to style.
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;
        

        // show the text area.
        EditorGUI.BeginChangeCheck();
        string input = EditorGUI.TextArea(textareaPos, prop.stringValue, style);
        if (EditorGUI.EndChangeCheck())
        {
            prop.stringValue = input;
        }
        // Text height
        GUIContent guiContent = new GUIContent(prop.stringValue);
        _textHeight = style.CalcHeight(guiContent, EditorGUIUtility.currentViewWidth);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Font Style");

        if(GUILayout.Button("B", GUILayout.Height(25)))
        {

        }
        GUILayout.EndHorizontal();
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _textHeight + 20 + 25;
    }
}
