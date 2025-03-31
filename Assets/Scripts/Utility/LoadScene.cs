using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LoadScene : MonoBehaviour
{
    [SerializeField]
    private string _sceneName;
    [SerializeField]
    private GameEvent _loadSceneEvent;

    public void LoadSceneByName(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        _loadSceneEvent.Raise(this, _sceneName);
    }
}