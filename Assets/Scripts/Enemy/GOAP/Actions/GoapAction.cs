using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public interface IActions
{
    void StartAction(WorldState currentWorldState, BlackboardReference blackboard);
    void UpdateAction(WorldState currentWorldState, BlackboardReference blackboard);
    bool IsVallid(WorldState currentWorldState, BlackboardReference blackboard);
    bool IsCompleted(WorldState current);
    bool IsInterupted(WorldState currentWorldState, BlackboardReference blackboard);
    void CancelAction();
    float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState);
}

public class GoapAction : MonoBehaviour, IActions
{
    [HideInInspector]
    public WorldState DesiredWorldState;
    [HideInInspector]
    public WorldState SatisfyingWorldState;
    public float Cost = 1.0f;
    [SerializeField, Tooltip("Maximum time this action will run, when negative it can run forever or until interupted by other sources")] 
    protected float _actionMaxRunTime = 3f;
    [SerializeField, Tooltip("when put to true, his max action runtime wil be randomised between the set value and 10% of it instead of the set value")]
    private bool _randomWalkingTime = false;

    protected bool _isActivated = false;
    protected Coroutine _actionCoroutine;
    protected GameObject npc;

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
        if (_actionMaxRunTime >= 0)
            _actionCoroutine = StartCoroutine(StartTimer(_actionMaxRunTime));

        npc = transform.parent.gameObject;


        if (_randomWalkingTime)
            _actionMaxRunTime = Random.Range(_actionMaxRunTime * 0.1f, _actionMaxRunTime);

        
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
        if (_actionCoroutine != null)
            StopCoroutine(_actionCoroutine);
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
        if (_actionCoroutine != null)
            StopCoroutine(_actionCoroutine);
        _isActivated = false;
        return true;
    }

    virtual public float CalculateCost(BlackboardReference blackboard, WorldState currentWorldState)
    {
        return Cost;
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

    protected static bool SeesParryableAttack(BlackboardReference blackboard, WorldState currentWorldState)
    {
        return blackboard.variable.ObservedAttack == blackboard.variable.TargetBlackboard.variable.CurrentAttack
                    && blackboard.variable.ObservedAttack != AttackType.None
                    && currentWorldState.TargetAttackRange == EWorldStateRange.InRange;
    }


}

