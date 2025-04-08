using System;

namespace UnityEngine
{
    public class AnimationEventArgs : EventArgs
    {
        public AnimationState AnimState;
        public bool DoResetIdle;
        public bool Interupt;
        public int AnimLayer;
        public float Speed = 1f;
    }
}
