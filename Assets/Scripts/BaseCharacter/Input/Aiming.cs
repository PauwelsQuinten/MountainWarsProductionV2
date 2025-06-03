using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Geometry;
using UnityEngine.InputSystem;

public class Aiming : MonoBehaviour
{
    private bool _isActive = true;

    [Header("InputVariable")]
    [SerializeField] private AimingInputReference _refAimingInput;

    [Header("Events")]
    [SerializeField] private GameEvent _AimOutputEvent;

    [Header("State")]
    [SerializeField, Tooltip("angle between your analog direction and the enemy direction max difference to accept as vallid stab input")] 
    private float _stabAcceptedRange = 45f;
    [SerializeField, Tooltip("analog input direction will be accepted as left,right until this angle is reached. higher will be set as wrong/invallid")] 
    private float _maxAllowedBlockAngle = 150f;
    [SerializeField, Tooltip("analog input direction will be accepted as center Block if between this angle and his negative")]
    private float _acceptedAngleForCenter = 35f;
    private float _minSwingSpeed = 1f;
    [SerializeField, Tooltip("This will determine the animation _movementSpeed of block and attack, the attack power will also be influenced by this")]
    [Range(1.1f, 2f)]
    private float _maxSwingSpeed = 2f;
    [SerializeField, Tooltip("the minimum angle your analog stick needs to travel to consider the attack not to be a feint")]
    [Range(F_ACCEPTED_MIN_ANGLE, 180f)]
    private float _minSwingAngle = 60f;
    [SerializeField] private bool _useDynamicBlock = true;

    private Vector2 _vec2previousDirection = Vector2.zero;
    private Vector2 _vec2Start = Vector2.zero;
    private AttackState _enmCurrentState = AttackState.Idle;
    private AimingInputState _enmAimingInput = AimingInputState.Idle;
    private AttackSignal _enmAttackSignal = AttackSignal.Swing;
    private const float F_MIN_DIFF_BETWEEN_INPUT = 0.03f;
    private const float F_MAX_TIME_NOT_MOVING = 0.1f;
    private const float F_MIN_ACCEPTED_VALUE = 0.40f;
    private const float F_TIME_BETWEEN_STAB = 0.10f;
    private const float F_MAX_ALLOWED_ANGLE_ON_ORIENTATION = 17f;
    private const float F_MIN_ACCEPTED_MOVEMENT_ANGLE = 30f;
    private const float F_ACCEPTED_MIN_ANGLE = 15f;

    private float _fNotMovingTime = 0f;
    private float _fMovingTime = 0f;
    private float _previousLength = 0f;
    private float _traversedAngle = 0f;
    private Direction _swingDirection = Direction.Idle;
    private bool _noFeintSignalSend = false;

    //private InputAction _moveAction;

    public void SetActive(bool active)
    {
        _isActive = active;
    }

    void Start()
    {
        _enmAimingInput = AimingInputState.Idle;
        _refAimingInput.variable.ValueChanged += Variable_ValueChanged;

    }

    void Update()
    {
        if (!_isActive) return; 

        CheckIfHoldingPosition();
        _fMovingTime += Time.deltaTime;

    }

    private void OnDestroy()
    {
        _refAimingInput.variable.ValueChanged -= Variable_ValueChanged;
    }

    private void Variable_ValueChanged(object sender, AimInputEventArgs e)
    {
        if (!_isActive) return;

        switch (e.ThisChanged)
        {
            case AimInputEventArgs.WhatChanged.State:
                OnStateChanged();
                break;
            case AimInputEventArgs.WhatChanged.Input:
                OnInputChanged();
                break;
            default:
                break;
        }
    }

