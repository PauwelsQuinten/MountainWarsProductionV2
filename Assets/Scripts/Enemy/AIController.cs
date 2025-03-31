using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class AIController : MonoBehaviour
{
    [Header("State Manager")]
    [SerializeField]
    private StateManager _stateManager;

    [Header("Variables")]
    [SerializeField]
    private MovingInputReference _moveInputRef;
    [SerializeField]
    private GameEvent _pickupEvent;

    [Header("Healing")]
    [SerializeField]
    private float _patchUpDuration;
    [SerializeField]
    private GameEvent _patchUpEvent;

    [Header("Perception")]
    [SerializeField] private GameEvent _LookForTarget;

    private Coroutine _resetAttackheight;

    private bool _wasSprinting;
    private bool _isHoldingShield;

    private float _patchTimer;
    void Start()
    {
        _moveInputRef.variable.StateManager = _stateManager;
        StartCoroutine(CheckSurrounding());
    }

    //Send Package to Block/Parry/Attack instead of going through aiming.
    //

   
    public void Sprint(bool sprint)
    {
        if (sprint)
        {
            _wasSprinting = true;
            _moveInputRef.variable.SpeedMultiplier = 1.5f;
        }
        if (!sprint && _wasSprinting)
        {
            _wasSprinting = false;
            _moveInputRef.variable.SpeedMultiplier = 1;
            return;
        }

    }

    public void Interact()
    {
        _pickupEvent.Raise(this);
    }

    private IEnumerator CheckSurrounding()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            _LookForTarget.Raise(this, new OrientationEventArgs { NewOrientation = _stateManager.Orientation});
        }

    }

}
