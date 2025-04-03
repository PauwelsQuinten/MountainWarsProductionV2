using System;

namespace UnityEngine
{
    public class TriggerExitEventArgs : EventArgs
    {
        public int newSceneIndex;
        public int CurrentSceneIndex;
        public int NewViewIndex;
        public int CurrentViewIndex;
    }
}
