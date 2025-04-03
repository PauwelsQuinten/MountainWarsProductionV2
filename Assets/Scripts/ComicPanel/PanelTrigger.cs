using UnityEngine;
using UnityEngine.UI;

public class PanelTrigger : MonoBehaviour
{
    [SerializeField]
    private GameEvent _triggerEnter;
    [SerializeField]
    private GameEvent _triggerExit;
    [SerializeField]
    private bool _hidingSpot;
    [SerializeField]
    private LayerMask _layerMask;

    [Header("Indexes")]
    [SerializeField]
    private int _nextViewIndex;
    [SerializeField]
    private int _nextSceneIndex;
    [SerializeField]
    private int _currentSceneIndex;
    [SerializeField]
    private int _currentViewIndex;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerMask) != 0)
            _triggerEnter.Raise(this, new TriggerEnterEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, IsHidingSpot = _hidingSpot, IsShowDown = false, NewViewIndex = _nextViewIndex });
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerMask) != 0)
            _triggerExit.Raise(this, new TriggerExitEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, NewViewIndex = _nextViewIndex });
    }
}
