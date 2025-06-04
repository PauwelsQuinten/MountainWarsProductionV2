using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class AnimationEventArgs : EventArgs
    {
        public AnimationState AnimState;
        public bool IsAttackHigh = false;
        public bool IsFeint = true;
        public bool IsFullBodyAnim = false;
        public float Speed = 1f;
        public int AnimLayer;
        public Direction BlockDirection = Direction.Default;
        public BlockMedium BlockMedium = BlockMedium.Shield;
        public bool AttackWithLeftHand = false;
    }
}
