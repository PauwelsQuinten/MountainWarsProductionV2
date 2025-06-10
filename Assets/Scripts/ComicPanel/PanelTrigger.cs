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
    private bool _scaleTriggerOnEnter;
    [SerializeField]
    private bool _doRunTriggerExit;
    [SerializeField]
    private bool _hidingSpot;
    [SerializeField] 
    private bool _isTeleportTrigger;
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

    [Header("Indexes")]
    [SerializeField]
    private Camera _currentCamera;
    [SerializeField]
    private Camera _nextCamera;

    private Vector3 _originalScale;
    private BoxCollider _trigger;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (_trigger == null) _trigger = GetComponent<BoxCollider>();
            if (_originalScale == Vector3.zero) _originalScale = _trigger.size;
            if (_scaleTriggerOnEnter) _trigger.size = _originalScale * 2;

            if (_isTeleportTrigger) 
                _biomeSwitch.Raise(this, new SwitchBiomeEventArgs { NextBiome = _nextBiome, IsEnter = true });
            _triggerEnter.Raise(this, new TriggerEnterEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, IsHidingSpot = _hidingSpot, IsShowDown = false, NewViewIndex = _nextViewIndex, CurrentCamera = _currentCamera, NextCamera = _nextCamera });
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (_scaleTriggerOnEnter) _trigger.size = _originalScale;

            if (_isTeleportTrigger)
                _biomeSwitch.Raise(this, new SwitchBiomeEventArgs { NextBiome = _nextBiome, IsEnter = false });
            _triggerExit.Raise(this, new TriggerExitEventArgs { CurrentSceneIndex = _currentSceneIndex, newSceneIndex = _nextSceneIndex, CurrentViewIndex = _currentViewIndex, NewViewIndex = _nextViewIndex, IsTeleportTrigger = _isTeleportTrigger, CurrentCamera = _currentCamera, NextCamera = _nextCamera, DoRunTriggerExit = _doRunTriggerExit });
        }
    }
}
