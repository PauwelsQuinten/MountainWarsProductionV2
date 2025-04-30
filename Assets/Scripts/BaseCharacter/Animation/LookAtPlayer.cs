using System.Collections;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private GameObject _animTarget;
    [SerializeField] private float _lerpSpeed = 10f;
    private GameObject _aimTarget;
    private Vector3 _defaultPosition;
    private Coroutine _lookToDefaultCoroutine;

    private void Start()
    {
        _defaultPosition = transform.forward *10 + transform.position + transform.up ;
        _animTarget.transform.position = _defaultPosition;
        //_defaultPosition = _animTarget.transform.position;
    }

    void Update()
    {
        if (_aimTarget)
        {
            _animTarget.transform.position = Vector3.Lerp(
                _animTarget.transform.position,
                _aimTarget.transform.position,
                Time.deltaTime * _lerpSpeed);
        }
    }

    public void SetAimTarget(Component sender, object obj)
    {
        if (sender.gameObject != transform.gameObject) return;

        NewTargetEventArgs args = obj as NewTargetEventArgs;
        if (args == null) return;

        _aimTarget = args.NewTarget;

        if (_animTarget == null && _lookToDefaultCoroutine != null)
            _lookToDefaultCoroutine = StartCoroutine(LookAtDefault());
    }

    private IEnumerator LookAtDefault()
    {
        _animTarget.transform.position = Vector3.Lerp(
                _animTarget.transform.position,
                _defaultPosition,
                Time.deltaTime * _lerpSpeed);
        yield return null;
    }

}
