using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LaunchGameEvent : MonoBehaviour
{
    [SerializeField]
    private GameEvent _gameEvent;
    public void DoGameEvent(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _gameEvent.Raise();
    }
}
