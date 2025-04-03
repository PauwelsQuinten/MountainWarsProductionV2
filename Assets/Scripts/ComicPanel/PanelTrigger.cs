using UnityEngine;

public class PanelTrigger : MonoBehaviour
{
    [SerializeField]
    private GameEvent _triggerUpdate;
    [SerializeField]
    private bool _hidingSpot;

    [Header("Indexes")]
    [SerializeField]
    private int _nextViewIndex;
    [SerializeField]
    private int _nextSceneIndex;

    private void OnTriggerEnter(Collider other)
    {
        _triggerUpdate.Raise(this, new TriggerUpdatedEventArgs { NewViewIndex = _nextViewIndex, NewSceneIndex = _nextSceneIndex, ExitedTrigger = false, IsHidingSpot = _hidingSpot });
    }

    private void OnTriggerExit(Collider other)
    {
        _triggerUpdate.Raise(this, new TriggerUpdatedEventArgs { NewViewIndex = _nextViewIndex, NewSceneIndex = _nextSceneIndex, ExitedTrigger = true, IsHidingSpot = _hidingSpot });
    }
}
