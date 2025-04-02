using System;

namespace UnityEngine
{
    public class TriggerUpdatedEventArgs : EventArgs
    {
        public int NewPanelIndex;
        public bool ExitedTrigger;
        public bool IsHidingSpot;
    }
}
