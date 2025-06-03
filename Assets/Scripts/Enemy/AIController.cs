using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIController : MonoBehaviour
{
    [Header("State Manager")]
    [SerializeField]
    private StateManager _stateManager;

    [Header("Variables")]
    [SerializeField]
    private GameEvent _MoveEvent;

    
    [SerializeField]
    private GameEvent _shieldBash;

    [Header("Perception")]
    [SerializeField] private GameEvent _LookForTarget;
    
    [Header("Actions")]
    [SerializeField] private GameEvent _inQueueAction;

    private Coroutine _resetAttackheight;
    private AttackState _storredAttackState = AttackState.Idle;

    private bool _wasSprinting;
    private bool _isHoldingShield;
    private float _patchTimer;

    void Start()
    {
        StartCoroutine(CheckSurrounding());
    }

    //This ClearQueue is purely for storing his attackstate for if he is hilding shield or not
    //!! its important that this event is called before the statemanager stunEvent!!!
    public void OnStun(Component sender, object obj)
    {
        LoseEquipmentEventArgs loseEquipmentEventArgs = obj as LoseEquipmentEventArgs;
        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            _storredAttackState =
                _stateManager.AttackState == AttackState.Stun || _stateManager.AttackState == AttackState.BlockAttack ?
                    AttackState.Idle : _stateManager.AttackState;
        }

        var args = obj as StunEventArgs;
        if (args == null) return;
        if (args.StunTarget != gameObject) return;

        _storredAttackState =
            _stateManager.AttackState == AttackState.Stun ? AttackState.Idle : _stateManager.AttackState;
    }

    public void RecoveredStun(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        if (!_stateManager.EquipmentManager.HasFullEquipment() && _storredAttackState == AttackState.BlockAttack)
            _storredAttackState = AttackState.Idle;

        _stateManager.AttackState = _storredAttackState;
    }

    public void AIEvents(Component sender, object obj)
    {
        AIInputEventArgs args = obj as AIInputEventArgs;
        if (obj is null) return;
        if (args.Sender != gameObject) return;

        switch (args.Input)
        {
            case AIInputAction.PatchUp:
                PatchUp();
                break;
            case AIInputAction.Dash:
                Sprint(true);
                break;
             case AIInputAction.StopDash:
                Sprint(false);
                break;
            case AIInputAction.PickUp:
                break;
            case AIInputAction.LockShield:
                LockShield();
                break;
            case AIInputAction.GrabShield:
                _inQueueAction.Raise(this, new AimingOutputArgs {Special = SpecialInput.ShieldGrab, AnimationStart = true });
                break;
        }
    }

    private void Sprint(bool sprint)
    {
        DirectionEventArgs package;
        if (sprint)
        {
            _wasSprinting = true;
            package = new DirectionEventArgs { SpeedMultiplier = 1.5f };
            _MoveEvent.Raise(this, package);
        }
        else if (!sprint && _wasSprinting)
        {
            _wasSprinting = false;
            package = new DirectionEventArgs { SpeedMultiplier = 1f };
            _MoveEvent.Raise(this, package);
        }
       
    }

    private void PatchUp()
    {
        _inQueueAction.Raise(this, new AimingOutputArgs { Special = SpecialInput.PatchUp, AnimationStart = true});
    }

    public void PickUp()
    {
        _inQueueAction.Raise(this, new AimingOutputArgs { Special = SpecialInput.PickUp, AnimationStart = true});
    }

    public void LockShield()
    {
        if (!_stateManager.EquipmentManager.HasFullEquipment()) return;
        _isHoldingShield = true;
        _stateManager.IsHoldingShield = _isHoldingShield;
        _stateManager.AttackState = AttackState.BlockAttack;
    }

    private IEnumerator CheckSurrounding()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            _LookForTarget.Raise(this, new OrientationEventArgs { NewOrientation = _stateManager.Orientation});
        }

    }

}
