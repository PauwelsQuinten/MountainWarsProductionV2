using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class SpearHitDetection : MonoBehaviour
{
    [SerializeField] private GameEvent _recieveAttackEvent;
    [SerializeField] private LayerMask _characterLayer;
    [SerializeField] private float _minPower = 1f;
    [SerializeField] private float _maxPower = 20f;
    [SerializeField] private float _powerOnSpeed = 7.5f;
    private float _velocity = 0f;
    private Vector3 _PreviousPosition = Vector3.zero;

    private float _power = 10f;
    private bool _active = true;

    void OnDestroy()
    {
        _active = false;
    }
    async void Start()
    {
        await foreach(float value in CalculateVelocity())
        {
            _power = value;
        }
    }

   
    private async IAsyncEnumerable<float> CalculateVelocity()
    {

        while (_active)
        {

            float value = Vector3.Distance(_PreviousPosition, transform.position);
            _PreviousPosition = transform.position;

            yield return value;
            await Task.Delay(100);
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (((1 << other.gameObject.layer) & _characterLayer) != 0)
        {
            if (other.gameObject == gameObject) return;

            float power = _power * _powerOnSpeed;
            if (power < 1) 
                power = 1;
            power*= _minPower;
            if (power >_maxPower ) 
                power = _maxPower;

            Debug.Log(power);
            _recieveAttackEvent.Raise(transform.root, new AttackEventArgs
            {
                Attacker = transform.root.gameObject, AttackPower = power, Defender = other.gameObject
            });

        }
    }

}
