using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace UnityEngine 
{
    public class DamageEventArgs : EventArgs
    {
        public List<BodyParts> HitParts = new List<BodyParts>();
        public float AttackPower;
    }
}