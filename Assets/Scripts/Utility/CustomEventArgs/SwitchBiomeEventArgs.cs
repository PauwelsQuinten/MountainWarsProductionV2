using System;

namespace UnityEngine
{
    public class SwitchBiomeEventArgs : EventArgs
    {
        public Biome NextBiome;
        public bool IsEnter;
    }
}
