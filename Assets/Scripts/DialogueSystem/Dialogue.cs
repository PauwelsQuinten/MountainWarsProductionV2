using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


[CreateAssetMenu(fileName = "New Dialogue", menuName = "Story/Dialogue", order = 0)]
public class Dialogue : ScriptableObject, ISerializationCallbackReceiver
{
    [SerializeField]
    List<DialogueNode> nodes = new List<DialogueNode>();

    [SerializeField]
    Vector2 newNodeOffset = new Vector2(250, 0);
    Dictionary<string, DialogueNode> nodeLookup = new Dictionary<string, DialogueNode>();

    [SerializeField]
    private bool isStaticDialogue;
    [SerializeField]
    private bool isStarted;


    private void OnValidate()
    {


        nodeLookup.Clear();
        foreach (DialogueNode node in GetAllNodes())
        {
            nodeLookup[node.name] = node;
        }
    }

    public bool GetIsStarted()
    {
        return isStarted;
    }

    public void SetIsStarted(bool started)
    {
        isStarted = started;
    }

    public bool GetIsStaticDialogue()
    {
        return isStaticDialogue;
    }

    public IEnumerable<DialogueNode> GetAllNodes()
    {
        return nodes;
    }

    public DialogueNode GetRootNode()
    {
        return nodes[0];
    }

    public IEnumerable<DialogueNode> GetAllChildren(DialogueNode parentNode)
    {

        foreach (string childID in parentNode.GetChildren())
        {
            if (nodeLookup.ContainsKey(childID))
            {
                yield return nodeLookup[childID];
            }
        }

    }
    public bool GetCurrentIsPlayer(DialogueNode currentNode)
    {
        return currentNode.IsPlayerSpeaking();

    }

    public IEnumerable<DialogueNode> GetPlayerChildren(DialogueNode currentNode)
    {
        foreach (DialogueNode node in GetAllChildren(currentNode))
        {
            if (node.IsPlayerSpeaking())
            {
                yield return node;
            }
        }
    }
    public IEnumerable<DialogueNode> GetAIChildren(DialogueNode currentNode)
    {
        foreach (DialogueNode node in GetAllChildren(currentNode))
        {
            if (!node.IsPlayerSpeaking())
            {
                yield return node;
            }
        }
    }

#if UNITY_EDITOR
    public void CreateNode(DialogueNode parent)
    {
        DialogueNode newNode = MakeNode(parent);
        Undo.RegisterCreatedObjectUndo(newNode, "Created Dialogue Node");

        Undo.RecordObject(this, "Added Dialogue Node");
        AddNode(newNode);
    }

    private void AddNode(DialogueNode newNode)
    {
        nodes.Add(newNode);
        OnValidate();
    }

    private DialogueNode MakeNode(DialogueNode parent)
    {
        DialogueNode newNode = CreateInstance<DialogueNode>();
        newNode.name = Guid.NewGuid().ToString();

        if (parent != null)
        {
            parent.AddChild(newNode.name);
            newNode.SetPlayerSpeaking(!parent.IsPlayerSpeaking());
            newNode.SetPosition(parent.GetRect().position + newNodeOffset);
        }

        return newNode;
    }

    public void DeleteNode(DialogueNode nodeToDelete)
    {
        Undo.RecordObject(this, "Deleted Dialogue Node");

        nodes.Remove(nodeToDelete);
        OnValidate();
        CleanDanglingChildren(nodeToDelete);
        Undo.DestroyObjectImmediate(nodeToDelete);

    }

    private void CleanDanglingChildren(DialogueNode nodeToDelete)
    {
        foreach (DialogueNode node in GetAllNodes())
        {
            node.RemoveChild(nodeToDelete.name);
        }
    }
#endif
    public void OnBeforeSerialize()
    {

#if UNITY_EDITOR
        if (nodes.Count == 0)
        {
            DialogueNode newNode = MakeNode(null);

            AddNode(newNode);
        }
        if (AssetDatabase.GetAssetPath(this) != "")
        {
            foreach (DialogueNode node in GetAllNodes())
            {
                if (AssetDatabase.GetAssetPath(node) == "")
                {
                    AssetDatabase.AddObjectToAsset(node, this);

                }
            }
        }
#endif
    }

    public void OnAfterDeserialize()
    {

    }
}
