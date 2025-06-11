using System;
using System.Threading;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class OnWalkInEvents : MonoBehaviour
{
    [SerializeField] private GameEvent _queueEvent;
    [SerializeField] private float _movementPower = 100f;
    [SerializeField] private float _movementPowerAtRiver = 100f;
    [SerializeField] private float _approachTime = 0.25f;
    [SerializeField] private float _contactTime = 0.25f;
    private const string NO_TAG = "Untagged";
    private const string TAG_Villager = "Untagged";
    private Rigidbody _rb;
    private StateManager _stateManager;

    Vector3 collisionPoint;
    private Collider _collider;
    private CancellationTokenSource _cts;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _stateManager = GetComponent<StateManager>();
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

                float power = tag == SpecialInput.DipWater? _movementPowerAtRiver: _movementPower;

                _ = TimeContact(_contactTime, power, tag);
                //_ = Interact(collisionPoint, power);
                //_queueEvent.Raise(this, new AimingOutputArgs {Special =  tag});

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
            _cts?.Cancel();
            _collider = null;
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
                _ = Interact(collisionPoint, power);
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
        Gizmos.DrawSphere(collisionPoint, 0.1f);
        
    }



}
