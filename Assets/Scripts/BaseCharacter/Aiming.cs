using TMPro;
using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using Unity.VisualScripting;

public class Aiming : MonoBehaviour
{
    [SerializeField] private AimingInputReference _refAimingInput;
    [SerializeField] private GameEvent _AimOutputEvent;
    [Header("Visual")]
    [SerializeField] private TextMeshProUGUI _textMeshPro;
    [SerializeField] private TextMeshProUGUI _textMeshPro2;
    [SerializeField] private TextMeshProUGUI _textMeshPro3;
    [SerializeField] private TextMeshProUGUI _textMeshPro4;
    private Vector2 _vec2previousDirection = Vector2.zero;
    private Vector2 _vec2Start = Vector2.zero;
    private AttackState _enmCurrentAttackState = AttackState.Idle;
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

    private float _fNotMovingTime = 0f;
    private float _fMovingTime = 0f;
    private float _previousLength = 0f;
    private float _traversedAngle = 0f;
    
    void Start()
    {
        _enmAimingInput = AimingInputState.Idle;
        _refAimingInput.variable.ValueChanged += Variable_ValueChanged;
    }

    void Update()
    {
        CheckIfHoldingPosition();
        UpdateMovingTime();

        if (_textMeshPro && _textMeshPro2 && _textMeshPro3 && _textMeshPro4)
        {
            _textMeshPro2.text = $"{_refAimingInput.variable.value}";
            _textMeshPro.text = $"{_enmAimingInput}";
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

        //Do nothing while it needs to reset himself
        if (_enmAimingInput == AimingInputState.Reset)
        {
            if (inputLength < F_MIN_ACCEPTED_VALUE)
                _vec2Start = Vector2.zero;

            _previousLength = 1.1f;
            return;
        }


        //Set to Idle when no input or very small
        if (_refAimingInput.variable.value == Vector2.zero || inputLength < F_MIN_ACCEPTED_VALUE)
        {
            if (_traversedAngle > F_MIN_ACCEPTED_MOVEMENT_ANGLE)
            {
                var dir = CalculateSwingDirection(_traversedAngle);
                _enmAttackSignal = IsFeintMovement(dir);
                _enmAimingInput = AimingInputState.Reset;
                _previousLength = 1.1f; //Set to higher then max magnitude, this is so that Stab can not be activated without any extra movement after swing
                SendPackage();

                //DebugLines(dir);
                _vec2Start = Vector2.zero;
                _fMovingTime = 0f;
                _traversedAngle = 0f;
            }
            
            if (_enmAimingInput != AimingInputState.Idle)
            {
                _enmAimingInput = AimingInputState.Idle;
                _enmAttackSignal = AttackSignal.Idle;
                _vec2Start = Vector2.zero;
                _traversedAngle = 0f;
                SendPackage();
            }
            else
            {
                _enmAttackSignal = AttackSignal.Idle;
                _vec2Start = Vector2.zero;
                _traversedAngle = 0f;

            }
            
        }


        //Check for the small specific input for Stab
        else if (IsStabMovement(inputLength))
        {
            _enmAttackSignal = AttackSignal.Stab;
            _enmAimingInput = AimingInputState.Reset;
            _vec2Start = Vector2.zero;
            _traversedAngle = 0f;
            StartCoroutine(ResetAttack(F_TIME_BETWEEN_STAB));
            //Debug.Log($"Stab owner: {gameObject}");
            SendPackage();
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
                    if (_refAimingInput.variable.State == AttackState.ShieldDefence)
                    {
                        SendPackage();
                        //Debug.Log("Shield Moving");
                    }

                    break;
            }
            _fNotMovingTime = 0f;
            
            //Debug.Log("Movement");
        }
            

        //Store measured length to use as comparision for the IsStabMovement
        _previousLength = inputLength;
    }

    private void OnStateChanged()
    {
        if (_enmCurrentAttackState == AttackState.ShieldDefence && _refAimingInput.variable.State == AttackState.BlockAttack)
        {
            SendPackage();
            _previousLength = 1.1f;
        }
        else if (_enmCurrentAttackState ==  AttackState.BlockAttack && _refAimingInput.variable.State != AttackState.BlockAttack)
        {
            SendPackage();
        }

        if (_enmCurrentAttackState != _refAimingInput.variable.State)
        {
            _enmCurrentAttackState = _refAimingInput.variable.State;
            if (_enmCurrentAttackState == AttackState.Stun)
            {
                ResetValues();
                SendPackage();
            }
            else if (_enmAttackSignal == AttackSignal.Idle)
                SendPackage();
        }
    }

    

