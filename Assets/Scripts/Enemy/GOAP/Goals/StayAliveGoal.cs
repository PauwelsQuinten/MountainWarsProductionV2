using UnityEngine;

public class StayAliveGoal : GoapGoal
{
    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.IsVallid(currentWorldState, blackboard);
    }

    public override float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (blackboard.variable.IsBleeding)
            return 1f;
        if (blackboard.variable.Stamina < 0.3f)
            return 0.8f;
       /* if (currentWorldState._isPlayerToAggressive)
            return 0.8f;
        if (!currentWorldState.IsBlockInCorrectDirection())
            return 0.9f;*/

        return 0.5f;
    }
    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.InteruptGoal(currentWorldState, blackboard);
    }
}
