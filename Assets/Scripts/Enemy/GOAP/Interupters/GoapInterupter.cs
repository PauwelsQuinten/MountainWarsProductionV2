using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine;
public interface IInterupter
{
    bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard);
}

public class GoapInterupter : MonoBehaviour, IInterupter
{
    public GoapGoal GoalToInterupt;
    [Tooltip("if true, it will disable the goal for the goal his set disabled time, false he will only reset his plan")]
    public bool SetInvallid = true;
    [SerializeField, Tooltip("When SetInvallid is false, this Interupter will disable itself for this set amount of time")]
    protected float _timeOutTime = 1f;
    protected bool _timeOut = false;
    public virtual bool InteruptCurrentGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return false; 
    }

    protected async Task StartTimeOut()
    {
        _timeOut = true;
        await Task.Delay(Mathf.RoundToInt(_timeOutTime * 1000f));
        _timeOut = false;
    }
}
