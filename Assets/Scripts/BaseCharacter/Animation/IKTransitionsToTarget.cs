using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKTransitionsToTarget : MonoBehaviour
{
    [Header("Look at IK")]
    [SerializeField] private GameObject _animTarget;
    [SerializeField] private float _lerpSpeedTorso = 10f;
    private GameObject _aimTarget;
    [Header("Stab IK")]
    [SerializeField] private GameObject _animStabTarget;
    [SerializeField] private Rig _rArmRig;
    [SerializeField] private float _lerpSpeedRArm = 0.1f;
    [SerializeField, Tooltip("compared to the aim of looking, lowered with this value")] private float _heightStab = 0.25f;
    private Vector3 _defaultPosition;
    private Coroutine _lookToDefaultCoroutine;
    private Coroutine _lerpWeightCoroutine;

    private void Start()
    {
        _defaultPosition = transform.forward *2f + transform.position + transform.up *0.75f ;
        _animTarget.transform.position = _defaultPosition;
        _animStabTarget.transform.position = _defaultPosition - Vector3.up * _heightStab;
        _animStabTarget.transform.rotation = Quaternion.Euler(130f, 0f, 0f);
        //_defaultPosition = _animTarget.transform.position;
    }

    void Update()
    {
        if (_aimTarget)
        {
            _animTarget.transform.position = Vector3.Lerp(
                _animTarget.transform.position,
                _aimTarget.transform.position,
                Time.deltaTime * _lerpSpeedTorso);
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

    public void SetStabIK(Component sender, object obj)
    {
        if (sender.gameObject != transform.gameObject) return;
        bool active = (int)obj == 1 ? true : false;

        _animStabTarget.transform.position = _animTarget.transform.position - Vector3.up * _heightStab;


        if (_lerpWeightCoroutine != null)
            StopCoroutine(_lerpWeightCoroutine);
        _lerpWeightCoroutine = StartCoroutine(LerpWeight(active));
    }
        
    private IEnumerator LookAtDefault()
    {
        _animTarget.transform.position = Vector3.Lerp(
                _animTarget.transform.position,
                _defaultPosition,
                Time.deltaTime * _lerpSpeedTorso);
        yield return null;
    }

    private IEnumerator LerpWeight(bool activate)
    {
        int multiplier = activate ? 1 : -5;
        //var rigComp = _rArmRig.GetComponentInChildren<TwoBoneIKConstraint>();
        var rigComp = _rArmRig.GetComponentInChildren<MultiAimConstraint>();

        while (true)
        {
            rigComp.weight += multiplier * Time.deltaTime * _lerpSpeedRArm;

            if (rigComp.weight < 0)
            {
                rigComp.weight = 0;
                yield break;
            }
            else if (rigComp.weight > 1)
            {
                rigComp.weight = 1;
                yield break;
            }

            yield return null;
        }
       
    }

}
