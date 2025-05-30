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
        if (blackboard.variable.IsPlayerAgressive || currentWorldState.TargetBehaviour == EBehaviourValue.Knock)
            return 0.5f;
        
        else
            return Cost;
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //Interupt action to defend if necesary, let him walk back between attacks
        return (blackboard.variable.TargetState == AttackState.Attack || blackboard.variable.TargetState == AttackState.BlockAttack)
            && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;
    }
}
