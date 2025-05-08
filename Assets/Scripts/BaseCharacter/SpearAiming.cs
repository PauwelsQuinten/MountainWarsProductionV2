using UnityEngine;

public class SpearAiming : MonoBehaviour
{
    private bool _isActive = true;

    [Header("InputVariable")]
    [SerializeField] private AimingInputReference _refAimingInput;

    [Header("Events")]
    [SerializeField] private GameEvent _AimOutputEvent;

    [Header("Angles")]
    [SerializeField, Tooltip("Max angle in degree derived from his center to left or right")]
    private float _maxAngle = 60f;

    [Header("IK")]
    [SerializeField, Tooltip("IK aim target")]
    private GameObject _aimTarget;

    private float _outputLength = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _refAimingInput.variable.ValueChanged += OnSpearMovement;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetActive(bool active)
    {
        _isActive = active;
    }

    private void OnSpearMovement(object sender, AimInputEventArgs e)
    {
        _outputLength = _refAimingInput.variable.value.magnitude;


    }

    private Vector2 ClampedMovementAngle(Vector2 input)
    {
        return input;
    }

}
