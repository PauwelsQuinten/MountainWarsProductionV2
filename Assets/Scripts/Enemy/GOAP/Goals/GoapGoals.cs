using System.Collections;
using UnityEngine;

public interface IGoals
{
    bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard);
    void SetInvallid();
    float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard);
    bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard);
}

public class GoapGoal : MonoBehaviour, IGoals
{
    public WorldState DesiredWorldState;
    protected bool _isVallid = true;
    protected Coroutine _goalCoroutine;
    [SerializeField] protected float _invalidTime = 2f;

    private void Start()
    {
        DesiredWorldState = GetComponent<WorldState>();
    }

    public virtual bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {

        return _isVallid;
    }

    public virtual float GoalScore(CharacterMentality menatlity, WorldState currentWorldState, BlackboardReference blackboard)
    {
        return 0.75f;
    }

    public virtual bool InteruptGoal(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return false;
    }
    public void SetInvallid()
    {
        _isVallid = false;
        _goalCoroutine = StartCoroutine(ResetGoalValidation(_invalidTime));
    }

    protected IEnumerator ResetGoalValidation(float time)
    {
        yield return new WaitForSeconds(time);
        _isVallid = true;
    }
}
