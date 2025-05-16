using UnityEngine;

public class OnHillMovement : MonoBehaviour
{
    [SerializeField] private float _mxWalkAngle = 60f;
    
    private Rigidbody _rb;
    private Vector3 _groundNormal = Vector3.zero;
    private bool _isGrounded = false;

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if (_isGrounded)
            RotateOnHill();
    }



    private void OnCollisionStay(Collision collision)
    {

        _isGrounded = false;

        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, Vector3.up);

            // Check if the contact is with the ground (not a wall)
            if (angle < _mxWalkAngle)
            {
                _groundNormal = contact.normal;
                _isGrounded = true;
                Debug.DrawRay(contact.point, contact.normal, Color.green);
                break;
            }

        }

    }

    private void RotateOnHill()
    {

        // Calculate the rotation needed to align the character's up with the ground normal
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, _groundNormal) * transform.rotation;

        // Optionally smooth the rotation
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

    }

}
