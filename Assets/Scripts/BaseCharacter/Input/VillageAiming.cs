using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using static UnityEngine.Rendering.DebugUI;

public class VillageAiming : MonoBehaviour
{
    [Header("InputVariable")]
    [SerializeField] private AimingInputReference _refAimingInput;
    [Header("Refrences")]
    [SerializeField] private Rig _fullRig;
    [Header("Events")]
    [SerializeField] private GameEvent _queueEvent;
    [SerializeField] private GameEvent _changeRotation;

    private bool _isActive = false;
    private bool _InMotion = false;
    private Vector3 _collisionPoint = Vector3.zero;
    private SpecialInput _tag;
    private Rigidbody _rb;
    private StateManager _stateManager;
    CancellationTokenSource _cts;
    private RigValue _value = RigValue.Default;


    void Start()
    {
        _refAimingInput.variable.ValueChanged += Variable_ValueChanged;
        _rb = GetComponent<Rigidbody>();
        _stateManager = GetComponent<StateManager>();
    }

    private void LateUpdate()
    {
        if (_value != RigValue.Default)
        {
            _fullRig.weight = (int)_value;
            _value = RigValue.Default;
        }
    }


    private void Variable_ValueChanged(object sender, AimInputEventArgs e)
    {
        if (!_isActive) return;

        float output = _refAimingInput.variable.value.magnitude;
        if (output > 0.9f && !_InMotion)
        {
            Debug.Log($"kick the {_tag}");
            _InMotion = true;

            _value = RigValue.Zero;
            _ = Interact(_collisionPoint, 100);
            float fOrienation = Geometry.Geometry.CalculatefOrientationToTarget(_collisionPoint, transform.position);
            Orientation orientation = Geometry.Geometry.FindOrientationFromAngle(fOrienation);
            if (_changeRotation)
                _changeRotation.Raise(this, new OrientationEventArgs { NewFOrientation = fOrienation, NewOrientation = orientation });
            if (_queueEvent)
                _queueEvent.Raise(this, new AimingOutputArgs { Special = _tag });

            _ = TimeContact(3.5f);
        }

    }

    public void SetActive(bool active, Vector3 collisionPoint, SpecialInput tag = SpecialInput.Default)
    {
        _isActive = active;
        _collisionPoint = collisionPoint;
        _tag = tag;
    }

    private async Task TimeContact(float timeBeforeInteraction)
    {
        await Task.Delay((int)timeBeforeInteraction * 1000);
       _value = RigValue.One;
        _InMotion = false;
    }

    private async Task Interact(Vector3 target, float power)
    {
        float elpased = 0f;
        var _offsetDirectionPos = (transform.position - target).normalized;

        while (elpased < 0.25f)
        {
            elpased += Time.deltaTime;
            _rb.AddForce(_offsetDirectionPos * power, ForceMode.Force);
            await Task.Yield();
        }

    }


}
