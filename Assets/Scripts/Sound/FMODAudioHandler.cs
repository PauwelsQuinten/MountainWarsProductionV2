using FMOD;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;

public class FMODAudioHandler : MonoBehaviour
{
    private ATTRIBUTES_3D attributes;
    private AimingOutputArgs _aimingEventArgs;
    private AttackEventArgs _attackEventArgs;

    //Parameters
    //Local
    private PARAMETER_ID _surfaceTypeID;
    private float _surfaceTypeIDValue;

    private PARAMETER_ID _weaponHitSurfaceID;
    private float _weaponHitSurfaceIDValue;

    private PARAMETER_ID _biomeID;
    private float _biomeIDValue;

    private PARAMETER_ID _rainIntensityID;
    private float _rainIntensityIDValue;

    private PARAMETER_ID _windIntensityID;

    private float _windIntensityIDValue;

    //Global
    private PARAMETER_ID _attacksStrengthID;
    private float _attacksStrengthIDValue;

    private PARAMETER_ID _currentWeaponID;
    private float _currentWeaponIDValue;

    // Events
    [Header("Ambience")] [SerializeField] private EventReference _ambience;
    private EventInstance _ambienceInstance;

    [Header("SFX/Character")] [SerializeField]
    private EventReference _footstepsSFX;

    private EventInstance _footstepsSFXInstance;

    [Header("SFX/Combat")] [SerializeField]
    private EventReference _attackChargeSFX;

    private EventInstance _attackChargeSFXInstance;
    [SerializeField] private EventReference _weaponWhooshSFX;
    private EventInstance _weaponWhooshSFXInstance;
    [SerializeField] private EventReference _weaponHitSFX;
    private EventInstance _weaponHitSFXInstance;

    private void Start()
    {
        attributes = RuntimeUtils.To3DAttributes(transform);
        _ambienceInstance = RuntimeManager.CreateInstance(_ambience);
        GetParameterID(_ambienceInstance, "Biome", out _biomeID);
        GetParameterID(_ambienceInstance, "WindIntensity", out _windIntensityID);
        GetParameterID(_ambienceInstance, "RainIntensity", out _rainIntensityID);
        SetParameterID(_ambienceInstance, _biomeID, 1.0f);
        SetParameterID(_ambienceInstance, _windIntensityID, 1.0f);
        SetParameterID(_ambienceInstance, _rainIntensityID, 1.0f);
        _ambienceInstance.set3DAttributes(attributes);
        _ambienceInstance.start();
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
        _footstepsSFXInstance = RuntimeManager.CreateInstance(_footstepsSFX);
        _footstepsSFXInstance.start();
        _footstepsSFXInstance.release();
    }

    public void PlayFootstepsSFX(Component sender, object obj)
    {
        _footstepsSFXInstance = RuntimeManager.CreateInstance(_footstepsSFX);
        _footstepsSFXInstance.start();
        _footstepsSFXInstance.release();
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
        _weaponWhooshSFXInstance.release();
    }

    public void PlayWhooshSFX(Component sender, object obj)
    {
        _weaponWhooshSFXInstance = RuntimeManager.CreateInstance(_weaponWhooshSFX);
        GetGlobalParameterID("AttackStrength", out _attacksStrengthID);
        GetGlobalParameterID("CurrentWeapon", out _currentWeaponID);
        if (_attackEventArgs == null)
        {
            _attackEventArgs = obj as AttackEventArgs;
        }

        if (_aimingEventArgs == null)
        {
            _aimingEventArgs = obj as AimingOutputArgs;
        }

        if (_aimingEventArgs.AttackSignal != AttackSignal.Idle)
        {
            SetGlobalParameterID(_attacksStrengthID, _aimingEventArgs.Speed);
            SetGlobalParameterID(_currentWeaponID, 1.0f);
            _weaponWhooshSFXInstance.start();
            _weaponWhooshSFXInstance.release();
        }
    }
}