    //--------------------------------------------------------------
    //Helper Functions
    private void CheckIfHoldingPosition()
    {
        switch( _enmAimingInput )
        {
            case AimingInputState.Reset:
                _fMovingTime = 0;
                return;
            case AimingInputState.Idle:
            case AimingInputState.Hold:
                 _fNotMovingTime = 0f;
                 _fMovingTime = 0;
            break;

            case AimingInputState.Moving:
                if (!DetectAnalogMovement(true))
                {
                    _fNotMovingTime += Time.deltaTime;
                    //Debug.Log($"{_fNotMovingTime}");
                }
                else
                    _fNotMovingTime = 0f;
            break;
        }

        if (_fNotMovingTime >= F_MAX_TIME_NOT_MOVING)
        {
            //Debug.Log("start hold");
            OnHoldevents();
            _fNotMovingTime = 0f;
        }
    }

    private void OnHoldevents()
    {
        //var angl = CalculateAngleLengthDegree();
        var dir = CalculateSwingDirection(_traversedAngle);

        switch (_refAimingInput.variable.State)
        {
            case AttackState.Idle:
            case AttackState.Attack:
            case AttackState.BlockAttack:
                //Check if you are stabing , return from function afterwards
                if (_traversedAngle < F_MIN_ACCEPTED_MOVEMENT_ANGLE)
                {
                    if (IsStabMovement(_refAimingInput.variable.value.magnitude))
                    {
                        _enmAttackSignal = AttackSignal.Stab;
                        _enmAimingInput = AimingInputState.Reset;
                        _previousLength = 1.1f;
                        StartCoroutine(ResetAttack(F_TIME_BETWEEN_STAB));
                        SendPackage();

                        _traversedAngle = 0f;
                        Debug.Log($"HStab");
                        return;
                    }
                    //Charging for next attack, reset _startVec so it wont interfere when going to stab
                    Vector2 orient = CalculateVectorFromOrientation(_refAimingInput.variable.StateManager.Orientation);
                    if (AreVectorWithinAngle(-orient, _refAimingInput.variable.value, 30))
                    {
                        _enmAttackSignal = AttackSignal.Charge;
                        _enmAimingInput = AimingInputState.Hold;
                        SendPackage();

                        _traversedAngle = 0f;
                        //Debug.Log($"Charge");
                        return;
                    }
                    //_enmAttackSignal = AttackSignal.Idle;
                    _traversedAngle = 0f;
                    //Debug.Log($"Nothing");
                    return;
                }
                else
                {
                    //feinting or throw your swing attack
                    Swing(dir);

                }

                //DebugLines(dir);
                break;

            case AttackState.ShieldDefence:
            case AttackState.SwordDefence:
                _enmAimingInput = AimingInputState.Hold;
                //DebugLines(dir);
                //Debug.Log("Block send");

                SendPackage();
                break;

            default:
                break;
        }

        

        //Reset values before next action
        _vec2Start = _refAimingInput.variable.value;
        _traversedAngle = 0f;
        _fMovingTime = 0f;

    }

    private void DebugLines(Direction dir)
    {
        Debug.Log($"{dir}");
        Debug.Log($"distance : {_traversedAngle}");
        var speed = CalculateSwingSpeed(_traversedAngle);
        Debug.Log($"speed : {speed}");
        Debug.Log($"signal : {_enmAttackSignal}");
        Debug.Log($"{CalculateBlockDirection(_refAimingInput.variable.StateManager.Orientation)}");

    }

