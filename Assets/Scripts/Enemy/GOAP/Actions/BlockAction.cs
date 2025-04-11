using System.Collections;
using UnityEngine;

public class BlockAction : GoapAction
{
    [Header("Input")]
    [SerializeField] private GameEvent _outputEvent;
    [SerializeField] float _executionTime = 0.25f;
    [Header("State")]
    [SerializeField] BlockMedium _blockMediume = BlockMedium.Shield;
    [SerializeField] Direction _direction = Direction.ToCenter;
    [SerializeField] bool _useAimedBlock = false;

    private Coroutine _defendCoroutine;
    

    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        if (_useAimedBlock)
        {
            switch (blackboard.variable.TargetCurrentAttack)
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
    }

    private IEnumerator ExecuteBlock(float executionTime)
    {
        SendPackage();
        yield return new WaitForSeconds(executionTime);
        ActionCompleted();
    }

}
