using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveCursor : MonoBehaviour
{
    [SerializeField]
    private bool _lockCursor = true;

    private void Awake()
    {
        if (_lockCursor )
            Cursor.lockState = CursorLockMode.Locked;
        else
            Cursor.lockState = CursorLockMode.None;
    }
}
