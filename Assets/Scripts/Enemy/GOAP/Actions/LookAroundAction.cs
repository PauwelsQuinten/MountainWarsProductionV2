using UnityEngine;

public class LookAroundAction : GoapAction
{
    [SerializeField] private float _rotationSpeed = 90f;
    private float _currentAngle = 0f;

    protected override void Start()
    {
        base.Start();
    }
    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        _currentAngle += Time.deltaTime * _rotationSpeed;
        _currentAngle = (_currentAngle >= 180f) ? _currentAngle - 360f : _currentAngle;
        
        var parentTrans = transform.parent;
        GameObject boss = null;
        if (parentTrans != null) 
            boss = parentTrans.gameObject;
        //boss.GetComponent<WalkAnimate>().Rotate(_currentAngle * Mathf.Deg2Rad);
        //boss.GetComponent<Eyes>().Rotate(_rotationSpeed);
    }

}
