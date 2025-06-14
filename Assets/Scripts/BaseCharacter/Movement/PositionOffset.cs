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

        AttackVillager attVilArgs = obj as AttackVillager;
        if (attVilArgs != null)
            HandleHitFeedback(attVilArgs);
    }

    private void HandleHitFeedback(AttackEventArgs args)
    {
        float power = 0f;
        if (args.Defender == gameObject)
        {
            //Push away the one who recieve the attack
            power = _pushbackForDefenderPowerRatio * args.AttackPower;
            MoveCharacter(args.Attacker.transform.position, power);
        }
        else if (args.Attacker == gameObject)
        {
            //Push away the one who throw the attack
            power = _pushbackForAttackerPowerRatio * args.BlockPower;
            MoveCharacter(args.Defender.transform.position, power);
        }
    }
    private void HandleHitFeedback(AttackVillager args)
    {
        HandleHitFeedback(new AttackEventArgs
        { Attacker = args.Attacker, Defender = args.Defender, AttackPower = args.AttackPower, BlockPower = args.BlockPower });
    }


    private void HandleAttackMovement(AttackMoveEventArgs args)
    {
        if (args.Attacker != gameObject) return; 

        //Move forward
        if (args.Target == null && args.AttackType != AttackType.None)
        {
            _offsetDirectionPos = transform.forward;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Impulse);
        }
        //Move back
        else if (args.AttackType == AttackType.None)
        {
            _offsetDirectionPos = transform.forward * -1;
            _rb.AddForce(_offsetDirectionPos * _movementPower * 0.8f, ForceMode.Impulse);
        }
        //move towards opponent
        else
        {
            _offsetDirectionPos = (args.Target.transform.position - transform.position).normalized;
            _rb.AddForce(_offsetDirectionPos * _movementPower, ForceMode.Impulse);
        }
    }

    private void MoveCharacter(Vector3 attackerPosition, float power)
    {
        _offsetDirectionPos = (transform.position - attackerPosition).normalized;
        _rb.AddForce(_offsetDirectionPos * power, ForceMode.Impulse);
    }

}
