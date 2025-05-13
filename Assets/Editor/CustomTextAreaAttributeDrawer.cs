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

    private string _firstLine;
    private string _secondLine;

    private string _UpdatedFirstLine;
    private string _UpdatedSecondLine;

    private SerializedProperty _firstProp;
    private SerializedProperty _secondProp;

    private TMP_FontAsset _newFont;
    private int _tempFontSize;

    private bool _hasSecondLine;
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        _firstProp = prop.FindPropertyRelative("FirstLine");
        _secondProp = prop.FindPropertyRelative("SecondLine");

        EditorGUI.BeginProperty(position, label, prop);

        // add word wrap to style.
        GUIStyle style = new GUIStyle(EditorStyles.textArea);
        style.wordWrap = true;

        // show the text area.
        EditorGUI.BeginChangeCheck();
        _firstLine = EditorGUILayout.TextArea(_firstProp.stringValue, style);

        EditorGUILayout.LabelField("Preview");

        GUIStyle richTextStyle = new GUIStyle(EditorStyles.label);
        richTextStyle.richText = true;
        EditorGUILayout.LabelField(_firstLine, richTextStyle);

        GUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Font Style", GUILayout.Width(75));

        //EditorGUILayout.LabelField("New Font Size", GUILayout.Width(100));
        _tempFontSize = (int)EditorGUILayout.IntField(_tempFontSize, GUILayout.Width(30));

        if (GUILayout.Button("Size", GUILayout.Height(25), GUILayout.Width(50)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("size", t, highlightedText, _firstLine, null, _tempFontSize);
            _UpdatedFirstLine = t.text;
        }

        GUILayout.Space(15);

        _newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(_newFont, typeof(TMP_FontAsset), false,GUILayout.Width(100));

        if (GUILayout.Button("Font", GUILayout.Height(25), GUILayout.Width(50)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("font", t, highlightedText, _firstLine, _newFont.name);
            _UpdatedFirstLine = t.text;
        }

        GUILayout.Space(15);

        if (GUILayout.Button("B", GUILayout.Height(25), GUILayout.Width(30)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("b", t, highlightedText, _firstLine);
            _UpdatedFirstLine = t.text;
        }

        GUILayout.Space(15);

        if (GUILayout.Button("I", GUILayout.Height(25), GUILayout.Width(30)))
        {
            TextEditor t = new TextEditor();
            string highlightedText = "";
            highlightedText = GetHighlighted(out t);

            t.text = AddTags("i", t, highlightedText, _firstLine);
            _UpdatedFirstLine = t.text;
        }

        GUILayout.Space(15);

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Has Second Line", GUILayout.Width(125));
        _hasSecondLine = EditorGUILayout.Toggle(_hasSecondLine);
        GUILayout.EndHorizontal();

        if (_hasSecondLine)
        {
            _secondLine = EditorGUILayout.TextArea(_secondProp.stringValue, style);

            EditorGUILayout.LabelField("Preview");

            richTextStyle = new GUIStyle(EditorStyles.label);
            richTextStyle.richText = true;
            EditorGUILayout.LabelField(_secondLine, richTextStyle);

            GUILayout.BeginHorizontal();

            EditorGUILayout.LabelField("Font Style", GUILayout.Width(75));

            //EditorGUILayout.LabelField("New Font Size", GUILayout.Width(100));
            _tempFontSize = (int)EditorGUILayout.IntField(_tempFontSize, GUILayout.Width(30));

            if (GUILayout.Button("Size", GUILayout.Height(25), GUILayout.Width(50)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);

                t.text = AddTags("size", t, highlightedText, _secondLine, null, _tempFontSize);
                _UpdatedSecondLine = t.text;
            }

            GUILayout.Space(15);

            _newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(_newFont, typeof(TMP_FontAsset), false, GUILayout.Width(100));

            if (GUILayout.Button("Font", GUILayout.Height(25), GUILayout.Width(50)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);

                t.text = AddTags("font", t, highlightedText, _secondLine, _newFont.name);
                _UpdatedSecondLine = t.text;
            }

            GUILayout.Space(15);

            if (GUILayout.Button("B", GUILayout.Height(25), GUILayout.Width(30)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);

                t.text = AddTags("b", t, highlightedText, _secondLine);
                _UpdatedSecondLine = t.text;
            }

            GUILayout.Space(15);

            if (GUILayout.Button("I", GUILayout.Height(25), GUILayout.Width(30)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);

                t.text = AddTags("i", t, highlightedText, _secondLine);
                _UpdatedSecondLine = t.text;
            }

            GUILayout.Space(15);

            GUILayout.EndHorizontal();
        }


        if (EditorGUI.EndChangeCheck())
        {
            if (_UpdatedFirstLine != null) _firstProp.stringValue = _UpdatedFirstLine;
            else _firstProp.stringValue = _firstLine;

            if (_UpdatedSecondLine != null) _secondProp.stringValue = _UpdatedSecondLine;
            else _secondProp.stringValue = _secondLine;
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
