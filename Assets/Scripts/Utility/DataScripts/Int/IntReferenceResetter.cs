using UnityEngine;

public class IntReferenceResetter : MonoBehaviour
{
    [SerializeField]
    private IntReference _valueToReset;
    [SerializeField]
    private int _value;

    private void Start()
    {
        _valueToReset.variable.value = _value;
    }
}
