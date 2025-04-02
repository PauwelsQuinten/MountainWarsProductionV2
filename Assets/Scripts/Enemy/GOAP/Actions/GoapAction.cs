using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public interface IActions
{
    void StartAction(WorldState currentWorldState, BlackboardReference blackboard);
    void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard);
    bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard);
    bool IsCompleted(WorldState current);
    bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard);
    void CancelAction();
}

public class GoapAction : MonoBehaviour, IActions
{
    [HideInInspector]
    public WorldState DesiredWorldState;
    [HideInInspector]
    public WorldState SatisfyingWorldState;
    public float Cost = 1.0f;
    [SerializeField] protected float _actionMaxRunTime = 3f;
    protected bool _isActivated = false;
    protected Coroutine _actionCoroutine;

    virtual protected void Start()
    {
        WorldState[] states = GetComponents<WorldState>();
        foreach (var item in states)
        {
            if (item.WorldStateType == WorldStateType.Desired)
                DesiredWorldState = item;
            else
                SatisfyingWorldState = item;
        }
    }
    public virtual void StartAction(WorldState currentWorldState, BlackboardReference blackboard)
    {
        if (_isActivated)
            return;
        _isActivated = true;
        _actionCoroutine = StartCoroutine(StartTimer(_actionMaxRunTime));
        //Debug.Log("Start action coroutine");
    }


    public virtual void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard)
    {

    }

    virtual public bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return true;
    }

    public virtual bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return false;
    }

    public virtual void CancelAction()
    {

    }


    virtual public bool IsCompleted(WorldState currentWorldState)
    {
        //set to complete if runtime runs out, done by coroutine started at startAction()
        //set to complete by UpdateAction()
        if (!_isActivated)
        {
            return true;
        }

        if (SatisfyingWorldState.WorldStateValues.Count != 0)
        {
            foreach (KeyValuePair<EWorldState, EWorldStateValue> updatingState in SatisfyingWorldState.WorldStateValues)
            {
                if (updatingState.Value - currentWorldState.WorldStateValues[updatingState.Key] != 0)
                    return false;
            }
        }
       
        if (SatisfyingWorldState.WorldStateBehaviours.Count != 0)
        {
            foreach (KeyValuePair<EWorldState, EBehaviourValue> updatingState in SatisfyingWorldState.WorldStateBehaviours)
            {
                if (updatingState.Value - currentWorldState.WorldStateBehaviours[updatingState.Key] != 0)
                    return false;
            }
        }
       
        if (SatisfyingWorldState.WorldStatePossesions.Count != 0)
        {
            foreach (KeyValuePair<EWorldState, EWorldStatePossesion> updatingState in SatisfyingWorldState.WorldStatePossesions)
            {
                if (updatingState.Value - currentWorldState.WorldStatePossesions[updatingState.Key] != 0)
                    return false;
            }
        }
       
        if (SatisfyingWorldState.WorldStateRanges.Count != 0)
        {
            foreach (KeyValuePair<EWorldState, EWorldStateRange> updatingState in SatisfyingWorldState.WorldStateRanges)
            {
                if (updatingState.Value - currentWorldState.WorldStateRanges[updatingState.Key] != 0)
                    return false;
            }
        }
       
        if (SatisfyingWorldState.WorldStateShields.Count != 0)
        {
            foreach (KeyValuePair<EWorldState, Direction> updatingState in SatisfyingWorldState.WorldStateShields)
            {
                if (updatingState.Value - currentWorldState.WorldStateShields[updatingState.Key] != 0)
                    return false;
            }
        }
       
        //Action finished
        StopCoroutine(_actionCoroutine);
        _isActivated = false;
        return true;
    }

    protected IEnumerator StartTimer(float runTime)
    {
        yield return new WaitForSeconds(runTime);
        _isActivated = false;

    }

    public void ActionCompleted()
    {
        _isActivated = false;
        if (_actionCoroutine != null)
            StopCoroutine(_actionCoroutine);
        CancelAction();
    }



    //HELP FUNCTIONS
    //-----------------------------------------------------------------------------------
    protected bool AboutToBeHit(WorldState currentWorldState, BlackboardReference blackboard)
    {
        return currentWorldState.WorldStateRanges[EWorldState.TargetAttackRange] == EWorldStateRange.InRange;
    }

}

