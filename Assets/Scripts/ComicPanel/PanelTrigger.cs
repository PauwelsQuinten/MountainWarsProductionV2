using UnityEngine;
using UnityEngine.UI;

public class PanelTrigger : MonoBehaviour
{
    [Header("Events")]
    [SerializeField]
    private GameEvent _triggerEnter;
    [SerializeField]
    private GameEvent _triggerExit;
    [SerializeField]
    private GameEvent _biomeSwitch;

    [Header("Stats")]
    [SerializeField]
    private bool _hidingSpot;
    [SerializeField] 
    private bool _isTeleportTrigger;
    [SerializeField]
    private LayerMask _layerMask;
    [SerializeField]
    private Biome _nextBiome;

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
        {
            if (_isTeleportTrigger) 
                _biomeSwitch.Raise(this, new SwitchBiomeEventArgs { NextBiome = _nextBiome, IsEnter = true });
            _triggerEnter.Raise(this, new TriggerEnterEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, IsHidingSpot = _hidingSpot, IsShowDown = false, NewViewIndex = _nextViewIndex });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerMask) != 0)
        {
            if (_isTeleportTrigger)
                _biomeSwitch.Raise(this, new SwitchBiomeEventArgs { NextBiome = _nextBiome, IsEnter = true });
            _triggerExit.Raise(this, new TriggerExitEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, NewViewIndex = _nextViewIndex, IsTeleportTrigger = _isTeleportTrigger });
        }
    }
}
