using FMOD.Studio;
using UnityEngine;
using UnityEngine.EventSystems;
using FMODUnity;

public class UIButtonHoverSound : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private EventReference _UIHoverSFX;
    private EventInstance _UIHoverSFXInstance;

    private void Start()
    {
        _UIHoverSFXInstance = RuntimeManager.CreateInstance(_UIHoverSFX);
    }

    
    public void OnPointerEnter(PointerEventData eventData)
    {
        _UIHoverSFXInstance.start();
    }

    public void OnSelect(BaseEventData eventData)
    {
        _UIHoverSFXInstance.start();
    }
    private void OnDestroy()
    {
        _UIHoverSFXInstance.release();
    }
}