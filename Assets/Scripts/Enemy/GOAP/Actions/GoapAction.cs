using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


public interface IActions
{
    void StartAction(WorldState currentWorldState);
    void UpdateAction(WorldState currentWorldState);
    bool IsVallid(WorldState currentWorldState);
    bool IsCompleted(WorldState current, WorldState activeActionDesiredState);
    bool IsInterupted(WorldState currentWorldState);
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
    public virtual void StartAction(WorldState currentWorldState)
    {
        if (_isActivated)
            return;
        _isActivated = true;
        _actionCoroutine = StartCoroutine(StartTimer(_actionMaxRunTime));
        //Debug.Log("Start action coroutine");
    }


    public virtual void UpdateAction(WorldState currentWorldState)
    {

    }

    virtual public bool IsVallid(WorldState currentWorldState)
    {
        return true;
    }

    public virtual bool IsInterupted(WorldState currentWorldState)
    {
        return false;
    }

    public virtual void CancelAction()
    {

    }


    virtual public bool IsCompleted(WorldState currentWorldState, WorldState activeActionDesiredState)
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
                if (/*_comparedWorldState.ContainsKey(updatingState.Key) && */Mathf.Abs(updatingState.Value - currentWorldState.WorldStateValues[updatingState.Key]) != 0)
                    return false;
            }
        }
       
        //TODO : FILL IN FOR THE OTHER WORLDSTATE VALUES AS WELL

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
    //protected bool AboutToBeHit(WorldState currentWorldState)
    //{
    //    return !currentWorldState.IsBlockInCorrectDirection()
    //       && (currentWorldState._worldStateValues2[EWorldState.TargetAttackRange] == WorldStateValue.OutOfRange
    //       || currentWorldState._worldStateValues2[EWorldState.TargetAttackRange] == WorldStateValue.InRange);
    //}
    //
    //protected bool FamiliarAttack(WorldState currentWorldState)
    //{
    //    bool parryMoveFound = false;
    //    foreach (KeyValuePair<AttackType, int> att in currentWorldState._attackCountList)
    //    {
    //        if (att.Value >= 5 && currentWorldState.TargetCurrentAttack == att.Key)
    //            parryMoveFound = true;
    //    }
    //    return parryMoveFound && (currentWorldState._worldStateValues2[EWorldState.TargetAttackRange] == WorldStateValue.OutOfRange
    //       || currentWorldState._worldStateValues2[EWorldState.TargetAttackRange] == WorldStateValue.InRange);
    //}

}

