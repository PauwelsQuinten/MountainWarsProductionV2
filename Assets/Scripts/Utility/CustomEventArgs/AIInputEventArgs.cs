using System;
namespace UnityEngine
{
    public class AIInputEventArgs : EventArgs
    {
        public AIInputAction Input;
        public GameObject Sender;
    }


}