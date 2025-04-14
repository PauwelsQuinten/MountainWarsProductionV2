using System.Collections.Generic;
using UnityEngine;

public class ActionQueue : MonoBehaviour
{
    private Queue<AimingOutputArgs> _inputQueue = new Queue<AimingOutputArgs>();
    [SerializeField] private GameEvent _activateAction;

    public void SendAction(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = (AimingOutputArgs)obj;
        if (args == null) return;

        //Is already action playing?
        //Interupt current attack action?
        //Is doubble action? stun or idle
        _inputQueue.Enqueue(args);

        _activateAction.Raise(this, _inputQueue.Dequeue());
    }

    public void OnStun(Component sender, object obj)
    {
        _inputQueue.Clear();
    }

}
