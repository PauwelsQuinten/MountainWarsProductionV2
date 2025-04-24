using System;
using System.Collections;
using UnityEngine;

public class LockOn : MonoBehaviour
{
    [SerializeField] private GameObject _lockonTarget;
    [SerializeField] private GameEvent _lockonEvent;
    [SerializeField] private GameEvent _sheathWeapon;

    [Header("Animation")]
    [SerializeField] private GameEvent _changePanel;

    private Orientation _storedOrientation = Orientation.East;
    private GameObject _previousTarget;

    private Coroutine _sheathingCoroutine;
    private bool _sheathing;

    private bool _canResheath;

    private StateManager _stateManager;

    private void Start()
    {
        if (!_lockonTarget)
            return;

        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);

        _storedOrientation = newOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation, NewFOrientation = fOrientation });
        
    }

    void Update()
    {
        if (!_lockonTarget)
            return;

        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);

        if (IsOrientationChanged(newOrientation))
        {
            _storedOrientation = newOrientation;
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

        if (_lockonTarget == null)
        {
            if (!_stateManager.WeaponIsSheathed)
            {
                if (!_sheathing && _canResheath)
                {
                    _sheathingCoroutine = StartCoroutine(SheathWeapon(5));
                }
            }
        }
        else
        {
            if (_stateManager.WeaponIsSheathed)
            {
                if (!_sheathing)
                {
                    _canResheath = true;
                    _sheathingCoroutine = StartCoroutine(SheathWeapon(0.1f));
                }
            }
            else _canResheath = true;
        }

        if (!_lockonTarget || !_lockonEvent)
            return;
        float fOrientation = 0f;
        var newOrientation = CalculateOrientation(out fOrientation);
        _storedOrientation = newOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation, NewFOrientation =  fOrientation});

        if (_previousTarget == _lockonTarget) return;
        _previousTarget = _lockonTarget;
        if (_changePanel == null) return;
        _changePanel.Raise(this, new TriggerEnterEventArgs { NewViewIndex = 0,IsShowDown = true});
    }

    public void WeaponSheathed(Component sender, object obj)
    {
        if(sender == this) return;
        if(_sheathingCoroutine != null)StopCoroutine(_sheathingCoroutine);
        _sheathing = false;
        _canResheath = false;
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

    private bool IsOrientationChanged(Orientation newOrientation)
    {
        return newOrientation != _storedOrientation;
    }

    private IEnumerator SheathWeapon(float duration)
    {
        _sheathing = true;
        yield return new WaitForSeconds(duration);
        _sheathWeapon.Raise(this, EventArgs.Empty);
        _sheathing = false;
    }
}
