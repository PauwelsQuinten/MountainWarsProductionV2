using System.Collections.Generic;
using UnityEngine;

public class GoapPlanner : MonoBehaviour
{
    private BlackboardReference _blackboard;
    [Header("CharacterStyle")]
    [SerializeField] private CharacterMentality _characterMentality = CharacterMentality.Basic;
    [Range(0, 10)]
    public int Perception = 8;
    [SerializeField] private List<GoapAction> _allActionPrefabs;
    [SerializeField] private List<GoapGoal> _allGoalPrefabs;
    [SerializeField] private List<GoapInterupter> _allInteruptPrefabs;
    private List<GoapAction> _allActions = new List<GoapAction>();
    private List<GoapGoal> _allGoals = new List<GoapGoal>();
    private List<GoapInterupter> _allInterupters = new List<GoapInterupter>();

    private WorldState _currentWorldState;
    private GoapGoal _activeGoal;
    private GoapAction _activeAction;
    private List<GoapAction> _actionPlan = new List<GoapAction>();
    private List<EWorldState> _comparedWorldState = new List<EWorldState>();

    private int _recursionCounter = 0;
    private void Awake()
    {
        _blackboard = GetComponent<StateManager>().BlackboardRef;
    }
    void Start()
    {
        _blackboard.variable.ResetAtStart();
        _currentWorldState = gameObject.AddComponent<WorldState>();
        _currentWorldState.WorldStateType = WorldStateType.Current;
        _blackboard.variable.SetPerception(Perception);
        _currentWorldState.AsignBlackboard(_blackboard);
        _currentWorldState.Init();

        foreach (var action in _allActionPrefabs)
        {
            if (action != null)
                _allActions.Add(Instantiate(action, gameObject.transform));
        }
        foreach (var goal in _allGoalPrefabs)
        {
            if (goal != null)
                _allGoals.Add(Instantiate(goal, gameObject.transform));
        }
        foreach (var interupter in _allInteruptPrefabs)
        {
            if (interupter != null)
                _allInterupters.Add(Instantiate(interupter, gameObject.transform));
        }
    }

    void Update()
    {
        _currentWorldState.UpdateWorldState();
        _recursionCounter = 0;

        //Check if the current goal has to be interupted through decision of itself or by an Interupter 
        if (_activeGoal && (_activeGoal.InteruptGoal(_currentWorldState, _blackboard) || CheckGoalWithInterupters()))
            ResetPlan(true);

        //Select a new Goal when previous one ended or got interupted
        if (_activeGoal == null || _actionPlan.Count == 0)
            _activeGoal = SelectCurrentGoal();

        //Do nothing when no vallid new goal is found.
        if (_activeGoal == null || _activeGoal.DesiredWorldState == null)
            return;

        //Create a plan for the new Goal, set goal invallid when this fails or start to execute.
        if (_actionPlan.Count == 0 && !Plan(_activeGoal.DesiredWorldState))
            _activeGoal.SetInvallid();
        else
            ExecutePlan();

    }

    private GoapGoal SelectCurrentGoal()
    {
        float highstScore = 0;
        GoapGoal bestGoal = null;

        foreach (var goal in _allGoals)
        {
            if (!goal.IsVallid(_currentWorldState, _blackboard))
                continue;

            float score = goal.GoalScore(_characterMentality, _currentWorldState, _blackboard);
            if (score > highstScore)
            {
                highstScore = score;
                bestGoal = goal;
            }
        }
      
        return bestGoal;
    }

    private void ExecutePlan()
    {
        //Update current Action or start a new on from the Plan
        if (_activeAction)
            _activeAction.UpdateAction(_currentWorldState, _blackboard);
        else if (_actionPlan.Count > 0)
        {
            _activeAction = _actionPlan[_actionPlan.Count - 1];
            _activeAction.StartAction(_currentWorldState, _blackboard);
        }
        else
            return;

        //When Active Action is completed we remove it from the Plan
        if (_activeAction.IsCompleted(_currentWorldState))
        {
            _actionPlan.RemoveAt(_actionPlan.Count - 1);
            _activeAction = null;
            return;
        }

        //Check if the active Action is interupting itself
        if (_activeAction.IsInterupted(_currentWorldState, _blackboard))
        {
            //_activeAction.ActionCompleted();
            ResetPlan(false);
        }

    }

