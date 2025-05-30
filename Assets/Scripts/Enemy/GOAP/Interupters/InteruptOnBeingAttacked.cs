using UnityEngine;

public class InteruptOnBeingAttacked : GoapInterupter
{ 
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return ( blackboard.variable.TargetBlackboard.variable.CurrentAttack != AttackType.None
                && currentWorldState.AttackRange == EWorldStateRange.InRange);
    }
}
