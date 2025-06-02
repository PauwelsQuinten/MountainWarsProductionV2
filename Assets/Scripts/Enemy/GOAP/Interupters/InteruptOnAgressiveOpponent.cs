using UnityEngine;

public class InteruptOnAgressiveOpponent : GoapInterupter
{
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_timeOut) return false;

        if (blackboard.variable.IsPlayerAgressive)
        {
            if (!SetInvallid)
                _= StartTimeOut();
            return true;
        }
        return false;
    }
}