    private bool Plan(WorldState desiredWorldState)
    {
        _comparedWorldState = _currentWorldState.CompareWorldState(desiredWorldState);

        foreach (EWorldState desiredState in _comparedWorldState)
        {
            float lowestScore = 9000f;
            GoapAction cheapestAction = null;

            foreach (var action in _allActions)
            {
                if (!action.IsVallid(_currentWorldState, _blackboard))
                    continue;

                //Compare Values
                if (action.SatisfyingWorldState.WorldStateValues.ContainsKey(desiredState) &&
                    action.SatisfyingWorldState.WorldStateValues[desiredState] == desiredWorldState.WorldStateValues[desiredState])
                {
                    float score = action.CalculateCost(_blackboard , _currentWorldState) /*+ action.DesiredWorldState._worldStateValues.Count + action.DesiredWorldState._worldStateValues2.Count*/;
                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        cheapestAction = action;
                    }
                }

                //Compare Behaviour
                else if (action.SatisfyingWorldState.WorldStateBehaviours.ContainsKey(desiredState) &&
                   action.SatisfyingWorldState.WorldStateBehaviours[desiredState] == desiredWorldState.WorldStateBehaviours[desiredState])
                {
                    float score = action.CalculateCost(_blackboard, _currentWorldState) /*+ action.DesiredWorldState._worldStateValues.Count + action.DesiredWorldState._worldStateValues2.Count*/;
                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        cheapestAction = action;
                    }
                }

                //Compare Possesions
                else if (action.SatisfyingWorldState.WorldStatePossesions.ContainsKey(desiredState) &&
                   action.SatisfyingWorldState.WorldStatePossesions[desiredState] == desiredWorldState.WorldStatePossesions[desiredState])
                {
                    float score = action.CalculateCost(_blackboard, _currentWorldState) /*+ action.DesiredWorldState._worldStateValues.Count + action.DesiredWorldState._worldStateValues2.Count*/;
                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        cheapestAction = action;
                    }
                }

                //Compare Range
                else if (action.SatisfyingWorldState.WorldStateRanges.ContainsKey(desiredState) &&
                   action.SatisfyingWorldState.WorldStateRanges[desiredState] == desiredWorldState.WorldStateRanges[desiredState])
                {
                    float score = action.CalculateCost(_blackboard , _currentWorldState) /*+ action.DesiredWorldState._worldStateValues.Count + action.DesiredWorldState._worldStateValues2.Count*/;
                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        cheapestAction = action;
                    }
                }

                //Compare Shield state
                else if (action.SatisfyingWorldState.WorldStateShields.ContainsKey(desiredState) &&
                   action.SatisfyingWorldState.WorldStateShields[desiredState] == desiredWorldState.WorldStateShields[desiredState])
                {
                    float score = action.CalculateCost(_blackboard, _currentWorldState) /*+ action.DesiredWorldState._worldStateValues.Count + action.DesiredWorldState._worldStateValues2.Count*/;
                    if (score < lowestScore)
                    {
                        lowestScore = score;
                        cheapestAction = action;
                    }
                }
            }

            if (cheapestAction == null)
                return false;

            //Remove previous inserted action, it is pointless to be called more times then once in the plan. so only call them with the highest priority
            if (_actionPlan.Contains(cheapestAction))
            {
                ++_recursionCounter;
                _actionPlan.Remove(cheapestAction);
            }


            _actionPlan.Add(cheapestAction);
            if (!Plan(cheapestAction.DesiredWorldState))
                return false;

            _comparedWorldState = _currentWorldState.CompareWorldState(desiredWorldState);//reset here back to startvalue before recursion
        }
        return true;
    }

    private void ResetPlan(bool setGoalInvallid)
    {
        if (_activeAction)
            _activeAction.ActionCompleted();
        _activeAction = null;
        if (_activeGoal && setGoalInvallid)
            _activeGoal.SetInvallid();
        _activeGoal = null;
        _actionPlan.Clear();
    }

    private bool CheckGoalWithInterupters()
    {
        foreach (var interupter in _allInterupters)
        {
            if (interupter== null || interupter.GoalToInterupt.GetType() == null)
                return false;
            if (interupter.GoalToInterupt.GetType() == _activeGoal.GetType() && interupter.InteruptCurrentGoal(_currentWorldState, _blackboard))
                return true;
        }
        return false;
    }

}