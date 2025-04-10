using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("State Manager")]
    [SerializeField]
    private StateManager _stateManager;

    [Header("Input")]
    [SerializeField]
    private AimingInputReference _aimInputRef;
    [SerializeField]
    private MovingInputReference _moveInputRef;

    [Header("Healing")]
    [SerializeField]
    private float _patchUpDuration;
    [SerializeField]
    private GameEvent _patchUpEvent;

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

    [Header("Dodge")]
    [SerializeField]
    private GameEvent _dodge;

    [Header("ShieldBash")]
    [SerializeField]
    private GameEvent _shieldBash;

    [Header("Interactions")]
    [SerializeField]
    private GameEvent _pickupEvent;
    [SerializeField]
    private GameEvent _hide;
    [SerializeField]
    private GameEvent _sheathWeapon;
    [SerializeField]
    private GameEvent _shieldGrab;


    [Header("Perception")]
    [SerializeField]
    private GameEvent _LookForTarget;

    [Header("Animations")]
    [SerializeField]
    private GameEvent _changeAnimation;

    [Header("Pause")]
    [SerializeField]
    private GameEvent _pauseGame;

    private Vector2 _moveInput;

    private Coroutine _resetAttackheight;
    private AttackState _storredAttackState = AttackState.Idle;

    private bool _isHoldingShield;

    private float _patchTimer;
    private float _patchStartTime;
    private float _patchEndTime;

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
        _moveInputRef.variable.StateManager = _stateManager;

        StartCoroutine(CheckSurrounding());
    }

    public void GetStun(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

        _storredAttackState = _stateManager.AttackState;
        _aimInputRef.variable.State = AttackState.Stun;
    }
    
    public void RecoveredStun(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;

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
        if (_stateManager.AttackState == AttackState.Stun)
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
        _moveInputRef.variable.value = ctx.ReadValue<Vector2>();
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
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.ShieldEquip, AnimLayer = 4, DoResetIdle = false, Interupt = false });
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
        _pickupEvent.Raise(this);
        _hide.Raise(this, EventArgs.Empty);

        if (_stateManager.WeaponIsSheathed)
        {
            _sheathWeapon.Raise(this, EventArgs.Empty);
            _stateManager.WeaponIsSheathed = false;
        }
        else if (!_stateManager.WeaponIsSheathed)
        {
            _sheathWeapon.Raise(this, EventArgs.Empty);
            _stateManager.WeaponIsSheathed = true;
        }
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
        if (_stateManager.AttackState != AttackState.ShieldDefence) return;
        _isHoldingShield = true;
        _stateManager.IsHoldingShield = _isHoldingShield;
        _stateManager.AttackState = AttackState.BlockAttack;
        _aimInputRef.variable.State = _stateManager.AttackState;
    }

    public void ProssesPatchUpInput(InputAction.CallbackContext ctx)
    {
        if (Time.timeScale == 0) return;
        if (!_stateManager.IsBleeding) return;
        if (ctx.action.WasPressedThisFrame())
        {
            _patchStartTime = Time.time;
            _patchUpEvent.Raise(this, false);
        }

        if(ctx.action.WasReleasedThisFrame())
        {
            _patchEndTime = Time.time;
            _patchTimer = _patchEndTime - _patchStartTime;

            if (_patchUpDuration >= _patchTimer) 
                _patchUpEvent.Raise(this, true);
            else _patchUpEvent.Raise(this, false);

            _patchStartTime = 0;
            _patchEndTime = 0;
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
        if (_stateManager.AttackState == AttackState.ShieldDefence || _stateManager.AttackState == AttackState.BlockAttack)
            _shieldGrab.Raise(this, EventArgs.Empty);
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
