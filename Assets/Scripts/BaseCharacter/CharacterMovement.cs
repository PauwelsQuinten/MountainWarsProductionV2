using System;
using Unity.VisualScripting;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class CharacterMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private MovingInputReference _moveInput;

    [Header("Movement")]
    [SerializeField]
    private float _speed;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _removeStamina;

    [Header("Animation")]
    [SerializeField]
    private GameEvent _changeAnimation;

    [Header("StateManager")]
    [SerializeField]
    private StateManager _stateManager;
    
    private Rigidbody _rb;
    private Vector3 _movedirection;
    private float _angleInterval = 22.5f;

    private StaminaManager _staminaManager;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _moveInput.variable.ValueChanged += MoveInput_ValueChanged;
    }

    private void FixedUpdate()
    {
        if (gameObject.CompareTag("Player")) 
            _rb.Move(new Vector3(transform.position.x, transform.position.y,transform.position.z) + (_movedirection * (_speed * _moveInput.variable.SpeedMultiplier)) * Time.deltaTime, transform.rotation);
    }

    private void MoveInput_ValueChanged(object sender, EventArgs e)
    {
        UpdateMoveVector(null);
    }

    public void MoveInput(Component sender, object obj)
    {
        var args = obj as DirectionEventArgs;
        if (args == null) return;
        if (args.Sender != gameObject) return;
        
        UpdateMoveVector(args);
    }

    private void UpdateMoveVector(DirectionEventArgs args )
    {
        float speedMultiplier = 0f;
        Vector2 input = Vector2.zero;

        if (args == null)
        {
            speedMultiplier = _moveInput.variable.SpeedMultiplier;
            input = _moveInput.Value;
        }
        else
        {
            speedMultiplier = args.SpeedMultiplier;
            input = args.MoveDirection;
        }

        if (!this.enabled)
        {
            _movedirection = Vector3.zero;
            return;
        }

        if (_staminaManager == null) _staminaManager = GetComponent<StaminaManager>();
        if (speedMultiplier > 1)
        {
            if (_staminaCost.value > _staminaManager.CurrentStamina)
            {
                _moveInput.variable.SpeedMultiplier = 1;
            }
            else
                _removeStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * Time.deltaTime });
        }

        _movedirection = new Vector3(input.x, 0f, input.y);

        if (input != Vector2.zero)
        {
            if (speedMultiplier > 1)
            {
                Animator animator = GetComponentInChildren<Animator>();
                var clipInfo = animator.GetCurrentAnimatorClipInfo(2);
                if (clipInfo.Length > 0)
                {
                    if (clipInfo[clipInfo.Length - 1].clip.name != "Run")
                        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Run, AnimLayer = 2, DoResetIdle = false, Interupt = false });
                }
                else _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Run, AnimLayer = 2, DoResetIdle = false, Interupt = false });
            }
            else
            {
                Animator animator = GetComponentInChildren<Animator>();
                AnimatorClipInfo[] clipInfo = null;
                if (animator != null)
                    clipInfo = animator.GetCurrentAnimatorClipInfo(2);
                if (clipInfo != null && clipInfo.Length > 0)
                {
                    if (clipInfo[clipInfo.Length - 1].clip.name != "Walk")
                        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Walk, AnimLayer = 2, DoResetIdle = false, Interupt = false });
                    
                }
                else _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Walk, AnimLayer = 2, DoResetIdle = false, Interupt = false });
            }
        }
        else
        {
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false, Interupt = false });
            _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 2, DoResetIdle = false , Interupt = false });
        }

        if (_stateManager.Target == null)
            UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        if (_movedirection == Vector3.zero) return;
        if (_movedirection.magnitude <= 0.2f) return;
        float moveInputAngle = Mathf.Atan2(_movedirection.z, _movedirection.x);
        moveInputAngle = moveInputAngle * Mathf.Rad2Deg;

        if (moveInputAngle > 0 - _angleInterval && moveInputAngle < 0 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.East) return;
            _stateManager.Orientation = Orientation.East;
            transform.rotation = Quaternion.Euler(new Vector3(0, 90, 0));
        }
        else if (moveInputAngle > 45 - _angleInterval && moveInputAngle < 45 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.NorthEast) return;
            _stateManager.Orientation = Orientation.NorthEast;
            transform.rotation = Quaternion.Euler(new Vector3(0, 45, 0));
        }
        else if (moveInputAngle > 90 - _angleInterval && moveInputAngle < 90 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.North) return;
            _stateManager.Orientation = Orientation.North;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        else if (moveInputAngle > 135 - _angleInterval && moveInputAngle < 135 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.NorthWest) return;
            _stateManager.Orientation = Orientation.NorthWest;
            transform.rotation = Quaternion.Euler(new Vector3(0, -45, 0));
        }
        else if (moveInputAngle > 180 - _angleInterval && moveInputAngle < 180 || moveInputAngle < -180 + _angleInterval && moveInputAngle > -180)
        {
            if (_stateManager.Orientation == Orientation.West) return;
            _stateManager.Orientation = Orientation.West;
            transform.rotation = Quaternion.Euler(new Vector3(0, -90, 0));
        }
        else if (moveInputAngle > -135 - _angleInterval && moveInputAngle < -135 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.SouthWest) return;
            _stateManager.Orientation = Orientation.SouthWest;
            transform.rotation = Quaternion.Euler(new Vector3(0, -135, 0));
        }
        else if (moveInputAngle > -90 - _angleInterval && moveInputAngle < -90 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.South) return;
            _stateManager.Orientation = Orientation.South;
            transform.rotation = Quaternion.Euler(new Vector3(0, 180, 0));
        }
        else if (moveInputAngle > -45 - _angleInterval && moveInputAngle < -45 + _angleInterval)
        {
            if (_stateManager.Orientation == Orientation.SouthEast) return;
            _stateManager.Orientation = Orientation.SouthEast;
            transform.rotation = Quaternion.Euler(new Vector3(0, 135, 0));
        }
    }

    private void OnDisable()
    {
        if(_changeAnimation == null) return;
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = 1, DoResetIdle = false });
        _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = 2, DoResetIdle = false });
    }

    private void OnDestroy()
    {
        _moveInput.variable.ValueChanged -= MoveInput_ValueChanged;
    }
}
