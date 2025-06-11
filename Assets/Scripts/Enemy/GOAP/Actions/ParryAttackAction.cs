using System.Collections;
using UnityEngine;

public class ParryAttackAction : GoapAction
{
    [Header("Input")]
    [SerializeField] private GameEvent _inQueueDefense;
    [SerializeField, Tooltip("Time before he starts execution the action")] float _timeBeforexEecuting = 0.15f;
    [SerializeField, Tooltip("Time after his action start until he quits this action")] float _timeInterval = 1f;
    [Header("State")]
    [SerializeField] BlockMedium _blockMediume = BlockMedium.Shield;
    [SerializeField] Direction _direction = Direction.Idle;
    [SerializeField] float _swingSpeed = 1.25f;
    [SerializeField] bool _disarmOpponent =false;
    
    private Coroutine _defendCoroutine;
    private bool _isMovementSet = false;
    private bool _isMovementEnded = false;

    override public void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        //Debug.Log("start parrying action");
        _isMovementSet = false;
        _isMovementEnded = false;

        if (_direction != Direction.Idle)
            return;
    }

    override public void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //Debug.Log("update");
        if (blackboard.variable.TargetBlackboard == null 
            || blackboard.variable.TargetBlackboard.variable.CurrentAttack == AttackType.None)
            return;
             

        if (_isMovementSet == false)
        {
            if (blackboard.variable.TargetBlackboard.variable.CurrentAttack == AttackType.HorizontalSlashToLeft)
                _direction = Direction.ToLeft;
            else
                _direction = Direction.ToRight;

            //Debug.Log("send parry value");
            _defendCoroutine = StartCoroutine(DefendRoutine(_timeInterval));
            _isMovementSet = true;
        }

    }

    override public bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (currentWorldState.Behaviour != EBehaviourValue.Knock
            && currentWorldState.TargetBehaviour == EBehaviourValue.Attacking 
            && blackboard.variable.RHEquipmentHealth > 0f)
            return true;
        return false;
    }

    override public bool IsCompleted(WorldState current)
    {
        //return base.IsCompleted(current);
        return _isMovementEnded;
    }

    override public bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return currentWorldState.Behaviour == EBehaviourValue.Knock;
    }

    override public void CancelAction()
    {
        base.CancelAction();
        if (_defendCoroutine != null) 
            StopCoroutine(_defendCoroutine);
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        if (SeesParryableAttack(blackboard, currentWorldState))
        {
            float cost = Random.Range(0.1f, 0.4f);

            //Lower the change for disarming compared to parry with shield
            if (_disarmOpponent)
                cost += 0.075f;
            return cost;
        }
        else
            return Cost;
    }

   
    //-----------------------------------------------------------------------
    //Helper functions
    //-----------------------------------------------------------------------

    private IEnumerator DefendRoutine(float time)
    {
        yield return new WaitForSeconds(_timeBeforexEecuting);
        DefendMovement(false);
        yield return new WaitForEndOfFrame();
        //StopMovement();
        if (_disarmOpponent)
        {
            yield return new WaitForSeconds(0.2f);
            DefendMovement(true);

        }
        yield return new WaitForSeconds(time);
        _isMovementEnded = true;
        _isMovementSet = false;
    }

    private void DefendMovement(bool reverse)
    {
        AttackState state = AttackState.Idle;

        switch (_blockMediume)
        {
            case BlockMedium.Shield:
                state = AttackState.ShieldDefence;
                break;
            case BlockMedium.Sword:
                state = AttackState.SwordDefence;
                break;
            case BlockMedium.Nothing:
                state = AttackState.Idle;
                break;
        }


        SwingPackage(state, reverse);
    }

    private void StopMovement()
    {
        SwingPackage(AttackState.Idle, false);
    }

    private void SwingPackage(AttackState state, bool reverse)
    {
        if (reverse)
        {
            switch (_direction)
            {
               
                case Direction.ToRight:
                    _direction = Direction.ToLeft;
                    break;
                case Direction.ToLeft:
                    _direction = Direction.ToRight;
                    break;
               
            }
        }

        var package = new AimingOutputArgs
        {
            AimingInputState = AimingInputState.Idle
               ,
            AngleTravelled = 45f
               ,
            AttackHeight = AttackHeight.Torso
               ,
            Direction = _direction
               ,
            BlockDirection = Direction.Idle
               ,
            Speed = _swingSpeed
               ,
            AttackSignal = AttackSignal.Swing
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
        _inQueueDefense.Raise(npc.transform, package);
    }


}
