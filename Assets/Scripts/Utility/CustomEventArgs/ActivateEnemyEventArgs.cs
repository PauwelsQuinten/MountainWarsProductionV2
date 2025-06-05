using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class ActivateEnemyEventArgs : EventArgs
    {
        public List<string> EnemyNames = new List<string>();
    }
}
