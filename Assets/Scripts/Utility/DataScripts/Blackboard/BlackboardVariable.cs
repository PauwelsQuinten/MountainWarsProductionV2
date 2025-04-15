using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BlackboardVariable", menuName = "DataScripts / Blackboard Variable")]
public class BlackboardVariable : ScriptableObject
{
    public event EventHandler<BlackboardEventArgs> ValueChanged;
    public void ResetAtStart()
    {
        _state = AttackState.Idle;
        //_stamina = 0f;
        //_health = 0f;
        _isBleeding = false;
        //_rHEquipmentHealth = 0f;
        //_lHEquipmentHealth = 0f;
        _self = null;
        _orientation = 0f;
        _target = null;
        _targetState = AttackState.Idle;
        //_targetStamina = 0f;
        //_targetHealth = 0f;
        _targetIsBleeding = false;
        //_targetRHEquipmentHealth = 0f;
        //_targetLHEquipmentHealth = 0f;
        //_targetWeaponRange = 0f;
        //_weaponRange = 0f;
        _storredAttacks = new Dictionary<AttackType, int>
        {
            { AttackType.Stab, 0 }, { AttackType.HorizontalSlashToRight, 0 }, { AttackType.HorizontalSlashToLeft, 0 }
        };
        _observedAttack = AttackType.None;
        _targetCurrentAttack = AttackType.None;
        _targetShieldState = Direction.Idle;
        _shieldState = Direction.Idle;
        _isPlayerAgressive = false;
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
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.Target });
            }
        }
    }

    private AttackState _targetState;
    public AttackState TargetState
    {
        get => _targetState;
        set
        {
            if (_targetState != value)
            {
                _targetState = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetBehaviour });
                TargetOpening = FindOpening();
            }
        }
    }

    private float _targetStamina;
    public float TargetStamina
    {
        get => _targetStamina;
        set
        {
            if (_targetStamina != value)
            {
                _targetStamina = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetStamina });
            }
        }
    }

    private float _targetHealth;
    public float TargetHealth
    {
        get => _targetHealth;
        set
        {
            if (_targetHealth != value)
            {
                _targetHealth = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetHealth });
            }
        }
    }
    
    private bool _targetIsBleeding;
    public bool TargetIsBleeding
    {
        get => _targetIsBleeding;
        set
        {
            if (_targetIsBleeding != value)
            {
                _targetIsBleeding = value;
            }
        }
    }

    private float _targetRHEquipmentHealth;
    public float TargetRHEquipmentHealth
    {
        get => _targetRHEquipmentHealth;
        set
        {
            if (_targetRHEquipmentHealth != value)
            {
                _targetRHEquipmentHealth = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetRHEquipment });
            }
        }
    }

    private float _targetLHEquipmentHealth;
    public float TargetLHEquipmentHealth
    {
        get => _targetLHEquipmentHealth;
        set
        {
            if (_targetLHEquipmentHealth != value)
            {
                _targetLHEquipmentHealth = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetLHEquipment });
            }
        }
    }

    private bool _hasRHEquipment;
    public bool HasRHEquipment
    {
        get => _hasRHEquipment;
        set
        {
            if (_hasRHEquipment != value)
            {
                _hasRHEquipment = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.RHEquipmentPossesion });
            }
        }
    }

    private bool _hasLHEquipment;
    public bool HasLHEquipment
    {
        get => _hasLHEquipment;
        set
        {
            if (_hasLHEquipment != value)
            {
                _hasLHEquipment = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.LHEquipmenPossesion });
            }
        }
    }

    private float _targetWeaponRange;
    public float TargetWeaponRange
    {
        get => _targetWeaponRange;
        set
        {
            if (_targetWeaponRange != value)
            {
                _targetWeaponRange = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetWeaponRange });
            }
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
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.WeaponRange });
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
    private AttackType EvaluateAttackCount()
    {
        int lowestCount = 10;
        int highestCount = 0;
        AttackType attackType = AttackType.None;
        foreach(KeyValuePair<AttackType, int> att in StorredAttacks)
        {
            if (att.Value < lowestCount) 
                lowestCount = att.Value;
            if (att.Value > highestCount)
            {
                highestCount = att.Value;
                attackType = att.Key;
            }
        }
        if (lowestCount > 0)
        {
            foreach (var key in StorredAttacks.Keys.ToList())
            {
                StorredAttacks[key] -= lowestCount;
            }
            highestCount -= lowestCount;
        }
        else if (highestCount > 5)
        {
            foreach (var key in StorredAttacks.Keys.ToList())
            {
                StorredAttacks[key] -= StorredAttacks[key] > 0? 1 : 0;
            }
            highestCount -= 1;
        }
        return highestCount >= 5? attackType : AttackType.None;
    }

    private AttackType _targetCurrentAttack;
    public AttackType TargetCurrentAttack
    {
        get => _targetCurrentAttack;
        set
        {
            if (_targetCurrentAttack == AttackType.None && _targetCurrentAttack == value) return;
            if ( Target )
            {
                _targetCurrentAttack = value;
                //Debug.Log($"new currentAttack{_targetCurrentAttack}");
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetCurrentAttack });

                if (StorredAttacks.ContainsKey(value))
                {
                    StorredAttacks[_targetCurrentAttack] += 1;
                    _observedAttack = EvaluateAttackCount();
                    ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetObservedAttack });

                }
            }
        }
    }
    public void ResetCurrentAttack()
    {
        _targetCurrentAttack = AttackType.None;
    }


    private Direction _targetShieldState;
    public Direction TargetShieldState
    {
        get => _targetShieldState;
        set
        {
            if (_targetShieldState != value)
            {
                _targetShieldState = value;
                ValueChanged?.Invoke(this, new BlackboardEventArgs { ThisChanged = BlackboardEventArgs.WhatChanged.TargetShieldState });
                TargetOpening = FindOpening();
            }
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

    private Opening FindOpening()
    {
        Direction direction = Direction.Idle;
        Size size = Size.Small;

        if (TargetState == AttackState.Stun)
        {
            size = Size.Large;
            direction = Direction.ToLeft;
        }
        else if (TargetShieldState == Direction.Idle)
        {
            size = Size.Medium;
            direction = Direction.ToCenter;
        }
        else if (TargetShieldState != Direction.ToCenter)
        {
            size = Size.Medium;
            if (TargetShieldState != Direction.ToLeft)
                direction = Direction.ToRight;
            else if (TargetShieldState != Direction.ToRight)
                direction = Direction.ToLeft;
        }
        else if (TargetShieldState == Direction.ToCenter)
        {
            size = Size.Small;
            direction = Direction.ToRight;
        }
        else
        {
            size = Size.None;
            direction = Direction.Idle;
        }


        return new Opening(direction, size);
    }

}