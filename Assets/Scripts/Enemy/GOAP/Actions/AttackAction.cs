using System.Collections;
using UnityEngine;

public class AttckAction : GoapAction
{
    [Header("Input")]
    [SerializeField] private GameEvent _outputEvent;
    [SerializeField] private float _executionTime = 0.25f;
    [Header("State")]
    [SerializeField] AttackSignal _attackSignal = AttackSignal.Idle;
    [SerializeField] Direction _swingDirection = Direction.ToCenter;
    [SerializeField] private float _attackSpeed = 1.25f;

    private Coroutine _attackCoroutine;

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
       base.StartAction(currentWorldState, blackboard);
        bool outOfRange = currentWorldState.AttackRange != EWorldStateRange.InRange;
       _attackCoroutine = StartCoroutine(ExecuteAttack(_executionTime, outOfRange));
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
        return base.IsCompleted(current);
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

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        return Random.Range(0.3f, 0.9f);
    }

    //-----------------------------------------------------------------------
    //Helper functions
    //-----------------------------------------------------------------------
    private void SendPackage(bool outOfRange)
    {
        if (outOfRange)
            return;

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
            Speed = _attackSpeed
               ,
            AttackSignal = outOfRange? AttackSignal.Idle : _attackSignal
               ,
            AttackState = outOfRange ? AttackState.Idle : AttackState.Attack
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

    private IEnumerator ExecuteAttack(float executionTime, bool outOfRange)
    {
        SendPackage(outOfRange);
        yield return new WaitForSeconds(executionTime);
        ActionCompleted();
    }

}
