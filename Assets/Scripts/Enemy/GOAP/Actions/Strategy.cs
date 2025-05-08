using UnityEngine;

public class Strategy : GoapAction
{
    //This action is only for collection the correct actions in the right order but nothing by itself.
    public override void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        ActionCompleted();
    }
}
