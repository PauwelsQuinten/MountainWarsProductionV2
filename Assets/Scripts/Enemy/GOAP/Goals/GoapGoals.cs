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
    [HideInInspector]
    public WorldState DesiredWorldState;
    protected bool _isVallid = true;
    protected Coroutine _goalCoroutine;
    [SerializeField, Tooltip("The amount of time this goal will be disabled after it got interupted. This is the average time, depnding on his agresion lvl.")]
    protected float _invalidTime = 2f;
    [SerializeField, Tooltip("This score is used to determine which goal to select (higher the better), this score will be adjusted determined on the situation in their script")]
    protected float _defaultScore = 0.5f;

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


    public void DeterminationSetup(int determination)
    {
        _invalidTime *= ((10f - determination)*0.2f);
    }

    protected IEnumerator ResetGoalValidation(float time)
    {
        yield return new WaitForSeconds(time);
        _isVallid = true;
    }
}
