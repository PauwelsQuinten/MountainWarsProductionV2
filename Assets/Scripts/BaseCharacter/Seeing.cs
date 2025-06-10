using UnityEngine;

public class Seeing : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private float _eyesightRange = 5f;
    [HideInInspector]
    public LayerMask CharacterMask;

    private Vector3 debugPos = Vector3.zero;

    public GameObject SeeSurrounding(Orientation orientation)
    {
        float angle = (float)orientation * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
        Vector3 position = transform.position + direction * _eyesightRange;
        debugPos = position;

        Collider[] hitColliders = Physics.OverlapSphere(position, _eyesightRange, CharacterMask);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                return hitCollider.gameObject;
            }
        }

        return null;
    }

    //void OnDrawGizmos()
    //{
    //    // Set the color for the debug sphere
    //    Gizmos.color = Color.cyan;
    //
    //    // Draw the debug sphere at the object's position with the specified radius
    //    Gizmos.DrawSphere(debugPos, _eyesightRange);
    //}
}
