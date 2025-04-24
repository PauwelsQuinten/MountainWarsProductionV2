using System.Collections;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private const string PLAYER = "Player";
    [Header("Events")]
    //[SerializeField] GameEvent _OnKnockbackRecovery;
    [SerializeField] GameEvent _OnStunRecovery;
    [Header("Refrence")]
    [SerializeField] BlackboardReference _blackboardRef;
    [Header("Values")]
    public AttackState AttackState;
    public AttackHeight AttackHeight = AttackHeight.Torso;
    public Orientation Orientation;
    //[HideInInspector]
    public float fOrientation = 0f;

    [HideInInspector] public GameObject Target;
    public bool IsHoldingShield;

    public EquipmentManager EquipmentManager;

    public bool IsBleeding;
    public bool InAnimiation = false;
    public bool WeaponIsSheathed;
    public bool IsNearHidingSpot = false;

    private Coroutine _recoverCoroutine;

    private void Start()
    {
        if (EquipmentManager == null)
            EquipmentManager = GetComponent<EquipmentManager>();

        ChangeOrientation(this, new OrientationEventArgs { NewOrientation = Orientation, NewFOrientation = (float)Orientation });

        if (!gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.State = AttackState;
            _blackboardRef.variable.Self = gameObject;
            _blackboardRef.variable.Orientation = Orientation;
        }

    }
    public void SetTarget(Component sender, object obj)
    {
        if(sender.gameObject != gameObject) return;
        var args = obj as NewTargetEventArgs;
        if (args == null) return;

        Target = args.NewTarget;

        //Update Blackboard
        if (!gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.Target = Target;
            
            if (Target)
                _blackboardRef.variable.TargetState = Target.GetComponent<StateManager>().AttackState;
        }
    }

    public void ChangeOrientation(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as OrientationEventArgs;
        if (args == null) return;

        Orientation = args.NewOrientation;
        fOrientation = args.NewFOrientation;
        transform.rotation = Quaternion.Euler(new Vector3(0, -(int)Orientation + 90, 0));


        if (!gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.Orientation = Orientation;
        }
    }

    public void OnAnimationStart(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        InAnimiation = true;
    }
    
    public void OnAnimationEnd(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        InAnimiation = false;
    }

    //Add event for disabling BlockAttack when equipment breaks

    public void GetStunned(Component sender, object obj)
    {
        LoseEquipmentEventArgs loseEquipmentEventArgs = obj as LoseEquipmentEventArgs;
        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            IsHoldingShield = false;
            SetStun(2f);
        }


        StunEventArgs args = obj as StunEventArgs;
        if (args == null) return;

        if (args.ComesFromEnemy)
        {
            if (sender.gameObject == gameObject) return;
        }
        else if (sender.gameObject != gameObject) return;

        //Debug.Log($"Stuned {gameObject.name}");
        SetStun(args.StunDuration);
    }

    public void EnterHidingSpot(Component sender, object obj)
    {
        if (!gameObject.CompareTag(PLAYER)) return;

        var args = obj as TriggerEnterEventArgs;
        if (args.IsHidingSpot)
            IsNearHidingSpot = true;
    }
    
    public void LeaveHidingSpot(Component sender, object obj)
    {
        if (!gameObject.CompareTag(PLAYER)) return;

            IsNearHidingSpot = false;
        
    }

    private void SetStun(float stunDuration)
    {
        if (AttackState != AttackState.Stun)
        {
            AttackState = AttackState.Stun;
            _recoverCoroutine = StartCoroutine(RecoverStun(stunDuration));
        }

        if (gameObject.CompareTag(PLAYER))
            _blackboardRef.variable.ResetCurrentAttack();
    }

    private IEnumerator RecoverStun(float stunDuration)
    {
        yield return new WaitForSeconds(stunDuration);
        AttackState = AttackState.Idle;
        _OnStunRecovery.Raise(this, null);
    }
}