using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[CreateAssetMenu(fileName = "BlackboardVariable", menuName = "DataScripts / Blackboard Variable")]
public class BlackboardVariable : ScriptableObject
{
    public event EventHandler<BlackboardEventArgs> ValueChanged;
    int _numOfAttacksSeen = 5;
    public int Perception = 5;
    public void ResetAtStart()
    {
        _state = AttackState.Idle;
        _isBleeding = false;
        _self = null;
        _orientation = 0f;
        _target = null;
        _targetBlackboard = null;
        _storredAttacks = new Dictionary<AttackType, int>
        {
            { AttackType.Stab, 0 }, { AttackType.HorizontalSlashToRight, 0 }, { AttackType.HorizontalSlashToLeft, 0 }
        };
        _observedAttack = AttackType.None;
        _currentAttack = AttackType.None;
        _shieldState = Direction.Idle;
        _isPlayerAgressive = false;
        _target = null;
        _targetBlackboard = null;
    }

    public void SetPerception(int perception)
    {
        Perception = perception;

        if (perception < 3)
            _numOfAttacksSeen = 7;
        else if (perception < 6)
            _numOfAttacksSeen = 5;
         else if (perception < 9)
            _numOfAttacksSeen = 3;
        else if (perception == 10)
            _numOfAttacksSeen = 1;

    }

