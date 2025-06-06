using UnityEngine;

public class FlyOff : MonoBehaviour
{
    [SerializeField] private float _speed = 5.0f;
    [SerializeField] private float _anglSpeed = 1.0f;
    [SerializeField] private float _gravity = 1.0f;
    private float _timeElapsed = 0.0f;
    [SerializeField] private bool _enabled = false;
    private Vector3 _startPosition = Vector3.zero;
    [SerializeField] private Vector3 _velocity = Vector3.zero;
    [SerializeField] private Vector3 direction = Vector3.zero;

    public void StartFly(Vector3 direction)
    {
        _enabled = true;
        _startPosition = transform.position;
        _velocity = direction * _speed;
        _timeElapsed = 0.0f;
    }

    private void FixedUpdate()
    {
        if (!_enabled)
        {
            _timeElapsed = 0.0f;
            _startPosition = Vector3.zero;
            return;
        }

        if (_startPosition == Vector3.zero)
        {
            _startPosition = transform.position;
            _velocity = direction * _speed;
        }

        _timeElapsed += Time.deltaTime;
        Vector3 displacement = new Vector3(
        _velocity.x * _timeElapsed,
        _velocity.y * _timeElapsed - 0.5f * _gravity * _timeElapsed * _timeElapsed,
        0f
        );
        transform.position = _startPosition + displacement;

    }


    void OnCollisionEnter(Collision collision)
    {

        if (collision.collider is TerrainCollider)
        {
            _enabled = false;
        }

    }


}
