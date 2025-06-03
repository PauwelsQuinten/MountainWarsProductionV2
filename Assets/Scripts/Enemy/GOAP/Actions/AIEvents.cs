using System.Collections;
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
    [SerializeField] AIInputAction _input = AIInputAction.PickUp;
    [SerializeField] float _actionActivationTime = 0f;
    [SerializeField] float _actionDuration = 0.25f;

    private ActionState _actionState = ActionState.Preparing;
    private Coroutine _actionPreperationCoroutine;
    private Coroutine _actionPerformingCoroutine;

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

        _aiInputEvent.Raise(this, new AIInputEventArgs { Input = _input, Sender = blackboard.variable.Self });
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
        base.CancelAction();
        if (_actionPerformingCoroutine != null)
            StopCoroutine(_actionPerformingCoroutine);
        if (_actionPreperationCoroutine != null)
            StopCoroutine(_actionPreperationCoroutine);
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        float cost = Cost;
        switch (_input)
        {
            case AIInputAction.PatchUp:
                if (blackboard.variable.Self != null && blackboard.variable.IsBleeding)
                    cost = 0.15f;
                break;
            case AIInputAction.Dash:
                break;
            case AIInputAction.StopDash:
                break;
            case AIInputAction.PickUp:
                break;
        }
        return cost;
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
