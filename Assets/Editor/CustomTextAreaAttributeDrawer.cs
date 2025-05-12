using NUnit;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.TerrainTools;
using UnityEngine;
using UnityEngine.Windows;
using static Codice.Client.BaseCommands.Import.Commit;
using static UnityEngine.GraphicsBuffer;

[CustomPropertyDrawer(typeof(CustomTextAreaAttribute))]
public class CustomTextAreaAttributeDrawer : PropertyDrawer
{
    private float _textHeight;

    private string _newInput;

    private TMP_FontAsset _newFont;
    private int _tempFontSize;
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

        EditorGUILayout.LabelField("Font Style", GUILayout.Width(75));

        //EditorGUILayout.LabelField("New Font Size", GUILayout.Width(100));
        _tempFontSize = (int)EditorGUILayout.IntField(_tempFontSize, GUILayout.Width(30));

        if (GUILayout.Button("Size", GUILayout.Height(25), GUILayout.Width(50)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("size", t, highlightedText, input, null, _tempFontSize);
            _newInput = t.text;
        }

        GUILayout.Space(15);

        _newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(_newFont, typeof(TMP_FontAsset), false,GUILayout.Width(100));

        if (GUILayout.Button("Font", GUILayout.Height(25), GUILayout.Width(50)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("font", t, highlightedText, input, _newFont.name);
            _newInput = t.text;
        }

        GUILayout.Space(15);

        if (GUILayout.Button("B", GUILayout.Height(25), GUILayout.Width(30)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("b", t, highlightedText, input);
            _newInput = t.text;
        }

        GUILayout.Space(15);

        if (GUILayout.Button("I", GUILayout.Height(25), GUILayout.Width(30)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("i", t, highlightedText, input);
            _newInput = t.text;
        }

        GUILayout.Space(15);

        GUILayout.EndHorizontal();

        if (EditorGUI.EndChangeCheck())
        {
            if (_newInput != null) prop.stringValue = _newInput;
            else prop.stringValue = input;
            Debug.Log($"text { prop.stringValue}");
        }
        EditorGUI.EndProperty();
    }

    private string GetHighlighted(out TextEditor t)
    {
        var field = typeof(EditorGUI).GetField("activeEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var temp = field.GetValue(null);
        t = temp as TextEditor;
        return t.SelectedText;
    }

    private string AddTags(string tag, TextEditor textEditor ,string highlightedInput, string input , string useFont = null, int fontSize = 0)
    {
        TextEditor t = textEditor;
        string highlighted = highlightedInput;

        int start = Mathf.Min(t.selectIndex, t.cursorIndex);
        int end = Mathf.Max(t.selectIndex, t.cursorIndex);

        string originalText = "";

        originalText = input;

        string modifiedText = "";
        string newText = "";

        if (tag != "font" && tag != "size")
        {
            modifiedText = $"<{tag}>{highlighted}</{tag}>";
            newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);
        }
        else if(tag == "font")
        {
            modifiedText = $"<font={useFont}>{highlighted}</font>";
            newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);
        }
        else if(tag == "size")
        {
            modifiedText = $"<size={fontSize}>{highlighted}</size>";
            newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);
        }
        return newText;
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return _textHeight + 20 + 25;
    }
}