    private void OnInputChanged()
    {
        float inputLength = _refAimingInput.variable.value.magnitude;

        //Do nothing while it is in cooldown
        if (_enmAimingInput == AimingInputState.Cooldown)
        {
            if (inputLength < F_MIN_ACCEPTED_VALUE)
                _vec2Start = Vector2.zero;

            _previousLength = 1.1f;
            return;
        }

        //Move your block smoothly when is holding and is set to dynamic
        if (_useDynamicBlock 
            && (_refAimingInput.variable.State == AttackState.ShieldDefence || _refAimingInput.variable.State == AttackState.SwordDefence) 
            && _enmAimingInput == AimingInputState.Hold
            && inputLength > 0.9f)
        {
            SendPackage();
        }

        //Reset values when putting analog stick to neutral
        else if (_refAimingInput.variable.value == Vector2.zero || inputLength < F_MIN_ACCEPTED_VALUE)
        {                                   
            //Reset signal e.g. when stop charging or stop Blocking
            if (_enmAimingInput != AimingInputState.Idle)
            {
                _enmAimingInput = AimingInputState.Idle;
                _enmAttackSignal = AttackSignal.Idle;
                SendPackage();
            }
            ResetValues();
        }

        //When pressing analog stick towards target perform a Stab
        else if (IsStabMovement(inputLength))
        {
            _enmAttackSignal = AttackSignal.Stab;
            SendPackage(true);

            ResetValues();
            _enmAimingInput = AimingInputState.Cooldown;
            StartCoroutine(ResetAttack(F_TIME_BETWEEN_STAB));
        }

        //Throw feint from direction you point to
        else if ( inputLength > 0.9f 
            && _swingDirection == Direction.Idle 
            && (_refAimingInput.variable.State == AttackState.Idle || _refAimingInput.variable.State == AttackState.Attack || _refAimingInput.variable.State == AttackState.BlockAttack)
            && _previousLength < inputLength)
        {
            var direction = Geometry.Geometry.CalculateFeintDirection(_refAimingInput.variable.StateManager.fOrientation, _refAimingInput.variable.value, _maxAllowedBlockAngle);
                  
            if (direction != Direction.Wrong)
            {
                _swingDirection = direction;
                _enmAttackSignal = AttackSignal.Swing;
                SendPackage(true);
            }
            
        }

        //When it is not a Stab and input is big enough, set to moving
        else if (_previousLength < 1.1f && (DetectAnalogMovement() || _traversedAngle > F_ACCEPTED_MIN_ANGLE))
        {
            _fNotMovingTime = 0f;
            switch (_enmAimingInput)
            {
                case AimingInputState.Idle:
                case AimingInputState.Hold:
                    _vec2Start = _refAimingInput.variable.value;
                    _enmAimingInput = AimingInputState.Moving;
                    _vec2previousDirection = Vector2.zero;
                    _fMovingTime = 0f;
                    //_traversedAngle = 0f;
                    _noFeintSignalSend = false;
                    break;
            }
            //To throw parry
            if (_traversedAngle >= F_ACCEPTED_MIN_ANGLE && _swingDirection == Direction.Idle)
            {
                _swingDirection = Geometry.Geometry.CalculateSwingDirection(_traversedAngle, _refAimingInput.variable.value, _vec2previousDirection, _vec2Start);
                _enmAttackSignal = AttackSignal.Swing;

                SendPackage(true);
            }
            //To finish the already executing attack
            else if (_traversedAngle > _minSwingAngle && !_noFeintSignalSend)
            {
                _noFeintSignalSend = true;
                SendPackage(false, false);
            }
        }
        //only whenn putting analog stick to neutral should this be able to be reseted
        //keeping it at 1.1 will make sure no extra input will be sent
        if (_previousLength < 1.1f)
            _previousLength = inputLength;
    }

    private void OnStateChanged()
    {
        if (_enmCurrentState == AttackState.ShieldDefence && _refAimingInput.variable.State == AttackState.BlockAttack)
        {
            //SendPackage();
            _previousLength = 1.1f;
        }
        //else if (_enmCurrentState ==  AttackState.BlockAttack && _refAimingInput.variable.State != AttackState.BlockAttack)
        //{
        //    SendPackage();
        //}

        if (_enmCurrentState != _refAimingInput.variable.State)
        {
            _enmCurrentState = _refAimingInput.variable.State;
            if (_enmCurrentState == AttackState.Stun)
            {
                ResetValues();
                SendPackage();
            }
            else if (_enmAttackSignal == AttackSignal.Idle)
                SendPackage();
        }
    }

    private void CheckIfHoldingPosition()
    {
        if ((_enmAimingInput == AimingInputState.Moving || _enmAimingInput == AimingInputState.Idle) && _fNotMovingTime >= F_MAX_TIME_NOT_MOVING)
        {            
            //HoldEvents are for Holding block or charging
            //Can also be used to throw an atack by stop moving instead of releasing the analog stick
            OnHoldevents();
            _fNotMovingTime = 0f;
            _traversedAngle = 0f;
            _fMovingTime = 0;
        }
            
        _fNotMovingTime += Time.deltaTime;    
    }

