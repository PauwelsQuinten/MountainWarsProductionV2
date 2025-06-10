using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public class SceneManager : MonoBehaviour
{
    [Header("SpawnPoints")]
    [SerializeField]
    private List<GameObject> _spawnPoints = new List<GameObject>();

    [Header("events")]
    [SerializeField]
    private GameEvent _changeCam;

    private GameObject _player;


    public void EnterPanels(Component sender, object obj)
    {
        TriggerEnterEventArgs args = obj as TriggerEnterEventArgs;
        if (args == null) return;

        if (args.IsHidingSpot || args.IsShowDown) return;

        Camera currentCam = null;
        Camera nextCam = null;
        if (args.CurrentSceneIndex == args.newSceneIndex)
        {
            currentCam = args.CurrentCamera;
            nextCam = args.NextCamera;

            currentCam.GetComponent<FollowObject>().enabled = false;
            nextCam.GetComponent<FollowObject>().enabled = true;

            _changeCam.Raise(this, nextCam);
            return;
        }

        currentCam = args.CurrentCamera;
        nextCam = args.NextCamera;

        currentCam.GetComponent<FollowObject>().enabled = false;
        nextCam.GetComponent<FollowObject>().enabled = true;

        _changeCam.Raise(this, nextCam);

        if (args.newSceneIndex == null) return;
        if (_player == null) _player = GameObject.Find("Player");

        _player.transform.position = _spawnPoints[args.newSceneIndex].transform.position;
    }

    public void ExitPanels(Component sender, object obj)
    {
        TriggerExitEventArgs args = obj as TriggerExitEventArgs;
        if (args == null) return;
        if (!args.DoRunTriggerExit) return;

        Camera currentCam = null;
        Camera nextCam = null;

        currentCam = args.NextCamera;
        nextCam = args.CurrentCamera;

        currentCam.GetComponent<FollowObject>().enabled = false;
        nextCam.GetComponent<FollowObject>().enabled = true;

        _changeCam.Raise(this, nextCam);
    }
}
