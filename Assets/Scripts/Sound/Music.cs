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
    private PARAMETER_ID _combatLevelID;
    private float _combatLevelIDValue;
    private string _currentSceneName;
    private SwitchBiomeEventArgs _switchBiomeEventArgs;
    private NewTargetEventArgs _newTargetEventArgs;
    private bool _isBiomeActive = false;
    private bool _canChangeSong = true;
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
        _musicInstance.start();

        GetGlobalParameterID("MusicZone", out _musiczoneID);
        GetGlobalParameterID("CombatLevel", out _combatLevelID);
    
        _currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
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
        // Prevent scene-based updates if biome music is active
        if (_isBiomeActive)
        {
            Debug.Log("Biome music is active, skipping scene-based updates.");
            return;
        }

        string activeSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        if (activeSceneName != _currentSceneName)
        {
            _currentSceneName = activeSceneName;
            Debug.Log($"Scene changed to: {_currentSceneName}");
            UpdateMusicForScene(_currentSceneName);
        }
    }

    private void UpdateMusicForScene(string sceneName)
    {
        switch (sceneName)
        {
            case "MainMenu":
                ResetToSceneMusic();
                _musiczoneIDValue = 0.0f;
                break;
            case "Gameplay":
                _musiczoneIDValue = 1.0f;
                break;
            case "GameWon":
                ResetToSceneMusic();
                _musiczoneIDValue = 4.0f;
                break;
            case "GameLost":
                ResetToSceneMusic();
                _musiczoneIDValue = 5.0f;
                break;
             default:
                 _musiczoneIDValue = 0.0f;
                 break;
        }

        SetGlobalParameterID(_musiczoneID, _musiczoneIDValue);
    }

    public void SwitchBiome(Component sender, object obj)
    {
        _switchBiomeEventArgs = obj as SwitchBiomeEventArgs;

        if (_switchBiomeEventArgs == null)
        {
            Debug.LogError("Invalid event data passed to SwitchBiome.");
            return;
        }

        if (_switchBiomeEventArgs.IsEnter)
        {
            if (!_canChangeSong)
            {
                return;
            }
            _canChangeSong = false;
            _isBiomeActive = true; // Set biome flag to true
            Debug.Log($"Switching to biome: {_switchBiomeEventArgs.NextBiome}");

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
            }

            SetGlobalParameterID(_musiczoneID, _musiczoneIDValue);
            Debug.Log($"Biome music set to: {_musiczoneIDValue}");
        }
        else
        {
            _canChangeSong = true;
        }
    }
    public void SetCombatLevel(Component sender, object obj)
    {
        // Ensure obj is of the correct type
        if (obj is NewTargetEventArgs newTargetArgs)
        {
            _newTargetEventArgs = newTargetArgs;

            if (_newTargetEventArgs.NewTarget == null)
            {
                _combatLevelIDValue = 0.0f;
            }
            else
            {
                _combatLevelIDValue = 1.0f;
            }

            SetGlobalParameterID(_combatLevelID, _combatLevelIDValue);
        }
    }
    private void ResetToSceneMusic()
    {
        if (!_isBiomeActive)
        {
            Debug.Log("ResetToSceneMusic called, but biome music is not active.");
            return;
        }

        Debug.Log("Resetting to scene-based music.");
        _isBiomeActive = false; // Reset biome flag
        UpdateMusicForScene(_currentSceneName); // Reapply scene music
    }
    private void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();
    }
}