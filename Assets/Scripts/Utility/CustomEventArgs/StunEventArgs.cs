using System;

namespace UnityEngine
{
    public class StunEventArgs : EventArgs
    {
        public float StunDuration;
        public bool ComesFromEnemy;

        public GameObject Attacker;
        public GameObject Defender;
    }
}
