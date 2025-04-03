using System.Collections;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private const string PLAYER = "Player";

    [SerializeField] GameEvent _OnKnockbackRecovery;
    [SerializeField] BlackboardReference _blackboardRef;
    [SerializeField] GameEvent _OnStunRecovery;
    public AttackState AttackState;
    public AttackHeight AttackHeight;
    public Orientation Orientation;

    public GameObject Target;
    public bool IsHoldingShield;

    public EquipmentManager EquipmentManager;

    public bool IsBleeding;

    private void Start()
    {
        if (EquipmentManager == null)
            EquipmentManager = GetComponent<EquipmentManager>();
        if (!gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.State = AttackState;
            _blackboardRef.variable.Self = gameObject;
            _blackboardRef.variable.Orientation = Orientation;
        }

    }
    
    public void GetStunned(Component sender, object obj)
    {
        StunEventArgs args = obj as StunEventArgs;
        if (args == null) return;

        if (args.ComesFromEnemy)
        {
            if (sender.gameObject == gameObject) return;
        }
        else if (sender.gameObject != gameObject) return;

        Debug.Log($"Stuned {gameObject}");
        AttackState = AttackState.Stun;
        StartCoroutine(RecoverStun(args.StunDuration));

        if (gameObject.CompareTag(PLAYER))
            _blackboardRef.variable.ResetCurrentAttack();
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

        if (!gameObject.CompareTag(PLAYER))
        {
            _blackboardRef.variable.Orientation = Orientation;
        }
    }
    
    private IEnumerator RecoverStun(float stunDuration)

    {
        yield return new WaitForSeconds(stunDuration);
        _OnStunRecovery.Raise(this);
        AttackState = AttackState.Idle;
    }
}