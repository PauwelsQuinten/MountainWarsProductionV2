using System;

namespace UnityEngine
{
    public class DefenceEventArgs : EventArgs
    {
        public BlockResult BlockResult;
        public AttackHeight AttackHeight;
        public float AttackPower;
        public BlockMedium BlockMedium;
        public GameObject Attacker;
        public GameObject Defender;
    }


}
