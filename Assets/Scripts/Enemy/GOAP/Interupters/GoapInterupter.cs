using UnityEngine;
public interface IInterupter
{
    bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard);
}

public class GoapInterupter : MonoBehaviour, IInterupter
{
    public GoapGoal GoalToInterupt;
    public virtual bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return false; 
    }

}
