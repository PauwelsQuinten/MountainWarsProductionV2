using System;

namespace UnityEngine
{
    public class AnimationEventArgs : EventArgs
    {
        public AnimationState AnimState;
        public bool DoResetIdle;
        public int AnimLayer;
    }
}
