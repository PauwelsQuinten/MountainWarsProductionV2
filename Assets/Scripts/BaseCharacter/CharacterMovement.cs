using System;
using UnityEngine;

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

    private Rigidbody _rb;
    private StateManager _stateManager;
    private Vector3 _movedirection;
    private float _angleInterval = 22.5f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _stateManager = _moveInput.variable.StateManager;
        _moveInput.variable.ValueChanged += MoveInput_ValueChanged;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(new Vector3(transform.position.x, transform.position.y,transform.position.z) + (_movedirection * (_speed * _moveInput.variable.SpeedMultiplier)) * Time.deltaTime);
    }

    private void MoveInput_ValueChanged(object sender, EventArgs e)
    {
        if (_moveInput.variable.SpeedMultiplier > 1)
            _removeStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value * Time.deltaTime });
        _movedirection = new Vector3(_moveInput.Value.x, 0, _moveInput.Value.y);

        if (_stateManager.Target == null)
            UpdateOrientation();
    }

    private void UpdateOrientation()
    {
        if (_movedirection == Vector3.zero) return;
        float moveInputAngle = Mathf.Atan2(_movedirection.z, _movedirection.x);
        moveInputAngle = moveInputAngle * Mathf.Rad2Deg;

        if (moveInputAngle > 0 - _angleInterval && moveInputAngle < 0 + _angleInterval)
            _moveInput.variable.StateManager.Orientation = Orientation.East;
        else if (moveInputAngle > 45 - _angleInterval && moveInputAngle < 45 + _angleInterval)
        _moveInput.variable.StateManager.Orientation = Orientation.NorthEast;
        else if (moveInputAngle > 90 - _angleInterval && moveInputAngle < 90 + _angleInterval)
        _moveInput.variable.StateManager.Orientation = Orientation.North;
        else if (moveInputAngle > 135 - _angleInterval && moveInputAngle < 135 + _angleInterval)
            _moveInput.variable.StateManager.Orientation = Orientation.NorthWest;
        else if (moveInputAngle > 180 - _angleInterval && moveInputAngle < 180 || moveInputAngle < -180 + _angleInterval && moveInputAngle > -180)
            _moveInput.variable.StateManager.Orientation = Orientation.West;
        else if (moveInputAngle > -135 - _angleInterval && moveInputAngle < -135 + _angleInterval)
            _moveInput.variable.StateManager.Orientation = Orientation.SouthWest;
        else if (moveInputAngle > -90 - _angleInterval && moveInputAngle < -90 + _angleInterval)
            _moveInput.variable.StateManager.Orientation = Orientation.South;
        else if (moveInputAngle > -45 - _angleInterval && moveInputAngle < -45 + _angleInterval)
            _moveInput.variable.StateManager.Orientation = Orientation.SouthEast;
    }
}
