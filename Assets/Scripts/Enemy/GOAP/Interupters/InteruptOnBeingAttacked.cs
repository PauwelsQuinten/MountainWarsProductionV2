using UnityEngine;

public class InteruptOnBeingAttacked : GoapInterupter
{ 
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_timeOut) return false;

        if (blackboard.variable.TargetBlackboard == null ) return false;

        if ( blackboard.variable.TargetBlackboard.variable.CurrentAttack != AttackType.None
                && currentWorldState.AttackRange == EWorldStateRange.InRange)
        {
            if (!SetInvallid)
                _= StartTimeOut();
            return true;
        }
        return false;
    }
}
