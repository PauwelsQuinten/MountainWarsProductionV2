using UnityEngine;

public class InteruptOnLowStamina : GoapInterupter
{
    [SerializeField, Tooltip("from below this stamina amount, the npc will interupt this Goal")]
    [Range(0f,1f)] private float _staminaTriggerPoint = 0.3f;
    public override bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_timeOut) return false;

        if (blackboard.variable.Stamina < _staminaTriggerPoint)
        {
            if (!SetInvallid)
                _ = StartTimeOut();
            return true;
        }
        return false;
    }
}
