using System.Collections;
using UnityEngine;

public class BlockAction : GoapAction
{
    [Header("Events")]
    [SerializeField, Tooltip("event send to the ActionQueue")] private GameEvent _outputEvent;
    [SerializeField, Tooltip("event send to the AIController")] private GameEvent _aiControllerEvent;
    [Header("State")]
    [SerializeField, Tooltip("time it will take after sending the event and the end of this action")] float _executionTime = 0.25f;
    [SerializeField] BlockMedium _blockMediume = BlockMedium.Shield;
    [SerializeField] Direction _direction = Direction.ToCenter;
    [SerializeField, Tooltip("When true, it will look at the opponent attack and direct his block towards it. els it will be in the direction you have it set")]
    bool _useAimedBlock = false;

    private Coroutine _defendCoroutine;
    

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        if (_useAimedBlock)
        {
            switch (blackboard.variable.CurrentAttack)
            {
                case AttackType.None:
                case AttackType.Stab:
                case AttackType.ShieldBash:
                    _direction = Direction.ToCenter;
                    break;
                case AttackType.HorizontalSlashToLeft:
                    _direction = Direction.ToLeft;
                    break;
                case AttackType.HorizontalSlashToRight:
                    _direction = Direction.ToRight;
                    break;
            }
        }

        _defendCoroutine = StartCoroutine(ExecuteBlock(_executionTime));
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {

    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return true;
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        return base.CalculateCost(blackboard, currentWorldState);
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
        if (_defendCoroutine != null) 
            StopCoroutine( _defendCoroutine );
    }

    //-----------------------------------------------------------------------
    //Helper functions
    //-----------------------------------------------------------------------
    private void SendPackage()
    {
        AttackState state = _blockMediume == BlockMedium.Shield ? AttackState.ShieldDefence : AttackState.SwordDefence;

        var package = new AimingOutputArgs
        {
            AimingInputState = AimingInputState.Hold
               ,
            AngleTravelled = 0f
               ,
            AttackHeight = AttackHeight.Torso
               ,
            Direction = Direction.Idle
               ,
            BlockDirection = _direction
               ,
            Speed = 15f
               ,
            AttackSignal = AttackSignal.Idle
               ,
            AttackState = state
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
        _aiControllerEvent.Raise(this, new AIInputEventArgs { Input = AIInputAction.LockShield, Sender = npc });
    }

    private IEnumerator ExecuteBlock(float executionTime)
    {
        SendPackage();
        yield return new WaitForSeconds(executionTime);
        ActionCompleted();
    }

}