    private void Swing(Direction dir)
    {
        _enmAttackSignal = IsFeintMovement(dir);
        _enmAimingInput = AimingInputState.Reset;
        _previousLength = 1.1f; //Set to higher then max magnitude, this is so that Stab can not be activated without any extra movement after swing
        if (_enmAttackSignal == AttackSignal.Swing)
            StartCoroutine(ResetAttack(F_TIME_BETWEEN_SWING));
        else
            StartCoroutine(ResetAttack(F_TIME_BETWEEN_FEINT));

        SendPackage();
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
                    && _refAimingInput.variable.StateManager.Orientation == CalculateOrientationOfInput(_refAimingInput.variable.value)
                    && (_refAimingInput.variable.State == AttackState.Attack || _refAimingInput.variable.State == AttackState.BlockAttack)
                    && _enmAttackSignal == AttackSignal.Idle
                    && _traversedAngle < F_MAX_ALLOWED_ANGLE_ON_ORIENTATION) ;
           /* return true;
        else
            Debug.Log($"{CalculateOrientationOfInput(_refAimingInput.variable.value)}, {_traversedAngle}");
        return false;*/
    }


    private void SendPackage()
    {
        float distance = _traversedAngle;
        Direction direction = CalculateSwingDirection(distance);
        var package = new AimingOutputArgs
        {
            AimingInputState = _enmAimingInput
                ,
            AngleTravelled = _traversedAngle
                ,
            AttackHeight = _refAimingInput.variable.StateManager.AttackHeight
                ,
            Direction = CalculateSwingDirection(distance)
                ,
            BlockDirection = CalculateBlockDirection(_refAimingInput.variable.StateManager.Orientation)
                ,
            Speed = CalculateSwingSpeed(distance)
                ,
            AttackSignal = _enmAttackSignal
                ,
            AttackState = _refAimingInput.variable.State
                ,
            EquipmentManager = _refAimingInput.variable.StateManager.EquipmentManager
        };
        _AimOutputEvent.Raise(this, package);
    }
    
    private Direction CalculateSwingDirection(float angleDegree)
    {
        Vector2 inputVec = Vector2.zero;
        if (_refAimingInput.variable.value == Vector2.zero)
            inputVec = _vec2previousDirection;
        else 
            inputVec = _refAimingInput.variable.value;

        float cross = _vec2Start.x * inputVec.y - _vec2Start.y * inputVec.x;
        if (cross == 0f)
            return Direction.ToCenter;
        
        if (angleDegree < 180)
            return cross > 0 ? Direction.ToLeft : Direction.ToRight;
        
        return cross < 0 ? Direction.ToLeft : Direction.ToRight;

        //return angleDegree > 0 ? Direction.ToLeft : Direction.ToRight;
    }

    private float CalculateAngleLengthDegree()
    {
        //float angleStart = -Mathf.Atan2(_vec2Start.y, _vec2Start.x) * Mathf.Rad2Deg;
        //Vector2 diff = Quaternion.AngleAxis(angleStart, Vector3.forward) * _refAimingInput.variable.value;
        //float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        bool wasNeg = false;
        float angle1 = Mathf.Atan2(_vec2Start.y, _vec2Start.x) * Mathf.Rad2Deg;
        if (angle1 < 0)
        {
            angle1 += 360;
            wasNeg = !wasNeg;
        }

        float angle2 = Mathf.Atan2(_refAimingInput.variable.value.y, _refAimingInput.variable.value.x) * Mathf.Rad2Deg;
        if (angle2 < 0)
        {
            angle2 += 360;
            wasNeg = !wasNeg;
        }
        float totAngle = Mathf.Abs(angle2 - angle1);
        if (wasNeg)
            totAngle += 180;
        return totAngle;
    }

    private void UpdateMovingTime()
    {
        _fMovingTime += Time.deltaTime;
    }

    private float CalculateSwingSpeed(float length)
    {
        return (length *1/ _fMovingTime) * 0.01f;
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

    private Direction CalculateBlockDirection(Orientation orientation)
    {
        
        int orient = (int)orientation;
        float input = CalculateAngleRadOfInput(_refAimingInput.variable.value) * Mathf.Rad2Deg;
        int diff = (int)input - orient;
        diff = diff < -180? 360 + diff  : diff;

        /*if (_enmAimingInput == AimingInputState.Hold)
            Debug.Log($"{diff}");*/

        if (diff > -30 && diff < 30)
            return Direction.ToCenter;
        else if (diff > 30 && diff < 100)
            return Direction.ToLeft;
        else if (diff < -30 && diff > -100)
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



        if (direction == Direction.ToLeft && startAngleRad < 0 && endAngleRad > 0)
            return AttackSignal.Swing;
        else if (direction == Direction.ToRight && startAngleRad > 0 && endAngleRad < 0)
            return AttackSignal.Swing;
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

    private void ResetValues()
    {
        _enmAimingInput = AimingInputState.Idle;
        _enmAttackSignal = AttackSignal.Idle;
        _vec2Start = Vector2.zero;
        _vec2previousDirection = Vector2.zero;
        _traversedAngle = 0f;
        _previousLength = 0f;
        _fNotMovingTime = 0f;
        _fMovingTime = 0f;
    }
}