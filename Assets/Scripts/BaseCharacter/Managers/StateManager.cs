using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateManager : MonoBehaviour
{
    private const string PLAYER = "Player";
    [Header("Events")]
    [SerializeField] GameEvent _OnStunRecovery;
    [SerializeField] GameEvent _vfx;

    [Header("Enemies")]
    public LayerMask TargetLayers;

    [Header("Refrence")]
    public BlackboardReference BlackboardRef;
    public BoolReference IsInDialogue;
    public BoolReference IsInStaticDialogue;

    [Header("Values")]
    [SerializeField] private float _rotationSpeed = 5.0f;
    [HideInInspector]
    public AttackState AttackState = AttackState.Idle;
    [HideInInspector]
    public AttackHeight AttackHeight = AttackHeight.Torso;
    private Orientation _orientation= Orientation.West;
    [HideInInspector]
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            _orientation = value;
        }
    }
    [HideInInspector]
    [Tooltip("Angle of orientation in degree")] public float fOrientation = 0f;
    [Header("Camera")]
    [SerializeField]
    public Camera StartCamera;
    [HideInInspector]
    public Camera CurrentCamera;

    [HideInInspector] 
    public GameObject Target;
    [HideInInspector]
    public bool IsHoldingShield;

    [HideInInspector]public EquipmentManager EquipmentManager;

    [HideInInspector]
    public bool IsBleeding;
    [HideInInspector]
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
        float bodyRotation = Geometry.Geometry.BodyRotationAngleFromOrientation(Orientation);
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, bodyRotation, 0));
        transform.rotation = targetRotation;

        StartCoroutine(InitBlackboard());         
    }

    private void FixedUpdate()
    {
        //Rotate smoothly between the 8 directions
        float bodyRotation = Geometry.Geometry.BodyRotationAngleFromOrientation(Orientation);
        Quaternion targetRotation = Quaternion.Euler(new Vector3(0, bodyRotation, 0));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
    }

    public void SetTarget(Component sender, object obj)
    {
        if(sender.gameObject != gameObject) return;
        var args = obj as NewTargetEventArgs;
        if (args == null) return;

        Target = args.NewTarget;


        BlackboardRef.variable.Target = Target;
        
        //Update Blackboard        
        if (Target)
        {
            BlackboardRef.variable.TargetBlackboard = Target.GetComponent<StateManager>().BlackboardRef;
        }
        else
            BlackboardRef.variable.TargetBlackboard = null;
    }

    public void ChangeOrientation(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as OrientationEventArgs;
        if (args == null) return;

        
        Orientation = args.NewOrientation;
        fOrientation = args.NewFOrientation;

        BlackboardRef.variable.Orientation = Orientation;
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
            InAnimiation = false;
            float duration = 2f;
            SetStun(duration);
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
            _vfx.Raise(this, new VfxEventArgs { Type = VfxType.Stuned, Duration = stunDuration });
        }

        BlackboardRef.variable.State = AttackState.Stun;
        BlackboardRef.variable.ResetCurrentAttack();
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

        BlackboardRef.variable.State = AttackState.Idle;
    }
       
    private IEnumerator InitBlackboard()
    {
        yield return new WaitForEndOfFrame();
        BlackboardRef.variable.State = AttackState;
        BlackboardRef.variable.Self = gameObject;
        BlackboardRef.variable.Orientation = Orientation;
    }

    private void AdjustOrientationToCamera(Camera cameraOld, Camera cameraNew)
    {
        if (cameraNew == null || cameraOld == null) return;

        float yawnOld = cameraOld.transform.eulerAngles.y;
        float yawnNew = cameraNew.transform.eulerAngles.y;
        float diffAngle = Mathf.DeltaAngle(yawnOld, yawnNew);


    }
}