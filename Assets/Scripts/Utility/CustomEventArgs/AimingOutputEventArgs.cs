using System;
namespace UnityEngine
{
    public class AimingOutputArgs : EventArgs
    {
        public Direction Direction;
        public Direction BlockDirection;
        public AimingInputState AimingInputState;
        public AttackHeight AttackHeight;
        public float Speed ;
        public float AngleTravelled;
        public AttackSignal AttackSignal;
        public AttackState AttackState;
        public EquipmentManager EquipmentManager;
        public bool IsHoldingBlock;
        public GameObject Sender;
        public bool AnimationStart = false;
        public bool IsFeint;
        public SpecialInput Special = SpecialInput.Default;
    }
    

}