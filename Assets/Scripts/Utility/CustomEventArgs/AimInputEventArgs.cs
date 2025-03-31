using System;
namespace UnityEngine 
{
    public class AimInputEventArgs : EventArgs
    {
        public WhatChanged ThisChanged;
        public enum WhatChanged
        {
            Input,
            State
        }
    }

}


