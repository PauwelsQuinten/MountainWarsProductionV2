using UnityEngine;

public class PositionOffset : MonoBehaviour
{
    [SerializeField, Tooltip("The ratio of offset in movement the hit character gets in comparison to the power of the attack")]
    private float _pushbackForDefenderPowerRatio = 0.5f;
    [SerializeField, Tooltip("The ratio of offset in movement the attacker gets in comparison to the power of the opposing block")]
    private float _pushbackForAttackerPowerRatio = 0.5f;
    [SerializeField, Tooltip("The ratio of offset in movement the attacker gets when starting an attack")]
    private float _movementPower = 15f;
    private Vector3 _offsetDirectionPos;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            Debug.LogError("no rigidbody found in poitionOffest component");
    }

    public void GetPositionOffset(Component sender, object obj)
    {
        AttackEventArgs args = obj as AttackEventArgs;
        if (args != null)
            HandleHitFeedback(args);

        AttackMoveEventArgs attArgs = obj as AttackMoveEventArgs;
        if (attArgs != null)
            HandleAttackMovement(attArgs);
    }

    private void HandleHitFeedback(AttackEventArgs args)
    {
        float power = 0f;
        if (args.Defender == gameObject)
        {
            power = _pushbackForDefenderPowerRatio * args.AttackPower;
            MoveCharacter(args.Attacker.transform.position, power);
        }
        else if (args.Attacker == gameObject)
        {
            power = _pushbackForAttackerPowerRatio * args.BlockPower;
            MoveCharacter(args.Defender.transform.position, power);
        }
    }
    
    private void HandleAttackMovement(AttackMoveEventArgs args)
    {
        if (args.Attacker != gameObject) return; 

        if (args.Target == null && args.AttackType != AttackType.None)
        {
            _offsetDirectionPos = transform.forward;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Impulse);
        }
        else if (args.AttackType == AttackType.None)
        {
            _offsetDirectionPos = transform.forward * -1;
            _rb.AddForce(_offsetDirectionPos * _movementPower * 0.8f, ForceMode.Impulse);
        }
        else
        {
            _offsetDirectionPos = (args.Target.transform.position - transform.forward).normalized;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Impulse);
        }
    }

    private void MoveCharacter(Vector3 attackerPosition, float power)
    {
        _offsetDirectionPos = (transform.position - attackerPosition).normalized;
        _rb.AddForce(_offsetDirectionPos * power, ForceMode.Impulse);
    }

}
