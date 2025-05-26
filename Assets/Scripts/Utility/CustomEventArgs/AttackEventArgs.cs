using System;
namespace UnityEngine 
{
    public class AttackEventArgs : EventArgs
    {
        public AttackType AttackType;
        public AttackHeight AttackHeight;
        public float AttackPower;
        public float BlockPower;
        public GameObject Attacker;
        public GameObject Defender;
    }
}