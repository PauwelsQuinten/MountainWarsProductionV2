using System;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using Debug = UnityEngine.Debug;

public class FMODAudioHandler : MonoBehaviour
{
    [SerializeField] private GameObject _player;
    private ATTRIBUTES_3D _attributes;
    private AimingOutputArgs _aimingEventArgs;
    private AttackEventArgs _attackEventArgs;

    // Parameters
    // Local
    private PARAMETER_ID _surfaceTypeID;
    private float _surfaceTypeIDValue;

    private PARAMETER_ID _TypeOfWalkingID;
    private float _TypeOfWalkingIDValue;
    
    private PARAMETER_ID _weaponHitSurfaceID;
    private float _weaponHitSurfaceIDValue;

    private PARAMETER_ID _biomeID;
    private float _biomeIDValue;

    private PARAMETER_ID _rainIntensityID;
    private float _rainIntensityIDValue;

    private PARAMETER_ID _windIntensityID;
    private float _windIntensityIDValue;

    // Global
    private PARAMETER_ID _attacksStrengthID;
    private float _attacksStrengthIDValue;

    private PARAMETER_ID _currentWeaponID;
    private float _currentWeaponIDValue;

    // Events
    [Header("Ambience")] 
    [SerializeField] private EventReference _ambience;
    private EventInstance _ambienceInstance;

    [Header("Music")] 
    [SerializeField] private EventReference _mainThemeMusic;
    private EventInstance _mainThemeMusicInstance;
    [SerializeField] private EventReference _forestMusic;
    private EventInstance _forestMusicInstance;
    
    [Header("SFX/Character")] 
    [SerializeField] private EventReference _footstepsSFX;
    private EventInstance _footstepsSFXInstance;

    [Header("SFX/Combat")] 
    [SerializeField] private EventReference _attackChargeSFX;
    private EventInstance _attackChargeSFXInstance;
    [SerializeField] private EventReference _weaponWhooshSFX;
    private EventInstance _weaponWhooshSFXInstance;
    [SerializeField] private EventReference _weaponHitSFX;
    private EventInstance _weaponHitSFXInstance;

    [Header("SFX/CameraMovements")] 
    [SerializeField] private EventReference _comicPanelSwapSFX;
    private EventInstance _comicPanelSwapSFXInstance;
    [SerializeField] private EventReference _showdownSFX;
    private EventInstance _showdownSFXInstance;
    private void Start()
    {
        _attributes = RuntimeUtils.To3DAttributes(transform);
        _ambienceInstance = RuntimeManager.CreateInstance(_ambience);
        _forestMusicInstance = RuntimeManager.CreateInstance(_forestMusic);
        GetParameterID(_ambienceInstance, "Biome", out _biomeID);
        GetParameterID(_ambienceInstance, "WindIntensity", out _windIntensityID);
        GetParameterID(_ambienceInstance, "RainIntensity", out _rainIntensityID);
        SetParameterID(_ambienceInstance, _biomeID, 1.0f);
        SetParameterID(_ambienceInstance, _windIntensityID, 1.0f);
        SetParameterID(_ambienceInstance, _rainIntensityID, 0.0f);
        _ambienceInstance.set3DAttributes(_attributes);
        _ambienceInstance.start();
        _forestMusicInstance.start();
    }

    private void GetParameterID(EventInstance eventInstance, string parameterName, out PARAMETER_ID parameterID)
    {
        // Get the parameter ID
        eventInstance.getDescription(out EventDescription eventDescription);
        eventDescription.getParameterDescriptionByName(parameterName, out PARAMETER_DESCRIPTION parameterDescription);
        parameterID = parameterDescription.id;
    }

