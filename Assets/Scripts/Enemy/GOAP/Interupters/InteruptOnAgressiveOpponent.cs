using UnityEngine;

public class InteruptOnAgressiveOpponent : GoapInterupter
{
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return blackboard.variable.IsPlayerAgressive;
    }
}
