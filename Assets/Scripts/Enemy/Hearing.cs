using UnityEngine;

public class Hearing : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private float _hearingRange = 2f;
    [SerializeField] private LayerMask _characterMask;
    
    
    public GameObject HearSurrounding()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _hearingRange, _characterMask);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject != gameObject)
            {
                return hitCollider.gameObject;
            }
        }

        return null;

    }

}
