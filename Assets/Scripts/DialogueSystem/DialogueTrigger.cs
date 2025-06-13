using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField]
    private BoolReference _startCondition;
    [SerializeField]
    private bool _needsToBeNegative;
    [SerializeField]
    private GameEvent _StartDialogue;
    [SerializeField]
    private int _nextDialogueIndex;
    private void OnTriggerEnter(Collider other)
    {
        if (_startCondition.variable != null)
        {
            if (!_startCondition.value && !_needsToBeNegative) return;
            else if (_startCondition.value && _needsToBeNegative) return;
        }
        if (other.gameObject.tag != "Player") return;

        _StartDialogue.Raise(this, new DialogueTriggerEventArgs { NextDialogueIndex = _nextDialogueIndex });
    }
}
