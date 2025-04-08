using UnityEditor;
using UnityEngine;

public class Blocking : MonoBehaviour
{
    private const string PLAYER = "Player";

    [SerializeField] private BlackboardReference _blackboard;

    [Header("Events")]
    [SerializeField] private GameEvent _succesfullBlockevent;
    [SerializeField] private GameEvent _equipmentUpdate;
    [SerializeField] private GameEvent _succesfullHitEvent;
    [SerializeField] private GameEvent _changeAnimation;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    private Direction _blockDirection;
    private AimingInputState _aimingInputState;
    private BlockMedium _blockMedium = BlockMedium.Shield;
    private AttackState _previousState = AttackState.Idle;

    public void BlockMovement(Component sender, object obj)
    {
        //Check for vallid signal
        if (sender.gameObject != gameObject) return;
        AimingOutputArgs args = obj as AimingOutputArgs;
        if (args == null) return;

        //When Shield is locked and state hasnt changed, keep previous values
        if (args.IsHoldingBlock && args.AttackState == AttackState.BlockAttack)
            return;
        //Debug.Log($"package to Block State = {args.AttackState}, hold: {args.AimingInputState}, {_blockMedium}, {args.BlockDirection}");

        //only set movement when using a valid Blocking input
        if ((args.AttackState == AttackState.ShieldDefence || 
            args.AttackState == AttackState.SwordDefence )
            && args.AimingInputState == AimingInputState.Hold
            )
        {
            //Store Blocking values
            _blockMedium = Blocking.GetBlockMedium(args);
            _aimingInputState = args.AimingInputState;
            _blockDirection = args.BlockDirection;
            _previousState = args.AttackState;

            //send event for animation
            if (_blockMedium == BlockMedium.Shield)
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ShieldEquip, AnimLayer = 3, DoResetIdle = false, Interupt = false });
            else if (_blockMedium == BlockMedium.Sword)
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.SwordEquip, AnimLayer = 3, DoResetIdle = false, Interupt = false });            


            UpdateBlackboard(args);
        }
        else if ( ((int)_previousState >= 3 && (int)args.AttackState < 3 )
            || ((int)args.AttackState >= 3 && args.AimingInputState != AimingInputState.Hold))
        {
            _aimingInputState = AimingInputState.Idle;
            _blockDirection = Direction.Idle;
            _previousState = args.AttackState;
            UpdateBlackboard(null);

            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false, Interupt = false });
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 3, DoResetIdle = false, Interupt = false });

        }

        else
        {
            _aimingInputState = AimingInputState.Idle;
            _blockDirection = Direction.Idle;
            _previousState = args.AttackState;

        }

     
    }
               
    public void CheckBlock(Component sender, object obj)
    {
        //Check for vallid signal
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;

        if(args.AttackType == AttackType.ShieldBash)
        {
            if (sender.gameObject == gameObject) return;
        }
        else if (sender.gameObject != gameObject) return;


        //Compare attack with current defence
        if (!IsBlockMediumVallid())
            _blockMedium = BlockMedium.Nothing;

        BlockResult blockResult;
        switch(_blockMedium)
        {
            case BlockMedium.Shield:
                blockResult = UsingShield(args);
                break;

            case BlockMedium.Sword:
                blockResult = UsingSword(args);
                break;

            case BlockMedium.Nothing:
            default:
                blockResult = BlockResult.Hit;
                break;

        }


        //Send the result as a gameEvent
        DefenceEventArgs defenceEventArgs = new DefenceEventArgs
        {
            BlockResult = blockResult,
            AttackHeight = args.AttackHeight,
            AttackPower = args.AttackPower,
            BlockMedium = _blockMedium
        };
        //Debug.Log($"{blockResult}, {gameObject}");


        if (blockResult == BlockResult.Hit)
        {
            _succesfullHitEvent.Raise(this, args);
        }
        else
        {
            switch (blockResult)
            {
                case BlockResult.FullyBlocked:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value});
                    _succesfullBlockevent.Raise(this, new StunEventArgs { StunDuration = 2f, ComesFromEnemy = true });
                    break;
                case BlockResult.HalfBlocked:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 1.5f});
                    _succesfullBlockevent.Raise(this, new StunEventArgs { StunDuration = 1f, ComesFromEnemy = true });
                    break;
                case BlockResult.SwordBlock:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.5f });
                    _succesfullBlockevent.Raise(this, new StunEventArgs { StunDuration = 0.75f, ComesFromEnemy = true });
                    break;
                case BlockResult.SwordHalfBlock:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.75f });
                    _succesfullBlockevent.Raise(this, new StunEventArgs { StunDuration = 0.5f, ComesFromEnemy = true });
                    break;
            }
                       
            _equipmentUpdate.Raise(this, defenceEventArgs);
        }

    }


    private void UpdateBlackboard(AimingOutputArgs args)
    {
        if (args == null)
        {
            if (gameObject.CompareTag(PLAYER))
                _blackboard.variable.TargetShieldState = Direction.Idle;
            else
                _blackboard.variable.ShieldState = Direction.Idle;
        }
        else
        {
            if (gameObject.CompareTag(PLAYER))
                _blackboard.variable.TargetShieldState = args.BlockDirection;
            else
                _blackboard.variable.ShieldState = args.BlockDirection;
        }
       
    }

    static public BlockMedium GetBlockMedium(AimingOutputArgs args)
    {
        if (args.AttackState == AttackState.SwordDefence)
        {
            if (args.EquipmentManager.HasEquipmentInHand(true))
                return BlockMedium.Sword;
            else if (args.EquipmentManager.HasEquipmentInHand(false))
                return BlockMedium.Shield;
            return BlockMedium.Nothing;
        }

        else if (args.AttackState != AttackState.Stun )
        {
             if (args.EquipmentManager.HasEquipmentInHand(false))
                return BlockMedium.Shield;
             else if (args.EquipmentManager.HasEquipmentInHand(true))
                return BlockMedium.Sword;
            return BlockMedium.Nothing;
        }

        else if (args.AttackState == AttackState.Stun)
            return BlockMedium.Nothing;

        Debug.LogError("No Blockstate, so what are you doing here");
        return BlockMedium.Nothing;
    }

    private bool IsBlockMediumVallid()
    {
        var equipment = GetComponent<EquipmentManager>();
        if (equipment == null) return false;

        bool isRightHand = _blockMedium == BlockMedium.Shield ? false:  true;
        return equipment.HasEquipmentInHand(isRightHand);
    }
    
    private bool IsShieldPresent()
    {
        var equipment = GetComponent<EquipmentManager>();
        if (equipment == null) return false;

        return equipment.HasEquipmentInHand(false);
    }


    private BlockResult UsingSword(AttackEventArgs args)
    {
        BlockResult blockResult = BlockResult.Hit;
        if (_aimingInputState != AimingInputState.Hold)
            _blockDirection = Direction.Idle;

        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (_blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.SwordHalfBlock;
                else
                    blockResult = BlockResult.Hit;
                break;

            case AttackType.HorizontalSlashToLeft:
                if (IsShieldPresent())
                {
                    if (_blockDirection == Direction.ToLeft)
                        blockResult = BlockResult.FullyBlocked;
                    else
                        blockResult = BlockResult.HalfBlocked;
                }
                else
                {
                    if (_blockDirection == Direction.ToLeft)
                        blockResult = BlockResult.SwordBlock;
                    else
                        blockResult = BlockResult.Hit;
                }
                
                break;

            case AttackType.HorizontalSlashToRight:
                if (_blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.Hit;
                else if (_blockDirection == Direction.ToRight)
                    blockResult = BlockResult.SwordBlock;
                else
                    blockResult = BlockResult.Hit;
                break;

            default:
                break;
        }

        return blockResult;
    }

    private BlockResult UsingShield(AttackEventArgs args)
    {
        BlockResult blockResult = BlockResult.Hit;
        if (_aimingInputState != AimingInputState.Hold)
            _blockDirection = Direction.Idle;

        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (_blockDirection == Direction.Idle)
                    blockResult = BlockResult.Hit;
                else if (_blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.FullyBlocked;
                break;

            case AttackType.HorizontalSlashToLeft:
                if (_blockDirection == Direction.ToLeft)
                    blockResult = BlockResult.FullyBlocked;
                else if (_blockDirection == Direction.ToRight)
                    blockResult = BlockResult.Hit;
                else
                    blockResult = BlockResult.HalfBlocked;
                break;

            case AttackType.HorizontalSlashToRight:
                if (_blockDirection == Direction.Idle)
                    blockResult = BlockResult.Hit;
                if (_blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.HalfBlocked;
                else if (_blockDirection == Direction.ToRight)
                    blockResult = BlockResult.FullyBlocked;
                else
                    blockResult = BlockResult.Hit;
                break;

            default:
                break;
        }

        return blockResult;
    }

}