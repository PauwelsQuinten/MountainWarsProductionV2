using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

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

    [Header("Attack")]
    [SerializeField]
    private float _attackRange;
    [SerializeField]
    private GameEvent _doAttack;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    [Header("Enemy")]
    [SerializeField]
    private LayerMask _characterLayer;
    [SerializeField] BlackboardReference _blackboardRef;

    [Header("Animation")]
    [SerializeField]
    private GameEvent _changeAnimation;


    private float _chargePower;
    private float _attackPower;
    private AttackType _attackType;
    private AttackHeight _attackHeight;

    private bool _wasCharging;
    private float _startChargeTime;
    private float _endChargeTime;

    public void Attack(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        AimingOutputArgs args = obj as AimingOutputArgs;
        if (args == null) return;

        if (args.AttackState == AttackState.ShieldDefence
            || args.AttackState == AttackState.SwordDefence
            || args.AttackState == AttackState.Stun) return;

        if (args.AttackSignal != AttackSignal.Idle)
            PrintInput(args);

        _attackType = DetermineAttack(args);

        if (args.AnimationStart)
        {
            //Debug.Log($"speed: {args.Speed}");
            StartAnimation(args.Speed);
            return;
        }

        CalculateChargePower(args);

        if (DidFeint(args.AttackSignal))
        {
            Debug.Log("---------------------------Feint-----------------------");
            InteruptAnimation();

            //Signal to blackboard
            if (gameObject.CompareTag(PLAYER))
                _blackboardRef.variable.TargetCurrentAttack = AttackType.None;
            return;
        }

        if (args.AttackSignal != AttackSignal.Stab && args.AttackSignal != AttackSignal.Swing)
        {
            //Signal to blackboard
            if (gameObject.CompareTag(PLAYER))
                _blackboardRef.variable.TargetCurrentAttack = AttackType.None;
            return;
        }


        if (args.AttackSignal != AttackSignal.Stab)
        {
            if (!IsAngleBigEnough(args.AngleTravelled)) return;
            if (DidOverCommit(args.AngleTravelled)) return;
        }

        _attackRange = GetAttackMediumRange(args);
        _attackPower = CalculatePower(args);
        _attackHeight = args.AttackHeight;

        if (_attackType == AttackType.Stab) _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * 0.75f });
        else _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });

       
        //PrintInput2(args);
        //Signal to blackboard
        if (gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.TargetCurrentAttack = _attackType;
            _blackboardRef.variable.TargetState = args.AttackState;
        }
    }

    public void SwordHit(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        if (!IsEnemyInRange()) return;
        _doAttack.Raise(this, new AttackEventArgs { AttackType = _attackType, AttackHeight = _attackHeight, AttackPower = _attackPower });

    }


    private void StartAnimation(float speed)
    {
        if (_attackType == AttackType.HorizontalSlashToLeft)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.SlashLeft, AnimLayer = 3, DoResetIdle = true, Interupt = false, Speed = speed });
        }
        else if (_attackType == AttackType.HorizontalSlashToRight)
        {
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.SlashRight, AnimLayer = 3, DoResetIdle = true, Interupt = false, Speed = speed });
        }
    }
    
    private void InteruptAnimation()
    {
         _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false, Interupt = true });
    }
        

    private bool DidFeint(AttackSignal signal)
    {
        if(signal == AttackSignal.Feint) return true;
        return false;
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
        }
        else if (args.AttackSignal != AttackSignal.Charge && _wasCharging)
        {
            _endChargeTime = Time.time;
            _chargePower = _chargeSpeed * (_endChargeTime - _startChargeTime);
            _startChargeTime = 0;
            _endChargeTime = 0;
            _wasCharging = false;
        }
    }

    private float CalculatePower(AimingOutputArgs aimOutput)
    {
        float swingAngle = aimOutput.AngleTravelled / 100;
        float power = aimOutput.EquipmentManager.GetEquipmentPower();
        if (aimOutput.Speed != 0) power += _basePower * aimOutput.Speed + _chargePower;
        else power += _basePower + _chargePower;
        _chargePower = 0f;
        return swingAngle + power;
    }

    private float GetAttackMediumRange(AimingOutputArgs aimOutput)
    {
        return aimOutput.EquipmentManager.GetAttackRange();        
    }

    private AttackType DetermineAttack(AimingOutputArgs aimOutput)
    {
        if(aimOutput.AttackSignal == AttackSignal.Stab) return AttackType.Stab;
        if (aimOutput.Direction == Direction.ToRight) return AttackType.HorizontalSlashToRight;
        return AttackType.HorizontalSlashToLeft;
    }

    private bool IsEnemyInRange()
    {
        List<Collider> enemy = Physics.OverlapSphere(transform.position, _attackRange * 2).ToList();
        if (enemy == null) return false;
        foreach (Collider c in enemy)
        {
            if (((1 << c.gameObject.layer) & _characterLayer) != 0)
            {
                if (c.gameObject == gameObject) continue;
                if(Vector3.Distance(transform.position, c.transform.position) < _attackRange) return true;
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
