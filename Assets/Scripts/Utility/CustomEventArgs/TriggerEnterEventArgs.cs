using System;

namespace UnityEngine
{
    public class TriggerEnterEventArgs : EventArgs
    {
        public int newSceneIndex;
        public int CurrentSceneIndex;
        public int NewViewIndex;
        public int CurrentViewIndex;

        public bool IsHidingSpot;
        public bool IsShowDown;
    }
}
