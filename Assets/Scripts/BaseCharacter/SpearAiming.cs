using NUnit.Framework.Constraints;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SpearAiming : MonoBehaviour
{
    private bool _isActive = true;

    [Header("InputVariable")]
    [SerializeField] private AimingInputReference _refAimingInput;

    [Header("Events")]
    [SerializeField] private GameEvent _AimOutputEvent;

    [Header("Movement")]
    [SerializeField, Tooltip("Max angle in degree derived from his center to left or right")]
    private float _maxAngle = 60f;
    [SerializeField, Tooltip("Max length of movement from spear to its idle position")]
    [Range(0f, 2f)]
    private float _moveDistance = 1f;
    [SerializeField, Tooltip("the ratio of how much the shoulder target moves along with the hand RH target")]
    [Range(0f, 1f)]
    private float _ratioShoulderHand = 0.6f;
    [SerializeField, Tooltip("Speed of rotating the spear in your hand")]
    private float _rotationSpeed = 5f;
    private Vector3 _spearForwardVector = Vector3.zero;
    private Quaternion _spearStartOrientation = Quaternion.identity;
    private Quaternion _newRotation = Quaternion.identity;
    private Equipment _spear;


    [Header("IK")]
    [SerializeField, Tooltip("IK aim target")]
    private GameObject _aimTarget;
    [SerializeField, Tooltip("IK Right shoulder target")]
    private GameObject _rShoulderTarget;
    [SerializeField, Tooltip("IK spine rotation target")]
    private GameObject _spineRotationTarget;
    
    private float _outputLength = 0f;
    private Vector3 _spearIdlePosition = Vector3.zero;
    private Vector3 _shoulderIdlePosition = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _refAimingInput.variable.ValueChanged += OnSpearMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isActive) return;

        if (_newRotation != _aimTarget.transform.localRotation)
            RotateSpear();
    }

    public void SetActive(bool active, Equipment forward )
    {
        _isActive = active;
        _spear = forward;
        if (_aimTarget && active)
        {
            _spearStartOrientation = _spineRotationTarget.transform.localRotation;
            _newRotation = _spearStartOrientation;
        }
    }

    private void OnSpearMovement(object sender, AimInputEventArgs e)
    {
        if (!_isActive) return;

        _outputLength = _refAimingInput.variable.value.magnitude;
        MoveSpearAimingPoint();
    }

    private void MoveSpearAimingPoint()
    {
       //Back or forwards movement
        float angle = Vector3.Angle(new Vector3(_refAimingInput.variable.value.x, 0f, _refAimingInput.variable.value.y), transform.forward);
        if (angle > 90f)
            _outputLength = 0;
            //_outputLength *= -0.5f;
        else
            _outputLength = _refAimingInput.variable.value.x * transform.forward.x + _refAimingInput.variable.value.y * transform.forward.z;

        //Slashing angle
        float clampedAngle = CalculateSpearAngle(angle);
        if (IsNegativeAngle(clampedAngle))
            clampedAngle *= -1;

        //Forward stab motion, find the output length and rotate it towards your orientation
        Quaternion rotation = Quaternion.Euler(0, _refAimingInput.variable.StateManager.fOrientation, 0);
        var offset = -transform.right * _outputLength * _moveDistance;
        if (_outputLength > 0f)
            _aimTarget.transform.localPosition = _spearIdlePosition + rotation * offset;
        _rShoulderTarget.transform.localPosition = _shoulderIdlePosition + rotation * offset * _ratioShoulderHand;

        //Set new rotation for swing
        _newRotation = _spearStartOrientation * Quaternion.Euler(0f, -clampedAngle, 0f);

    }

    private bool IsNegativeAngle( float angle )
    {
        float crossProduct = _refAimingInput.variable.value.x * transform.forward.z - _refAimingInput.variable.value.y * transform.forward.x;
        return crossProduct > 0;
    }

    private void RotateSpear()
    {
        float t = Time.deltaTime * _rotationSpeed;
        _spineRotationTarget.transform.localRotation = Quaternion.Slerp(_spineRotationTarget.transform.localRotation, _newRotation, t);
        //if (Quaternion.Dot(a, b) > 1.0f - tolerance)
    }

    public void SetIdlePosition()
    {
        _spearIdlePosition = _aimTarget.transform.localPosition;
        _shoulderIdlePosition = _rShoulderTarget.transform.localPosition ;
    }

    private float CalculateSpearAngle(float inputAngle)
    {
        float angle = inputAngle;
        float sign = Mathf.Sign(inputAngle);
        float absAngle = Mathf.Abs(angle);
        float deadAngle = 90f - _maxAngle;

        if (absAngle > 90 + deadAngle)
        {
            //float newAngle = _maxAngle - (absAngle - _maxAngle);
            //angle = (newAngle >= 0f)? sign * newAngle : 0f;
            angle = sign * (180 - absAngle);
        }
        else if (absAngle > _maxAngle)
        {
            angle = sign * _maxAngle;
        }
        return angle;
    }

}
