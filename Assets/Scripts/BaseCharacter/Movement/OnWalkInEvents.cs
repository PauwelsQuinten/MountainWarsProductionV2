using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class OnWalkInEvents : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private GameEvent _queueEvent;
    [SerializeField] private GameEvent _changeRotation;
    [Header("Movement")]
    [SerializeField] private float _movementPower = 100f;
    [SerializeField] private float _approachTime = 0.25f;
    [SerializeField] private float _contactTime = 0.25f;
    [SerializeField] private bool _useInput = false;

    [Header("Refrences")]
    [SerializeField] private Rig _fullRig;
    private const string NO_TAG = "Untagged";
    private const string TAG_Villager = "Untagged";

    private Rigidbody _rb;
    private StateManager _stateManager;
    private Aiming _aimingComp;
    private VillageAiming _villageAimingComp;

    Vector3 collisionPoint;
    private Collider _collider;
    private CancellationTokenSource _cts;
    private RigValue _value = RigValue.Default;
    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _stateManager = GetComponent<StateManager>();
        _aimingComp = GetComponent<Aiming>();
        _villageAimingComp = GetComponent<VillageAiming>();
    }

    private void LateUpdate()
    {
        if (_value != RigValue.Default)
        {
            _fullRig.weight = (int)_value;
            _value = RigValue.Default;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_stateManager.Target != null) return;

        if (collision.collider.gameObject.CompareTag(NO_TAG) || collision.collider.gameObject.CompareTag(TAG_Villager))
            return;

        foreach (SpecialInput tag in Enum.GetValues(typeof(SpecialInput)))
        {
            if ((int)tag >= 100  && collision.transform.CompareTag(tag.ToString()))
            {
                _collider = collision.collider;
                ContactPoint contact = collision.GetContact(0);
                collisionPoint = contact.point;

                float power =  _movementPower;

                if (_useInput)
                {
                    _aimingComp.SetActive(false);
                    _villageAimingComp.SetActive(true, collisionPoint, tag);
                }
                else
                    _ = TimeContact(_contactTime, power, tag);


                Debug.Log("enter collider");

                return;
            }
        }

        Debug.Log("No vallid tag name found on hit object");
    }

    private void OnCollisionExit(Collision collision)
    {
        if (_collider == collision.collider)
        {
            if (_useInput)
            {
                _aimingComp.SetActive(true);
                _villageAimingComp.SetActive(false, Vector3.zero);
            }
            else
            {
                _cts?.Cancel();
                _collider = null;
                _ = EnableRig(3f);
            }          
            Debug.Log("leave collider");
        }

    }

    private async Task TimeContact(float timeBeforeInteraction, float power, SpecialInput tag)
    {
        _cts = new CancellationTokenSource();
        CancellationToken token = _cts.Token;
        try
        {
            await Task.Delay((int)timeBeforeInteraction * 1000, token);
            if (!_cts.IsCancellationRequested)
            {
                _value = RigValue.Zero;
                _ = Interact(collisionPoint, power);
                float fOrienation = Geometry.Geometry.CalculatefOrientationToTarget(collisionPoint, transform.position);
                Orientation orientation = Geometry.Geometry.FindOrientationFromAngle(fOrienation);
                if (_changeRotation)
                    _changeRotation.Raise(this, new OrientationEventArgs { NewFOrientation = fOrienation, NewOrientation = orientation });
                if (_queueEvent)
                    _queueEvent.Raise(this, new AimingOutputArgs { Special = tag });
            }
            
        }
        catch { }
       
    }
    
    private async Task Interact(Vector3 target, float power)
    {        
        float elpased = 0f;
        var _offsetDirectionPos = (transform.position - target).normalized;
    
        while( elpased < _approachTime)
        {
            elpased += Time.deltaTime;
            _rb.AddForce(_offsetDirectionPos * power, ForceMode.Force);
            await Task.Yield();  
        }
    
    }


    private void OnDrawGizmos()
    {
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(collisionPoint, 0.5f);
        
    }

    private async Task EnableRig(float time)
    {
        await Task.Delay((int)time * 1000);
        _value = RigValue.One;
    }

}
