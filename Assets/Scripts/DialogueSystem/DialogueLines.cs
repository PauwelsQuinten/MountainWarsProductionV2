using UnityEngine;

[CreateAssetMenu(fileName = "DialogueLine", menuName = "Dialogue/DialogueLine")]
public class DialogueLines : ScriptableObject
{
    [TextArea]
    public string Text;
    public float DisplaySpeed;
    public GameObject TextBalloon;
    public bool _flipTextBalloon;
    public string _characterName;
}