    private void OnHoldevents()
    {
        Vector2 orient = Geometry.Geometry.CalculateVectorFromOrientation(_refAimingInput.variable.StateManager.Orientation);

        switch (_refAimingInput.variable.State)
        {
            case AttackState.Idle:
            case AttackState.Attack:
            case AttackState.BlockAttack:
                //When stop moving with small angle its chargUp or nothing.
                if (_traversedAngle < F_MIN_ACCEPTED_MOVEMENT_ANGLE 
                    && Geometry.Geometry.AreVectorWithinAngle(-orient, _refAimingInput.variable.value, F_MIN_ACCEPTED_MOVEMENT_ANGLE)
                    && _enmAttackSignal != AttackSignal.Charge)
                {                                   
                    _enmAttackSignal = AttackSignal.Charge;
                    SendPackage(true);
                }
                break;

                //Moving the shield to hold block
            case AttackState.ShieldDefence:
            case AttackState.SwordDefence:
                if (_enmAimingInput == AimingInputState.Moving)
                {
                    _enmAimingInput = AimingInputState.Hold;
                    _swingDirection = Direction.Wrong;//Put to wrong so the inputChanged function wont call a parry movement when switching block sides
                    SendPackage();
                }             
                break;

            default:
                break;

        }       
    }
 
    private bool DetectAnalogMovement()
    {
        var diff = _vec2previousDirection - _refAimingInput.variable.value;
        bool value = diff.magnitude > F_MIN_DIFF_BETWEEN_INPUT;

        //_traversedAngle = Vector2.Angle(_vec2Start, _refAimingInput.variable.value);
        _traversedAngle += Vector2.Angle(_vec2previousDirection, _refAimingInput.variable.value);
        _vec2previousDirection = _refAimingInput.variable.value;
        
        return value;
    }

    private bool IsStabMovement(float inputLength)
    {
        return (inputLength >= 0.9f && inputLength > _previousLength + 0.01f
                    && Geometry.Geometry.IsInputInFrontOfOrientation(_refAimingInput.variable.value, _stabAcceptedRange, _refAimingInput.variable.StateManager.fOrientation)
                    && (_refAimingInput.variable.State != AttackState.SwordDefence && _refAimingInput.variable.State != AttackState.ShieldDefence)
                    && _enmAttackSignal == AttackSignal.Idle
                    && _traversedAngle < F_MAX_ALLOWED_ANGLE_ON_ORIENTATION) ;
        /*if (!value)
            Debug.Log($"inputlength = {inputLength}, previousLength = {_previousLength}, state = {_refAimingInput.variable.State}, signal = {_enmAttackSignal}, angle = {_traversedAngle}");
        return value;*/
    }

    private void SendPackage(bool isUninteruptedAnimation = false, bool isFeint = true )
    {
        var package = new AimingOutputArgs
        {
            AimingInputState = _enmAimingInput
                ,
            AngleTravelled = _traversedAngle
                ,
            AttackHeight = _refAimingInput.variable.StateManager.AttackHeight
                ,
            Direction = _swingDirection
                ,
            BlockDirection = Geometry.Geometry.CalculateBlockDirection
                (_refAimingInput.variable.StateManager.fOrientation, _refAimingInput.variable.value, _refAimingInput.variable.StateManager.IsHoldingShield, _acceptedAngleForCenter, _maxAllowedBlockAngle)
                ,
            Speed = Geometry.Geometry.CalculateSwingSpeed(_traversedAngle, _fMovingTime, _minSwingSpeed, _maxSwingSpeed)
                ,
            AttackSignal = _enmAttackSignal
                ,
            AttackState = _refAimingInput.variable.State
                ,
            EquipmentManager = _refAimingInput.variable.StateManager.EquipmentManager
                ,
            IsHoldingBlock = _refAimingInput.variable.StateManager.IsHoldingShield
                ,
            AnimationStart = isUninteruptedAnimation
                ,
            IsFeint = isFeint
            //IsFeint = IsFeintMovement(_swingDirection)

        };
        //Debug.Log($"Send package: {package.AttackState}, {package.AttackSignal}, {_enmAimingInput}, angle : {_traversedAngle}, swing direction: {package.Direction}, block direction: {package.BlockDirection} holding = {package.IsHoldingBlock}");
        _AimOutputEvent.Raise(this, package);
    }

    private void ResetValues()
    {
        _enmAimingInput = AimingInputState.Idle;
        _enmAttackSignal = AttackSignal.Idle;
        _swingDirection = Direction.Idle;
        _vec2Start = Vector2.zero;
        _vec2previousDirection = Vector2.zero;
        _traversedAngle = 0f;
        _previousLength = 0f;
        _fMovingTime = 0f;
    }

    private IEnumerator ResetAttack(float time)
    {
        yield return new WaitForSeconds(time);
        _enmAimingInput = AimingInputState.Idle;
    }
}