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
    private GameEvent _interactEvent;
    [SerializeField]
    private GameEvent _MoveEvent;

    [Header("Healing")]
    [SerializeField]
    private float _patchUpDuration;
    [SerializeField]
    private GameEvent _patchUpEvent;

    [SerializeField]
    private GameEvent _shieldBash;

    [Header("Perception")]
    [SerializeField] private GameEvent _LookForTarget;

    private Coroutine _resetAttackheight;

    private bool _wasSprinting;
    private bool _isHoldingShield;

    private float _patchTimer;
    void Start()
    {
        StartCoroutine(CheckSurrounding());
    }

    //Send Package to Block/ParryAction/Attack instead of going through aiming.
    //

    public void AIEvents(Component sender, object obj)
    {
        AIInputEventArgs args = obj as AIInputEventArgs;
        if (obj is null) return;
        if (args.Sender != gameObject) return;

        switch (args.Input)
        {
            case AIInputAction.PatchUp:
                PatchUp();
                break;
            case AIInputAction.Dash:
                Sprint(true);
                break;
             case AIInputAction.StopDash:
                Sprint(false);
                break;
            case AIInputAction.Interact:
                break;
        }
    }

    private void Sprint(bool sprint)
    {
        DirectionEventArgs package;
        if (sprint)
        {
            _wasSprinting = true;
            package = new DirectionEventArgs { SpeedMultiplier = 1.5f };
            _MoveEvent.Raise(this, package);
        }
        else if (!sprint && _wasSprinting)
        {
            _wasSprinting = false;
            package = new DirectionEventArgs { SpeedMultiplier = 1f };
            _MoveEvent.Raise(this, package);
        }
       
    }

    private void PatchUp()
    {
        _patchUpEvent.Raise(this, false);
    }

    public void Interact()
    {
        _interactEvent.Raise(this);
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
