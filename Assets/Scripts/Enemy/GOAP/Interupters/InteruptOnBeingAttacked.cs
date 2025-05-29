using UnityEngine;

public class InteruptOnBeingAttacked : GoapInterupter
{ 
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return ( blackboard.variable.ObservedAttack != AttackType.None
                && currentWorldState.AttackRange == EWorldStateRange.InRange);
    }
}
