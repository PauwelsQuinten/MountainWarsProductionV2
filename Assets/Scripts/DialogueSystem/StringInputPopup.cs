using UnityEditor;
using UnityEngine;

public class StringInputPopup : EditorWindow
{
    private string inputText = "";
    private System.Action<string> onOk;

    // Show the popup and provide a callback for when OK is pressed
    public static void Show(string title, string initialText, System.Action<string> onOk)
    {
        var window = ScriptableObject.CreateInstance<StringInputPopup>();
        window.titleContent = new GUIContent(title);
        window.inputText = initialText;
        window.onOk = onOk;
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 100);
        window.ShowUtility();
    }

    void OnGUI()
    {
        GUILayout.Label("Enter font Name:", EditorStyles.boldLabel);
        inputText = EditorGUILayout.TextField(inputText);

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("OK"))
        {
            onOk?.Invoke(inputText);
            Close();
        }
        if (GUILayout.Button("Cancel"))
        {
            Close();
        }
        GUILayout.EndHorizontal();
    }
}
