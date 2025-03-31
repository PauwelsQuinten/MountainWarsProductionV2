using System;

namespace UnityEngine
{
    public class LoseEquipmentEventArgs : EventArgs
    {
        public EquipmentType EquipmentType;
        public bool ToSelf;
    }


}
