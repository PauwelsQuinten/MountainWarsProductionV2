using System;

namespace UnityEngine
{
    public class StaminaEventArgs : EventArgs
    {
        public float CurrentStamina;
        public float MaxStamina;
        public float StaminaCost;
    }
}
