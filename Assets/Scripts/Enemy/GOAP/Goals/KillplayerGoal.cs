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
        return 0.7f;
    }

    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        //Interupt this goal when his survival is at stake
        //Dissarm him if he uses a move to much
        return (
            blackboard.variable.IsBleeding                                  //Patch up
            || blackboard.variable.Stamina < 0.3f                           //Rest up
            || blackboard.variable.IsPlayerAgressive                        //Get distance
            || (blackboard.variable.ObservedAttack == blackboard.variable.TargetCurrentAttack 
                && blackboard.variable.ObservedAttack != AttackType.None
                && currentWorldState.AttackRange == EWorldStateRange.InRange)//Disarm
            || currentWorldState.Behaviour == EBehaviourValue.Knock);//No attacks when you got hit

    }
}
