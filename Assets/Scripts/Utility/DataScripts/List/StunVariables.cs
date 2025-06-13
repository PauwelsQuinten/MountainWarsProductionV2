using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "StunVariables", menuName = "DataScripts / StunDuration Variable")]
public class StunVariables : ScriptableObject
{
    public float StunOnHit;
    public float StunWhenGettingFullyBlocked;
    public float StunWhenGettingPartiallyBlocked;
    public float StunWhenGettingFullyBlockedBySword;
    public float StunWhenGettingPartiallyBlockedBySword;
    public float StunOnShieldDraggedDown;

}