using System;
using UnityEngine;
using UnityEngine.Windows;

public class CharacterMovement : MonoBehaviour
{
    [Header("Input")]
    [SerializeField]
    private MovingInputReference _moveInput;

    [Header("Movement")]
    [SerializeField]
    private FloatReference _speed;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _sprintCost;
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
    Quaternion _targetRotation;
    float _rotationSpeed = 5f;

    private StaminaManager _staminaManager;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _moveInput.variable.ValueChanged += MoveInput_ValueChanged;
        _targetRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (gameObject.CompareTag("Player")) 
            _rb.Move(new Vector3(transform.position.x, transform.position.y,transform.position.z) + (_movedirection * (_speed.value * _moveInput.variable.SpeedMultiplier)) * Time.deltaTime, transform.rotation);
        _rb.AddForce(Vector3.down * 9.81f * 300, ForceMode.Force);

        transform.rotation = Quaternion.Slerp(transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime);
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
            if (_sprintCost.value > _staminaManager.CurrentStamina)
            {
                _moveInput.variable.SpeedMultiplier = 1;
            }
            else
                _removeStamina.Raise(this, new StaminaEventArgs { StaminaCost = _sprintCost.value * Time.deltaTime });
        }

            _movedirection = new Vector3(input.x, 0f, input.y);


        _changeAnimation.Raise(this, new WalkingEventArgs 
        { WalkDirection = input, Speed = speedMultiplier, IsLockon = _stateManager.Target != null, Orientation = _stateManager.fOrientation });
        //if (input != Vector2.zero)
        //{
        //}
        //else
        //{
        //    _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Idle, AnimLayer = { 1 }, DoResetIdle = false });
        //    _changeAnimation.Raise(this, new AnimationEventArgs { AnimState = AnimationState.Empty, AnimLayer = { 2 }, DoResetIdle = false });
        //}

        if (_stateManager.Target == null)
            UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        if (_movedirection == Vector3.zero) return;
        if (_movedirection.magnitude <= 0.2f) return;
        float moveInputAngle = Mathf.Atan2(_movedirection.z, _movedirection.x);
        moveInputAngle = moveInputAngle * Mathf.Rad2Deg;

        foreach (Orientation direction in Enum.GetValues(typeof(Orientation)))
        {
            if (moveInputAngle > (float)direction - _angleInterval && moveInputAngle < (float)direction + _angleInterval)
            {
                if (_stateManager.Orientation == direction) return;
                _stateManager.Orientation = direction;
                _stateManager.fOrientation = (float)direction;
                float targetAngle =0f;
                switch (direction)
                {
                    case Orientation.North:
                        targetAngle = 0;
                        break;
                    case Orientation.NorthEast:
                        targetAngle = 45;
                        break;
                    case Orientation.East:
                        targetAngle = 90;
                        break;
                    case Orientation.SouthEast:
                        targetAngle = 135;
                        break;
                    case Orientation.South:
                        targetAngle = 180;
                        break;
                    case Orientation.SouthWest:
                        targetAngle = -135;
                        break;
                    case Orientation.West:
                        targetAngle =  -90;
                        break;
                    case Orientation.NorthWest:
                        targetAngle = -45;
                        break;
                }

                _targetRotation = Quaternion.Euler(new Vector3(0, targetAngle, 0));
            }
        }
    }

    private void OnDisable()    
    {
        if(_changeAnimation == null) return;
        _changeAnimation.Raise(this, new WalkingEventArgs
        { WalkDirection = Vector2.zero, Speed = 0f, IsLockon = _stateManager.Target != null, Orientation = _stateManager.fOrientation });
    }

    private void OnDestroy()
    {
        _moveInput.variable.ValueChanged -= MoveInput_ValueChanged;
    }
}
