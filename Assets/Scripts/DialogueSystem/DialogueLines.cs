using UnityEngine;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogue/DialogueLine")]
public class DialogueLines : ScriptableObject
{
    [Header("Text")]
    [TextArea]
    public string Text;
    public float DisplaySpeed;
    public int FontSize = 36;
    public bool Bold = false;

    [Header("Text Balloon")]
    public GameObject TextBalloon;
    public bool _flipTextBalloon;

    [Header("Character Name")]
    public string _characterName;
}