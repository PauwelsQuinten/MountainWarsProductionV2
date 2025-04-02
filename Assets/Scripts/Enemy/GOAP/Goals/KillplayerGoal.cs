using System.Collections.Generic;
using UnityEngine;


public class KillPlayerGoal : GoapGoal
{
    public override bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return _isVallid && currentWorldState.WorldStatePossesions[EWorldState.HasTarget] == EWorldStatePossesion.InPossesion;
    }

    public override float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        return 0.7f;
    }

    public override bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        /*bool parryMoveFound = false;
        foreach (KeyValuePair<AttackType, int> att in currentWorldState._attackCountList)
        {
            if (att.Value >= 5 && currentWorldState.TargetCurrentAttack == att.Key)
                parryMoveFound = true;
        }

        //return currentWorldState.IsBleeding || currentWorldState.Stamina < 0.3f || currentWorldState._isPlayerToAggressive || !currentWorldState.IsBlockInCorrectDirection();
        return (currentWorldState.IsBleeding || currentWorldState.Stamina < 0.3f
            || currentWorldState._isPlayerToAggressive || !currentWorldState.IsBlockInCorrectDirection()
            || parryMoveFound);*/

        return false;

    }
}
