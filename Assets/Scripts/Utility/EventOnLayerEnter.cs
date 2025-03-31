using UnityEngine;

public class EventOnLayerEnter : MonoBehaviour
{
    [SerializeField]
    private GameEvent _gameEvent;
    [SerializeField]
    private LayerMask _layerEnter;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _layerEnter) != 0)
        {
            _gameEvent.Raise(this, other.gameObject);
        }
    }
}
