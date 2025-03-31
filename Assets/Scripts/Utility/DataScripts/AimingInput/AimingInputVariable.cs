using System;
using UnityEngine;

[CreateAssetMenu(fileName = "AimingInputVariable", menuName = "DataScripts / AimingInput Variable")]
public class AimingInputVariable : ScriptableObject
{
    public event EventHandler<AimInputEventArgs> ValueChanged;
    public StateManager StateManager;

    [SerializeField]
    private Vector2 _value;
    [SerializeField]
    private AttackState _state;

    public Vector2 value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                ValueChanged?.Invoke(this, new AimInputEventArgs { ThisChanged = AimInputEventArgs.WhatChanged.Input });
            }
        }
    }

    public AttackState State
    {
        get => _state;
        set
        {
            if (_state != value)
            {
                _state = value;
                ValueChanged?.Invoke(this, new AimInputEventArgs { ThisChanged = AimInputEventArgs.WhatChanged.State });
            }
        }
    }
}