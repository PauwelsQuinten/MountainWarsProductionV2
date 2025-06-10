using UnityEngine;

public class Hearing : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private float _hearingRange = 2f;
    [HideInInspector]
    public LayerMask CharacterMask;


    public GameObject HearSurrounding()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _hearingRange, CharacterMask);
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
