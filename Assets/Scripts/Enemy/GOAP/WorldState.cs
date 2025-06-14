using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField]
    private EWorldStateValue _movementSpeed = EWorldStateValue.Default;

    [Header("Possesion")]
    [SerializeField]
    private EWorldStatePossesion _hasTarget = EWorldStatePossesion.Default;
    [SerializeField]
    private EWorldStatePossesion _hasOpening = EWorldStatePossesion.Default;
    [SerializeField]
    private EWorldStatePossesion _hasRHEquipment = EWorldStatePossesion.Default;
    [SerializeField]
    private EWorldStatePossesion _hasLHEquipment = EWorldStatePossesion.Default;
    
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
    }

    public void Init()
    {
        FillLists();
        
        _blackboard.variable.ValueChanged += Blackboard_ValueChanged;
        SetStartValues();      
    }


    public void UpdateWorldState()
    {
        if (WorldStateType != WorldStateType.Current)
            return;

        UpdateBehaviour();

        if (HasTarget == EWorldStatePossesion.InPossesion)
        {
            CalculateRange();
        }
        
    }

    public List<EWorldState> CompareWorldState(WorldState desiredWorldState)
    {
        List<EWorldState> listOfDifference = new List<EWorldState>();

        if (desiredWorldState.PriorityList != null && desiredWorldState.PriorityList.Count > 0)
        {
            foreach (var item in desiredWorldState.PriorityList)
            {
                if (WorldStateValues.ContainsKey(item) && WorldStateValues[item] - desiredWorldState.WorldStateValues[item] != 0)
                {
                    listOfDifference.Add(item);
                }
                else if (WorldStatePossesions.ContainsKey(item) && WorldStatePossesions[item] - desiredWorldState.WorldStatePossesions[item] != 0)
                {
                    listOfDifference.Add(item);
                }
                else if (WorldStateBehaviours.ContainsKey(item) && WorldStateBehaviours[item] - desiredWorldState.WorldStateBehaviours[item] != 0)
                {
                    listOfDifference.Add(item);
                }
                else if (WorldStateShields.ContainsKey(item) && WorldStateShields[item] - desiredWorldState.WorldStateShields[item] != 0)
                {
                    listOfDifference.Add(item);
                }
                else if (WorldStateRanges.ContainsKey(item) && WorldStateRanges[item] - desiredWorldState.WorldStateRanges[item] != 0)
                {
                    listOfDifference.Add(item);
                }

            }
            return listOfDifference;
        }

        return CheckOverAllLists(desiredWorldState, listOfDifference);
    }

    private List<EWorldState> CheckOverAllLists(WorldState desiredWorldState, List<EWorldState> listOfDifference)
    {

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

        //Check Shield orientation
        foreach (KeyValuePair<EWorldState, Direction> worldState in desiredWorldState.WorldStateShields)
        {
            if (worldState.Value - WorldStateShields[worldState.Key] != 0 && worldState.Value != Direction.Idle)
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
            case BlackboardEventArgs.WhatChanged.Stamina:
                Stamina = CalculateValue(_blackboard.variable.Stamina);
                break;
            case BlackboardEventArgs.WhatChanged.Health:
                Health = CalculateValue(_blackboard.variable.Health);
                break;
            case BlackboardEventArgs.WhatChanged.RHEquipment:
                RHEquipment = CalculateValue(_blackboard.variable.RHEquipmentHealth);
                HasRHEquipment = _blackboard.variable.RHEquipmentHealth <= 0f? 
                    EWorldStatePossesion.InPossesion : EWorldStatePossesion.NotInPossesion ;
                break;
            case BlackboardEventArgs.WhatChanged.LHEquipment:
                LHEquipment = CalculateValue(_blackboard.variable.LHEquipmentHealth);
                HasLHEquipment = _blackboard.variable.LHEquipmentHealth <= 0f ? 
                    EWorldStatePossesion.InPossesion : EWorldStatePossesion.NotInPossesion;
                break;
            case BlackboardEventArgs.WhatChanged.ShieldState:
                ShieldState = _blackboard.variable.ShieldState;
                break;


            case BlackboardEventArgs.WhatChanged.TargetBlackboard:
                HasTarget = SetInPossesion(_blackboard.variable.Target);
                if (HasTarget == EWorldStatePossesion.InPossesion)
                    SetTargetValues();
                else
                    ResetTargetValues();
                break;
            case BlackboardEventArgs.WhatChanged.TargetBehaviour:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetBehaviour = WatchBehaviour(_blackboard.variable.TargetBlackboard.variable.State);
                break;
            case BlackboardEventArgs.WhatChanged.TargetStamina:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetStamina = CalculateValue(_blackboard.variable.TargetBlackboard.variable.Stamina);
                break;
            case BlackboardEventArgs.WhatChanged.TargetHealth:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetHealth = CalculateValue(_blackboard.variable.TargetBlackboard.variable.Health);
                break;
            case BlackboardEventArgs.WhatChanged.TargetRHEquipment:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetRHEquipment = CalculateValue(_blackboard.variable.TargetBlackboard.variable.RHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.TargetLHEquipment:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetLHEquipment = CalculateValue(_blackboard.variable.TargetBlackboard.variable.LHEquipmentHealth);
                break;
            case BlackboardEventArgs.WhatChanged.TargetShieldState:
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    TargetShieldState = _blackboard.variable.TargetBlackboard.variable.ShieldState;
                break;
           case BlackboardEventArgs.WhatChanged.TargetOpening:
                //Debug.Log($"{_blackboard.variable.TargetOpening.OpeningDirection}, {_blackboard.variable.TargetOpening.OpeningSize}");
                if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
                    HasOpening = _blackboard.variable.TargetOpening.OpeningSize != Size.Small || _blackboard.variable.TargetOpening.OpeningSize != Size.None ? 
                        EWorldStatePossesion.InPossesion : EWorldStatePossesion.NotInPossesion;
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
        HasRHEquipment = RHEquipment != EWorldStateValue.Zero ? EWorldStatePossesion.InPossesion : EWorldStatePossesion.NotInPossesion;
        LHEquipment = CalculateValue(_blackboard.variable.LHEquipmentHealth);
        HasLHEquipment = LHEquipment != EWorldStateValue.Zero ? EWorldStatePossesion.InPossesion : EWorldStatePossesion.NotInPossesion;

    }

    private void SetTargetValues()
    {
        TargetStamina = CalculateValue(_blackboard.variable.TargetBlackboard.variable.Stamina);
        TargetHealth = CalculateValue(_blackboard.variable.TargetBlackboard.variable.Health);
        TargetRHEquipment = CalculateValue(_blackboard.variable.TargetBlackboard.variable.RHEquipmentHealth);
        TargetLHEquipment = CalculateValue(_blackboard.variable.TargetBlackboard.variable.LHEquipmentHealth);

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

    private void UpdateBehaviour()
    {
        if (_blackboard.variable.TargetBlackboard != null && _blackboard.variable.TargetBlackboard.variable.Self == null)
            _blackboard.variable.TargetBlackboard = null;

        Behaviour = WatchBehaviour(_blackboard.variable.State);

        if (HasTarget == EWorldStatePossesion.InPossesion && _blackboard.variable.TargetBlackboard != null)
        {
            TargetBehaviour = WatchBehaviour(_blackboard.variable.TargetBlackboard.variable.State);
        }
        else
            TargetBehaviour = EBehaviourValue.Default;

        CcalculateSpeed();

    }

    private void CalculateRange()
    {
        if (_blackboard.variable.TargetBlackboard == null) return;

        float weaponRange = _blackboard.variable.WeaponRange;
        float targetWeaponRange = _blackboard.variable.TargetBlackboard.variable.WeaponRange;
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
            if (!WorldStateValues.ContainsKey(EWorldState.TargetHealth))
                WorldStateValues.Add(EWorldState.TargetHealth, TargetHealth);
            if (!WorldStateValues.ContainsKey(EWorldState.TargetStamina))
                WorldStateValues.Add(EWorldState.TargetStamina, TargetStamina);
            if (!WorldStateValues.ContainsKey(EWorldState.TargetRHEquipment))
                WorldStateValues.Add(EWorldState.TargetRHEquipment, TargetRHEquipment);
            if (!WorldStateValues.ContainsKey(EWorldState.TargetLHEquipment))
                WorldStateValues.Add(EWorldState.TargetLHEquipment, TargetLHEquipment);
            if (!WorldStateValues.ContainsKey(EWorldState.TargetLHEquipment))
                WorldStateValues.Add(EWorldState.TargetLHEquipment, TargetLHEquipment);

            if (!WorldStateValues.ContainsKey(EWorldState.Health))
                WorldStateValues.Add(EWorldState.Health, Health);
            if (!WorldStateValues.ContainsKey(EWorldState.Stamina))
                WorldStateValues.Add(EWorldState.Stamina, Stamina);
            if (!WorldStateValues.ContainsKey(EWorldState.RHEquipment))
                WorldStateValues.Add(EWorldState.RHEquipment, RHEquipment);
            if (!WorldStateValues.ContainsKey(EWorldState.MovementSpeed))
                WorldStateValues.Add(EWorldState.MovementSpeed, MovementSpeed);


            //Possesions
            if (!WorldStatePossesions.ContainsKey(EWorldState.HasTarget))
                WorldStatePossesions.Add(EWorldState.HasTarget, HasTarget);
            if (!WorldStatePossesions.ContainsKey(EWorldState.TargetOpening))
                WorldStatePossesions.Add(EWorldState.TargetOpening, HasOpening);
             if (!WorldStatePossesions.ContainsKey(EWorldState.HasRHEquipment))
                WorldStatePossesions.Add(EWorldState.HasRHEquipment, HasRHEquipment);
            if (!WorldStatePossesions.ContainsKey(EWorldState.HasLHEquipment))
                WorldStatePossesions.Add(EWorldState.HasLHEquipment, HasLHEquipment);


            //Behaviours
            if (!WorldStateBehaviours.ContainsKey(EWorldState.Behaviour))
                WorldStateBehaviours.Add(EWorldState.Behaviour, Behaviour);
            if (!WorldStateBehaviours.ContainsKey(EWorldState.TargetBehaviour))
                WorldStateBehaviours.Add(EWorldState.TargetBehaviour, TargetBehaviour);


            //Ranges
            if (!WorldStateRanges.ContainsKey(EWorldState.AttackRange))
                WorldStateRanges.Add(EWorldState.AttackRange, AttackRange);
            if (!WorldStateRanges.ContainsKey(EWorldState.TargetAttackRange))
                WorldStateRanges.Add(EWorldState.TargetAttackRange, TargetAttackRange);

            //Shields
            if (!WorldStateShields.ContainsKey(EWorldState.ShieldState))
                WorldStateShields.Add(EWorldState.ShieldState, ShieldState);
            if (!WorldStateShields.ContainsKey(EWorldState.TargetShieldState))
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
            if (MovementSpeed != EWorldStateValue.Default)
                WorldStateValues.Add(EWorldState.MovementSpeed, MovementSpeed);

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

    private void CcalculateSpeed()
    {
        var comp = GetComponent<AnimationManager>();
        if (!comp) return;
        float speed = comp.Velocity.magnitude;
        if (speed > 1f) MovementSpeed = EWorldStateValue.High;
        if (speed > 0.9f) MovementSpeed = EWorldStateValue.Mid;
        if (speed == 0f) MovementSpeed = EWorldStateValue.Zero;
        else MovementSpeed = EWorldStateValue.Low;
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
    
    public EWorldStateValue MovementSpeed
    {
        get { return _movementSpeed; }
        set
        {
            _movementSpeed = value;
            WorldStateValues[EWorldState.MovementSpeed] = _movementSpeed;
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
    
    public EWorldStatePossesion HasRHEquipment
    {
        get { return _hasRHEquipment; }
        set
        {
            _hasRHEquipment = value;
            WorldStatePossesions[EWorldState.HasRHEquipment] = _hasRHEquipment;
        }
    }

    public EWorldStatePossesion HasLHEquipment
    {
        get { return _hasLHEquipment; }
        set
        {
            _hasLHEquipment = value;
            WorldStatePossesions[EWorldState.HasLHEquipment] = _hasLHEquipment;
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
