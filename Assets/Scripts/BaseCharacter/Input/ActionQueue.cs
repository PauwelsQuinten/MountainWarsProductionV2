using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;
using static UnityEngine.Rendering.ReloadAttribute;

public class ActionQueue : MonoBehaviour
{
    private class TimedPackage
    {
        public TimedPackage(AimingOutputArgs args, float time)
        {
            Package = args;
            TimeOfInsterted = time;
        }
        public AimingOutputArgs Package;
        public float TimeOfInsterted = 0f;
    }

            
    private Queue<TimedPackage> _inputQueue = new Queue<TimedPackage>();
    [SerializeField] private GameEvent _activateAction;
    [SerializeField] private int _maxQueueSize = 5;
    [SerializeField] private float _maxTimeInQueue = 2f;
    private StateManager _stateManager;
    private int _actionCount = 0;

    private void Update()
    {
        if (_stateManager == null)
            _stateManager = GetComponent<StateManager>();

        if (_stateManager && !_stateManager.InAnimiation && _inputQueue.Count > 0)
        {
            if (_inputQueue.Peek().Package.AnimationStart && _actionCount > 0)
                _actionCount--;
            var package = _inputQueue.Dequeue();
            //Debug.Log($"feint = {package.Package.IsFeint}");
            _activateAction.Raise(this, package.Package);
        }

        //Call next element imediatly if its about the feint signal, this is to continue the attack
        if (_inputQueue.Count > 0 && !_inputQueue.Peek().Package.IsFeint)
        {
            //Debug.Log("Conti ue attack");
            _activateAction.Raise(this, _inputQueue.Dequeue().Package);
        }
        if (IsOldestElementInQueueToLong(_maxTimeInQueue))
        {
            if (_inputQueue.Peek().Package.AnimationStart && _actionCount > 0)
                _actionCount--;
            _inputQueue.Dequeue();
        }
    }
    public void SendAction(Component sender, object obj)
    {
        if (sender.gameObject != gameObject) return;
        var args = (AimingOutputArgs)obj;
        if (args == null) return;
        if (_actionCount < _maxQueueSize)
        {
            if (args.AnimationStart) _actionCount++;
            _inputQueue.Enqueue(new TimedPackage(args, Time.time));
        }
    }
    public void ClearQueue(Component sender, object obj)
    {
        //Event from Sheating sword
        if (obj is bool && sender.gameObject == gameObject)
        {
            _inputQueue.Clear();
            _actionCount = 0;
            return;
        }

        //Event from Stun
        StunEventArgs args = obj as StunEventArgs;
        if (args == null) return;
        if (args.StunTarget != gameObject) return;

        _inputQueue.Clear();
        _actionCount = 0;

    }

    private bool IsOldestElementInQueueToLong(float maxTime)
    {
        if (_inputQueue == null || _inputQueue.Count == 0) return false;
        var element = _inputQueue.Peek();
        return Time.time - element.TimeOfInsterted > maxTime;
    }


}
