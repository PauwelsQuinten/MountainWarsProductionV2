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
       
        return true;
    }

    public override bool IsCompleted(WorldState current)
    {
        return !_isActivated;
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        if (blackboard.variable.IsPlayerAgressive)
            return 0.5f;
        else if (currentWorldState.TargetAttackRange == EWorldStateRange.InRange)
            return 0.25f;
        else
            return Cost;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        /* return !currentWorldState.IsBlockInCorrectDirection()
           && currentWorldState._worldStateValues2[EWorldState.TargetDistance] == WorldStateValue.InRange;*/
        return (blackboard.variable.TargetState == AttackState.Attack || blackboard.variable.TargetState == AttackState.BlockAttack)
            && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;
    }
}