    private void SetParameterID(EventInstance eventInstance, PARAMETER_ID parameterID, float desiredParameterValue)
    {
        // Set the parameter value by ID
        eventInstance.setParameterByID(parameterID, desiredParameterValue);
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

    public void PlayFootstepsSFX(Component sender, object obj)
    {
        string surfaceType = DetectSurfaceType();
        _footstepsSFXInstance = RuntimeManager.CreateInstance(_footstepsSFX);
        GetParameterID(_footstepsSFXInstance, "SurfaceType", out _surfaceTypeID);
        GetParameterID(_footstepsSFXInstance, "TypeOfWalking", out _TypeOfWalkingID);
        switch (surfaceType)
        {
            case "Concrete":
                _surfaceTypeIDValue = 0.0f;
                break;
            case "Stone":
                _surfaceTypeIDValue = 2.0f;
                break;
            case "Gravel":
                _surfaceTypeIDValue = 3.0f;
                break;
            case "Grass":
                _surfaceTypeIDValue = 4.0f;
                break;
            case "Sand":
                _surfaceTypeIDValue = 5.0f;
                break;
            case "Wood":
                _surfaceTypeIDValue = 6.0f;
                break;
            case "Tall Grass":
                _surfaceTypeIDValue = 7.0f;
                break;
            default:
                _surfaceTypeIDValue = 0.0f;
                break;
        }
        Debug.Log(surfaceType);
        if (_player.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(2).IsName("Walk"))
        {
            _TypeOfWalkingIDValue = 1.0f;
        }
        else
        {
            _TypeOfWalkingIDValue = 2.0f;
        }
        _footstepsSFXInstance.start();
        SetParameterID(_footstepsSFXInstance, _surfaceTypeID, _surfaceTypeIDValue);
        SetParameterID(_footstepsSFXInstance, _TypeOfWalkingID, _TypeOfWalkingIDValue);

        
       // _footstepsSFXInstance.release();
    }

    private string DetectSurfaceType()
    {
        RaycastHit hit;
        Vector3 offset = new Vector3(0, 1,0);
        Vector3 rayOrigin = _player.transform.position - offset;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 5f;
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            string layerName = LayerMask.LayerToName(hit.collider.gameObject.layer);
            return layerName;
        }

        return "Default";// Default surface type if no specific surface is detected
    }

    public void PlayWeaponHitSFX(Component sender, object obj)
    {
        _weaponHitSFXInstance = RuntimeManager.CreateInstance(_weaponHitSFX);
        GetParameterID(_weaponHitSFXInstance, "WeaponSurfaceHit", out _weaponHitSurfaceID);
        GetGlobalParameterID("AttacksStrength", out _attacksStrengthID);
        GetGlobalParameterID("CurrentWeapon", out _currentWeaponID);
        if (_attackEventArgs == null)
        {
            _attackEventArgs = obj as AttackEventArgs;
        }

        if (_aimingEventArgs == null)
        {
            _aimingEventArgs = obj as AimingOutputArgs;
        }

        SetGlobalParameterID(_attacksStrengthID, _attackEventArgs.AttackPower);
        SetGlobalParameterID(_currentWeaponID, 2.0f);
        SetParameterID(_weaponHitSFXInstance, _weaponHitSurfaceID, _weaponHitSurfaceIDValue);
        _weaponHitSFXInstance.start();
       // _weaponWhooshSFXInstance.release();
    }

    public void PlayWhooshSFX(Component sender, object obj)
    {
        _weaponWhooshSFXInstance = RuntimeManager.CreateInstance(_weaponWhooshSFX);
            _weaponWhooshSFXInstance.start();
           // _weaponWhooshSFXInstance.release();
    }
    
    public void PlayComicSwapSFX(Component sender, object obj)
    {
        _comicPanelSwapSFXInstance = RuntimeManager.CreateInstance(_comicPanelSwapSFX);
        _comicPanelSwapSFXInstance.start();
        //_comicPanelSwapSFXInstance.release();
    }
    
    public void PlayShowdownSFX(Component sender, object obj)
    {
        _showdownSFXInstance = RuntimeManager.CreateInstance(_showdownSFX);
        _showdownSFXInstance.start();
       // _showdownSFXInstance.release();
    }

    private void OnDestroy()
    {
        _ambienceInstance.release();
        _footstepsSFXInstance.release();
        _attackChargeSFXInstance.release();
        _forestMusicInstance.release();
        _mainThemeMusicInstance.release();
        _showdownSFXInstance.release();
        _weaponWhooshSFXInstance.release();
        _comicPanelSwapSFXInstance.release();
        _weaponHitSFXInstance.release();
    }
}