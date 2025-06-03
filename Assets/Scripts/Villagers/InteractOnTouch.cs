using UnityEngine;

public class InteractOnTouch : MonoBehaviour
{
    [Header("Collider")]
    [SerializeField] private float _radius = 1f;
    [Header("Event")]
    [SerializeField] private GameEvent _foundTarget;
    private SphereCollider _collider;
    private StateManager _stateManager;

    const string TAG_PLAYER = "Player";
    const string TAG_VILLAGER = "Villager";
    private void Start()
    {
        _collider = gameObject.AddComponent<SphereCollider>();
        _collider.enabled = true;
        _collider.isTrigger = true;
        _collider.radius = _radius;

        _stateManager = GetComponent<StateManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_stateManager.Target != null) return;   

        if (other.CompareTag(TAG_PLAYER) || other.CompareTag(TAG_VILLAGER))
            _foundTarget.Raise(this, new NewTargetEventArgs { NewTarget = other.gameObject });
    }
}
