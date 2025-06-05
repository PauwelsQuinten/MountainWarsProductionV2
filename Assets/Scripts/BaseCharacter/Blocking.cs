using System.Collections.Generic;
using UnityEngine;

public class Blocking : MonoBehaviour
{
    private const string PLAYER = "Player";

    private BlackboardReference _blackboard;

    [Header("Events")]
    [SerializeField] private GameEvent _stunFeedbackEvent;
    [SerializeField] private GameEvent _equipmentUpdate;
    [SerializeField] private GameEvent _succesfullHitEvent;
    [SerializeField] private GameEvent _changeAnimation;
    [SerializeField] private GameEvent _blockAnimation;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    [Header("StunFeedback")]
    [SerializeField] StunVariablesReference _stunValues;

    private StateManager _stateManager;
    private Direction _blockDirection;
    private Direction _storredHoldDirection;
    private AimingInputState _aimingInputState;
    private BlockMedium _blockMedium = BlockMedium.Shield;
    private AttackState _previousState = AttackState.Idle;
    private bool _isInParryMotion = false; //is used for the block result, when the shield is in a parry animation the result should be different

    private void Awake()
    {
        if (_stateManager == null)
            _stateManager = GetComponent<StateManager>();
        _blackboard = _stateManager.BlackboardRef;
    }
    public void BlockMovement(Component sender, object obj)
    {        
        //Check for vallid signal
        AimingOutputArgs args = obj as AimingOutputArgs;
        if (args == null) return;
        if (sender.gameObject != gameObject && args.Sender != gameObject) return;
        if (args.Special != SpecialInput.Default) return;

        //When Shield is locked and state hasnt changed, keep previous values
        //Make sure that it will always be a vallid Block, even after recovering from Stun
        if (args.IsHoldingBlock && args.AttackState == AttackState.BlockAttack)
        {
            UpdateBlackboard(args);
            return;
        }
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

            PlayShieldAnimation();
            UpdateBlackboard(args);
        }
        //Keep block direction when getting stunned in animator, on stun the block will allready be invallid so no need to temporary adjust it
        else if (args.AttackState == AttackState.Stun)
        {
            if (!args.EquipmentManager.HasFullEquipment())
            {
                _blockDirection = Direction.Idle;
                _aimingInputState = AimingInputState.Idle;
                _previousState = AttackState.Idle;
                LowerEquipment();
            }
            UpdateBlackboard(null);
        }
        //Lower shield when no vallid block input or state eg lowering shield
        else if ( ((int)_previousState >= 3 && (int)args.AttackState < 2 )
            || ((int)args.AttackState >= 3 && args.AimingInputState == AimingInputState.Idle))
        {
            _aimingInputState = AimingInputState.Idle;
            //_blockDirection = Direction.Idle;
            _previousState = args.AttackState;
            UpdateBlackboard(null);

            LowerEquipment();
        }

