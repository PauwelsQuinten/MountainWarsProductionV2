using System;
using System.Threading.Tasks;
using UnityEngine;

public class VillagerReaction : GoapAction
{
    [Header("State")]
    [SerializeField] private float _pushPower = 10f;

    [Header("Events")]
    [SerializeField] private GameEvent _changeAnim;
    [SerializeField] private GameEvent _changeRotation;
    [SerializeField] private GameEvent _moveOffset;
    [SerializeField] private GameEvent _changeTarget;
    private bool _isMoving = false;
    public override async void  StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        base.StartAction(currentWorldState, blackboard);
        if (_isMoving) return;

        _isMoving = true;
        _ = Reaction(blackboard);

    }

    public override float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        if (currentWorldState.HasTarget == EWorldStatePossesion.InPossesion)
            return 0.1f;
        else
            return Cost;

    }

    async Task Reaction(BlackboardReference blackboard)
    {
        try
        {
            //Add some force
            _moveOffset.Raise(npc.transform, new AttackVillager
            { Attacker = blackboard.variable.Target, Defender = npc, BlockPower = 0.1f, AttackPower = _pushPower });

            //Play animation
            _changeAnim.Raise(npc.transform, new AnimationEventArgs
            { AnimState = AnimationState.Angry, IsFullBodyAnim = true, AnimLayer = 1 });
            await Task.Delay(100);

            //Turn npc towards the target

            float fOrienation = Geometry.Geometry.CalculatefOrientationToTarget(blackboard.variable.Target.transform.position, npc.transform.position);
            Orientation orientation = Geometry.Geometry.FindOrientationFromAngle(fOrienation);
            _changeRotation.Raise(npc.transform, new OrientationEventArgs
            { NewFOrientation = fOrienation, NewOrientation = orientation });
            while(npc.GetComponent<StateManager>().InAnimiation)
            {
                await Task.Yield();
            }

            //Target eliminated
            _changeTarget.Raise(npc.transform, new NewTargetEventArgs { NewTarget = null });

            _isMoving = false;
            ActionCompleted();
        }

        catch (Exception ex)
   {
            Debug.LogError($"Error in RunReactionSequence: {ex}");
            ActionCompleted();
        }

    }
}
