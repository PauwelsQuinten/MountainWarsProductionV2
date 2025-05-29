using UnityEngine;

public class InteruptOnBleeding : GoapInterupter
{
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return blackboard.variable.IsBleeding;
    }
}
