using UnityEngine;

public class FloatReferenceResetter : MonoBehaviour
{
    [SerializeField]
    private FloatReference _valueToReset;
    [SerializeField]
    private float _value;

    private void Awake()
    {
        _valueToReset.variable.value = _value;
    }
}
