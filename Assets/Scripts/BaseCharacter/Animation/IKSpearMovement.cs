using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKSpearMovement : MonoBehaviour
{
    [Header("Rigs")]
    [SerializeField, Tooltip("the parent rig of all th spear IK")]
    private Rig _spearRig;
    [SerializeField, Tooltip("the position on the spear for the left hand to attach to")]
    private GameObject _lhHoldingPosition;
    [SerializeField, Tooltip("the target of the Left hand IK 2 bone component")]
    private GameObject _IKTargetLh;
    [SerializeField, Tooltip("the target of the Right hand IK 2 bone component")]
    private GameObject _IKTargetRh;
    [SerializeField, Tooltip("IK Right shoulder target")]
    private GameObject _rShoulderTarget;

    
    private float _lerpLHSpeed;

    private GameObject _aimTarget;



    private void Start()
    {
        if (_IKTargetRh && _rShoulderTarget)
        {
            GetComponent<SpearAiming>()?.SetIdlePosition();
        }
    }

    private void Update()
    {
        _IKTargetLh.transform.position = _lhHoldingPosition.transform.position;
        ////the spear stance, holding the Left hand on the spear below
        //if (_lhHoldingPosition && _IKTargetLh)
        //{
        //    _IKTargetLh.transform.position = Vector3.Lerp(
        //        _IKTargetLh.transform.position,
        //        _lhHoldingPosition.transform.position,
        //        Time.deltaTime * _lerpLHSpeed);
        //}

    }

    //public void SetAimTarget(Component sender, object obj)
    //{
    //    if (sender.gameObject != transform.gameObject) return;
    //
    //    NewTargetEventArgs args = obj as NewTargetEventArgs;
    //    if (args == null) return;
    //
    //    _aimTarget = args.NewTarget;
    //
    //    if (_animTarget == null && _lookToDefaultCoroutine != null)
    //        _lookToDefaultCoroutine = StartCoroutine(LookAtDefault());
    //}


    public void SwitchStance(Component sender, object obj)
    {
        if (sender.gameObject != transform.gameObject) return;
        var args = obj as ChangeIKStanceEventArgs;
        if (args == null) return;

        if (args.UseSpear)
        {            
            _lhHoldingPosition = args.LHSocket;
            _spearRig.weight = 1f;
            //var rotation = _IKTargetLh.transform.rotation;
            //_IKTargetLh.transform.parent = _lhHoldingPosition.transform;
            //_IKTargetLh.transform.localPosition = Vector3.zero;
            //_IKTargetLh.transform.rotation = rotation;

        }
        else
        {          
            _spearRig.weight = 0f;
        }

    }

}
