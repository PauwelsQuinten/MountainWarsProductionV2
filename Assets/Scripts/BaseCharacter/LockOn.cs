using System;
using System.Collections;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    private GameObject _lockonTarget;
    [Header("Events")]
    [SerializeField] private GameEvent _lockonEvent;
    [SerializeField] private GameEvent _inQueue;

    [Header("Animation")]
    [SerializeField] private GameEvent _changePanel;

    [Header("updateValue")]
    [SerializeField, Tooltip("will send event to statemanager to update orientation on every change of this angle")]
    private float _minAngleBeforeSendEvent = 10f;
    [SerializeField, Tooltip("The time he takes without target to sheat away his sword")]
    private float _timewithoutTarget = 5f;

    private Orientation _storedOrientation = Orientation.East;
    private float _storedfOrientation = 0;
    private GameObject _previousTarget;

    private Coroutine _sheathingCoroutine;
    private bool _sheathing;


    private StateManager _stateManager;

    private void Start()
    {
        if (!_lockonTarget)
            return;

        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);

        _storedOrientation = newOrientation;
        _storedfOrientation = fOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation, NewFOrientation = fOrientation });
        
    }

    void Update()
    {
        if (!_lockonTarget)
            return;

        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);

        if (IsOrientationChanged(newOrientation, fOrientation))
        {
            _storedOrientation = newOrientation;
            _storedfOrientation = fOrientation;
            _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation, NewFOrientation = fOrientation });
        }
    }

    public void FoundTarget(Component sender, object obj)
    {
        if (_stateManager == null) _stateManager = GetComponent<StateManager>();
        if (sender.gameObject != gameObject) return;
        var args = obj as NewTargetEventArgs;
        if (args == null) return;

        _lockonTarget = args.NewTarget;
        SheatSword();

        if (!_lockonTarget || !_lockonEvent)
            return;
        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);
        _storedOrientation = newOrientation;
        _storedfOrientation = fOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation, NewFOrientation = fOrientation });

        if (_previousTarget == _lockonTarget) return;
        _previousTarget = _lockonTarget;
        if (_changePanel == null) return;
        _changePanel.Raise(this, new TriggerEnterEventArgs { NewViewIndex = 0, IsShowDown = true, VsTarget = _lockonTarget });
    }

    public void WeaponSheathed(Component sender, object obj)
    {
        if(sender == this) return;
        if(_sheathingCoroutine != null)StopCoroutine(_sheathingCoroutine);
        _sheathing = false;
    }

    private void SheatSword()
    {
        if (_lockonTarget == null)
        {
            if (!_stateManager.WeaponIsSheathed)
            {
                if (!_sheathing )
                {
                    _sheathingCoroutine = StartCoroutine(SheathWeapon(_timewithoutTarget, true));
                }
            }
        }
        else
        {
            if (_sheathingCoroutine != null)
                StopCoroutine(_sheathingCoroutine);
            if (_stateManager.WeaponIsSheathed)
            {
                if (!_sheathing)
                {
                    _sheathingCoroutine = StartCoroutine(SheathWeapon(0.1f, false));
                }
            }
        }
    }

    private Orientation CalculateOrientation(out float fOrientation)
    {
        var direction = _lockonTarget.transform.position - transform.position;
        fOrientation = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        int clampedAngle = (Mathf.RoundToInt(fOrientation / 45)) * 45;
        clampedAngle = (clampedAngle == -180 )? 180 : clampedAngle;
        Orientation newOrientation = (Orientation)clampedAngle;

        return newOrientation ;
    }

    private bool IsOrientationChanged(Orientation newOrientation, float orientation)
    {
        return Mathf.Abs(_storedfOrientation - orientation) > _minAngleBeforeSendEvent;
    }

    private IEnumerator SheathWeapon(float duration, bool sheat)
    {
        _sheathing = true;
        yield return new WaitForSeconds(duration);
        if (sheat)
            _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.SheatSword, AnimationStart = true });
        else    
            _inQueue.Raise(this, new AimingOutputArgs { Special = SpecialInput.UnSheatSword, AnimationStart = true });
        _sheathing = false;
    }
}
