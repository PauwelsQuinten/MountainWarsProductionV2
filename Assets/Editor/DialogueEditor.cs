using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Callbacks;
using PlasticGui.WorkspaceWindow.PendingChanges.Changelists;
using System;
using TMPro;
using System.Text.RegularExpressions;
using static UnityEngine.GraphicsBuffer;
using System.Security.Cryptography;
//using Unity.VisualScripting;


public class DialogueEditor : EditorWindow
{

    Dialogue selectedDialogue = null;
    [NonSerialized]
    GUIStyle nodeStyle;
    [NonSerialized]
    GUIStyle playerNodeStyle;
    [NonSerialized]
    DialogueNode draggingNode = null;
    [NonSerialized]
    Vector2 draggindOffset;
    [NonSerialized]
    DialogueNode creatingNode = null;
    [NonSerialized]
    DialogueNode deletingNode = null;
    [NonSerialized]
    DialogueNode linkingParentNode = null;

    Vector2 scrollPosition;

    [NonSerialized]
    bool draggingCanvas = false;
    [NonSerialized]
    Vector2 draggingCanvasOffset;

    const float canvasSize = 8000;
    const float backgroundSize = 50f;


    [MenuItem("Window/Story Tools/Dialogue Editor")]
    public static void ShowEditorWindow()
    {
        GetWindow(typeof(DialogueEditor), false, "Dialogue Editor");
    }

