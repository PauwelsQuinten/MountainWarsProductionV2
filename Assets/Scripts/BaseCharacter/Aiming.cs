using TMPro;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Aiming : MonoBehaviour
{
    [Header("InputVariable")]
    [SerializeField] private AimingInputReference _refAimingInput;

    [Header("Event")]
    [SerializeField] private GameEvent _AimOutputEvent;
    [SerializeField] private GameEvent _sheathWeapon;
    [Header("State")]
    [SerializeField] private float _stabAcceptedRange = 60f;
    [SerializeField] private float _maxAllowedBlockAngle = 150f;

    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private TextMeshProUGUI _textMeshPro2;
    [SerializeField] private TextMeshProUGUI _textMeshPro3;
    [SerializeField] private TextMeshProUGUI _textMeshPro4;

    private Queue<AimingOutputArgs> _inputQueue = new Queue<AimingOutputArgs>();
    private Vector2 _vec2previousDirection = Vector2.zero;
    private Vector2 _vec2Start = Vector2.zero;
    private AttackState _enmCurrentState = AttackState.Idle;
    private AimingInputState _enmAimingInput = AimingInputState.Idle;
    private AttackSignal _enmAttackSignal = AttackSignal.Swing;
    private const float F_MIN_DIFF_BETWEEN_INPUT = 0.04f;
    private const float F_MAX_TIME_NOT_MOVING = 0.1f;
    private const float F_MIN_ACCEPTED_VALUE = 0.40f;
    private const float F_TIME_BETWEEN_SWING = 0.40f;
    private const float F_TIME_BETWEEN_FEINT = 0.10f;
    private const float F_TIME_BETWEEN_STAB = 0.20f;
    private const float F_MAX_ALLOWED_ANGLE_ON_ORIENTATION = 17f;
    private const float F_MIN_ACCEPTED_MOVEMENT_ANGLE = 30f;
    private const float F_ACCEPTED_MIN_ANGLE = 10f;

    private float _fNotMovingTime = 0f;
    private float _fMovingTime = 0f;
    private float _previousLength = 0f;
    private float _traversedAngle = 0f;
    private Direction _swingDirection = Direction.Idle;
    private Direction _storredBlockDirection = Direction.Idle;
    
    void Start()
    {
        _enmAimingInput = AimingInputState.Idle;
        _refAimingInput.variable.ValueChanged += Variable_ValueChanged;
    }

    void Update()
    {
        CheckIfHoldingPosition();
        UpdateMovingTime();
        UpdateActionQueue();

        if (_textMeshPro && _textMeshPro2 && _textMeshPro3 && _textMeshPro4)
        {
            _textMeshPro2.text = $"{_enmAttackSignal}";
            _textMeshPro.text = $"{_enmCurrentState}";
            _textMeshPro3.text = $"storedVec : {_vec2Start}";
            _textMeshPro4.text = $"traversed angle : {_traversedAngle}";
        }

    }

    private void Variable_ValueChanged(object sender, AimInputEventArgs e)
    {
        switch(e.ThisChanged)
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


        //Throw attack/feint or return to idle when releasing the analogstick
        if (_refAimingInput.variable.value == Vector2.zero || inputLength < F_MIN_ACCEPTED_VALUE)
        {                       
            //Attack/feint
            if (_traversedAngle > F_MIN_ACCEPTED_MOVEMENT_ANGLE)
            {
                _enmAttackSignal = IsFeintMovement(_swingDirection);
                if (_enmAttackSignal == AttackSignal.Feint)
                    SendPackage();
                _enmAimingInput = AimingInputState.Idle;

            }

            //Reset signal e.g. when stop charging or stop Blocking
            else if (_enmAimingInput != AimingInputState.Idle)
            {
                _enmAimingInput = AimingInputState.Idle;
                _enmAttackSignal = AttackSignal.Idle;
                SendPackage();
            }

            _enmAttackSignal = AttackSignal.Idle;
            _vec2Start = Vector2.zero;
            _traversedAngle = 0f;
            _swingDirection = Direction.Idle;
        }


        //When pressing analog stick towards target perform a Stab
        else if (IsStabMovement(inputLength))
        {
            _enmAttackSignal = AttackSignal.Stab;
            SendPackage(true);

            _vec2Start = Vector2.zero;
            _traversedAngle = 0f;
            _enmAimingInput = AimingInputState.Cooldown;
            StartCoroutine(ResetAttack(F_TIME_BETWEEN_STAB));
            //Debug.Log($"Stab owner: {gameObject}");
        }


        //When it is not a Stab and input is big enough, set to moving
        else if (DetectAnalogMovement(false)/* && _enmAttackSignal == AttackSignal.Idle*/)
        {
            switch (_enmAimingInput)
            {
                case AimingInputState.Idle:
                case AimingInputState.Hold:
                    _vec2Start = _refAimingInput.variable.value;
                    _enmAimingInput = AimingInputState.Moving;
                    _vec2previousDirection = Vector2.zero;
                    _fMovingTime = 0f;
                    
                    break;
            }

            if (_traversedAngle >= F_ACCEPTED_MIN_ANGLE && _swingDirection == Direction.Idle)
            {
                _swingDirection = CalculateSwingDirection(_traversedAngle);
                _enmAttackSignal = AttackSignal.Swing;

                SendPackage(true);
            }
        }
        //Store measured length to use as comparision for the IsStabMovement
        _previousLength = inputLength;
    }

    private void OnStateChanged()
    {
        if (_enmCurrentState == AttackState.ShieldDefence && _refAimingInput.variable.State == AttackState.BlockAttack)
        {
            SendPackage();
            _previousLength = 1.1f;
        }
        else if (_enmCurrentState ==  AttackState.BlockAttack && _refAimingInput.variable.State != AttackState.BlockAttack)
        {
            SendPackage();
        }

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
        if (_enmAimingInput == AimingInputState.Moving && !DetectAnalogMovement(true))
        {
            _fNotMovingTime += Time.deltaTime;
            if (_fNotMovingTime >= F_MAX_TIME_NOT_MOVING)
            {
                //HoldEvents are for Holding block or charging
                //Can also be used to throw an aatck by stop moving instead of releasing the analog stick
                OnHoldevents();
                _fNotMovingTime = 0f;
                _traversedAngle = 0f;
                _fMovingTime = 0;
            }
        }
        else if (_enmAimingInput == AimingInputState.Moving)
        {
            _fNotMovingTime = 0f;
        }
    }

    private void OnHoldevents()
    {
        Vector2 orient = CalculateVectorFromOrientation(_refAimingInput.variable.StateManager.Orientation);

        switch (_refAimingInput.variable.State)
        {
            case AttackState.Idle:
            case AttackState.Attack:
            case AttackState.BlockAttack:
                //When stop moving with small agle, it can only be stab, chargUp or nothing.
                if (_traversedAngle < F_MIN_ACCEPTED_MOVEMENT_ANGLE)
                {
                    if (IsStabMovement(_refAimingInput.variable.value.magnitude))
                    {
                        _enmAttackSignal = AttackSignal.Stab;
                        SendPackage();
                    }

                    //Charging for next attack, reset _startVec so it wont interfere when going to stab
                    else if (AreVectorWithinAngle(-orient, _refAimingInput.variable.value, F_MIN_ACCEPTED_MOVEMENT_ANGLE))
                    {
                        _enmAttackSignal = AttackSignal.Charge;
                        SendPackage();
                    }
                }

                //When the angle is big enough, you are feinting or throw your swing attack
                else
                {
                    _enmAttackSignal = IsFeintMovement(_swingDirection);
                    if (_enmAttackSignal == AttackSignal.Feint)
                        SendPackage();
                }

                _previousLength = 1.1f;
                SetCooldown();
                break;


            case AttackState.ShieldDefence:
            case AttackState.SwordDefence:
                _enmAimingInput = AimingInputState.Hold;
                _swingDirection = Direction.Wrong;//Put to wrong so the inputChanged function wont call a parry movement when switching block sides
                SendPackage();
                break;

            default:
                break;

        }       

    }

    private void SetCooldown()
    {
        _enmAimingInput = AimingInputState.Cooldown;
        switch (_enmAttackSignal)
        {
            case AttackSignal.Stab:
                StartCoroutine(ResetAttack(F_TIME_BETWEEN_STAB));
                break;
            case AttackSignal.Feint:
                StartCoroutine(ResetAttack(F_TIME_BETWEEN_FEINT));
                break;
            case AttackSignal.Swing:
                StartCoroutine(ResetAttack(F_TIME_BETWEEN_SWING));
                break;
            case AttackSignal.Idle:
            case AttackSignal.Charge:
                _enmAimingInput = AimingInputState.Idle;
                break;
        } 
    }

    private void DebugLines(Direction dir)
    {
        Debug.Log($"{dir}");
        Debug.Log($"distance : {_traversedAngle}");
        var speed = CalculateSwingSpeed(_traversedAngle, 1.5f, 2.5f);
        Debug.Log($"speed : {speed}");
        Debug.Log($"signal : {_enmAttackSignal}");
        Debug.Log($"{CalculateBlockDirection(_refAimingInput.variable.StateManager.Orientation)}");

    }

    
    private bool DetectAnalogMovement(bool setNewValue)
    {
        var diff = _vec2previousDirection - _refAimingInput.variable.value;
        bool value = diff.magnitude > F_MIN_DIFF_BETWEEN_INPUT;

        if (setNewValue)
        {
            _traversedAngle += Vector2.Angle(_vec2previousDirection, _refAimingInput.variable.value);
            _vec2previousDirection = _refAimingInput.variable.value;
        }

        return value;
    }


    private bool IsStabMovement(float inputLength)
    {
        return (inputLength >= 0.9f && inputLength > _previousLength + 0.01f
                    && IsInputInFrontOfOrientation(_refAimingInput.variable.value, _stabAcceptedRange)
                    //&& _refAimingInput.variable.StateManager.Orientation == CalculateOrientationOfInput(_refAimingInput.variable.value)
                    && (_refAimingInput.variable.State == AttackState.Attack || _refAimingInput.variable.State == AttackState.BlockAttack)
                    && _enmAttackSignal == AttackSignal.Idle
                    && _traversedAngle < F_MAX_ALLOWED_ANGLE_ON_ORIENTATION) ;
           /* return true;
        else
            Debug.Log($"{CalculateOrientationOfInput(_refAimingInput.variable.value)}, {_traversedAngle}");
        return false;*/
    }


    private void SendPackage(bool earlyMessage = false)
    {

        var package = new AimingOutputArgs
        {
            AimingInputState = _enmAimingInput
                ,
            AngleTravelled = _traversedAngle
                ,
            AttackHeight = _refAimingInput.variable.StateManager.AttackHeight
                ,
            //Direction = CalculateSwingDirection(distance)
            Direction = _swingDirection
                ,
            BlockDirection = CalculateBlockDirection(_refAimingInput.variable.StateManager.Orientation)
                ,
            //Speed = 2f
            Speed = CalculateSwingSpeed(_traversedAngle, 1.5f, 2.5f)
                ,
            AttackSignal = _enmAttackSignal
                ,
            AttackState = _refAimingInput.variable.State
                ,
            EquipmentManager = _refAimingInput.variable.StateManager.EquipmentManager
                ,
            IsHoldingBlock = _refAimingInput.variable.StateManager.IsHoldingShield
                ,
            AnimationStart = earlyMessage

        };
        //Debug.Log($"Send package: {package.AttackState}, {package.AttackSignal}, {_enmAimingInput}, angle : {_traversedAngle}, block direction: {package.BlockDirection} holding = {package.IsHoldingBlock}");

        if (_enmAttackSignal == AttackSignal.Feint)
            _AimOutputEvent.Raise(this, package);
            //Debug.Log("feint input");
        else if (_refAimingInput.variable.StateManager.InAnimiation)
        {
            if (package.AttackState != AttackState.Idle)
            {
                _inputQueue.Enqueue(package);
                //Debug.Log($"Enqueue: {package.AttackState}, {package.AttackSignal}, angle : {_traversedAngle}, early start: {package.AnimationStart}");
                Debug.Log($"{gameObject}, {package.AttackState}, {package.AttackSignal}, angle : {_traversedAngle}, early start: {package.AnimationStart}");
            }

        }
        else
            _AimOutputEvent.Raise(this, package);
    }

    private Direction CalculateSwingDirection(float angleDegree)
    {
        Vector2 inputVec = Vector2.zero;
        if (_refAimingInput.variable.value == Vector2.zero)
            inputVec = _refAimingInput.variable.value;
        else 
            inputVec = _vec2previousDirection;//Debug switching--------------------------------------------------

        float cross = _vec2Start.x * inputVec.y - _vec2Start.y * inputVec.x;
        if (cross == 0f)
            return Direction.ToCenter;

        var startAngle = CalculateAngleRadOfInput(_vec2Start);
        var endAngle = CalculateAngleRadOfInput(inputVec);

        if (angleDegree < 180)
        //if (Mathf.Abs(startAngle) + Mathf.Abs(endAngle) < Mathf.PI)
            return cross > 0 ? Direction.ToLeft : Direction.ToRight;
        
        return cross < 0 ? Direction.ToLeft : Direction.ToRight;

        //return angleDegree > 0 ? Direction.ToLeft : Direction.ToRight;
    }

    private void UpdateMovingTime()
    {
        _fMovingTime += Time.deltaTime;
    }

    private float CalculateSwingSpeed(float length, float minResult, float maxResult)
    {
        if (_fMovingTime == 0 )
            return minResult;
        float speed = (length * 1 / _fMovingTime) * 0.01f;
        speed = speed < minResult ? minResult:  speed;
        return speed > maxResult ? maxResult : speed;
    }
    
     private float CalculateAngleRadOfInput(Vector2 direction)
    {
        return Mathf.Atan2(direction.y, direction.x);
    }
    private Orientation CalculateOrientationOfInput(Vector2 direction)
    {
        //Debug.Log($"{Mathf.Atan2(_refAimingInput.variable.value.y, _refAimingInput.variable.value.x)}");
        float angle = CalculateAngleRadOfInput(direction) * Mathf.Rad2Deg;
        if (angle == 0)
            return Orientation.East;

        int newAngle = (int) Mathf.Round(angle / 45);
        newAngle = (newAngle == -4) ? 4 : newAngle;       

        return (Orientation)(newAngle * 45) ;
    }

    private bool IsInputInFrontOfOrientation(Vector2 direction , float acceptedRange)
    {
        float angle = CalculateAngleRadOfInput(direction) * Mathf.Rad2Deg;
        float angleDiff = angle - (float)_refAimingInput.variable.StateManager.Orientation;
        return Mathf.Abs(angleDiff) < acceptedRange;
    }

    private Direction CalculateBlockDirection(Orientation orientation)
    {
        float length = _refAimingInput.variable.value.magnitude;
        if (length < 0.5f && !_refAimingInput.variable.StateManager.IsHoldingShield)
            return Direction.Idle;

        int orient = (int)orientation;
        float input = CalculateAngleRadOfInput(_refAimingInput.variable.value) * Mathf.Rad2Deg;
        int diff = (int)input - orient;
        diff = diff < -180? 360 + diff  : diff;

        /*if (_enmAimingInput == AimingInputState.Hold)
            Debug.Log($"{diff}");*/

        if (diff > -30 && diff < 30)
            return Direction.ToCenter;
        else if (diff > 30 && diff < _maxAllowedBlockAngle || (_refAimingInput.variable.StateManager.IsHoldingShield && diff > 30))
            return Direction.ToLeft;
        else if (diff < -30 && diff > -100 || (_refAimingInput.variable.StateManager.IsHoldingShield && diff < -30))
            return Direction.ToRight;
        return Direction.Wrong;
    }

    private Vector2 CalculateVectorFromOrientation(Orientation orientation)
    {
        float angle = (int)orientation * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    private AttackSignal IsFeintMovement(Direction direction)
    {
        Vector2 inputVec = Vector2.zero;
        if (_refAimingInput.variable.value == Vector2.zero)
            inputVec = _vec2previousDirection;
        else
            inputVec = _refAimingInput.variable.value;

        var orientAngleRad = (int)_refAimingInput.variable.StateManager.Orientation * Mathf.Deg2Rad;

        var startAngleRad = CalculateAngleRadOfInput(_vec2Start) - orientAngleRad;
        startAngleRad = ClampAngle(startAngleRad);

        var endAngleRad = CalculateAngleRadOfInput(inputVec) - orientAngleRad;
        endAngleRad = ClampAngle(endAngleRad);


        //Debug.Log($"storredDirection= {_swingDirection}");
        //Debug.Log($"end: {_refAimingInput.variable.value} angle= {endAngleRad * Mathf.Rad2Deg}, start: {_vec2Start} angle= {startAngleRad * Mathf.Rad2Deg}");

        if (direction == Direction.ToLeft && startAngleRad < 0 && endAngleRad > 0)
            return AttackSignal.Swing;
        else if (direction == Direction.ToRight && startAngleRad > 0 && endAngleRad < 0)
            return AttackSignal.Swing;
        //else if (Mathf.Abs(startAngleRad) + Mathf.Abs(endAngleRad) >= Mathf.PI || _traversedAngle >= 180)
        else if (_traversedAngle >= 180)
            return AttackSignal.Swing;

        return AttackSignal.Feint;
    }

    private static float ClampAngle(float startAngleRad)
    {
        if (startAngleRad < -Mathf.PI)
            startAngleRad += Mathf.PI * 2f;
        else if (startAngleRad > Mathf.PI)
            startAngleRad -= Mathf.PI * 2f;
        return startAngleRad;
    }

    private IEnumerator ResetAttack(float time)
    {
        yield return new WaitForSeconds(time);
        _enmAimingInput = AimingInputState.Idle;
    }

    private bool AreVectorWithinAngle(Vector2 one, Vector2 two, float angleDegree)
    {
        Vector2 nOne = one.normalized;
        Vector2 nTwo = two.normalized;
        float dot = Vector2.Dot(nOne, nTwo);


        return Mathf.Acos(dot) < angleDegree * Mathf.Deg2Rad;
    }

    private void UpdateActionQueue()
    {
        if (!_refAimingInput.variable.StateManager.InAnimiation && _inputQueue.Count > 0)
        {
            var package = _inputQueue.Peek();
            _AimOutputEvent.Raise(this, _inputQueue.Dequeue());
            //Debug.Log($"Enqueue: {package.AttackState}, {package.AttackSignal}, early start: {package.AnimationStart}");
        }
    }

    private void ResetValues()
    {
        _enmAimingInput = AimingInputState.Idle;
        _enmAttackSignal = AttackSignal.Idle;
        _vec2Start = Vector2.zero;
        _vec2previousDirection = Vector2.zero;
        _traversedAngle = 0f;
        _previousLength = 0f;
        _fMovingTime = 0f;
    }
}