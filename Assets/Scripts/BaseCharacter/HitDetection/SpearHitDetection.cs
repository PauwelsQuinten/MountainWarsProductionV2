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

    private Vector3 _spearForwardDirection = Vector3.zero;
    private Vector3 _PreviousPosition = Vector3.zero;
    private float _fOrientation = 0f;
    private AttackType _attackType = UnityEngine.AttackType.Stab;

    private float _power = 10f;
    private bool _active = true;

    void OnDestroy()
    {
        _active = false;
    }
   
    async void Start()
    {
        await foreach(Vector3 value in CalculateVelocity())
        {
            if (value == Vector3.zero)
            {
                _power = _minPower;
                continue;
            }

            float inputLength = value.magnitude;
            _attackType = AttackType(value, inputLength);
            _power = _power * _powerOnSpeed;
        }
    }

    private AttackType AttackType(Vector3 direction, float distance)
    {
        float dot = Vector3.Dot(_spearForwardDirection, direction);
        if (distance < 0.1f)
        {
            return UnityEngine.AttackType.Stab;
        }

        float dotDiff = dot / distance;
        if (dotDiff > 0.5f && distance < 0.5f)
        {
            Debug.Log($"Dot = {dotDiff}, power = {distance}, stab");
            return UnityEngine.AttackType.Stab;
        }
        else if (dotDiff > 0f)
        {
            float cross = direction.x * _spearForwardDirection.y - direction.y * _spearForwardDirection.x;
            if (cross > 0f) 
                return UnityEngine.AttackType.HorizontalSlashToRight;
            else
                return UnityEngine.AttackType.HorizontalSlashToLeft;
        }
        else
            return UnityEngine.AttackType.Stab;
    }

    private void Update()
    {
        var comp = transform.root.GetComponent<StateManager>();
        if (comp != null) 
            _fOrientation = comp.fOrientation * Mathf.Deg2Rad;
        _spearForwardDirection = Geometry.Geometry.CalculateVectorFromfOrientation(_fOrientation);
    }

    private async IAsyncEnumerable<Vector3> CalculateVelocity()
    {
        while (_active)
        {
            Vector3 value = _PreviousPosition - transform.position;
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
                Attacker = transform.root.gameObject, AttackPower = power, Defender = other.gameObject, AttackType = _attackType
            });

        }
    }

}
