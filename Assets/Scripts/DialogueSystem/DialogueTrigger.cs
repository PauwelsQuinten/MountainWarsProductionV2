using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField]
    private BoolReference _startCondition;
    [SerializeField]
    private GameEvent _StartDialogue;
    [SerializeField]
    private int _nextDialogueIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (_startCondition.variable != null)
        {
            if (!_startCondition.value) return;
        }
        if (other.gameObject.tag != "Player") return;

        _StartDialogue.Raise(this, new DialogueTriggerEventArgs { NextDialogueIndex = _nextDialogueIndex });
    }
}
