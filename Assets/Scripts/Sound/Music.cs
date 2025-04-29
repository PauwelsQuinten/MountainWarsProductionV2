using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class Music : MonoBehaviour
{
    [Header("Music")]
    [SerializeField] private EventReference _music;
    private EventInstance _musicInstance;
    
    [Header("Snapshots")]
    [SerializeField] private EventReference _townReverbSnapshot;
    private EventInstance _townReverbSnapshotInstance;
    [SerializeField] private EventReference _forestReverbSnapshot;
    private EventInstance _forestReverbSnapshotInstance;
    [SerializeField] private EventReference _mountainReverbSnapshot;
    private EventInstance _mountainReverbSnapshotInstance;
    
    private PARAMETER_ID _musiczoneID;
    private float _musiczoneIDValue;
    private PARAMETER_ID _combatLevelID;
    private float _combatLevelIDValue;
    private string _currentSceneName;
    private SwitchBiomeEventArgs _switchBiomeEventArgs;
    private NewTargetEventArgs _newTargetEventArgs;
    private bool _canChangeSong = true;

    private GameObject _currentTrigger;
    private GameObject _previousTrigger;
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
        _townReverbSnapshotInstance = RuntimeManager.CreateInstance(_townReverbSnapshot);
        _forestReverbSnapshotInstance = RuntimeManager.CreateInstance(_forestReverbSnapshot);
        _mountainReverbSnapshotInstance = RuntimeManager.CreateInstance(_mountainReverbSnapshot);

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
                _combatLevelIDValue = 0.0f;
                _musiczoneIDValue = 0.0f;
                break;
            case "Gameplay":
                _musiczoneIDValue = 1.0f;
                _townReverbSnapshotInstance.start();
                _forestReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                _mountainReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                break;
            case "GameWon":
                _combatLevelIDValue = 0.0f;
                _musiczoneIDValue = 4.0f;
                break;
            case "GameLost":
                _combatLevelIDValue = 0.0f;
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
            return;
        }

        if (_switchBiomeEventArgs.IsEnter)
        {
            _previousTrigger = _currentTrigger;
            _currentTrigger = sender.gameObject;

            if (!_canChangeSong) return;

            _canChangeSong = false;

            switch (_switchBiomeEventArgs.NextBiome)
            {
                case Biome.village:
                    _musiczoneIDValue = 1.0f;
                    break;
                case Biome.Forest:
                    _musiczoneIDValue = 2.0f;
                    _forestReverbSnapshotInstance.start();
                    _mountainReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                    _townReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                    break;
                case Biome.Mountain:
                    _musiczoneIDValue = 3.0f;
                    _mountainReverbSnapshotInstance.start();
                    _forestReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                    _townReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
                    break;
            }

            SetGlobalParameterID(_musiczoneID, _musiczoneIDValue);
        }
        else
        {
            if (_previousTrigger == null) return;

            if(_previousTrigger != _currentTrigger)
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
    private void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _musicInstance.release();
        _forestReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
        _townReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
        _mountainReverbSnapshotInstance.stop(STOP_MODE.IMMEDIATE);
        _forestReverbSnapshotInstance.release();
        _townReverbSnapshotInstance.release();
        _mountainReverbSnapshotInstance.release();
    }
}