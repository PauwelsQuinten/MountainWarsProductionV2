using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class SceneManager : MonoBehaviour
{
    [Header("SpawnPoints")]
    [SerializeField]
    private List<GameObject> _spawnPoints = new List<GameObject>();

    private bool _canTeleport = true;
    private GameObject _player;

    private TriggerExitEventArgs _previousArgs;

    public void EnterPanels(Component sender, object obj)
    {
        TriggerEnterEventArgs args = obj as TriggerEnterEventArgs;
        if (args == null) return;
        if (args.CurrentSceneIndex == args.newSceneIndex) return;

        if (args.IsHidingSpot || args.IsShowDown) return;

        if (!_canTeleport) return;
        _canTeleport = false;

        Camera currentCam = _spawnPoints[args.CurrentSceneIndex].transform.parent.GetComponentInChildren<Camera>();
        Camera Nextcam = _spawnPoints[args.newSceneIndex].transform.parent.GetComponentInChildren<Camera>();

        currentCam.GetComponent<CameraFollow>().enabled = false;
        Nextcam.GetComponent<CameraFollow>().enabled = true;

        if (_player == null) _player = GameObject.Find("Player");

        _player.transform.position = _spawnPoints[args.newSceneIndex].transform.position;
    }

    public void ExitTrigger(Component sender, object obj)
    {
        TriggerExitEventArgs args = obj as TriggerExitEventArgs;
        if (args == null) return;
        if(!args.IsTeleportTrigger) return;
        if (_previousArgs == null)
        {
            _previousArgs = args;
            return;
        }
        else if (sender.gameObject.transform.parent != _spawnPoints[_previousArgs.CurrentSceneIndex].gameObject.transform.parent)
            _canTeleport = true;
        _previousArgs = args;
    }
}
