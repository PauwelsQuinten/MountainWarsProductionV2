using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

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
    private float _moveDistance = 5f;
    [SerializeField, Tooltip("Speed of rotating the spear in your hand")]
    private float _rotationSpeed = 5f;
    private Vector3 _spearForwardVector = Vector3.zero;
    private Quaternion _spearStartOrientation = Quaternion.identity;
    private Quaternion _newRotation = Quaternion.identity;
    private Equipment _spear;


    [Header("IK")]
    [SerializeField, Tooltip("IK aim target")]
    private GameObject _aimTarget;

    private float _outputLength = 0f;
    private Vector3 _spearIdlePosition = Vector3.zero;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _refAimingInput.variable.ValueChanged += OnSpearMovement;
    }

    // Update is called once per frame
    void Update()
    {
        if (!_isActive) return;

        if ( _newRotation != _aimTarget.transform.localRotation)
            RotateSpear();
    }

    public void SetActive(bool active, Equipment forward )
    {
        _isActive = active;
        _spear = forward;
        if (_aimTarget)
        {
            _spearStartOrientation = _aimTarget.transform.localRotation;
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
        //var offset = new Vector3(_refAimingInput.variable.value.x, 0f, _refAimingInput.variable.value.y) * _moveDistance;
        //var offset = _spear.transform.up * _outputLength * _moveDistance;

        //Back or forwards movement
        float angle = Vector3.Angle(new Vector3(_refAimingInput.variable.value.x, 0f, _refAimingInput.variable.value.y), transform.forward);
        if (angle > 90f)
            _outputLength = 0;
        else
            _outputLength = _refAimingInput.variable.value.x * transform.forward.x + _refAimingInput.variable.value.y * transform.forward.z;

        //Clamp angle
        float clampedAngle = ClampAngle(angle);
        if (IsNegativeAngle(clampedAngle))
            clampedAngle *= -1;

        //Forward stab motion, find the output length and rotate it towards your orientation
        Quaternion rotation = Quaternion.Euler(0, _refAimingInput.variable.StateManager.fOrientation, 0);
        var offset = -transform.right * _outputLength * _moveDistance;
        _aimTarget.transform.localPosition = _spearIdlePosition + rotation * offset;

        //Rotation for swing
        _newRotation = _spearStartOrientation * Quaternion.Euler(clampedAngle, 0f, 0f);

        Debug.Log($"angle: {angle}, clamped angle: {clampedAngle}, input :{_refAimingInput.Value}, forward: {transform.forward}");
    }

    private float ClampAngle(float angle)
    {
        float clampedAngle = 0f;
        if (angle > 1f)
            clampedAngle = (angle / 180f) * _maxAngle;

        return clampedAngle;
    }

    private bool IsNegativeAngle( float angle )
    {
        float crossProduct = _refAimingInput.variable.value.x * transform.forward.z - _refAimingInput.variable.value.y * transform.forward.x;
        return crossProduct > 0;
    }

    private void RotateSpear()
    {
        float t = Time.deltaTime * _rotationSpeed;
        _aimTarget.transform.localRotation = Quaternion.Slerp(_aimTarget.transform.localRotation, _newRotation, t);
    }

    public void SetIdlePosition()
    {
        _spearIdlePosition = _aimTarget.transform.localPosition;
    }

   
}
