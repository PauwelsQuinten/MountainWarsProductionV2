using System;
using System.Collections;
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
    [SerializeField]
    private bool _stopMovingDuringAttack = false;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _sprintCost;
    [SerializeField]
    private GameEvent _removeStamina;

    [Header("Animation")]
    [SerializeField]
    private GameEvent _changeAnimation;
    [SerializeField]
    private GameEvent _stopFullBodyMovement;

    private StateManager _stateManager;
    private StaminaManager _staminaManager;
    private Rigidbody _rb;

    private Vector3 _movedirection;
    private float _angleInterval = 22.5f;
    private Quaternion _targetRotation;
    private float _rotationSpeed = 5f;
    private bool _inAttackMotion = false;
    private Coroutine _inAttckMotionTimer ;

    private void Start()
    {
        _stateManager = GetComponent<StateManager>();
        _rb = GetComponent<Rigidbody>();
        _moveInput.variable.ValueChanged += MoveInput_ValueChanged;
        _targetRotation = transform.rotation;
    }

    private void FixedUpdate()
    {
        if (_stateManager.IsInStaticDialogue.value)
        {
            _movedirection = Vector3.zero;
            _changeAnimation.Raise(this, new WalkingEventArgs
            { WalkDirection = _movedirection, Speed = 1, IsLockon = _stateManager.Target != null, Orientation = _stateManager.fOrientation });
            return;
        }
        if (_stopMovingDuringAttack && _inAttackMotion)
            return;

        //npc get moved by navmesh
        if (gameObject.CompareTag("Player")) 
            _rb.Move(new Vector3(transform.position.x, transform.position.y,transform.position.z) + (_movedirection * (_speed.value * _moveInput.variable.SpeedMultiplier)) * Time.deltaTime, transform.rotation);
        _rb.AddForce(Vector3.down * 9.81f * 300, ForceMode.Force);

        if (_stateManager.Target == null)
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
        
        if (_stateManager.Target == null)
            UpdateOrientation();
    }

    public void SetInAttackMovement(Component sender, object obj)
    {
        StunEventArgs stunEventArgs = obj as StunEventArgs;
        if (stunEventArgs != null && stunEventArgs.StunTarget == gameObject)
        {
            _inAttackMotion = false;
            if (_inAttckMotionTimer != null)
            {
                StopCoroutine( _inAttckMotionTimer );
                _inAttckMotionTimer = null; 
            }
            return;
        }

        AttackEventArgs attackEventArgs = obj as AttackEventArgs;
        if (attackEventArgs != null && attackEventArgs.Attacker == gameObject)
        {
            _inAttackMotion = false;
            if (_inAttckMotionTimer != null)
            {
                StopCoroutine(_inAttckMotionTimer);
                _inAttckMotionTimer = null;
            }
            return;
        }

        AttackMoveEventArgs args = obj as AttackMoveEventArgs;
        if (args == null || args.Attacker != gameObject) return;

        _inAttackMotion = args.AttackType == AttackType.None? false : true;
        if (_inAttackMotion)
            _inAttckMotionTimer = StartCoroutine(StartTimer(1f));
        else
        {
            if (_inAttckMotionTimer != null)
            {
                StopCoroutine(_inAttckMotionTimer);
                _inAttckMotionTimer = null;
            }
        }
    }

    private void UpdateOrientation()
    {
        if (_movedirection == Vector3.zero) return;
        if (_movedirection.magnitude <= 0.2f) return;
        float moveInputAngle = Mathf.Atan2(_movedirection.z, _movedirection.x);
        moveInputAngle = moveInputAngle * Mathf.Rad2Deg;
        if (moveInputAngle - _angleInterval < -180)
            moveInputAngle *= -1;

        foreach (Orientation direction in Enum.GetValues(typeof(Orientation)))
        {
            if (moveInputAngle > (float)direction - _angleInterval && moveInputAngle < (float)direction + _angleInterval)
            {
                if (_stateManager.Orientation == direction) return;
                _stateManager.Orientation = direction;
                _stateManager.fOrientation = (float)direction;
                float targetAngle = Geometry.Geometry.BodyRotationAngleFromOrientation(direction);
                
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

    private IEnumerator StartTimer(float time)
    {
        yield return new WaitForSeconds(time);
        if (_inAttckMotionTimer != null)
        {
            _inAttackMotion = false;
            if (_stopFullBodyMovement)
                _stopFullBodyMovement.Raise(this, null);
            Debug.Log("attackmotion reset by timer");
        }

    }

}
