using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class PositionOffset : MonoBehaviour
{
    [SerializeField, Tooltip("The ratio of offset in movement he gets in comparison to the power of the attack")] private float _offsetOnPower = 0.5f;
    private Vector3 _offsetDirectionPos;
    private Rigidbody _rb;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb == null)
            Debug.LogError("no rigidbody found in poitionOffest component");
    }

    public async void GetPositionOffset(Component sender, object obj)
    {
        AttackEventArgs args = obj as AttackEventArgs;
        if (args == null || args.Defender != gameObject) return;

        MoveCharacter(args.Attacker.transform.position);
    }

    private void MoveCharacter(Vector3 attackerPosition)
    {
        _offsetDirectionPos = (transform.position - attackerPosition).normalized;
        _rb.AddForce(_offsetDirectionPos * _offsetOnPower, ForceMode.Impulse);
    }

}
