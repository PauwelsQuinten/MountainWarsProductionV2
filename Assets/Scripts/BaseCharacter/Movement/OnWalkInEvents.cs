using System;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class OnWalkInEvents : MonoBehaviour
{
    [SerializeField] private GameEvent _queueEvent;
    [SerializeField] private float _movementPower = 100f;
    [SerializeField] private float _movementPowerAtRiver = 100f;
    [SerializeField] private float _approachTime = 0.25f;
    private const string NO_TAG = "Untagged";
    private const string TAG_Villager = "Untagged";
    private Rigidbody _rb;
    private StateManager _stateManager;

    Vector3 collisionPoint;

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

                ContactPoint contact = collision.GetContact(0);
                collisionPoint = contact.point;

                float power = tag == SpecialInput.DipWater? _movementPowerAtRiver: _movementPower;

                Interact(collisionPoint, power);
                _queueEvent.Raise(this, new AimingOutputArgs {Special =  tag});
                return;
            }
        }

        Debug.Log("No vallid tag name found on hit object");
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
