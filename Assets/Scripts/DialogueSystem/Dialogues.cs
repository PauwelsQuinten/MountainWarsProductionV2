using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Dialogues", menuName = "Dialogue/Dialogues")]
public class Dialogues : ScriptableObject
{
    public List<DialogueLines> Lines = new List<DialogueLines>();
    public bool IsStarted;
}