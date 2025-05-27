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
                Interact(collision.gameObject);
                _queueEvent.Raise(this, new AimingOutputArgs {Special =  tag});
                return;
            }
        }

        Debug.Log("No vallid tag name found on hit object");
    }

    private async Task Interact(GameObject target)
    {        
        float elpased = 0f;
        while( elpased < _approachTime)
        {
            elpased += Time.deltaTime;
            var _offsetDirectionPos = (transform.position - target.transform.position).normalized;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Force);
            await Task.Yield();  
        }
    }


}
