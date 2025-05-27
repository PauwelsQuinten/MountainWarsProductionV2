using System;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class OnWalkInEvents : MonoBehaviour
{
    [SerializeField] private GameEvent _queueEvent;
    [SerializeField] private float _movementPower = 100f;
    [SerializeField] private float _approachTime = 0.25f;
    private const string NO_TAG = "Untagged";
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag(NO_TAG))
            return;

        foreach (SpecialInput tag in Enum.GetValues(typeof(SpecialInput)))
        {
            if ((int)tag >= 100  && collision.transform.CompareTag(tag.ToString()))
            {

                ContactPoint contact = collision.GetContact(0);
                Vector3 collisionPoint = contact.point;

                Interact(collisionPoint);
                _queueEvent.Raise(this, new AimingOutputArgs {Special =  tag});
                return;
            }
        }

        Debug.Log("No vallid tag name found on hit object");
    }

    private async Task Interact(Vector3 target)
    {        
        float elpased = 0f;
        while( elpased < _approachTime)
        {
            elpased += Time.deltaTime;
            var _offsetDirectionPos = (transform.position - target).normalized;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Force);
            await Task.Yield();  
        }
    }


}
