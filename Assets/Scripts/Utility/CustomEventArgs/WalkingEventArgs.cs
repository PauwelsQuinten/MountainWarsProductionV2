using System;

namespace UnityEngine
{
    public class WalkingEventArgs : EventArgs
    {
        public Vector2 WalkDirection;
        public float Orientation;
        public float Speed;
        public bool IsLockon = false;
    }


}
