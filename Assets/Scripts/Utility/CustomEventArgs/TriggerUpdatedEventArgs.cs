using System;

namespace UnityEngine
{
    public class TriggerUpdatedEventArgs : EventArgs
    {
        public int NewSceneIndex;
        public int NewViewIndex;
        public bool ExitedTrigger;
        public bool IsHidingSpot;
        public bool DoShowdown;
    }
}
