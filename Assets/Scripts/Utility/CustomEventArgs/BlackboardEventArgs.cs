using System;

namespace UnityEngine
{
    public class BlackboardEventArgs : EventArgs
    {
        
        public WhatChanged ThisChanged;
        public enum WhatChanged
        {
            Behaviour,//Set refrence at start -> check on update
            Stamina,//Set in staminaManager
            Health,//Set in healthManager
            RHEquipment,//set in EquipmentManager
            LHEquipment,//set in EquipmentManager
            WeaponRange,//set in EquipmentManager
            CurrentAttack,//Set in Attacking script
            ShieldState,

            TargetStamina,
            TargetBehaviour,// Set refrence when found -> check on update
            Target,//Set in statemanager
            TargetBlackboard,//Set in statemanager
            TargetHealth,
            TargetRHEquipment,
            TargetLHEquipment,
            TargetShieldState,

            TargetObservedAttack, //Calculated in Blackboard when target attacks
            TargetOpening
        }

    }
}
