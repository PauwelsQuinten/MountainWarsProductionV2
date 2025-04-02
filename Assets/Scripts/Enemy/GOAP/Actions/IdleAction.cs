using System.Collections.Generic;
using UnityEngine;

public class IdleAction : GoapAction
{
    
    public override void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //base.StartAction(currentWorldState);
    }

    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //base.UpdateAction(currentWorldState);
    }

    public override bool IsCompleted(WorldState currentWorldState)
    {
        return true;
        //return base.IsCompleted(currentWorldState, activeActionDesiredState);
    }

    public override bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        /*return (currentWorldState._worldStateValues[EWorldState.TargetSwingSpeed] > 50f
            && currentWorldState._worldStateValues2[EWorldState.TargetDistance] == WorldStateValue.OutOfRange);
       */
        return false;
    }

}