    private AttackState _state;
    public AttackState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.Behaviour });
            }
        }
    }

    private float _stamina;
    public float Stamina
    {
        get => _stamina;
        set
        {
            if (_stamina != value)
            {
                _stamina = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.Stamina});
                
            }
        }
    }

    private float _health;
    public float Health
    {
        get => _health;
        set
        {
                _health = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.Health });
            //if (_health != value)
            //{
            //}
        }
    }

    private bool _isBleeding;
    public bool IsBleeding
    {
        get => _isBleeding;
        set
        {
            if (_isBleeding != value)
            {
                _isBleeding = value;
            }
        }
    }

    private float _rHEquipmentHealth;
    public float RHEquipmentHealth
    {
        get => _rHEquipmentHealth;
        set
        {
            if (_rHEquipmentHealth != value)
            {
                _rHEquipmentHealth = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.RHEquipment });
            }
        }
    }

    private float _lHEquipmentHealth;
    public float LHEquipmentHealth
    {
        get => _lHEquipmentHealth;
        set
        {
            if (_lHEquipmentHealth != value)
            {
                _lHEquipmentHealth = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.LHEquipment });
            }
        }
    }

    private GameObject _self;
    public GameObject Self
    {
        get => _self;
        set
        {
            if (_self != value)
            {
                _self = value;
            }
        }
    }

    private Orientation _orientation = Orientation.East;
    public Orientation Orientation
    {
        get => _orientation;
        set
        {
            if (value != _orientation)
            {
                _orientation = value;
            }
        }
    }
    
    private GameObject _target;
    public GameObject Target
    {
        get => _target;
        set
        {
            if (_target != value)
            {
                _target = value;
                if (_target == null)
                    TargetBlackboard = null;
            }
        }
    }

    private BlackboardReference _targetBlackboard;
    public BlackboardReference TargetBlackboard
    {
        get => _targetBlackboard;
        set
        {
            if (_targetBlackboard != value && value != null)
            {
                _targetBlackboard = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetBlackboard });
                _targetBlackboard.variable.ValueChanged += Variable_ValueChanged;
            }
            else if (_targetBlackboard != value && value == null)
            {
                if (_targetBlackboard != null)
                    _targetBlackboard.variable.ValueChanged -= Variable_ValueChanged;
                _targetBlackboard = null;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetBlackboard });
            }
        }
    }

    private void Variable_ValueChanged(object sender, BlackboardEventArgs e)
    {
        switch (e.ThisChanged)
        {
            case BlackboardEventArgs.WhatChanged.Behaviour:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetBehaviour });
                break;
            case BlackboardEventArgs.WhatChanged.Stamina:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetStamina });
                break;
            case BlackboardEventArgs.WhatChanged.Health:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetHealth });
                break;
            case BlackboardEventArgs.WhatChanged.RHEquipment:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetRHEquipment });
                break;
            case BlackboardEventArgs.WhatChanged.LHEquipment:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetLHEquipment });
                break;           
            case BlackboardEventArgs.WhatChanged.CurrentAttack:
                AttackType value = _targetBlackboard.variable.CurrentAttack;
                if (StorredAttacks.ContainsKey(value))
                {
                    StorredAttacks[value] += 1;
                    _observedAttack = EvaluateAttackCount(value);
                }
                break;
            case BlackboardEventArgs.WhatChanged.ShieldState:
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetShieldState });
                break;
           
        }
    }

    private float _weaponRange;
    public float WeaponRange
    {
        get => _weaponRange;
        set
        {
            if (_weaponRange != value)
            {
                _weaponRange = value;
            }
        }
    }

    //Holds the attacks the opponent throw at him, this is used for as the opponent uses 1 move to much. it needs to be parried/Disarmed.
    private Dictionary<AttackType, int> _storredAttacks = 
        new Dictionary<AttackType, int> 
        { 
            { AttackType.Stab, 0 }, { AttackType.HorizontalSlashToRight, 0 }, { AttackType.HorizontalSlashToLeft, 0 }
        };
    public Dictionary<AttackType, int> StorredAttacks
    {
        get => _storredAttacks;
        set
        {
            if (_storredAttacks != value)
            {
                _storredAttacks = value;               
            }
        }
    }
    private AttackType _observedAttack;
    public AttackType ObservedAttack
    {
        get => _observedAttack;
    }
    private AttackType EvaluateAttackCount(AttackType addedAttack)
    {
        foreach (var key in StorredAttacks.Keys.ToList())
        {
            if (key != addedAttack)
                StorredAttacks[key] -= StorredAttacks[key] > 0 ? 1 : 0;
            else
                StorredAttacks[key] -= StorredAttacks[key] > _numOfAttacksSeen ? 1 : 0;
        }
                
        int highestCount = 0;
        AttackType attackType = AttackType.None;
        foreach(KeyValuePair<AttackType, int> att in StorredAttacks)
        {            
            if (att.Value > highestCount)
            {
                highestCount = att.Value;
                attackType = att.Key;
            }
        }
           
        return highestCount >= _numOfAttacksSeen ? attackType : AttackType.None;
    }

    private AttackType _currentAttack;
    public AttackType CurrentAttack
    {
        get => _currentAttack;
        set
        {
            if (_currentAttack != value)
            {
                _currentAttack = value;
                //Debug.Log($"new currentAttack{_currentAttack}");
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.CurrentAttack });
            }
        }
    }
    public void ResetCurrentAttack()
    {
        _currentAttack = AttackType.None;
    }

    private int _numOffAttacks = 0;
    public int NumOffAttacks
    {
        get => _numOffAttacks;
        set
        {
            if ( _numOffAttacks >= 5)
                IsPlayerAgressive = true;
            else
                IsPlayerAgressive = false;
        }
    }

    private Direction _shieldState;
    public Direction ShieldState
    {
        get => _shieldState;
        set
        {
            if (_shieldState != value)
            {
                _shieldState = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.ShieldState });
            }
        }
    }

    private bool _isPlayerAgressive = false;
    public bool IsPlayerAgressive
    {
        get => _isPlayerAgressive;
        set
        {
            if (_isPlayerAgressive != value)
            {
                _isPlayerAgressive = value;
            }
        }
    }

    private Opening _targetOpening;
    public Opening TargetOpening
    {
        get => _targetOpening;
        set
        {
            if (_targetOpening != value)
            {
                _targetOpening = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetOpening });

            }
        }
    }

    //private Opening FindOpening()
    //{
    //    Direction direction = Direction.Idle;
    //    Size size = Size.Small;
    //
    //    if (TargetState == AttackState.Stun)
    //    {
    //        size = Size.Large;
    //        direction = Direction.ToLeft;
    //    }
    //    else if (TargetShieldState == Direction.Idle)
    //    {
    //        size = Size.Medium;
    //        direction = Direction.ToCenter;
    //    }
    //    else if (TargetShieldState != Direction.ToCenter)
    //    {
    //        size = Size.Medium;
    //        if (TargetShieldState != Direction.ToLeft)
    //            direction = Direction.ToRight;
    //        else if (TargetShieldState != Direction.ToRight)
    //            direction = Direction.ToLeft;
    //    }
    //    else if (TargetShieldState == Direction.ToCenter)
    //    {
    //        size = Size.Small;
    //        direction = Direction.ToRight;
    //    }
    //    else
    //    {
    //        size = Size.None;
    //        direction = Direction.Idle;
    //    }
    //
    //
    //    return new Opening(direction, size);
    //}

}