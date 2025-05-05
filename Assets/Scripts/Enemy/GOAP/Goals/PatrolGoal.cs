using UnityEngine;

public class PatrolGoal : GoapGoal
{
    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.IsVallid(currentWorldState, blackboard);
    }

    public override float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        
        return _defaultScore;
    }
    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.InteruptGoal(currentWorldState, blackboard);
    }
}
