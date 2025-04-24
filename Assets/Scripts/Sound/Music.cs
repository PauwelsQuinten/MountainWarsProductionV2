using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class Music : MonoBehaviour
{
    [SerializeField] private EventReference _music;
    private EventInstance _musicInstance;
    private PARAMETER_ID _musiczoneID;
    private float _musiczoneIDValue;
    private string _currentSceneName;
    private SwitchBiomeEventArgs _switchBiomeEventArgs;


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
    private void GetGlobalParameterID(string parameterName, out PARAMETER_ID parameterID)
    {
        // Get the global parameter value by ID
        RuntimeManager.StudioSystem.getParameterDescriptionByName(parameterName,
            out PARAMETER_DESCRIPTION parameterDescription);
        parameterID = parameterDescription.id;
    }

    private void SetGlobalParameterID(PARAMETER_ID parameterID, float desiredParameterValue)
    {
        // Set the global parameter value by ID
        RuntimeManager.StudioSystem.setParameterByID(parameterID, desiredParameterValue);
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
    public void SwitchBiome(Component sender, object obj)
    {
        GetGlobalParameterID("MusicZone", out _musiczoneID);
        if (_switchBiomeEventArgs == null)
        {
            _switchBiomeEventArgs = obj as SwitchBiomeEventArgs;
        }

        switch (_switchBiomeEventArgs.NextBiome)
        {
            case Biome.village:
                _musiczoneIDValue = 1.0f;
                break;
            case Biome.Forest:
                _musiczoneIDValue = 2.0f;
                break;
            case Biome.Mountain:
                _musiczoneIDValue = 3.0f;
                break;
            default:
                _musiczoneIDValue = 0.0f;
                break;
        }
        SetGlobalParameterID(_musiczoneID, _musiczoneIDValue);
    }
    private void OnDestroy()
    {
         _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
         _musicInstance.release();
    }
}
