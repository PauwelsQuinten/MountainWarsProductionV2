using UnityEditor.SpeedTree.Importer;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class IKSpearMovement : MonoBehaviour
{
    private enum RigValue
    {
        Default = -1,
        Zero = 0,
        One = 1
    }


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
    [SerializeField, Tooltip("IK target for aiming adjustment to opponent")]
    private GameObject _aimTargetAdjuster;

    
    private float _lerpLHSpeed;
    private RigValue _setWeight = RigValue.Default;
    private StateManager _stateManager;
    private Quaternion _defaultOrientation;
    private Quaternion _targetRotation = Quaternion.identity;


    private void Start()
    {
        _stateManager = GetComponent<StateManager>();
        _defaultOrientation = _aimTargetAdjuster.transform.localRotation;

        

        if (_IKTargetRh && _rShoulderTarget)
        {
            GetComponent<SpearAiming>()?.SetIdlePosition();
        }
    }

    private void Update()
    {
        if (!_IKTargetLh || !_lhHoldingPosition) return; 
        _IKTargetLh.transform.position = _lhHoldingPosition.transform.position;
        AdjustAim();
        if (_targetRotation != Quaternion.identity) 
            _aimTargetAdjuster.transform.localRotation = Quaternion.Slerp(_aimTargetAdjuster.transform.localRotation, _targetRotation, Time.deltaTime);
    }

    private void LateUpdate()
    {
        //At restart of the game, this value gets reset at start, so change again when that happens
        //Make sure the Idle State in animator is set to "Write Defaults" or this IK will not work
        if (_spearRig.weight != (int)_setWeight)
        {            
            _spearRig.weight = (int)_setWeight;
            _spearRig.enabled = false;
        }

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
            _setWeight = RigValue.One;
            
        }
        else
        {
            _setWeight = RigValue.Zero;
        }

    }

    private void AdjustAim()
    {
        float orient = (float)_stateManager.Orientation;
        float fOrient = _stateManager.fOrientation;
        float diff = orient - fOrient;
        if (Mathf.Abs(diff) < 1f)
        {
            _targetRotation = Quaternion.identity;
            return;
        }
        Quaternion rotationAdjustment = Quaternion.Euler(0, diff, 0);
       _targetRotation = _defaultOrientation * rotationAdjustment;
    }

}
