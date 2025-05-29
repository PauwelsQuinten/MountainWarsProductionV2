using UnityEngine;

public class VillagerReaction : GoapAction
{
    [SerializeField] private GameEvent _changeAnim;
    [SerializeField] private GameEvent _changeRotation;
    public override void  StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);

        float fOrienation = Geometry.Geometry.CalculatefOrientationToTarget(blackboard.variable.Target.transform.position, npc.transform.position);
        Orientation orientation = Geometry.Geometry.FindOrientationFromAngle(fOrienation);
        _changeRotation.Raise(npc.transform, new OrientationEventArgs
        { NewFOrientation = fOrienation, NewOrientation = orientation });

        _changeAnim.Raise(npc.transform, new AnimationEventArgs 
        { AnimState = AnimationState.Angry, IsFullBodyAnim = true, DoResetIdle = true });
    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        if (currentWorldState.HasTarget == EWorldStatePossesion.InPossesion)
            return 0.1f;
        else
            return Cost;

    }

}
