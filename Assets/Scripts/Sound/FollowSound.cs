using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Serialization;

public class FollowSound : MonoBehaviour
{
    [SerializeField] private EventReference _followSoundEvent;
    private EventInstance _followSoundInstance;
    
   [SerializeField] private  Transform followTransform;
    [SerializeField] private  Collider riverCollider;
    void Start()
    {
        _followSoundInstance = FMODUnity.RuntimeManager.CreateInstance(_followSoundEvent);
        _followSoundInstance.set3DAttributes(FMODUnity.RuntimeUtils.To3DAttributes(gameObject));
        _followSoundInstance.start();
    }
   
    void Update()
    {
        if (followTransform != null && riverCollider is MeshCollider meshCollider)
        {
            Mesh mesh = meshCollider.sharedMesh;
            if (mesh == null)
            {
                Debug.LogWarning("MeshCollider has no sharedMesh assigned.");
                return;
            }

            Vector3 closestPoint = followTransform.position;
            float closestDistance = float.MaxValue;

            // Iterate through all vertices of the mesh
            foreach (Vector3 vertex in mesh.vertices)
            {
                // Transform vertex to world space
                Vector3 worldVertex = meshCollider.transform.TransformPoint(vertex);

                // Calculate the distance to the followTransform position
                float distance = Vector3.Distance(followTransform.position, worldVertex);

                // Log distances for debugging
               // Debug.Log($"Vertex: {worldVertex}, Distance: {distance}");

                // Update the closest point if this vertex is closer
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPoint = worldVertex;
                }
            }

            // Log the closest point for debugging
            //Debug.Log($"Closest Point: {closestPoint}");

            // Update the 3D attributes of the FMOD sound instance
            _followSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(closestPoint));

            Debug.DrawLine(followTransform.position, closestPoint, Color.red, 1.0f);
        }
    }
    void OnDestroy()
    {
        // Clean up the instance
        _followSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _followSoundInstance.release();
    }

}
