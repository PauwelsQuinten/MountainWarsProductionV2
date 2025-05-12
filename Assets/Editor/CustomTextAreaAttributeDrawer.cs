using NUnit;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Codice.Client.BaseCommands.Import.Commit;

[CustomPropertyDrawer(typeof(CustomTextAreaAttribute))]
public class CustomTextAreaAttributeDrawer : PropertyDrawer
{
    private float _textHeight;

    private string _newInput;
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // make a rect of fixed height for text area.
        Rect textareaPos = position;
        //textareaPos.height = _textHeight;
        EditorGUI.BeginProperty(textareaPos, label, prop);

        // add word wrap to style.
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;

        // show the text area.
        EditorGUI.BeginChangeCheck();
        string input = input = EditorGUI.TextArea(textareaPos, prop.stringValue, style);

        EditorGUILayout.LabelField("Preview");

        GUIStyle richTextStyle = new GUIStyle(EditorStyles.label);
        richTextStyle.richText = true;
        EditorGUILayout.LabelField(input, richTextStyle);

        // Text height
        GUIContent guiContent = new GUIContent(prop.stringValue);
        _textHeight = style.CalcHeight(guiContent, EditorGUIUtility.currentViewWidth);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Font Style");

        if(GUILayout.Button("B", GUILayout.Height(25)))
        {
            TextEditor t = new TextEditor();
            string highlighted = GetHighlighted(out t);

            int start = Mathf.Min(t.selectIndex, t.cursorIndex);
            int end = Mathf.Max(t.selectIndex, t.cursorIndex);

            string originalText = "";

            originalText = input;

            string modifiedText = $"<b> {highlighted} </b>";
            string newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);

            t.text = newText;
            _newInput = newText;
        }
        if (EditorGUI.EndChangeCheck())
        {
            if (_newInput != null) prop.stringValue = _newInput;
            else prop.stringValue = input;
        }
        GUILayout.EndHorizontal();
        EditorGUI.EndProperty();
    }

    private string GetHighlighted(out TextEditor t)
    {
        var field = typeof(EditorGUI).GetField("activeEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var temp = field.GetValue(null);
        t = temp as TextEditor;
        return t.SelectedText;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _textHeight + 20 + 25;
    }
}
