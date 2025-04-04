using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class WorldState : MonoBehaviour
{
    [Header("Type")]
    public WorldStateType WorldStateType = WorldStateType.Desired;
    //Low to high priority
    [SerializeField] private List<EWorldState> PriorityList = new List<EWorldState>();
    private BlackboardReference _blackboard;

    [Header("Values")]
    [SerializeField]
    private EWorldStateValue _targetHealth = EWorldStateValue.Default;
    [SerializeField]         
    private EWorldStateValue _targetStamina = EWorldStateValue.Default;
    [SerializeField]         
    private EWorldStateValue _targetRHEquipment = EWorldStateValue.Default;
    [SerializeField]         
    private EWorldStateValue _targetLHEquipment = EWorldStateValue.Default;
    [SerializeField]
    private EWorldStateValue _health = EWorldStateValue.Default;
    [SerializeField]
    private EWorldStateValue _stamina = EWorldStateValue.Default;
    [SerializeField]
    private EWorldStateValue _rHEquipment = EWorldStateValue.Default;
    [SerializeField]
    private EWorldStateValue _lHEquipment = EWorldStateValue.Default;

    [Header("Possesion")]
    [SerializeField]
    private EWorldStatePossesion _hasTarget = EWorldStatePossesion.Default;
    [SerializeField]
    private EWorldStatePossesion _hasOpening = EWorldStatePossesion.Default;
    
    [Header("Behaviour")]
    [SerializeField]
    private EBehaviourValue _targetBehaviour = EBehaviourValue.Default;
    [SerializeField]
    private EBehaviourValue _behaviour = EBehaviourValue.Default;
    
    [Header("Range")]
    [SerializeField]
    private EWorldStateRange _targetAttackRange = EWorldStateRange.Default;
    [SerializeField]
    private EWorldStateRange _attackRange = EWorldStateRange.Default;
    
    [Header("Shield")]
    [SerializeField]
    private Direction _targetShieldState = Direction.Idle;
    [SerializeField]
    private Direction _shieldState = Direction.Idle;
    
    public Dictionary<EWorldState, EWorldStateValue> WorldStateValues = new Dictionary<EWorldState, EWorldStateValue>();
    public Dictionary<EWorldState, EWorldStatePossesion> WorldStatePossesions = new Dictionary<EWorldState, EWorldStatePossesion>();
    public Dictionary<EWorldState, EBehaviourValue> WorldStateBehaviours = new Dictionary<EWorldState, EBehaviourValue>();
    public Dictionary<EWorldState, EWorldStateRange> WorldStateRanges = new Dictionary<EWorldState, EWorldStateRange>();
    public Dictionary<EWorldState, Direction> WorldStateShields = new Dictionary<EWorldState, Direction>();


    private void Start()
    {
        FillLists();

        if (WorldStateType == WorldStateType.Current)
        {
            _blackboard.variable.ValueChanged += Blackboard_ValueChanged;
            SetStartValues();
        }
    }


    public void UpdateWorldState()
    {
        if (WorldStateType != WorldStateType.Current)
            return;

        UpdateBehaviour();

        if (HasTarget == EWorldStatePossesion.InPossesion)
        {
            CalculateRange();
            LookForOpenings();
        }
        
    }

    public List<EWorldState> CompareWorldState(WorldState desiredWorldState)
    {
        List<EWorldState> listOfDifference = new List<EWorldState>();

        //Values
        foreach (KeyValuePair<EWorldState, EWorldStateValue> worldState in desiredWorldState.WorldStateValues)
        {
            if (worldState.Value - WorldStateValues[worldState.Key] != 0)
            {
                listOfDifference.Add(worldState.Key);
            }
        }

        //Check Possesion
        foreach (KeyValuePair<EWorldState, EWorldStatePossesion> worldState in desiredWorldState.WorldStatePossesions)
        {
            if (worldState.Value - WorldStatePossesions[worldState.Key] != 0 && worldState.Value != EWorldStatePossesion.Default)
            {
                listOfDifference.Add(worldState.Key);
            }
        }
        
        //Check Ranges
        foreach (KeyValuePair<EWorldState, EWorldStateRange> worldState in desiredWorldState.WorldStateRanges)
        {
            if (worldState.Value - WorldStateRanges[worldState.Key] != 0 && worldState.Value != EWorldStateRange.Default)
            {
                listOfDifference.Add(worldState.Key);
            }
        }
        
        //Check behaviour
        foreach (KeyValuePair<EWorldState, EBehaviourValue> worldState in desiredWorldState.WorldStateBehaviours)
        {
            if (worldState.Value - WorldStateBehaviours[worldState.Key] != 0 && worldState.Value != EBehaviourValue.Default)
            {
                listOfDifference.Add(worldState.Key);
            }
        }

        return listOfDifference;
    }


    //------------------------------------------------------------------------------
    //WORLDSTATE VALUES UPDATE FUNCTIONS
    //------------------------------------------------------------------------------

    public void AsignBlackboard(BlackboardReference blackboardReference)
    {
        _blackboard = blackboardReference;
    }

    private void Blackboard_ValueChanged(object sender, BlackboardEventArgs e)
    {
        switch (e.ThisChanged)
        {
            case BlackboardEventArgs.WhatChanged.Behaviour:
                Behaviour = WatchBehaviour(_blackboard.variable.State);
                break;
            case BlackboardEventArgs.WhatChanged.TargetBehaviour:
                TargetBehaviour = WatchBehaviour(_blackboard.variable.TargetState);
                break;
            case BlackboardEventArgs.WhatChanged.Stamina:
                Stamina = CalculateValue(_blackboard.variable.Stamina);
                break;
            case BlackboardEventArgs.WhatChanged.Health:
                Health = CalculateValue(_blackboard.variable.Health);
                break;
            case BlackboardEventArgs.WhatChanged.RHEquipment:
                RHEquipment = CalculateValue(_blackboard.variable.RHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.LHEquipment:
                LHEquipment = CalculateValue(_blackboard.variable.LHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.ShieldState:
                ShieldState = _blackboard.variable.ShieldState;
                break;
            case BlackboardEventArgs.WhatChanged.Target:
                HasTarget = SetInPossesion(_blackboard.variable.Target);
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    SetTargetValues();
                else
                    ResetTargetValues();
                break;
            case BlackboardEventArgs.WhatChanged.TargetStamina:
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    TargetStamina = CalculateValue(_blackboard.variable.TargetStamina);
                break;
            case BlackboardEventArgs.WhatChanged.TargetHealth:
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    TargetHealth = CalculateValue(_blackboard.variable.TargetHealth);
                break;
            case BlackboardEventArgs.WhatChanged.TargetRHEquipment:
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    TargetRHEquipment = CalculateValue(_blackboard.variable.TargetRHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.TargetLHEquipment:
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    TargetLHEquipment = CalculateValue(_blackboard.variable.TargetLHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.TargetShieldState:
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    TargetShieldState = _blackboard.variable.TargetShieldState;
                break;
           
        }
    }


    //------------------------------------------------------------------------------
    //HELPER FUNCTIONS
    //------------------------------------------------------------------------------


    private void SetStartValues()
    {
        Stamina = CalculateValue(_blackboard.variable.Stamina);
        Health = CalculateValue(_blackboard.variable.Health);
        RHEquipment = CalculateValue(_blackboard.variable.RHEquipmentHealth);
        LHEquipment = CalculateValue(_blackboard.variable.LHEquipmentHealth);

    }

    private void SetTargetValues()
    {
        TargetStamina = CalculateValue(_blackboard.variable.TargetStamina);
        TargetHealth = CalculateValue(_blackboard.variable.TargetHealth);
        TargetRHEquipment = CalculateValue(_blackboard.variable.TargetRHEquipmentHealth);
        TargetLHEquipment = CalculateValue(_blackboard.variable.TargetLHEquipmentHealth);

    }

    private void ResetTargetValues()
    {
        TargetStamina = EWorldStateValue.Default;
        TargetHealth = EWorldStateValue.Default;
        TargetRHEquipment = EWorldStateValue.Default;
        TargetLHEquipment = EWorldStateValue.Default;

    }


    private EWorldStateValue CalculateValue(float fValue)
    {
        if (fValue == 1f )
            return EWorldStateValue.Full;

        if (fValue >= 0.66f )
            return EWorldStateValue.High;

        if (fValue >= 0.33f )
            return EWorldStateValue.Mid;

        if (fValue < 0.33f && fValue > 0f)
            return EWorldStateValue.Low;

        if (fValue <= 0f )
            return EWorldStateValue.Zero;

        return EWorldStateValue.Default;
    }

    private EWorldStatePossesion SetInPossesion(GameObject target)
    {
        EWorldStatePossesion possesion;
        possesion = target == null ? EWorldStatePossesion.NotInPossesion : EWorldStatePossesion.InPossesion;
        return possesion;
    }

    private EBehaviourValue WatchBehaviour(AttackState attackState)
    {
        switch (attackState)
        {
            case AttackState.Idle:
                return EBehaviourValue.Idle;

            case AttackState.Attack:
                return EBehaviourValue.Attacking;

            case AttackState.ShieldDefence:
            case AttackState.SwordDefence:
            case AttackState.BlockAttack:
                return EBehaviourValue.Defending;
                
            case AttackState.Stun:
                return EBehaviourValue.Knock;
        }
        
        return EBehaviourValue.Default;
    }

    private void LookForOpenings()
    {
        HasOpening = EWorldStatePossesion.Default;
    }

    private void UpdateBehaviour()
    {
        Behaviour = WatchBehaviour(_blackboard.variable.State);

        if (HasTarget == EWorldStatePossesion.InPossesion)
        {
            TargetBehaviour = WatchBehaviour(_blackboard.variable.TargetState);
        }
        else
            TargetBehaviour = EBehaviourValue.Default;
    }

    private void CalculateRange()
    {
        float weaponRange = _blackboard.variable.WeaponRange;
        float targetWeaponRange = _blackboard.variable.TargetWeaponRange;
        float targetDistance = Vector3.Distance(gameObject.transform.position, _blackboard.variable.Target.transform.position);

        if (targetDistance < weaponRange)
            AttackRange = EWorldStateRange.InRange;
        else if (targetDistance > weaponRange && targetDistance < weaponRange * 3f)
            AttackRange = EWorldStateRange.OutOfRange;
        else
            AttackRange = EWorldStateRange.FarAway;

        if (targetDistance < targetWeaponRange)
            TargetAttackRange = EWorldStateRange.InRange;
        else if (targetDistance > targetWeaponRange && targetDistance < targetWeaponRange * 3f)
            TargetAttackRange = EWorldStateRange.OutOfRange;
        else
            TargetAttackRange = EWorldStateRange.FarAway;
    }

    private void FillLists()
    {
        if (WorldStateType == WorldStateType.Current)
        {
            //Values
            WorldStateValues.Add(EWorldState.TargetHealth, TargetHealth);
            WorldStateValues.Add(EWorldState.TargetStamina, TargetStamina);
            WorldStateValues.Add(EWorldState.TargetRHEquipment, TargetRHEquipment);
            WorldStateValues.Add(EWorldState.TargetLHEquipment, TargetLHEquipment);

            WorldStateValues.Add(EWorldState.Health, Health);
            WorldStateValues.Add(EWorldState.Stamina, Stamina);
            WorldStateValues.Add(EWorldState.RHEquipment, RHEquipment);
            WorldStateValues.Add(EWorldState.LHEquipment, LHEquipment);


            //Possesions
            WorldStatePossesions.Add(EWorldState.HasTarget, HasTarget);
            WorldStatePossesions.Add(EWorldState.TargetOpening, HasOpening);


            //Behaviours
            WorldStateBehaviours.Add(EWorldState.Behaviour, Behaviour);
            WorldStateBehaviours.Add(EWorldState.TargetBehaviour, TargetBehaviour);


            //Ranges
            WorldStateRanges.Add(EWorldState.AttackRange, AttackRange);
            WorldStateRanges.Add(EWorldState.TargetAttackRange, TargetAttackRange);

            //Shields
            WorldStateShields.Add(EWorldState.ShieldState, ShieldState);
            WorldStateShields.Add(EWorldState.TargetShieldState, TargetShieldState);

        }

        //If the Worldstate is not of Current Type, it is not used to compare all values but only the not default ones
        else
        {
            //Values
            if (TargetHealth != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.TargetHealth, TargetHealth);
            if (TargetStamina != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.TargetStamina, TargetStamina);
            if (TargetRHEquipment != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.TargetRHEquipment, TargetRHEquipment);
            if (TargetLHEquipment != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.TargetLHEquipment, TargetLHEquipment);

            if (Health != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.Health, Health);
            if (Stamina != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.Stamina, Stamina);
            if (RHEquipment != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.RHEquipment, RHEquipment);
            if (LHEquipment != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.LHEquipment, LHEquipment);


            //Possesions
            if (HasTarget != EWorldStatePossesion.Default)
                WorldStatePossesions.Add(EWorldState.HasTarget, HasTarget);
            if (HasOpening != EWorldStatePossesion.Default)
                WorldStatePossesions.Add(EWorldState.TargetOpening, HasOpening);


            //Behaviours
            if (Behaviour != EBehaviourValue.Default)
                WorldStateBehaviours.Add(EWorldState.Behaviour, Behaviour);
            if (TargetBehaviour != EBehaviourValue.Default)
                WorldStateBehaviours.Add(EWorldState.TargetBehaviour, TargetBehaviour);


            //Ranges
            if (AttackRange != EWorldStateRange.Default)
                WorldStateRanges.Add(EWorldState.AttackRange, AttackRange);
            if (TargetAttackRange != EWorldStateRange.Default)
                WorldStateRanges.Add(EWorldState.TargetAttackRange, TargetAttackRange);

            //Shields
            if (ShieldState != Direction.Idle)
                WorldStateShields.Add(EWorldState.ShieldState, ShieldState);
            if (TargetShieldState != Direction.Idle)
                WorldStateShields.Add(EWorldState.TargetShieldState, TargetShieldState);

        }

    }


    //------------------------------------------------------------------------------
    //WORLDSTATE PUBLIC SETTERS TO UPDATE LIST AT SAME TIME
    //------------------------------------------------------------------------------

    #region Setters

    public EWorldStateValue TargetHealth
    {
        get { return _targetHealth; }
        set
        {
            _targetHealth = value;
            WorldStateValues[EWorldState.TargetHealth] = _targetHealth;
        }
    }

    public EWorldStateValue TargetStamina
    {
        get { return _targetStamina; }
        set
        {
            _targetStamina = value;
            WorldStateValues[EWorldState.TargetStamina] = _targetStamina;
        }
    }

    public EWorldStateValue TargetRHEquipment
    {
        get { return _targetRHEquipment; }
        set
        {
            _targetRHEquipment = value;
            WorldStateValues[EWorldState.TargetRHEquipment] = _targetRHEquipment;
        }
    }

    public EWorldStateValue TargetLHEquipment
    {
        get { return _targetLHEquipment; }
        set
        {
            _targetLHEquipment = value;
            WorldStateValues[EWorldState.TargetLHEquipment] = _targetLHEquipment;
        }
    }

    public EWorldStateValue Health
    {
        get { return _health; }
        set
        {
            _health = value;
            WorldStateValues[EWorldState.Health] = _health;
        }
    }

    public EWorldStateValue Stamina
    {
        get { return _stamina; }
        set
        {
            _stamina = value;
            WorldStateValues[EWorldState.Stamina] = _stamina;
        }
    }

    public EWorldStateValue RHEquipment
    {
        get { return _rHEquipment; }
        set
        {
            _rHEquipment = value;
            WorldStateValues[EWorldState.RHEquipment] = _rHEquipment;
        }
    }

    public EWorldStateValue LHEquipment
    {
        get { return _lHEquipment; }
        set
        {
            _lHEquipment = value;
            WorldStateValues[EWorldState.LHEquipment] = _lHEquipment;
        }
    }


    public EWorldStatePossesion HasTarget
    {
        get { return _hasTarget; }
        set
        {
            _hasTarget = value;
            WorldStatePossesions[EWorldState.HasTarget] = _hasTarget;
        }
    }

    public EWorldStatePossesion HasOpening
    {
        get { return _hasOpening; }
        set
        {
            _hasOpening = value;
            WorldStatePossesions[EWorldState.TargetOpening] = _hasOpening;
        }
    }

    public EBehaviourValue TargetBehaviour
    {
        get { return _targetBehaviour; }
        set
        {
            _targetBehaviour = value;
            WorldStateBehaviours[EWorldState.TargetBehaviour] = _targetBehaviour;
        }
    }

    public EBehaviourValue Behaviour
    {
        get { return _behaviour; }
        set
        {
            _behaviour = value;
            WorldStateBehaviours[EWorldState.Behaviour] = _behaviour;
        }
    }

    public EWorldStateRange TargetAttackRange
    {
        get { return _targetAttackRange; }
        set
        {
            _targetAttackRange = value;
            WorldStateRanges[EWorldState.TargetAttackRange] = _targetAttackRange;
        }
    }
    
    public EWorldStateRange AttackRange
    {
        get { return _attackRange; }
        set
        {
            _attackRange = value;
            WorldStateRanges[EWorldState.AttackRange] = _attackRange;
        }
    }
    
    public Direction TargetShieldState
    {
        get { return _targetShieldState; }
        set
        {
            _targetShieldState = value;
            WorldStateShields[EWorldState.TargetShieldState] = _targetShieldState;
        }
    }
    
    public Direction ShieldState
    {
        get { return _shieldState; }
        set
        {
            _shieldState = value;
            WorldStateShields[EWorldState.ShieldState] = _shieldState;
        }
    }


    #endregion Setters

}
