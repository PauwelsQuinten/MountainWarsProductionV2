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
        if (args.CurrentSceneIndex == args.newSceneIndex) return;

        if (args.IsHidingSpot || args.IsShowDown) return;

        Camera currentCam = _spawnPoints[args.CurrentSceneIndex].transform.parent.GetComponentInChildren<Camera>();
        Camera Nextcam = _spawnPoints[args.newSceneIndex].transform.parent.GetComponentInChildren<Camera>();

        currentCam.GetComponent<FollowObject>().enabled = false;
        Nextcam.GetComponent<FollowObject>().enabled = true;

        _changeCam.Raise(this, Nextcam);

        if (_player == null) _player = GameObject.Find("Player");

        _player.transform.position = _spawnPoints[args.newSceneIndex].transform.position;
    }
}
