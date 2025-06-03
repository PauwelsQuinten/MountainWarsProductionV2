using UnityEngine;

public class LookAroundAction : GoapAction
{
    [SerializeField] private GameEvent _changeOrientationEvent;

    [SerializeField] private float _rotationSpeed = 90f;
    private float _currentAngle = 0f;

    protected override void Start()
    {
        base.Start();
        StateManager state = null;
        if (npc)
            state = npc.GetComponent<StateManager>();
        if (state)
            _currentAngle = state.fOrientation * Mathf.Rad2Deg;
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        _currentAngle += Time.deltaTime * _rotationSpeed;
        _currentAngle = (_currentAngle >= 180f) ? _currentAngle - 360f : _currentAngle;
        
        float newfAngle = _currentAngle;

        int clampedAngle = (Mathf.RoundToInt(_currentAngle / 45)) * 45;
        clampedAngle = (clampedAngle == -180) ? 180 : clampedAngle;
        Orientation newOrientation = (Orientation)clampedAngle;

        _changeOrientationEvent.Raise(npc.transform, new OrientationEventArgs { NewFOrientation = newfAngle, NewOrientation = newOrientation });
        //boss.GetComponent<WalkAnimate>().Rotate(_currentAngle * Mathf.Deg2Rad);
        //boss.GetComponent<Eyes>().Rotate(_rotationSpeed);
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        return base.CalculateCost(blackboard, currentWorldState);
    }

}
