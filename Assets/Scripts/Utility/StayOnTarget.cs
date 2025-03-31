using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayOnTarget : MonoBehaviour
{
    [SerializeField]
    private Transform _target;
    [SerializeField]
    private Vector3 _locationOffset;
    [SerializeField]
    private Vector3 _rotationOffset;

    void Start()
    {
        StartCoroutine(FollowTarget());
    }

    private IEnumerator FollowTarget()
    {
        while (true)
        {
            transform.position = _target.position;
            transform.localPosition += _locationOffset;
            transform.rotation = _target.rotation;
            transform.localRotation *= Quaternion.Euler(_rotationOffset);
            yield return null;
        }
    }
}
