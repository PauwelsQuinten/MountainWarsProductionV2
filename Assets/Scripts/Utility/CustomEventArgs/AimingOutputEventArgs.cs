using System;
namespace UnityEngine
{
    public class AimingOutputArgs : EventArgs
    {
        public Direction Direction;
        public Direction BlockDirection;
        public AimingInputState AimingInputState;
        public AttackHeight AttackHeight;
        public float Speed = 0f;
        public float AngleTravelled = 0f;
        public AttackSignal AttackSignal;
        public AttackState AttackState;
        public EquipmentManager EquipmentManager;
        public bool IsHoldingBlock = false;
    }
    

}