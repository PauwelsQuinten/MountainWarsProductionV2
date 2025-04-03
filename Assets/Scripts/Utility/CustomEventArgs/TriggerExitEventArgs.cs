using System;

namespace UnityEngine
{
    public class TriggerExitEventArgs : EventArgs
    {
        public int NewSpawnIndex;
        public int CurrentIndex;
        public int NewViewIndex;
        public bool ExitedTrigger;
        public bool IsHidingSpot;
        public bool DoShowdown;
    }
}
