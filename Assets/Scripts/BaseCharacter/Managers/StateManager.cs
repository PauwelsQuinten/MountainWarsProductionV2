using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private const string PLAYER = "Player";
    [Header("Events")]
    //[SerializeField] GameEvent _OnKnockbackRecovery;
    [SerializeField] GameEvent _OnStunRecovery;
    [SerializeField] GameEvent _chancheAnim;
    [Header("Refrence")]
    [SerializeField] List<BlackboardReference> _blackboardRefs = new List<BlackboardReference>();
    [Header("Values")]
    [SerializeField] private float _rotationSpeed = 5.0f;
    [HideInInspector]
    public AttackState AttackState = AttackState.Idle;
    [HideInInspector]
    public AttackHeight AttackHeight = AttackHeight.Torso;
    [HideInInspector]
    public Orientation Orientation;
    [HideInInspector]
    public float fOrientation = 0f;
    [Header("Camera")]
    [SerializeField]
    public Camera StartCamera;
    [HideInInspector]
    public Camera CurrentCamera;

    [HideInInspector] 
    public GameObject Target;
    [HideInInspector]
    public bool IsHoldingShield;

    public EquipmentManager EquipmentManager;

    [HideInInspector]
    public bool IsBleeding;
    //[HideInInspector]
    public bool InAnimiation = false;
    [HideInInspector]
    public bool WeaponIsSheathed;
    [HideInInspector]
    public bool IsNearHidingSpot = false;

    private Coroutine _recoverCoroutine;

    private void Start()
    {
        CurrentCamera = StartCamera;
        if (EquipmentManager == null)
            EquipmentManager = GetComponent<EquipmentManager>();

        ChangeOrientation(this, new OrientationEventArgs { NewOrientation = Orientation, NewFOrientation = (float)Orientation });

        if (!gameObject.CompareTag(PLAYER))
        {
            StartCoroutine(InitBlackboard());         
        }
    }

    private void FixedUpdate()
    {
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, -(int)Orientation + 90, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
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
            //When its not the player, it means he only has 1 blackboardRef, his own. so always zero
            _blackboardRefs[0].variable.Target = Target;
            
            if (Target)
            {                
                foreach (var blackboard in _blackboardRefs)
                    blackboard.variable.TargetState = Target.GetComponent<StateManager>().AttackState;
            }

        }
    }

    public void ChangeOrientation(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as OrientationEventArgs;
        if (args == null) return;

        
        Orientation = args.NewOrientation;
        fOrientation = args.NewFOrientation;


        if (!gameObject.CompareTag(PLAYER))
        {
            foreach (var blackboard in _blackboardRefs)
                blackboard.variable.Orientation = Orientation;
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
        StunEventArgs args = obj as StunEventArgs;

        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            IsHoldingShield = false;
            SetStun(2f);
            InAnimiation = false;
        }

        else if (args != null && args.StunTarget == gameObject)
        {
            SetStun(args.StunDuration);
            InAnimiation = false;
        }
        
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
            InAnimiation = false;
        }

        //Update Blaackboard
        if (gameObject.CompareTag(PLAYER))
        {
            foreach (var blackboard in _blackboardRefs)
            {
                blackboard.variable.ResetCurrentAttack();
                blackboard.variable.TargetState = AttackState.Stun;
            }
        }
        else
            _blackboardRefs[0].variable.State = AttackState.Stun;
    }

    public void SetNewActiveCamera(Component sender, object obj)
    {
        Camera newCam = obj as Camera;
        if (newCam == null) return;

        CurrentCamera = newCam;
    }

    private IEnumerator RecoverStun(float stunDuration)
    {
        yield return new WaitForSeconds(stunDuration);
        AttackState = AttackState.Idle;
        InAnimiation = false;
        _OnStunRecovery.Raise(this, null);

        if (!gameObject.CompareTag(PLAYER))
            _blackboardRefs[0].variable.State = AttackState.Idle;
        else
            foreach (var blackboard in _blackboardRefs)
            {
                blackboard.variable.TargetState = AttackState.Idle;
            }
    }
    private IEnumerator InitBlackboard()
    {
        yield return new WaitForEndOfFrame();
        _blackboardRefs[0].variable.State = AttackState;
        _blackboardRefs[0].variable.Self = gameObject;
        _blackboardRefs[0].variable.Orientation = Orientation;
    }
}