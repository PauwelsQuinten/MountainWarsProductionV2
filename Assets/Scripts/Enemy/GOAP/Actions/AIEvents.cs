using System.Collections;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class AIEvents : GoapAction
{
    private enum ActionState
    {
        Preparing,
        Start,
        Going,
        Finished
    }

    [Header("Input")]
    [SerializeField] private GameEvent _aiInputEvent;
    [Header("State")]
    [SerializeField] AIInputAction _swingDirection = AIInputAction.Interact;
    [SerializeField] float _actionActivationTime = 0f;
    [SerializeField] float _actionDuration = 0.25f;

    private ActionState _actionState = ActionState.Preparing;
    private Coroutine _actionPreperationCoroutine;
    private Coroutine _actionCoroutine;

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_actionActivationTime > 0f)
        {
            _actionPreperationCoroutine = StartCoroutine(StartPreparingAction());
            _actionState = ActionState.Preparing;
        }
        else
            _actionState = ActionState.Going;
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_actionState != ActionState.Start)
            return;

        _aiInputEvent.Raise(this, new AIInputEventArgs { Input = _swingDirection, Sender = blackboard.variable.Self });
        _actionCoroutine = StartCoroutine(StartAction());
        _actionState = ActionState.Going;
    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return true;
    }

    public override bool IsCompleted(WorldState current)
    {
        return _actionState == ActionState.Finished;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return currentWorldState.Behaviour == EBehaviourValue.Knock;
    }

    public override void CancelAction()
    {
        if (_actionCoroutine != null)
            StopCoroutine(_actionCoroutine);
        if (_actionPreperationCoroutine != null)
            StopCoroutine(_actionPreperationCoroutine);
        _actionState = ActionState.Preparing;
    }

    //------------------------------------------------------------------
    //Helper functions
    //------------------------------------------------------------------

    private IEnumerator StartAction()
    {
        yield return new WaitForSeconds(_actionDuration);
        _actionState = ActionState.Finished;
    }
    
    private IEnumerator StartPreparingAction()
    {
        yield return new WaitForSeconds(_actionActivationTime);
        _actionState = ActionState.Start;
    }

}
