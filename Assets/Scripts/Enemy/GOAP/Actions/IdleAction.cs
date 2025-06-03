using System.Collections.Generic;
using UnityEngine;

public class IdleAction : GoapAction
{
    
    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //base.UpdateAction(currentWorldState);
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        return base.IsCompleted(currentWorldState);
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {       
        return currentWorldState.TargetBehaviour == EBehaviourValue.Attacking
             && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;
    }

}


