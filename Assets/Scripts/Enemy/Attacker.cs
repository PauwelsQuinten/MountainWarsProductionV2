using System.Collections;
using UnityEngine;

public class Attacker : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private GameEvent _attackEvent;
    [SerializeField] float _timeInterval = 1f;
    [Header("State")]
    [SerializeField] AttackType _attackType = AttackType.Stab;
    [SerializeField] AttackHeight _attackHeight = AttackHeight.Torso;
    [SerializeField] float _attackAngle = 180f;
    [SerializeField] float _attackSpeed = 5f;
    
    private Coroutine _AttackCoroutine;


    void Start()
    {
        _AttackCoroutine = StartCoroutine(Attack(_timeInterval));
    }

    private IEnumerator Attack(float time)
    {
        while (true)
        {
            yield return new WaitForSeconds(time);
            Attack();
        }
    }

    private void Attack()
    {
        //var attackPackage = new AttackEventArgs
        //{
        //    AttackType = _attackType
        //           ,
        //    AttackHeight = _attackHeight
        //           ,
        //    AttackPower = _attackPower
        //};
        //_deffendEvent.Raise(this, attackPackage);
        //    Debug.Log("Attack send");

        switch (_attackType)
        {
            case AttackType.Stab:
                StabPackage();
                break;
            case AttackType.HorizontalSlashToLeft:
                SwingPackage(true);
                break;
            case AttackType.HorizontalSlashToRight:
                SwingPackage(false);
                break;
            case AttackType.ShieldBash:
                break;
            case AttackType.None:
                break;
        }
        Debug.Log("Attack send");
    }

    private void SwingPackage(bool toLeft)
    {
        var package = new AimingOutputArgs
        {
            AimingInputState = AimingInputState.Idle
               ,
            AngleTravelled = _attackAngle
               ,
            AttackHeight = _attackHeight
               ,
            Direction = toLeft? Direction.ToLeft : Direction.ToRight
               ,
            BlockDirection = Direction.Idle
               ,
            Speed = _attackSpeed
               ,
            AttackSignal = AttackSignal.Swing
               ,
            AttackState = AttackState.Attack
               ,
            EquipmentManager = GetComponent<EquipmentManager>()
               ,
            IsHoldingBlock = false
        };
        _attackEvent.Raise(this, package);
    }


    private void StabPackage()
    {
        var package = new AimingOutputArgs
        {
            AimingInputState = AimingInputState.Idle
               ,
            AngleTravelled = 5f
               ,
            AttackHeight = _attackHeight
               ,
            Direction = Direction.ToCenter
               ,
            BlockDirection = Direction.Idle
               ,
            Speed = _attackSpeed
               ,
            AttackSignal = AttackSignal.Stab
               ,
            AttackState = AttackState.Attack
               ,
            EquipmentManager = GetComponent<EquipmentManager>()
               ,
            IsHoldingBlock = false
        };
        _attackEvent.Raise(this, package);
    }

}
