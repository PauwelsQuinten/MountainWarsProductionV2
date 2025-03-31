#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class TextureArrayCreator : EditorWindow
{
    private List<Texture2D> textures = new List<Texture2D>();
    private string arrayName = "NewTextureArray";
    private bool isLinear = false;

    [MenuItem("Tools/Texture Array Creator")]
    public static void ShowWindow()
    {
        GetWindow<TextureArrayCreator>("Texture Array Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Create Texture2DArray", EditorStyles.boldLabel);

        // Input field for naming the array
        arrayName = EditorGUILayout.TextField("Array Name", arrayName);

        // Toggle for linear/sRGB color space
        isLinear = EditorGUILayout.Toggle("Linear (Non-Color)", isLinear);

        // Drag and drop area for textures
        GUILayout.Label("Drag and Drop Textures Here:");
        Rect dropArea = GUILayoutUtility.GetRect(0, 100, GUILayout.ExpandWidth(true));
        GUI.Box(dropArea, "Drop Textures Here");

        // Handle drag and drop
        Event evt = Event.current;
        switch (evt.type)
        {
            case EventType.DragUpdated:
            case EventType.DragPerform:
                if (!dropArea.Contains(evt.mousePosition))
                    return;

                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                if (evt.type == EventType.DragPerform)
                {
                    DragAndDrop.AcceptDrag();

                    foreach (Object draggedObject in DragAndDrop.objectReferences)
                    {
                        if (draggedObject is Texture2D texture)
                        {
                            textures.Add(texture);
                        }
                    }
                }
                break;
        }

        // Display the list of textures
        GUILayout.Label("Textures to Include:");
        for (int i = 0; i < textures.Count; i++)
        {
            textures[i] = (Texture2D)EditorGUILayout.ObjectField($"Texture {i + 1}", textures[i], typeof(Texture2D), false);
        }

        // Button to create the Texture2DArray
        if (GUILayout.Button("Create Texture2DArray"))
        {
            CreateTextureArray();
        }

        // Button to clear the list
        if (GUILayout.Button("Clear Textures"))
        {
            textures.Clear();
        }
    }

    private void CreateTextureArray()
    {
        if (textures.Count == 0)
        {
            Debug.LogError("No textures added!");
            return;
        }

        // Check if all textures have the same dimensions and format
        int width = textures[0].width;
        int height = textures[0].height;
        TextureFormat format = textures[0].format;

        foreach (Texture2D texture in textures)
        {
            if (texture.width != width || texture.height != height || texture.format != format)
            {
                Debug.LogError("All textures must have the same dimensions and format!");
                return;
            }
        }

        // Create the Texture2DArray
        Texture2DArray textureArray = new Texture2DArray(width, height, textures.Count, format, true, isLinear);
        textureArray.name = arrayName;

        // Copy pixel data from textures to the array
        for (int i = 0; i < textures.Count; i++)
        {
            textureArray.SetPixels(textures[i].GetPixels(), i);
        }

        textureArray.Apply();

        // Save the Texture2DArray as an asset
        string path = EditorUtility.SaveFilePanelInProject("Save Texture2DArray", arrayName, "asset", "Save the Texture2DArray");
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(textureArray, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Texture2DArray saved at {path}");
        }
    }
}
#endif