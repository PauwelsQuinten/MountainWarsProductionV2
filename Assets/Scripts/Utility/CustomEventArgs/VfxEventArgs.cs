using System;
using UnityEngine;

public class VfxEventArgs : EventArgs
{
    public VfxType Type;
    public float Duration = 0f;
    public bool Cancel = false;
}
