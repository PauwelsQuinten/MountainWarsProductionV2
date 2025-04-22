using System;
using FMOD.Studio;
using FMODUnity;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Music : MonoBehaviour
{
    [SerializeField] private EventReference _music;
    private EventInstance _musicInstance;
    private PARAMETER_ID _musiczoneID;
    private float _musiczoneIDValue;
    private string _currentSceneName;


    private void Awake()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("Music");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }

        DontDestroyOnLoad(this.gameObject);
    }
    private void Start()
    {
        _musicInstance = RuntimeManager.CreateInstance(_music);
        RuntimeManager.StudioSystem.getParameterDescriptionByName("MusicZone",
            out PARAMETER_DESCRIPTION parameterDescription);
        _musiczoneID = parameterDescription.id;
        _musiczoneIDValue = 0.0f;
        
        _musicInstance.start();
        
        UpdateMusicForScene(_currentSceneName);
    }
    private void Update()
    {
        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeSceneName != _currentSceneName)
        {
            _currentSceneName = activeSceneName;
            UpdateMusicForScene(_currentSceneName);
        }
    }

    private void UpdateMusicForScene(string sceneName)
    {
        
        switch (sceneName)
        {
            case "MainMenu":
                _musiczoneIDValue = 0.0f; 
                break;
            case "Gameplay":
                _musiczoneIDValue = 1.0f; 
                break;
            case "GameWon":
                _musiczoneIDValue = 4.0f; 
                break;
            case "GameLost":
                _musiczoneIDValue = 5.0f; 
                break;
            default:
                _musiczoneIDValue = 0.0f;
                break;
        }
        
        RuntimeManager.StudioSystem.setParameterByID(_musiczoneID, _musiczoneIDValue);
    }

    private void OnDestroy()
    {
         _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
         _musicInstance.release();
    }
}
