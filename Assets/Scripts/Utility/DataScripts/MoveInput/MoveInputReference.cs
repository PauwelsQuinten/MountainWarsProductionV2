using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class MovingInputReference 
{
    public bool useConstant;
    public Vector2 constantValue;
    public MovingInputVariable variable;

    public Vector2 Value
    {
        get
        {
            return useConstant ? constantValue :
                                 variable.value;
        }
    }

    public void SetValue(Vector2 value)
    {
        variable.value = value;
    }
}
