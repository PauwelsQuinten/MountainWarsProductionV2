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
            RHEquipmentPossesion,//set in EquipmentManager
            LHEquipmenPossesion,//set in EquipmentManager
            WeaponRange,//set in EquipmentManager
            ShieldState,

            TargetBehaviour,// Set refrence when found -> check on update
            Target,//Set in statemanager
            TargetStamina,//Set in staminaManager
            TargetHealth,//Set in healthManager
            TargetRHEquipment,//set in EquipmentManager
            TargetLHEquipment,//set in EquipmentManager
            TargetWeaponRange,//set in EquipmentManager -> not in use. checked on update
            TargetShieldState,
            TargetCurrentAttack,//Set in Attacking script
            TargetObservedAttack, //Calculated in Blackboard when target attacks
            TargetOpening
        }

    }
}
