using UnityEngine;

public class StayAliveGoal : GoapGoal
{
    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.IsVallid(currentWorldState, blackboard);
    }

    public override float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        //NOTE TO SELF : this is probalbly not necessary since the attack goal will be cancelled if one off these is concidered necessary
        //Find way to score this higher then for instance the Patrol goal without being specific like this
        if (blackboard.variable.IsBleeding)
            return 1f;
        if (blackboard.variable.Stamina < 0.3f)
            return 0.8f;
        if (blackboard.variable.IsPlayerAgressive)
            return 0.8f;        
        if (blackboard.variable.ObservedAttack == blackboard.variable.CurrentAttack && blackboard.variable.ObservedAttack != AttackType.None )
            return 0.9f;        

        return _defaultScore;
    }
    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return base.InteruptGoal(currentWorldState, blackboard);
    }
}
