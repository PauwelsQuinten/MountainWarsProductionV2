using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class HealthEventArgs : EventArgs
    {
        public float CurrentHealth;
        public float MaxHealth;

        public Dictionary<BodyParts, float> BodyPartsHealth;
        public Dictionary<BodyParts, float> MaxBodyPartsHealth;

        public List<BodyParts> DamagedBodyParts = new List<BodyParts>();
    }
}
