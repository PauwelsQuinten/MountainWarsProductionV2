using UnityEngine;

public class RepositionAction : GoapAction
{
    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        _isActivated = true;
        Debug.Log($"start reposition");

    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        ActionCompleted();
    }

    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (blackboard.variable.IsPlayerAgressive)
            Cost = 0.5f;
        else
            Cost = 0.8f;
        return true;
    }

    public override bool IsCompleted(WorldState current)
    {
        return !_isActivated;
    }
    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
       /* return !currentWorldState.IsBlockInCorrectDirection()
          && currentWorldState._worldStateValues2[EWorldState.TargetDistance] == WorldStateValue.InRange;*/
        return false;
    }
}
