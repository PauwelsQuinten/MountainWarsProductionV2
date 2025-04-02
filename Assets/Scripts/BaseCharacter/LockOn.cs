using UnityEngine;

public class LockOn : MonoBehaviour
{
    [SerializeField] private GameObject _lockonTarget;
    [SerializeField] private GameEvent _lockonEvent;

    private Orientation _storedOrientation = Orientation.East;

    private void Start()
    {
        if (!_lockonTarget)
            return;

        var newOrientation = CalculateOrientation();

        _storedOrientation = newOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation });
        
    }

    void Update()
    {
        if (!_lockonTarget)
            return;

        var newOrientation = CalculateOrientation();

        if (IsOrientationChanged(newOrientation))
        {
            _storedOrientation = newOrientation;
            _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation });
        }

    }

    public void FoundTarget(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = obj as NewTargetEventArgs;
        if (args == null) return;

        _lockonTarget = args.NewTarget;

        if (!_lockonTarget || !_lockonEvent)
            return;
        var newOrientation = CalculateOrientation();
        _storedOrientation = newOrientation;
        _lockonEvent.Raise(this, new OrientationEventArgs { NewOrientation = _storedOrientation });
    }

    private Orientation CalculateOrientation()
    {
        var direction = _lockonTarget.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;

        int clampedAngle = (Mathf.RoundToInt(angle / 45)) * 45;
        clampedAngle = (clampedAngle == -180 )? 180 : clampedAngle;
        Orientation newOrientation = (Orientation)clampedAngle;

        return newOrientation ;
    }

    private bool IsOrientationChanged(Orientation newOrientation)
    {
        return newOrientation != _storedOrientation;
    }

}
