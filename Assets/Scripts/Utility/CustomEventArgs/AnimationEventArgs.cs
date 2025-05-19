using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class AnimationEventArgs : EventArgs
    {
        public AnimationState AnimState;
        public bool IsAttackHigh = false;
        public bool DoResetIdle;
        public bool IsFeint = true;
        public float Speed = 1f;
        public List<int> AnimLayer = new List<int>();
        public Direction BlockDirection = Direction.Default;
        public BlockMedium BlockMedium = BlockMedium.Shield;
        public BlockMedium AttackMedium = BlockMedium.Sword;
    }
}
