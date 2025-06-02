using UnityEngine;

public class InteruptOnBleeding : GoapInterupter
{
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if ( _timeOut) return false;
        
        if (blackboard.variable.IsBleeding)
        {
            if (!SetInvallid)
                _ = StartTimeOut();
            return true;
        }
        return false;
    }
}
