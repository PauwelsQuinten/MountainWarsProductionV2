using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.GPUSort;
using System.Collections.Generic;


public class Parry : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _succesfullParryEvent;
    [SerializeField] private GameEvent _onFailedParryEvent;
    [SerializeField] private GameEvent _onDisarmEvent;
    [SerializeField] private GameEvent _changeAnimation;

    [Header("ParryValues")]
    [SerializeField] private float _disarmTime = 2.5f;
    [SerializeField] private float _parrySpeed = 1.5f;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;


    private Direction _swingDirection = Direction.Idle;
    private Coroutine _disarmRoutine;
    private bool _tryDisarm = false;
    private BlockMedium _parryMedium;
    private bool _InParryZone = false;
    private AttackType _opponentsAttack = AttackType.None;


    public void ParryMovement(Component sender, object obj)
    {
        AimingOutputArgs args = obj as AimingOutputArgs;
        if (args == null) return;
        if (sender.gameObject != gameObject && args.Sender != gameObject)
            return;
        if (args.Special != SpecialInput.Default) return;

        if (/*args.AttackState == AttackState.BlockAttack || */args.AttackState == AttackState.ShieldDefence || args.AttackState == AttackState.SwordDefence)
            _parryMedium = Blocking.GetBlockMedium(args);
        else
            return;


        if (args.AnimationStart)
        {
            Debug.Log($"{gameObject} ParryInput :{args.Direction}, {args.AngleTravelled}, {_parryMedium}");
            StartAnimation(args, _parryMedium);
            _swingDirection = args.Direction;
        }
        
    }

    //This is the signal coming from the attacker, checks if you where parying or not. if not it will be handled further in the BlockingScript
    public void StartParry(Component sender, object obj)
    {
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;
        if (args.Defender != gameObject) return;
       
        if (_InParryZone )
        {
            if (IsSuccesfullParry(args))
                OnSuccesfullParry(args);
            else
                OnFaildedParry(args);
        }
        else
            OnFaildedParry(args);
        
    }

    //Event called from the parry animation, determening when you are in the motion for a parry 
    public void InParryMotion(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        _InParryZone = (bool)obj;
        Debug.Log($"Parry mode = {_InParryZone}");

        if (_tryDisarm && IsSuccesfullDisarm())
            OnSuccesfullDisarm();
    }


    public void OnStun(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        _InParryZone = false;
    }

    //---------------------------------------------------------------------------------------------
    //Helper Functions
    //---------------------------------------------------------------------------------------------

    public bool IsSuccesfullParry(AttackEventArgs args)
    {
        if (_parryMedium == BlockMedium.Nothing )
            return false;

        //Debug.Log($"ParryAction : {_swingAngle} in {_swingDirection}");
        switch (args.AttackType)
        {
            case AttackType.Stab:
                return true;
                
            case AttackType.HorizontalSlashToLeft:
                if (_swingDirection == Direction.ToLeft )
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToRight:
                if (_swingDirection == Direction.ToRight)
                {
                    return true;
                } 
                break;
        }
        Debug.Log("wrong paryDirection");
        return false;
    }

    public bool IsSuccesfullDisarm()
    {
        //Debug.Log($"DisarmAction : {_swingAngle} in {_swingDirection}");
        if (_parryMedium != BlockMedium.Sword)
            return false;

        switch (_opponentsAttack)
        {
            case AttackType.Stab:
                return true;

            case AttackType.HorizontalSlashToLeft:
                if (_swingDirection == Direction.ToRight)
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToRight:
                if (_swingDirection == Direction.ToLeft)
                {
                    return true;
                }
                break;
        }

        return false;
    }

    private void OnSuccesfullParry(AttackEventArgs attackValues)
    {
        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });
        _succesfullParryEvent.Raise(this, new StunEventArgs {StunDuration = 2, StunTarget = attackValues.Attacker});
        Debug.Log("succesfullParry");

        _tryDisarm = true;
        _opponentsAttack = attackValues.AttackType;
        _disarmRoutine = StartCoroutine(DisarmAction(_disarmTime));
        
    }
    private void OnSuccesfullDisarm()
    {
        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 1.5f });
        _onDisarmEvent.Raise(this, new LoseEquipmentEventArgs{EquipmentType = EquipmentType.Melee, ToSelf = false});
        _tryDisarm = false;
        //_attackEventValues = null;

        Debug.Log("Disarmed");
        StopCoroutine(_disarmRoutine);
       
    }
    
    private void OnFaildedParry(AttackEventArgs attackValues)
    {       
        //Debug.Log("Failed Parry");
        //signal to Block
        _onFailedParryEvent.Raise(this, attackValues);
    }
    
    private void StartAnimation(AimingOutputArgs args, BlockMedium parryMedium)
    {
        List<int> animLayers = new List<int>{ 1 };
        //float _parrySpeed = args.Speed < 2.5f ? 2.5f : args.Speed;
        if (args.Direction == Direction.ToLeft)
        {
            if (parryMedium == BlockMedium.Shield)
                _changeAnimation.Raise(this, new AnimationEventArgs 
                { AnimState = AnimationState.ParryShieldLeft, AnimLayer = animLayers, DoResetIdle = true, Speed = _parrySpeed, IsFullBodyAnim = true });
            else if (parryMedium == BlockMedium.Sword)
            {
                
                if (_tryDisarm)
                    _parrySpeed = 4f;
                _changeAnimation.Raise(this, new AnimationEventArgs
                { AnimState = AnimationState.ParrySwordLeft, AnimLayer = animLayers, DoResetIdle = true, Speed = _parrySpeed, IsFullBodyAnim = true });
            }
        
        }
        else if (args.Direction == Direction.ToRight)
        {
            if (parryMedium == BlockMedium.Shield)
                _changeAnimation.Raise(this, new AnimationEventArgs 
                { AnimState = AnimationState.ParryShieldRight, AnimLayer = animLayers, DoResetIdle = true, Speed = _parrySpeed, IsFullBodyAnim = true });
            else if (parryMedium == BlockMedium.Sword)
            {
                if (_tryDisarm)
                    _parrySpeed = 4f;
                _changeAnimation.Raise(this, new AnimationEventArgs 
                { AnimState = AnimationState.ParrySwordRight, AnimLayer = animLayers, DoResetIdle = true, Speed = _parrySpeed, IsFullBodyAnim = true });
            }

        }
    }

    private IEnumerator DisarmAction(float timeForParrying)
    {
        yield return new WaitForSeconds(timeForParrying);

        Debug.Log("Failed Disarm");
        _tryDisarm = false;
        _opponentsAttack = AttackType.None;
    }


}
