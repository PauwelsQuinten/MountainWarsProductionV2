using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("State Manager")]
    [SerializeField] private bool _wantToSeePatchingAnim = false;


    [Header("State Manager")]
    [SerializeField]
    private StateManager _stateManager;

    [Header("Input")]
    [SerializeField]
    private AimingInputReference _aimInputRef;
    [SerializeField]
    private MovingInputReference _moveInputRef;

    [Header("Stamina")]
    [SerializeField]
    private StaminaManager _staminaManager;
    [SerializeField]
    private FloatReference _dodgeCost;
    [SerializeField]
    private FloatReference _shieldBashCost;
    [SerializeField]
    private FloatReference _aimCost;
    [SerializeField]
    private FloatReference _sprintCost;

    [Header("Events")]
    [SerializeField]
    private GameEvent _dodge;
    [SerializeField]
    private GameEvent _shieldBash;
    [SerializeField]
    private GameEvent _pickupEvent;
    [SerializeField]
    private GameEvent _LookForTarget;
    [SerializeField]
    private GameEvent _changeAnimation;
    [SerializeField]
    private GameEvent _hide;
    [SerializeField]
    private GameEvent _pauseGame;
    [SerializeField]
    private GameEvent _sheathWeapon;
    [SerializeField]
    private GameEvent _inQueue;

    private Vector2 _moveInput;

    private Coroutine _resetAttackheight;
    private AttackState _storredAttackState = AttackState.Idle;

    private bool _isHoldingShield;

    private bool _wasSprinting;
    
    public Vector3 CharacterPosition
    {
        get => transform.position;
        set => transform.position = value;
    }
   
    private void Start()
    {
        _aimInputRef.variable.ValueChanged += AimInputRef_ValueChanged;
        _aimInputRef.variable.StateManager = _stateManager;
        _aimInputRef.variable.State = _stateManager.AttackState;
        _moveInputRef.variable.StateManager = _stateManager;

        _aimInputRef.variable.State = _stateManager.AttackState;

        StartCoroutine(CheckSurrounding());
    }

    //This OnStun is purely for storing his attackstate for if he is hilding shield or not
    //!! its important that this event is called before the statemanager stunEvent!!!
    public void GetStun(Component sender, object obj)
    {
        LoseEquipmentEventArgs loseEquipmentEventArgs = obj as LoseEquipmentEventArgs;
        if (loseEquipmentEventArgs != null && sender.gameObject == gameObject)
        {
            _storredAttackState =
                _stateManager.AttackState == AttackState.Stun || _stateManager.AttackState == AttackState.BlockAttack ? 
                    AttackState.Idle : _stateManager.AttackState;
            _aimInputRef.variable.State = AttackState.Stun;
        }

        var args = obj as StunEventArgs;
        if (args == null) return;
        if (args.ComesFromEnemy && sender.gameObject == gameObject) return;
        else if (!args.ComesFromEnemy && sender.gameObject != gameObject) return;

        _storredAttackState = 
            _stateManager.AttackState == AttackState.Stun? _storredAttackState : _stateManager.AttackState;
        _aimInputRef.variable.State = AttackState.Stun;
        //_stateManager.AttackState = AttackState.Stun;
    }
    
    public void RecoveredStun(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        if (!_stateManager.EquipmentManager.HasFullEquipment() && _storredAttackState == AttackState.BlockAttack)
            _storredAttackState = AttackState.Idle;

        _stateManager.AttackState = _storredAttackState;
        _aimInputRef.variable.State = _storredAttackState;
    }

    public void ProcessAimInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (_staminaManager.CurrentStamina < _aimCost.value) return;
        _aimInputRef.variable.value = ctx.ReadValue<Vector2>();
    }

    private void AimInputRef_ValueChanged(object sender, AimInputEventArgs e)
    {
        if (Time.timeScale == 0) return;
        if (_stateManager.AttackState == AttackState.Stun || _aimInputRef.variable.State == AttackState.Stun)
        {
            return;
        }


        if (_stateManager.AttackState == AttackState.ShieldDefence ||
            _stateManager.AttackState == AttackState.SwordDefence ||
            _stateManager.AttackState == AttackState.BlockAttack)
        {
            _aimInputRef.variable.State = _stateManager.AttackState;
            return;
        }

        if (_stateManager.AttackState != AttackState.Idle && _aimInputRef.Value == Vector2.zero) _stateManager.AttackState = AttackState.Idle;
        else if(_stateManager.AttackState != AttackState.Attack && _aimInputRef.Value != Vector2.zero) _stateManager.AttackState = AttackState.Attack;


       _aimInputRef.variable.State = _stateManager.AttackState;

    }

    public void ProccesMoveInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        Vector2 input = ctx.ReadValue<Vector2>();

        // Get camera vectors and flatten them to horizontal plane
        Vector3 forward = _stateManager.CurrentCamera.transform.forward;
        Vector3 right = _stateManager.CurrentCamera.transform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // Combine vectors with proper axis input
        Vector3 moveDirection = (forward * input.y) + (right * input.x);

        // If you need Vector2 output for ground movement (XZ plane)
        Vector2 output = new Vector2(moveDirection.x, moveDirection.z);

        _moveInputRef.variable.value = output;
    }

    public void ProccesSetBlockInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (_stateManager.AttackState == AttackState.Stun)
        {
            if (ctx.action.WasPressedThisFrame())
            {
                _storredAttackState = AttackState.ShieldDefence;
                //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ShieldEquip, AnimLayer = 3, DoResetIdle = false, Interupt = false });
            }

            if (ctx.action.WasReleasedThisFrame())
            {
                _storredAttackState = AttackState.Idle;
                //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false, Interupt = false });

            }
            return;
        }


        if (_stateManager.AttackState != AttackState.BlockAttack)
        {
            if (ctx.action.WasPressedThisFrame())
            {
                _stateManager.AttackState = AttackState.ShieldDefence;
                //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ShieldEquip, AnimLayer = 3, DoResetIdle = false, Interupt = false });
            }

            if (ctx.action.WasReleasedThisFrame())
            {
                _stateManager.AttackState = AttackState.Idle;
                //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false, Interupt = false });
                //_changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 3, DoResetIdle = false, Interupt = false });
            }
        }
        else if (ctx.performed)
        {
            _stateManager.AttackState = AttackState.ShieldDefence;
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ShieldEquip, AnimLayer = 4, DoResetIdle = false });
            _isHoldingShield = false;
            _stateManager.IsHoldingShield = _isHoldingShield;
        }

            _aimInputRef.variable.State = _stateManager.AttackState;
    }

    public void ProccesSetParryInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (_stateManager.AttackState == AttackState.Stun)
        {
            if (ctx.action.WasPressedThisFrame())
            {
               
                _storredAttackState = AttackState.SwordDefence;
            }

            if (ctx.action.WasReleasedThisFrame())
            {
                
                _storredAttackState = AttackState.Idle;
            }
            return;
        }

        if (ctx.action.WasPressedThisFrame())
        {
            if (_stateManager.AttackState == AttackState.BlockAttack) _isHoldingShield = true;

            _stateManager.IsHoldingShield = _isHoldingShield;
            _stateManager.AttackState = AttackState.SwordDefence;
        }

        if (ctx.action.WasReleasedThisFrame())
        {
            if (_isHoldingShield) _stateManager.AttackState = AttackState.BlockAttack;
            else _stateManager.AttackState = AttackState.Idle;
        }


        _aimInputRef.variable.State = _stateManager.AttackState;
    }

    public void ProccesDodgeInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
            if (ctx.performed)
        {
            _wasSprinting = true;
            _moveInputRef.variable.SpeedMultiplier = 1.5f;
        }
        if (ctx.canceled)
        {
            if (!_wasSprinting)
            {
                if(_stateManager.AttackState == AttackState.ShieldDefence || _stateManager.AttackState == AttackState.BlockAttack)
                {
                    if (_staminaManager.CurrentStamina < _shieldBashCost.value) return;
                    _shieldBash.Raise(this, EventArgs.Empty);
                }
                else
                {
                    if (_staminaManager.CurrentStamina < _dodgeCost.value) return;
                    _dodge.Raise(this, EventArgs.Empty);
                }
            }
            else
            {
                if (_staminaManager.CurrentStamina < _sprintCost.value) return;
                _wasSprinting = false;
                _moveInputRef.variable.SpeedMultiplier = 1;
            }
        }
    }

    public void ProccesInteractInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!ctx.performed) return;

        if (!_stateManager.EquipmentManager.HasEquipmentInHand(true))
            _pickupEvent.Raise(this);
        else if (_stateManager.IsNearHidingSpot)
            _hide.Raise(this, EventArgs.Empty);
        else
            _pickupEvent.Raise(this);

    }

    public void ProccesAtackHeightInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!ctx.performed) return;
        _stateManager.AttackHeight = AttackHeight.Head;

        if (_resetAttackheight != null) StopCoroutine(_resetAttackheight);
        _resetAttackheight = StartCoroutine(ResetAttackHeight());
    }

    public void ProssesLockShieldInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!ctx.performed) return;
        if (_stateManager.AttackState != AttackState.ShieldDefence ) return;
        if (!_stateManager.EquipmentManager.HasFullEquipment() ) return;
        _isHoldingShield = true;
        _stateManager.IsHoldingShield = _isHoldingShield;
        _stateManager.AttackState = AttackState.BlockAttack;
        _aimInputRef.variable.State = _stateManager.AttackState;
    }

    public void ProssesPatchUpInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        //This button will be used to sheat/unsheat sord when not bleeding
        if (ctx.action.WasPerformedThisFrame())
        {
            if (!_stateManager.IsBleeding && !_wantToSeePatchingAnim)
            {
                if (_stateManager.WeaponIsSheathed)
                {
                    _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.UnSheatSword, AnimationStart = true });
                }
                else if (!_stateManager.WeaponIsSheathed)
                {
                    _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.SheatSword, AnimationStart = true });
                }
            }
            else
            {               
                _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.PatchUp, AnimationStart = true });
            }
        }

        if(ctx.action.WasReleasedThisFrame() )
        {           
            _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.PatchUp, AnimationStart = false });
        }
    }

    public void ProccesPauseInput(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        _pauseGame.Raise(this, EventArgs.Empty);
    }

    public void ProccesShieldGrab(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!ctx.performed) return;
        //if (_stateManager.AttackState == AttackState.ShieldDefence || _stateManager.AttackState == AttackState.BlockAttack)
            _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.ShieldGrab, AnimationStart = true});
    }


    private IEnumerator ResetAttackHeight()
    {
        yield return new WaitForSeconds(1);
        _stateManager.AttackHeight = AttackHeight.Torso;
    }

    private IEnumerator CheckSurrounding()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            _LookForTarget.Raise(this, new OrientationEventArgs { NewOrientation = _stateManager.Orientation });
        }
    }
}
