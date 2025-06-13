using UnityEngine;


public class KillPlayerGoal : GoapGoal
{
    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return _isVallid 
            && currentWorldState.Behaviour != EBehaviourValue.Knock
            && currentWorldState.WorldStatePossesions.ContainsKey(EWorldState.HasTarget)
            && currentWorldState.WorldStatePossesions[EWorldState.HasTarget] == EWorldStatePossesion.InPossesion;
    }

    public override float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        return _defaultScore;
    }

    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return (currentWorldState.Behaviour == EBehaviourValue.Knock || currentWorldState.HasTarget == EWorldStatePossesion.NotInPossesion);

    }
}
