using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Attacking : MonoBehaviour
{
    private const string PLAYER = "Player";

    [Header("AttackAngles")]
    [SerializeField]
    private float _minAttackAngle;
    [SerializeField]
    private float _overCommitAngle;

    [Header("Power")]
    [SerializeField]
    private float _basePower;
    [SerializeField]
    private float _chargeSpeed;
    [SerializeField]
    private float _chargeDownSpeed;
    [SerializeField]
    private float _maxChargedPower = 20f;

    [Header("Attack")]
    private float _attackRange;
    [SerializeField, Tooltip("This event is called from the moment the attack is hit and sends its info to the opponent defence")]
    private GameEvent _doAttack;
    [SerializeField, Tooltip("This event is called from the moment the attack starts, after the feint. This is for giving it a forwards push ")]
    private GameEvent _moveAttack;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    private LayerMask _characterLayer;
    private BlackboardReference _blackboardRef;

    [Header("Animation")]
    [SerializeField]
    private GameEvent _changeAnimation;

    private bool _isFullBodyAnim = false;

    private float _chargePower = 0f;
    [HideInInspector] public float ChargedPower
    {
        get { return _chargePower; }
    }
    [HideInInspector] public bool ChargePowerUsed = false;
    private float _attackPower = 0f;
    private AttackType _attackType;
    private AttackHeight _attackHeight = AttackHeight.Torso;

    private bool _wasCharging;
    private float _startChargeTime;

    private StateManager _stateManager;

    private void Awake()
    {
        if (_stateManager == null) 
            _stateManager = GetComponent<StateManager>();
        _blackboardRef = _stateManager.BlackboardRef;
    }

    private void Start()
    {
        _characterLayer = _stateManager.TargetLayers;
    }
    private void Update()
    {
        UPdateChargePowerOnRelease();
    }

    public void Attack(Component sender, object obj)
    {
        AimingOutputArgs args = obj as AimingOutputArgs;
        if (args == null || args.Special != SpecialInput.Default) return;
        if (sender.gameObject != gameObject && args.Sender != gameObject) return;
        if (_stateManager.WeaponIsSheathed) return;

        if (args.AttackState == AttackState.ShieldDefence
            || args.AttackState == AttackState.SwordDefence
            || args.AttackState == AttackState.Stun) return;

        //if (args.AttackSignal != AttackSignal.Idle)
        //    PrintInput(args);
        
        //All attack signals are by default feint = true, when the angle movement is bigger then the set min value
        //it will sent a signal to set feint to false and continue the attck animation instead of cancelling it.
        if (!args.IsFeint )
        {
            _changeAnimation.Raise(this, new AnimationEventArgs { IsFeint = false });
            _moveAttack.Raise(this, new AttackMoveEventArgs { Attacker = gameObject, AttackType = _attackType});
            _changeAnimation.Raise(this, new AttackMoveEventArgs { Attacker = gameObject, AttackType= _attackType });
            return;
        }

        if (args.AttackSignal == AttackSignal.Idle )
        {
            _blackboardRef.variable.CurrentAttack = AttackType.None;
            CalculateChargePower(args);
            return;
        }

        CalculateChargePower(args);

        _attackType = DetermineAttack(args);
        _attackRange = GetAttackMediumRange(args);
        if (_attackType != AttackType.Charge)
            _attackPower = CalculatePower(args);
        _attackHeight = args.AttackHeight;
        //Debug.Log($"charging : {_wasCharging}, power: {_attackPower}");

        if (args.AnimationStart)
        {
            //Debug.Log($"_movementSpeed: {args.Speed}");
            bool useRightArm = args.EquipmentManager.HasEquipmentInHand(true) || args.EquipmentManager.HasNoneInHand();
            StartAnimation(args.Speed, useRightArm, args.AttackHeight == AttackHeight.Head);
        }

        //Signal to blackboard
        _blackboardRef.variable.CurrentAttack = _attackType;
        _blackboardRef.variable.State = AttackState.Attack;
       
    }

    public void SwordHit(Component sender, object obj)
    {
        //Event coming from the spear
        AttackEventArgs args = obj as AttackEventArgs;
        if (args != null && args.Attacker == gameObject)
        {
            if (_attackType == AttackType.Stab) _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.75f });
            else _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });

            _doAttack.Raise(this, args);
        }

        //event coming from the sword attack animations
        else if (sender.gameObject != gameObject) return;

        if (_attackType == AttackType.Stab) _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.75f });
        else _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });

        GameObject target = null;
        if (!IsEnemyInRange(out target)) return;

        _doAttack.Raise(this, new AttackEventArgs {
            AttackType = _attackType, 
            AttackHeight = _attackHeight, 
            AttackPower = _attackPower ,
            Attacker = gameObject,
            Defender = target
        });

    }

    public void AttackFinished(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        if (_stateManager != null)
        {            
            _blackboardRef.variable.CurrentAttack = AttackType.None;
            _blackboardRef.variable.State = _stateManager.AttackState == AttackState.Stun? _stateManager.AttackState : AttackState.Idle;
        }
    }

    private void StartAnimation(float speed, bool useRightArm, bool isAttackHigh)
    {
        //FullBodyAnim -> for setting a bool in animator so that the lowerBody mask will not override the layer
        //DoResetIdle -> To make sure no idle animation is playing at same time
        //Speed -> The animation speed
        //AttackWithLeftHand -> a bool set so the attacks will be with shield or sword.

        int animLayers = 1 ;
        if (_attackType == AttackType.HorizontalSlashToLeft)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs 
            { AnimState = AnimationState.SlashLeft, AnimLayer = animLayers, Speed = 1.5f
            , IsAttackHigh = isAttackHigh, AttackWithLeftHand = !useRightArm
            , IsFullBodyAnim = _isFullBodyAnim
            });
        }
        else if (_attackType == AttackType.HorizontalSlashToRight)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs 
            { AnimState = AnimationState.SlashRight, AnimLayer = animLayers, Speed = 1.5f
            , IsAttackHigh = isAttackHigh, AttackWithLeftHand = !useRightArm
            , IsFullBodyAnim = _isFullBodyAnim
            });
        }
        else if (_attackType == AttackType.Stab)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs 
            { AnimState = AnimationState.Stab, AnimLayer = animLayers , Speed = 1.5f
            , IsAttackHigh = isAttackHigh, AttackWithLeftHand = !useRightArm
            , IsFullBodyAnim = _isFullBodyAnim
            });
        }
        else if (_attackType == AttackType.Charge)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Charge, AnimLayer = 3 });
        }

    }
        
    private bool IsAngleBigEnough(float currentAngle)
    {
        if (currentAngle > _minAttackAngle) return true;
        return false;
    }

    private bool DidOverCommit(float currentAngle)
    {
        if (currentAngle > _overCommitAngle)
        {
            Debug.Log("overcomiited");
            return true;
        }
        return false;
    }

    private void CalculateChargePower(AimingOutputArgs args)
    {
        if(args.AttackSignal == AttackSignal.Charge && !_wasCharging)
        {
            _wasCharging = true;
            _startChargeTime = Time.time;
            ChargePowerUsed = false;
        }
        else if (args.AttackSignal != AttackSignal.Charge && _wasCharging)
        {
            _startChargeTime = 0;
            _wasCharging = false;
        }
        
    }

    private void UPdateChargePowerOnRelease()
    {
        if (_startChargeTime != 0)
        {
            float _chargedTime = Time.time - _startChargeTime;
            _chargePower = _chargeSpeed * _chargedTime + 1f;
            if (_chargePower > _maxChargedPower)
                _chargePower = _maxChargedPower;
        }
        
        else if (_chargePower > 0 && _startChargeTime == 0)
        {
            float newTime = _chargePower - _chargeDownSpeed * Time.deltaTime;
            _chargePower = (newTime > 0f) ? newTime : 0f;
        }
    }

    private float CalculatePower(AimingOutputArgs aimOutput)
    {
        float swingAngle = aimOutput.AngleTravelled / 100;
        float power = aimOutput.EquipmentManager.GetEquipmentPower();
        if (aimOutput.Speed != 0) power += _basePower * aimOutput.Speed + _chargePower;
        else power += _basePower + _chargePower;
        _chargePower = 0f;
        ChargePowerUsed = true;
        return swingAngle + power;
    }

    private float GetAttackMediumRange(AimingOutputArgs aimOutput)
    {
        return aimOutput.EquipmentManager.GetAttackRange();        
    }

    private AttackType DetermineAttack(AimingOutputArgs aimOutput)
    {
        if(aimOutput.AttackSignal == AttackSignal.Stab) return AttackType.Stab;
        if(aimOutput.AttackSignal == AttackSignal.Charge) return AttackType.Charge;
        if (aimOutput.Direction == Direction.ToRight) return AttackType.HorizontalSlashToRight;
        return AttackType.HorizontalSlashToLeft;
    }

    private bool IsEnemyInRange(out GameObject hitTarget)
    {
        List<Collider> enemy = Physics.OverlapSphere(transform.position, _attackRange * 2).ToList();
        hitTarget = null;
        if (enemy == null) return false;
        foreach (Collider c in enemy)
        {
            if (((1 << c.gameObject.layer) & _characterLayer) != 0)
            {
                if (c.gameObject == gameObject) continue;
                if (Vector3.Distance(transform.position, c.transform.position) < _attackRange)
                {
                    hitTarget = c.gameObject;                
                    return true;
                }
            }
        }
            return false;
    }

    private void PrintInput(AimingOutputArgs args)
    {
        Debug.Log($"attack input : {args.AttackSignal}, state: {args.AttackState}, {args.Direction}, {args.AngleTravelled}. owner: {gameObject}");
    }
    
    private void PrintInput2(AimingOutputArgs args)
    {
        Debug.Log($"attack input after checking : {args.AttackSignal}, state: {args.AttackState}, {args.Direction}, {args.AngleTravelled}. owner: {gameObject}, power = {_attackPower},{args.AttackHeight}");
    }
}
