using System;

namespace UnityEngine
{
    public class DirectionEventArgs : EventArgs
    {
        public Vector2 MoveDirection;
        public float SpeedMultiplier;
        public GameObject Sender;
    }


}
