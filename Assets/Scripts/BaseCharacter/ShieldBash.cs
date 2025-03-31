using System.Collections;
using UnityEngine;

public class ShieldBash : MonoBehaviour
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

    [Header("Events")]
    [SerializeField]
    private GameEvent _checkBlock;

    [Header("Damage")]
    [SerializeField]
    private float _damage = 25;

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
        if (_dashing) PerformDodge();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name != "Enemy") return;
        if (!_dashing) return;
        _checkBlock.Raise(this, new AttackEventArgs { AttackHeight = AttackHeight.Torso, AttackPower = _damage, AttackType = AttackType.ShieldBash});
    }

    public void ActivateShieldBash(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        if (!_canRun) return;

        _loseStamina.Raise(this, new StaminaEventArgs { StaminaCost = _staminaCost.value });

        _dashDirection = new Vector3(_moveInput.Value.x, _moveInput.Value.y, 0);
        _dashing = true;
        _canRun = false;
        StartCoroutine(EndDodge());
    }

    private void PerformDodge()
    {
        _rb.linearVelocity = _dashDirection * _dashSpeed;
        Debug.Log("ShieldBash");
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
