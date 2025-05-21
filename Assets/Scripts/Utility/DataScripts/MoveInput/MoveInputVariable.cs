using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MovingInputVariable", menuName = "DataScripts / MovingInput Variable")]
public class MovingInputVariable : ScriptableObject
{
    public event EventHandler<EventArgs> ValueChanged;
    public StateManager StateManager;

    [SerializeField]
    private Vector2 _value;
    [SerializeField]
    private float _speedMultiplier = 1;

    public Vector2 value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
    public float SpeedMultiplier
    {
        get => _speedMultiplier;
        set
        {
            if (_speedMultiplier != value)
            {
                _speedMultiplier = value;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}