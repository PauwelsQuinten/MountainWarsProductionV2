using System;

namespace UnityEngine
{
    public class LoseEquipmentEventArgs : EventArgs
    {
        public EquipmentType EquipmentType;
        public GameObject WhoLostIt;
        public GameObject ParryMaster;
    }


}
