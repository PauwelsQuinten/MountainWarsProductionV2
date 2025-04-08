using System.Collections;
using UnityEngine;

public class AttckAction : GoapAction
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
       
        _isMovementSet = true;
        _attackCoroutine = StartCoroutine(ExecuteAttack(_executionTime));
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
        base.CancelAction();
        if (_attackCoroutine != null) 
            StopCoroutine(_attackCoroutine);
    }

    //-----------------------------------------------------------------------
    //Helper functions
    //-----------------------------------------------------------------------
    private void SendPackage()
    {
        var package = new AimingOutputArgs
        {
            AimingInputState = AimingInputState.Hold
               ,
            AngleTravelled = 0f
               ,
            AttackHeight = AttackHeight.Torso
               ,
            Direction = _swingDirection
               ,
            BlockDirection = Direction.Idle
               ,
            Speed = 1.5f
               ,
            AttackSignal = _attackSignal
               ,
            AttackState = AttackState.Attack
               ,
            EquipmentManager = npc.GetComponent<EquipmentManager>()
               ,
            IsHoldingBlock = false
                ,
            Sender = npc
            ,
            AnimationStart = true
        };
        _outputEvent.Raise(this, package);
    }

    private IEnumerator ExecuteAttack(float executionTime)
    {
        SendPackage();
        yield return new WaitForSeconds(executionTime);
        _isMovementSet = false;
    }

}