        else
        {
            _aimingInputState = AimingInputState.Idle;
            //_blockDirection = Direction.Idle;
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
        BlockMedium tempMedium = _blockMedium;
        if (!IsBlockMediumVallid() || _stateManager.AttackState == AttackState.Stun)
            tempMedium = BlockMedium.Nothing;


        BlockResult blockResult;
        switch(tempMedium)
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

        if (blockResult == BlockResult.Hit)
        {
            _succesfullHitEvent.Raise(this, args);
            _stunFeedbackEvent.Raise(this, new StunEventArgs
            { StunDuration = _stunValues.variable.StunOnHit, StunTarget = gameObject });
            args.BlockPower = 0f;

        }
        else
        {
            switch (blockResult)
            {
                case BlockResult.FullyBlocked:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value});
                    _stunFeedbackEvent.Raise(this, new StunEventArgs 
                    { StunDuration = _stunValues.variable.StunWhenGettingFullyBlocked, StunTarget = args.Attacker });
                    args.AttackPower *= 0.5f;
                    args.BlockPower = 10f;
                    break;
                case BlockResult.HalfBlocked:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 1.5f});
                    _stunFeedbackEvent.Raise(this, new StunEventArgs 
                    { StunDuration = _stunValues.variable.StunWhenGettingPartiallyBlocked, StunTarget = args.Attacker });
                    args.AttackPower *= 0.7f;
                    args.BlockPower = 6f;
                    break;
                case BlockResult.SwordBlock:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.5f });
                    _stunFeedbackEvent.Raise(this, new StunEventArgs
                    { StunDuration = _stunValues.variable.StunWhenGettingFullyBlockedBySword, StunTarget = args.Attacker });
                    args.AttackPower *= 0.8f;
                    args.BlockPower = 6f;

                    break;
                case BlockResult.SwordHalfBlock:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.75f });
                    _stunFeedbackEvent.Raise(this, new StunEventArgs 
                    { StunDuration =_stunValues.variable.StunWhenGettingPartiallyBlockedBySword, StunTarget = args.Attacker });
                    args.AttackPower *= 0.85f;
                    args.BlockPower = 3f;
                    //_succesfullHitEvent.Raise(this, args);
                    break;
                case BlockResult.PassiveBlock:
                    _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 1.5f});
                    _stunFeedbackEvent.Raise(this, new StunEventArgs 
                    { StunDuration = _stunValues.variable.StunWhenGettingPartiallyBlocked *0.5f, StunTarget = gameObject });
                    args.AttackPower *= 0.8f;
                    args.BlockPower = 3f;
                    break;
            }
            //Sent to equipment to deal with the damage to used equipment
            if (_equipmentUpdate)
                _equipmentUpdate.Raise(this, defenceEventArgs);
        }
        //Sent feedback animation to blocker and attacker
        if (_blockAnimation)
            _blockAnimation.Raise(this, args);       

    }

    public void SetStorredBlockValue(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        _storredHoldDirection = _blockDirection;
    }

    public void SetInParryMotion(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        _isInParryMotion = (bool)obj;
    }

    private void PlayShieldAnimation()
    {
        //send event for animation
        if (_blockMedium == BlockMedium.Shield)
            _changeAnimation.Raise(this, new AnimationEventArgs 
            { AnimState = AnimationState.ShieldEquip, AnimLayer = 4, BlockDirection = _blockDirection, BlockMedium = BlockMedium.Shield });
        else if (_blockMedium == BlockMedium.Sword)
            _changeAnimation.Raise(this, new AnimationEventArgs 
            { AnimState = AnimationState.SwordEquip, AnimLayer = 3, BlockDirection = _blockDirection, BlockMedium = BlockMedium.Sword });
    }

    private void LowerEquipment()
    {
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 3, BlockDirection = Direction.Idle });
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 4, BlockDirection = Direction.Idle });
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1});
    }

    private void UpdateBlackboard(AimingOutputArgs args)
    {
        if (args == null)
        {
            _blackboard.variable.ShieldState = Direction.Idle;
        }
        else
        {
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
        Direction blockDirection = _blockDirection;
        BlockResult blockResult = BlockResult.Hit;

        if (_aimingInputState != AimingInputState.Hold)
            blockDirection = Direction.Idle;

        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.SwordHalfBlock;
                else
                    blockResult = BlockResult.Hit;
                break;

            case AttackType.HorizontalSlashToLeft:
                if (IsShieldPresent())
                {
                    if (blockDirection == Direction.ToLeft)
                        blockResult = BlockResult.FullyBlocked;
                    else
                        blockResult = BlockResult.HalfBlocked;
                }
                else
                {
                    if (blockDirection == Direction.ToLeft)
                        blockResult = BlockResult.SwordBlock;
                    else
                        blockResult = BlockResult.Hit;
                }
                
                break;

            case AttackType.HorizontalSlashToRight:
                if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.Hit;
                else if (blockDirection == Direction.ToRight)
                    blockResult = BlockResult.SwordBlock;
                else
                    blockResult = BlockResult.Hit;
                break;

            case AttackType.ShieldBash:                
                blockResult = BlockResult.Hit;
                break;
            default:
                break;
        }

        return blockResult;
    }

    private BlockResult UsingShield(AttackEventArgs args)
    {
        Direction blockDirection = _blockDirection;
        BlockResult blockResult = BlockResult.Hit;

        if (_aimingInputState != AimingInputState.Hold )
            blockDirection = Direction.Idle;
         
        //When in parryMotion, it means that the parry failed and went over to block. This would be by a wrong direction or timing     
        if (_isInParryMotion)
            return BlockResult.Hit;
         

        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (blockDirection == Direction.Idle)
                    blockResult = BlockResult.PassiveBlock;
                else if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.FullyBlocked;
                break;

            case AttackType.HorizontalSlashToLeft:
                if (blockDirection == Direction.ToLeft)
                    blockResult = BlockResult.FullyBlocked;
                else if (blockDirection == Direction.ToRight )
                    blockResult = BlockResult.Hit;
                else if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.HalfBlocked;
                break;

            case AttackType.HorizontalSlashToRight:
                if (blockDirection == Direction.Idle)
                    blockResult = BlockResult.Hit;
                else if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.HalfBlocked;
                else if (blockDirection == Direction.ToRight)
                    blockResult = BlockResult.FullyBlocked;
                else
                    blockResult = BlockResult.Hit;
                break;

            case AttackType.ShieldBash: 
                if (blockDirection == Direction.ToCenter)
                    blockResult = BlockResult.HalfBlocked;
                else
                    blockResult = BlockResult.Hit;
                break;
            default:
                break;
        }

        return blockResult;
    }
}