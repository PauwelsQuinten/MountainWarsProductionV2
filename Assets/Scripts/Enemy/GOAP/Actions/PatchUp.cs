using UnityEngine;

public class PatchUp : GoapAction
{
    [Header("Input")]
    [SerializeField] private GameEvent _outputEvent;
    [SerializeField] float _executionTime = 0.25f;
    [Header("State")]
    [SerializeField] AttackSignal _attackSignal = AttackSignal.Idle;
    [SerializeField] Direction _swingDirection = Direction.ToCenter;

    private Coroutine _attackCoroutine;
    private bool _isMovementSet = false;

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {

    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {

    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return true;
    }

    public override bool IsCompleted(WorldState current)
    {
        return _isMovementSet || base.IsCompleted(current);
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return false;
    }

    public override void CancelAction()
    {

    }
}
