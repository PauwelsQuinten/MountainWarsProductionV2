using UnityEngine;
using System.Collections;
using static UnityEngine.Rendering.GPUSort;


public class Parry : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _succesfullParryEvent;
    [SerializeField] private GameEvent _onFailedParryEvent;
    [SerializeField] private GameEvent _onDisarmEvent;

    [Header("ParryValues")]
    [SerializeField] private float _minParrySwingAngle = 100f;
    [SerializeField] private float _minParryStabAngle = 60f;
    [SerializeField] private float _timeForParryingSwing = 1f;
    [SerializeField] private float _timeForParryingStab = 0.4f;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;


    private Direction _swingDirection = Direction.Idle;
    private float _swingAngle = 0f;
    private Coroutine _parryroutine;
    private AttackEventArgs _attackEventValues;
    private bool _tryDisarm = false;
    private BlockMedium _parryMedium;


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

        Debug.Log($"ParryInput :{args.Direction}, {args.AngleTravelled}, {_parryMedium}");

        if (_attackEventValues != null && _tryDisarm && _parryMedium == BlockMedium.Sword)
        {
            AttemptDisarm(args);
            
        }
        else if (_attackEventValues != null && (_parryMedium == BlockMedium.Sword || _parryMedium == BlockMedium.Shield))
        {
            AttemptParry(args);
        }
        else
        {
            _swingDirection = Direction.Idle;
            _swingAngle = 0f;
        }
    }

    public void StartParry(Component sender, object obj)
    {
        if (sender.gameObject == gameObject)
            return;
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null) return;

        float time = args.AttackType == AttackType.Stab? _timeForParryingStab: _timeForParryingSwing;

        if (_attackEventValues == null)
        {
            _parryroutine = StartCoroutine(ParryAction(time));
            _attackEventValues = args;
        }
    }


    private void AttemptParry(AimingOutputArgs args)
    {
        _swingDirection = args.Direction;
        _swingAngle = args.AngleTravelled;
        if (IsSuccesfullParry(_attackEventValues))
        {
            OnSuccesfullParry(_attackEventValues);
        }
    }

    private void AttemptDisarm(AimingOutputArgs args)
    {
        _swingDirection = args.Direction;
        _swingAngle = args.AngleTravelled;
        if (IsSuccesfullDisarm(_attackEventValues))
        {
            OnSuccesfullDisarm();
            _attackEventValues = null;
        }
    }

    public bool IsSuccesfullParry(AttackEventArgs args)
    {
        //Debug.Log($"ParryAction : {_swingAngle} in {_swingDirection}");
        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (_swingAngle >= _minParryStabAngle)
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToLeft:
                if (_swingDirection == Direction.ToLeft && _swingAngle >= _minParrySwingAngle)
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToRight:
                if (_swingDirection == Direction.ToRight && _swingAngle >= _minParrySwingAngle)
                {
                    return true;
                } 
                break;
        }

        return false;
    }

    public bool IsSuccesfullDisarm(AttackEventArgs args)
    {
        //Debug.Log($"DisarmAction : {_swingAngle} in {_swingDirection}");
        switch (args.AttackType)
        {
            case AttackType.Stab:
                if (_swingAngle >= _minParryStabAngle)
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToLeft:
                if (_swingDirection == Direction.ToRight && _swingAngle >= _minParrySwingAngle)
                {
                    return true;
                }
                break;

            case AttackType.HorizontalSlashToRight:
                if (_swingDirection == Direction.ToLeft && _swingAngle >= _minParrySwingAngle)
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
        float time = attackValues.AttackType == AttackType.Stab ? _timeForParryingStab : _timeForParryingSwing;
        StartCoroutine(DisarmAction(time));
        
    }
    private void OnSuccesfullDisarm()
    {
        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 1.5f });
        _onDisarmEvent.Raise(this, new LoseEquipmentEventArgs{EquipmentType = EquipmentType.Melee, ToSelf = false});
        _tryDisarm = false;
        _attackEventValues = null;
        Debug.Log("Disarmed");
    }
    
    private void OnFaildedParry(AttackEventArgs attackValues)
    {
       
        Debug.Log("Failed Parry");
        //signal to Block
        _onFailedParryEvent.Raise(this, attackValues);
    }
    

    private IEnumerator ParryAction(float timeForParrying)
    {
        yield return new WaitForSeconds(timeForParrying);

        if(!_tryDisarm)
        {
            OnFaildedParry(_attackEventValues);
            _attackEventValues = null;
        }
    }
    
    private IEnumerator DisarmAction(float timeForParrying)
    {
        yield return new WaitForSeconds(timeForParrying);

        _tryDisarm = false;
        _attackEventValues = null;

    }


}
