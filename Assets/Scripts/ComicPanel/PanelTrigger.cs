using UnityEngine;

public class PanelTrigger : MonoBehaviour
{
    [SerializeField]
    private GameEvent _triggerUpdate;
    [SerializeField]
    private int _nextIndex;
    [SerializeField]
    private bool _hidingSpot;

    private void OnTriggerEnter(Collider other)
    {
        _triggerUpdate.Raise(this, new TriggerUpdatedEventArgs { NewPanelIndex = _nextIndex, ExitedTrigger = false, IsHidingSpot = _hidingSpot });
    }

    private void OnTriggerExit(Collider other)
    {
        _triggerUpdate.Raise(this, new TriggerUpdatedEventArgs { NewPanelIndex = _nextIndex, ExitedTrigger = true, IsHidingSpot = _hidingSpot });
    }
}
