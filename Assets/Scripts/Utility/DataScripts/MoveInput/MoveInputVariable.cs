using System;
using UnityEngine;

[CreateAssetMenu(fileName = "MovingInputVariable", menuName = "DataScripts / MovingInput Variable")]
public class MovingInputVariable : ScriptableObject
{
    public event EventHandler<EventArgs> ValueChanged;
    public StateManager StateManager;
    public float SpeedMultiplier;

    [SerializeField]
    private Vector2 _value;

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
}