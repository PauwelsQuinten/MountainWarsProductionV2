using System;

namespace UnityEngine
{
    public class WalkingEventArgs : EventArgs
    {
        public Vector2 WalkDirection;
        public Orientation Orientation;
        public bool IsLockon = false;
    }


}
