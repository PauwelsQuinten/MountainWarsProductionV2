using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using UnityEngine.Splines;
using Unity.Mathematics;

public class SoundAcrossSpline : MonoBehaviour
{
    [SerializeField] private EventReference _followSoundEvent;
    private EventInstance _followSoundInstance;

    [SerializeField] private Transform _followTransform;
    [SerializeField] private SplineContainer _spline;

    void Start()
    {
        _followSoundInstance = RuntimeManager.CreateInstance(_followSoundEvent);
        _followSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
        _followSoundInstance.start();
    }

    void Update()
    {
        if (_followTransform != null && _spline != null)
        {
            float3 targetPosition = _followTransform.position;
            float closestT = 0f;
            float minDistance = float.MaxValue;

            // Evaluate using SplineContainer directly
            for (float t = 0; t <= 1; t += 0.01f)
            {
                float3 pointOnSpline = _spline.EvaluatePosition(0, t); // Uses world space
                float distance = math.distance(targetPosition, pointOnSpline);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestT = t;
                }
            }

            float3 closestPoint = _spline.EvaluatePosition(0, closestT);
            Vector3 closestPointVector3 = closestPoint;
            Debug.DrawLine(targetPosition, closestPointVector3, Color.red);

            _followSoundInstance.set3DAttributes(RuntimeUtils.To3DAttributes(closestPointVector3));
        }
    }

    void OnDestroy()
    {
        _followSoundInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _followSoundInstance.release();
    }
}