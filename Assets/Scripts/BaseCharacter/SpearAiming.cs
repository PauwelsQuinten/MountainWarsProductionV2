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
    private Vector3 _spearForwardVector = Vector3.zero;
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

    }

    public void SetActive(bool active, Equipment forward )
    {
        _isActive = active;
        _spear = forward;
    }

    private void OnSpearMovement(object sender, AimInputEventArgs e)
    {
        _outputLength = _refAimingInput.variable.value.magnitude;

        MoveSpearAimingPoint();
    }

    private void MoveSpearAimingPoint()
    {
        //var offset = new Vector3(_refAimingInput.variable.value.x, 0f, _refAimingInput.variable.value.y) * _moveDistance;
        //var offset = _spear.transform.up * _outputLength * _moveDistance;

        Quaternion rotation = Quaternion.Euler(0, _refAimingInput.variable.StateManager.fOrientation, 0);
        var offset = -transform.right * _outputLength * _moveDistance;
        _aimTarget.transform.localPosition = _spearIdlePosition + rotation * offset;
    }

    private Vector2 ClampedMovementAngle(Vector2 input)
    {
        return input;
    }

    public void SetIdlePosition()
    {
        _spearIdlePosition = _aimTarget.transform.localPosition;

    }

}
