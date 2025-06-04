using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField]
    private GameEvent _StartDialogue;
    [SerializeField]
    private int _nextDialogueIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag != "Player") return;

        _StartDialogue.Raise(this, new DialogueTriggerEventArgs { NextDialogueIndex = _nextDialogueIndex });
    }
}
