using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKTransitionsToTarget : MonoBehaviour
{
    [Header("Look at IK")]
    [SerializeField] private GameObject _animTarget;
    [SerializeField] private float _lerpSpeedTorso = 10f;
    [SerializeField] private MultiAimConstraint _lookRig;
    [SerializeField, Tooltip("the weight of the spine rotation when holding a spear")] private float _torsoWeightSpear = 0.25f;
    [SerializeField, Tooltip("the weight of the spine rotation when holding a sword")] private float _torsoWeightSword = 0.65f;
    [Header("Spear IK")]
    [SerializeField, Tooltip("the position on the spear for the left hand to attach to")]
    private GameObject _lhHoldingPosition;
    [SerializeField, Tooltip("the target of the Left hand IK 2 bone component")]
    private GameObject _IKTargetLh;
    [SerializeField, Tooltip("the target of the Right hand IK 2 bone component")]
    private GameObject _IKTargetRh;
    private GameObject _aimTarget;
    [SerializeField, Tooltip("the height of the spear in idle pos compared with his up vector")]
    private float _spearheight = 0.65f;
    [SerializeField, Tooltip("the distance of the spear to the right in idle pos compared with his right vector")]
    private float _spearRight = 0.65f;
    [SerializeField, Tooltip("the distance of the spear to the front in idle pos compared with his forward vector")]
    private float _spearForward = 0.65f;
    [SerializeField, Tooltip("the angle of the up down of the spear ")]
    private float _spearAngle = 25f;
    [SerializeField, Tooltip("speed of left hand adjusting to the spear movement")]
    private float _lerpSpeedLHand = 20f;

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
        _defaultPosition = transform.forward * 2f + transform.position + transform.up * 0.75f;
        _animTarget.transform.position = _defaultPosition;
        _animStabTarget.transform.position = _defaultPosition - Vector3.up * _heightStab;
        _animStabTarget.transform.rotation = Quaternion.Euler(130f, 0f, 0f);
        //_defaultPosition = _animTarget.transform.position;

        var startPos = transform.position
             + transform.right * _spearRight
             + transform.up * _spearheight
             + transform.forward * _spearForward;

        if (_IKTargetRh)
        {
            _IKTargetRh.transform.position = startPos;
            _IKTargetRh.transform.Rotate(Vector3.forward, _spearAngle);
            GetComponent<SpearAiming>()?.SetIdlePosition();
        }
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

        //the spear stance, holding the idle hand on the spear below
        if (_lhHoldingPosition && _IKTargetLh)
        {
            _IKTargetLh.transform.position = Vector3.Lerp(
                _IKTargetLh.transform.position,
                _lhHoldingPosition.transform.position,
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

    public void SwitchStance(Component sender, object obj)
    {
        if (sender.gameObject != transform.gameObject) return;
        var args = obj as ChangeIKStanceEventArgs;
        if (args == null) return;

        if (args.UseSpear)
        {
            _lookRig.weight = _torsoWeightSpear;
            _lhHoldingPosition = args.LHSocket;
        }
        else
            _lookRig.weight = _torsoWeightSword;
        
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
