using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogue/DialogueLine")]
public class DialogueLines : ScriptableObject
{
    [Header("Text")]
    [CustomTextArea(5,10)]
    public MultiLineText Text;
    [HideInInspector]
    public int LineSpacing = 36;
    [HideInInspector]
    public int CharacterSpacing = 0;
    public float DisplaySpeed;
    public TMP_FontAsset BaseFont;
    public int BaseFontSize = 36;

    [Header("Images")]
    public List<GameObject> Images = new List<GameObject>();
    public bool HasSecondImageLine;

    [Header("Text Balloon")]
    public GameObject TextBalloon;
    public GameObject Tail;
    public float BorderSize = 0.2f;
    public bool FlipTextBalloon;
    public Vector2 Padding;

    [Header("Character Name")]
    public string _characterName;
}