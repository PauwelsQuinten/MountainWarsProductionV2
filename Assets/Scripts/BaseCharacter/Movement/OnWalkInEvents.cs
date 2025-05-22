using System;
using System.Collections.Generic;
using UnityEngine;

public class OnWalkInEvents : MonoBehaviour
{
    [SerializeField] private GameEvent _queueEvent;
    private const string NO_TAG = "Untagged";

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag(NO_TAG))
            return;

        foreach (SpecialInput tag in Enum.GetValues(typeof(SpecialInput)))
        {
            if ((int)tag >= 100  && collision.transform.CompareTag(tag.ToString()))
            {
                _queueEvent.Raise(this, new AimingOutputArgs {Special =  tag});
                return;
            }
        }

        Debug.Log("No vallid tag name found on hit object");
    }
}
