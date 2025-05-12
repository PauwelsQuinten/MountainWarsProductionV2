using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogue/DialogueLine")]
public class DialogueLines : ScriptableObject
{
    [Header("Text")]
    [CustomTextArea(5,10)]
    public string Text;
    [HideInInspector]
    public int LineSpacing = 36;
    [HideInInspector]
    public int CharacterSpacing = 0;
    public float DisplaySpeed;
    public TMP_FontAsset Font;
    public int FontSize = 36;
    public bool Bold = false;

    [Header("Images")]
    public List<GameObject> Images = new List<GameObject>();
    public bool HasSecondLine;

    [Header("Text Balloon")]
    public GameObject TextBalloon;
    public GameObject Tail;
    public float BorderSize = 0.2f;
    public bool FlipTextBalloon;
    public Vector2 Padding;

    [Header("Character Name")]
    public string _characterName;
}