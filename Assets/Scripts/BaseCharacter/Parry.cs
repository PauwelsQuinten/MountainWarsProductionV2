using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.GPUSort;


public class Parry : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _succesfullParryEvent;
    [SerializeField] private GameEvent _onFailedParryEvent;
    [SerializeField] private GameEvent _onDisarmEvent;
    [SerializeField] private GameEvent _changeAnimation;

    [Header("ParryValues")]
    [SerializeField] private float _disarmTime = 2.5f;
    //[SerializeField] private float _minParrySwingAngle = 100f;
    //[SerializeField] private float _minParryStabAngle = 60f;
    //[SerializeField] private float _timeForParryingSwing = 1f;
    //[SerializeField] private float _timeForParryingStab = 0.4f;

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


        if (args.AttackState == AttackState.BlockAttack || args.AttackState == AttackState.ShieldDefence || args.AttackState == AttackState.SwordDefence)
            _parryMedium = Blocking.GetBlockMedium(args);
        else
            return;


        if (args.AnimationStart)
        {
            //Debug.Log($"ParryInput :{args.Direction}, {args.AngleTravelled}, {_parryMedium}");
            StartAnimation(args, _parryMedium);
            _swingDirection = args.Direction;
        }
        else
        {
            _swingDirection = Direction.Idle;
        }
    }

    public void StartParry(Component sender, object obj)
    {
        if (sender.gameObject == gameObject)
            return;
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;

       
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

    public void InParryMotion(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        _InParryZone = (bool)obj;

        if (_tryDisarm && IsSuccesfullDisarm())
            OnSuccesfullDisarm();
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
        _succesfullParryEvent.Raise(this, new StunEventArgs {StunDuration = 3, ComesFromEnemy = true});
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
        if (args.Direction == Direction.ToLeft)
        {
            if (parryMedium == BlockMedium.Shield)
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ParryShieldLeft, AnimLayer = 4, DoResetIdle = true, Interupt = false, Speed = args.Speed });
            else if (parryMedium == BlockMedium.Sword)
            {
                float speed = args.Speed < 2f ? 2f : args.Speed;
                if (_tryDisarm)
                    speed = 4f;
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ParrySwordLeft, AnimLayer = 3, DoResetIdle = true, Interupt = false, Speed = speed  });
            }
        
        }
        else if (args.Direction == Direction.ToRight)
        {
            if (parryMedium == BlockMedium.Shield)
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ParryShieldRight, AnimLayer = 4, DoResetIdle = true, Interupt = false, Speed = args.Speed });
            else if (parryMedium == BlockMedium.Sword)
            {
                float speed = args.Speed < 2f ? 2f : args.Speed;
                if (_tryDisarm)
                    speed = 4f;
                _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ParrySwordRight, AnimLayer = 3, DoResetIdle = true, Interupt = false, Speed = speed });
            }

        }
    }

    //---------------------------------------------------------------------------------------------
    //Coroutines Functions
    //---------------------------------------------------------------------------------------------


    //private IEnumerator ParryAction(float timeForParrying)
    //{
    //    yield return new WaitForSeconds(timeForParrying);
    //
    //    if(!_tryDisarm)
    //    {
    //        OnFaildedParry(_attackEventValues);
    //        _attackEventValues = null;
    //    }
    //}
    
    private IEnumerator DisarmAction(float timeForParrying)
    {
        yield return new WaitForSeconds(timeForParrying);

        Debug.Log("Failed Disarm");
        _tryDisarm = false;
        _opponentsAttack = AttackType.None;
    }


}