    [OnOpenAsset(1)]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        Dialogue dialogue = EditorUtility.InstanceIDToObject(instanceID) as Dialogue;
        if (dialogue != null)
        {
            ShowEditorWindow();

            return true;
        }
        return false;

    }


    private void OnEnable()
    {
        Selection.selectionChanged += OnSelectionChanged;

        nodeStyle = new GUIStyle();
        nodeStyle.normal.background = EditorGUIUtility.Load("node0") as Texture2D;

        nodeStyle.normal.textColor = Color.white;
        nodeStyle.padding = new RectOffset(20, 20, 20, 20);
        nodeStyle.border = new RectOffset(12, 12, 12, 12);



        playerNodeStyle = new GUIStyle();
        playerNodeStyle.normal.background = EditorGUIUtility.Load("node1") as Texture2D;

        playerNodeStyle.normal.textColor = Color.white;
        playerNodeStyle.padding = new RectOffset(20, 20, 20, 20);
        playerNodeStyle.border = new RectOffset(12, 12, 12, 12);

    }
    private void OnSelectionChanged()
    {
        Dialogue newDialogue = Selection.activeObject as Dialogue;
        if (newDialogue != null)
        {
            selectedDialogue = newDialogue;
            Repaint();
        }
    }

    private void OnGUI()
    {
        if (selectedDialogue == null)
        {
            EditorGUILayout.LabelField("No Active Dialogues");
        }
        else
        {
            ProcessEvents();
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            Rect canvas = GUILayoutUtility.GetRect(canvasSize, canvasSize);
            Texture2D backgroundTex = Resources.Load("background") as Texture2D;
            Rect texCoords = new Rect(0, 0, canvasSize / backgroundSize, canvasSize / backgroundSize);
            GUI.DrawTextureWithTexCoords(canvas, backgroundTex, texCoords);



            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                DrawConnections(node);
            }
            foreach (DialogueNode node in selectedDialogue.GetAllNodes())
            {
                DrawNode(node);

            }


            EditorGUILayout.EndScrollView();

            if (creatingNode != null)
            {
                selectedDialogue.CreateNode(creatingNode);
                creatingNode = null;
            }
            if (deletingNode != null)
            {
                selectedDialogue.DeleteNode(deletingNode);
                deletingNode = null;
            }
        }

    }



    private void ProcessEvents()
    {
        if (Event.current.type == EventType.MouseDown && Event.current.button != 0 && draggingNode == null)
        {
            draggingNode = GetNodeAtPoint(Event.current.mousePosition + scrollPosition);
            if (draggingNode != null)
            {
                draggindOffset = draggingNode.GetRect().position - Event.current.mousePosition;
                Selection.activeObject = draggingNode;
            }
            else
            {
                draggingCanvas = true;
                draggingCanvasOffset = Event.current.mousePosition + scrollPosition;
                Selection.activeObject = selectedDialogue;
            }
            // Record dragOffset and dragging
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button != 0 && draggingNode != null)
        {
            draggingNode.SetPosition(Event.current.mousePosition + draggindOffset);



            //Update scrollPosition
            GUI.changed = true;
        }
        else if (Event.current.type == EventType.MouseDrag && Event.current.button != 0 && draggingCanvas)
        {
            scrollPosition = draggingCanvasOffset - Event.current.mousePosition;
            GUI.changed = true;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button != 0 && draggingNode != null)
        {
            draggingNode = null;
        }
        else if (Event.current.type == EventType.MouseUp && Event.current.button != 0 && draggingCanvas)
        {
            draggingCanvas = false;
        }
    }



    private void DrawNode(DialogueNode node)
    {
        int baseNodeHeight = 0;
        GUIStyle previewStyle = new GUIStyle(EditorStyles.label);
        previewStyle.wordWrap = true;
        previewStyle.richText = true;

        GUIStyle labelStyle = new GUIStyle(EditorStyles.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;

        GUIStyle style = nodeStyle;
        if (node.IsPlayerSpeaking())
        {
            style = playerNodeStyle;
        }

        GUILayout.BeginArea(node.GetRect(), style);
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Has Image Support", GUILayout.Width(142.5f), GUILayout.Height(20));
        node.SetHasImageSupport(EditorGUILayout.Toggle(node.GetHasImageSupport(), GUILayout.Height(20)));
        EditorGUILayout.EndHorizontal();
        baseNodeHeight += 20;

        float textHeight = 0;
        float previewHeight = 0;
        float characterNameOffset = 0;

        if (node.GetHasImageSupport())
        {
            EditorGUILayout.LabelField("Character", GUILayout.Height(20));
            baseNodeHeight += 20;
            SerializedObject so = new SerializedObject(node);
            SerializedProperty characterNameProp = so.FindProperty("CharacterName");
            EditorGUILayout.PropertyField(characterNameProp, true);
            so.ApplyModifiedProperties();

            EditorGUILayout.LabelField("-Images-", labelStyle, GUILayout.Height(20));
            baseNodeHeight += 20;
            EditorGUILayout.LabelField("Shouting Images", GUILayout.Height(20));
            baseNodeHeight += 20;
            SerializedObject so2 = new SerializedObject(node); // 'target' is your DialogueNode
            SerializedProperty imagesProp = so2.FindProperty("shoutingImages");
            EditorGUILayout.PropertyField(imagesProp, true); // 'true' draws children (the list elements)
            so2.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Has second image line", GUILayout.Width(142.5f), GUILayout.Height(20));
            node.SethasSecondImageLine(EditorGUILayout.Toggle(node.GetHasSecondImageLine(), GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Image Size", GUILayout.Width(115), GUILayout.Height(20));
            node.SetImageSize(EditorGUILayout.IntField(node.getImageSize(), GUILayout.Width(40), GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;
        }
        else
        {
            GUIContent guiContent = new GUIContent(node.GetText());
            GUIStyle textStyle = new GUIStyle(EditorStyles.textArea);
            textStyle.wordWrap = true;

            float availableWidth = 60;

            textHeight = textStyle.CalcHeight(guiContent, availableWidth);

            previewHeight = previewStyle.CalcHeight(guiContent, availableWidth);

            EditorGUILayout.LabelField("-Text-", labelStyle, GUILayout.Height(20));
            baseNodeHeight += 20;
            EditorGUILayout.LabelField("Character", GUILayout.Height(20));
            baseNodeHeight += 20;
            SerializedObject so = new SerializedObject(node);
            SerializedProperty characterNameProp = so.FindProperty("Name");
            EditorGUILayout.PropertyField(characterNameProp, true);
            so.ApplyModifiedProperties();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text", GUILayout.Width(60));
            node.SetText(EditorGUILayout.TextArea(node.GetText(), textStyle));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Text Display Speed", GUILayout.Width(115), GUILayout.Height(20));
            node.SetTextDisplaySpeed(EditorGUILayout.FloatField(node.GetTextDisplaySpeed(), GUILayout.Width(40), GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Has secondary Dialogue", GUILayout.Width(142.5f), GUILayout.Height(20));
            node.SetHasSecondaryLine(EditorGUILayout.Toggle(node.GetHasSecondaryLine(), GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;

            EditorGUILayout.LabelField("-Preview-", labelStyle, GUILayout.Height(20));
            baseNodeHeight += 20;
            EditorGUILayout.LabelField(node.GetText(), previewStyle);

            EditorGUILayout.LabelField("-Font Style-", labelStyle, GUILayout.Height(20));
            baseNodeHeight += 20;

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Size", GUILayout.Height(20)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);
                if (!RemoveRichTextTags(node, t))
                {
                    node.SetText(AddTags("size", t, highlightedText, node.GetText(), null, node.GetNewFontSize()));
                }
            }

            if (GUILayout.Button("Font", GUILayout.Height(20)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);
                if (!RemoveRichTextTags(node, t))
                {
                    node.SetText(AddTags("font", t, highlightedText, node.GetText(), node.GetNewFont().ToString()));
                }
            }

            if (GUILayout.Button("B", GUILayout.Height(20)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);
                if (!RemoveRichTextTags(node, t))
                {
                    node.SetText(AddTags("b", t, highlightedText, node.GetText()));
                }
            }

            if (GUILayout.Button("I", GUILayout.Height(20)))
            {
                TextEditor t = new TextEditor();
                string highlightedText = "";
                highlightedText = GetHighlighted(out t);
                if (!RemoveRichTextTags(node, t))
                {
                    node.SetText(AddTags("i", t, highlightedText, node.GetText()));
                }
            }
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Font Size", GUILayout.Width(60), GUILayout.Height(20));
            node.SetNewSize(EditorGUILayout.IntField(node.GetNewFontSize(), GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;

            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("New Font", GUILayout.Width(60), GUILayout.Height(20));
            node.SetNewFont((TMP_FontAsset)EditorGUILayout.ObjectField(node.GetNewFont(), typeof(TMP_FontAsset), false, GUILayout.Height(20)));
            GUILayout.EndHorizontal();
            baseNodeHeight += 20;
        }

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Is Shouting", GUILayout.Width(142.5f), GUILayout.Height(20));
        node.SetIsShouting(EditorGUILayout.Toggle(node.GetIsShouting(), GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        EditorGUILayout.LabelField("Shouting Intensity", GUILayout.Height(20));
        baseNodeHeight += 20;
        node.SetIsShoutIntensity(EditorGUILayout.Slider(node.GetShoutIntensity(), 0.5f, 1.3f, GUILayout.Height(20)));
        baseNodeHeight += 20;

        EditorGUILayout.LabelField("-Text Balloon-", labelStyle, GUILayout.Height(20));
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Balloon Image", GUILayout.Width(90), GUILayout.Height(20));
        node.SetBalloonObject((GameObject)EditorGUILayout.ObjectField(node.GetBalloonObject(), typeof(GameObject), false, GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Tail Image", GUILayout.Width(90), GUILayout.Height(20));
        node.SetTailObject((GameObject)EditorGUILayout.ObjectField(node.GetTailObject(), typeof(GameObject), false, GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Border Size", GUILayout.Width(90), GUILayout.Height(20));
        node.SetBorderSize(EditorGUILayout.FloatField(node.GetBorderSize(), GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        //EditorGUILayout.LabelField("Padding", GUILayout.Width(90));
        node.SetSizePadding(EditorGUILayout.Vector2Field("Padding", node.GetSizePadding(), GUILayout.Width(150), GUILayout.Height(40)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 40;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Needs to be flipped", GUILayout.Width(142.5f), GUILayout.Height(20));
        node.SetNeedsToBeFlipped(EditorGUILayout.Toggle(node.GetNeedsToBeFlipped(), GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        EditorGUILayout.LabelField("-Event-", labelStyle, GUILayout.Height(20));
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("On Completion", GUILayout.Width(100), GUILayout.Height(20));
        node.SetOnCompletionEvent((GameEvent)EditorGUILayout.ObjectField(node.GetOnCompletionEvent(), typeof(GameEvent), false, GUILayout.Height(20)));
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        EditorGUILayout.LabelField("-Nodes-", labelStyle, GUILayout.Height(20));
        baseNodeHeight += 20;

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("X", GUILayout.Height(20)))
        {
            deletingNode = node;
        }
        DrawLinkButtons(node);

        if (GUILayout.Button("+", GUILayout.Height(20)))
        {
            creatingNode = node;
        }
        GUILayout.EndHorizontal();
        baseNodeHeight += 20;

        if (node.GetCharacterName().Count != 0)
        {
            characterNameOffset = 60 * (node.GetCharacterName().Count - 1);
        }
        else characterNameOffset = 60;

        float imageOffset = 0;
        if (node.GetShoutingImages().Count != 0)
        {
            imageOffset = (60 * (node.GetShoutingImages().Count - 1));
        }
        else imageOffset = 60;

        if (node.GetHasImageSupport())
        {
            node.SetHeight(imageOffset + characterNameOffset + baseNodeHeight + 140);
        }
        else node.SetHeight(characterNameOffset + baseNodeHeight + textHeight + previewHeight + 140);


        GUILayout.EndArea();
    }

    private void DrawLinkButtons(DialogueNode node)
    {
        if (linkingParentNode == null)
        {
            if (GUILayout.Button("link"))
            {
                linkingParentNode = node;
            }
        }
        else if (linkingParentNode == node)
        {
            if (GUILayout.Button("cancel"))
            {
                linkingParentNode = null;
            }
        }
        else if (linkingParentNode.GetChildren().Contains(node.name))
        {
            if (GUILayout.Button("unlink"))
            {
                linkingParentNode.RemoveChild(node.name);
                linkingParentNode = null;
            }

        }
        else
        {
            if (GUILayout.Button("child"))
            {
                linkingParentNode.AddChild(node.name);
                linkingParentNode = null;
            }
        }
    }

    private void DrawConnections(DialogueNode node)
    {
        Vector3 startPoisiton = new Vector2(node.GetRect().xMax, node.GetRect().center.y);

        foreach (DialogueNode childNode in selectedDialogue.GetAllChildren(node))
        {
            Vector3 endPosition = new Vector2(childNode.GetRect().xMin, childNode.GetRect().center.y);
            Vector3 controlPointOffset = endPosition - startPoisiton;
            controlPointOffset.y = 0;
            controlPointOffset.x *= 0.8f;
            Handles.DrawBezier(
                startPoisiton, endPosition,
                startPoisiton + controlPointOffset,
                endPosition - controlPointOffset,
                Color.white, null, 4f);
        }
    }

    private DialogueNode GetNodeAtPoint(Vector2 point)
    {
        DialogueNode foundNode = null;
        foreach (DialogueNode node in selectedDialogue.GetAllNodes())
        {
            if (node.GetRect().Contains(point))
            {
                foundNode = node;
            }
        }
        return foundNode;
    }

    private string GetHighlighted(out TextEditor t)
    {
        var field = typeof(EditorGUI).GetField("activeEditor", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
        var temp = field.GetValue(null);
        t = temp as TextEditor;
        return t.SelectedText;
    }

    private string AddTags(string tag, TextEditor textEditor, string highlightedInput, string input, string useFont = null, int fontSize = 0)
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
        else if (tag == "font")
        {
            modifiedText = $"<font={useFont}>{highlighted}</font>";
            newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);
        }
        else if (tag == "size")
        {
            modifiedText = $"<size={fontSize}>{highlighted}</size>";
            newText = originalText.Substring(0, start)
               + modifiedText
               + originalText.Substring(end);
        }
        return newText;
    }

    public bool RemoveRichTextTags(DialogueNode node, TextEditor t)
    {
        string original = node.GetText();
        int start = Mathf.Min(t.selectIndex, t.cursorIndex);
        int end = Mathf.Max(t.selectIndex, t.cursorIndex);

        // Handle empty selection (remove tags from entire text)
        if (start == end)
        {
            return false;
        }

        // Remove tags only from selected portion
        string before = original.Substring(0, start);
        string selected = original.Substring(start, end - start);
        string after = original.Substring(end);

        string cleanedSelected = Regex.Replace(selected, "<.*?>", string.Empty);
        string rebuilt = before + cleanedSelected + after;

        node.SetText(rebuilt);
        return selected != cleanedSelected;
    }
}