using UnityEngine;
using FMOD.Studio;
using FMODUnity;
public class FollowSound : MonoBehaviour
{
    [SerializeField] private GameObject _followTarget;
    [SerializeField] private GameObject _sourceObject;
    [SerializeField] private EventReference _followSoundEvent;
    private EventInstance _followSoundInstance;
    
    [SerializeField] private  Transform playerTransform;
    [SerializeField] private  Collider riverCollider;
    void Start()
    {
        _followSoundInstance = FMODUnity.RuntimeManager.CreateInstance(_followSoundEvent);
        _followSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        _followSoundInstance.start();
    }
   
    void Update()
    {
        if (playerTransform != null && riverCollider != null)
        {
            // Find the closest point on the river to the player
            Vector3 closestPoint = riverCollider.ClosestPoint(playerTransform.position);

            // Update the 3D attributes of the FMOD sound instance
            _followSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(closestPoint));
            
            Debug.DrawLine(playerTransform.position, closestPoint, Color.red);

        }
    }
    void OnDestroy()
    {
        // Clean up the instance
        _followSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _followSoundInstance.release();
    }

}
