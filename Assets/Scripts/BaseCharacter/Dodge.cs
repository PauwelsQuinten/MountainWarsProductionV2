using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor.Experimental;
using UnityEngine;
using UnityEngine.EventSystems;

public class Dodge : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField]
    private MovingInputReference _moveInput;

    [Header("Stamina")]
    [SerializeField]
    private FloatReference _staminaCost;
    [SerializeField]
    private GameEvent _loseStamina;

    [Header("DodgeStats")]
    [SerializeField]
    private float _dashSpeed;
    [SerializeField]
    private float _dashDistance;
    [SerializeField]
    private float _cooldown;

    private Rigidbody _rb;
    private bool _canRun = true;

    private bool _dashing;

    private Vector3 _dashDirection;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if(_dashing) PerformDodge();
    }

    public void ActivateDodge(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (!_canRun) return;

        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });

        if (_moveInput.Value != Vector2.zero)
            _dashDirection = new Vector3(_moveInput.Value.x, 0, _moveInput.Value.y);
        else _dashDirection = transform.forward;
        _dashing = true;
        _canRun = false;
        StartCoroutine(EndDodge());
    }

    private void PerformDodge()
    {
        _rb.linearVelocity = _dashDirection * _dashSpeed;
    }

    private IEnumerator EndDodge()
    {
        yield return new WaitForSeconds(_dashDistance);
        _dashing = false;
        _rb.linearVelocity = Vector3.zero;
        StartCoroutine(ResetCanRun());
    }

    private IEnumerator ResetCanRun()
    {
        yield return new WaitForSeconds(_cooldown);
        _canRun = true;
    }
}
