using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using UnityEngine;
using FMODUnity;
using UnityEngine.Serialization;
using Debug = UnityEngine.Debug;

public class FMODAudioHandler : MonoBehaviour
{
    private TerrainChecker _checker;
    private string _currentLayerName;
    [SerializeField] private GameObject _player;
    private ATTRIBUTES_3D _attributes;
    private AimingOutputArgs _aimingEventArgs;
    private AttackEventArgs _attackEventArgs;
    private SwitchBiomeEventArgs _switchBiomeEventArgs;
    private FootstepSwapper _footstepSwapper;

    private bool isSheathing = true;

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

    private PARAMETER_ID _sheathID;
    private float _sheathIDValue;

    // Global
    private PARAMETER_ID _attacksStrengthID;
    private float _attacksStrengthIDValue;

    private PARAMETER_ID _currentWeaponID;
    private float _currentWeaponIDValue;

    // Events
    [Header("Ambience")] [SerializeField] private EventReference _ambience;
    private EventInstance _ambienceInstance;
    
    [Header("Music")] 
    [SerializeField] private EventReference _music;
    private EventInstance _musicInstance;

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
    [SerializeField] private EventReference _sheathSFX;
    private EventInstance _sheathSFXInstance;
    [SerializeField] private EventReference _unsheathSFX;
    private EventInstance _unsheathSFXInstance;

    [Header("SFX/Panels")] [SerializeField]
    private EventReference _comicPanelSwapSFX;

    private EventInstance _comicPanelSwapSFXInstance;
    [SerializeField] private EventReference _showdownSFX;
    private EventInstance _showdownSFXInstance;
    [SerializeField] private EventReference _showdownMusic;
    private EventInstance _showdownMusicInstance;
    private void OnEnable()
    {
        _checker = new TerrainChecker();
        _attributes = RuntimeUtils.To3DAttributes(transform);
        _ambienceInstance = RuntimeManager.CreateInstance(_ambience);
        GetParameterID(_ambienceInstance, "Biome", out _biomeID);
        GetParameterID(_ambienceInstance, "WindIntensity", out _windIntensityID);
        GetParameterID(_ambienceInstance, "RainIntensity", out _rainIntensityID);
        SetParameterID(_ambienceInstance, _biomeID, 0.0f);
        SetParameterID(_ambienceInstance, _windIntensityID, 1.0f);
        SetParameterID(_ambienceInstance, _rainIntensityID, 0.0f);
        StartCoroutine(ChangeWindIntensity());
        _ambienceInstance.set3DAttributes(_attributes);
        _ambienceInstance.start();
    }
    private IEnumerator ChangeWindIntensity()
    {
        while (true)
        {
            // Wait for a random interval between 5 and 15 seconds
            float waitTime = UnityEngine.Random.Range(5f, 15f);
            yield return new WaitForSeconds(waitTime);

            // Generate a random wind intensity value between 0 and 3
            float randomWindIntensity = UnityEngine.Random.Range(0f, 3f);

            // Set the wind intensity parameter
            SetParameterID(_ambienceInstance, _windIntensityID, randomWindIntensity);
        }
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
public void SwitchBiome(Component sender, object obj)
    {
        GetParameterID(_ambienceInstance, "Biome", out _biomeID);
        if (_switchBiomeEventArgs == null)
        {
            _switchBiomeEventArgs = obj as SwitchBiomeEventArgs;
        }

        switch (_switchBiomeEventArgs.NextBiome)
        {
            case Biome.village:
                _biomeIDValue = 0.0f;
                break;
            case Biome.Mountain:
                _biomeIDValue = 1.0f;
                break;
            case Biome.Forest:
                _biomeIDValue = 2.0f;
                break;
            default:
                _biomeIDValue = 0.0f;
                break;
        }
        SetParameterID(_ambienceInstance, _biomeID, _biomeIDValue);

    }
    public void PlayFootstepsSFX(Component sender, object obj)
    {
        string surfaceType = DetectSurfaceType();
        _footstepsSFXInstance = RuntimeManager.CreateInstance(_footstepsSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _footstepsSFXInstance.set3DAttributes(_attributes);
        GetParameterID(_footstepsSFXInstance, "SurfaceType", out _surfaceTypeID);
        GetParameterID(_footstepsSFXInstance, "TypeOfWalking", out _TypeOfWalkingID);
        switch (surfaceType)
        {
            case "TerrainRock":
                _surfaceTypeIDValue = 0.0f;
                break;
            case "Stones":
                _surfaceTypeIDValue = 1.0f;
                break;
            case "Gravel":
                _surfaceTypeIDValue = 2.0f;
                break;
            case "TerrainGrass":
                _surfaceTypeIDValue = 3.0f;
                break;
            case "TerrainDirt":
                _surfaceTypeIDValue = 4.0f;
                break;
            case "Wood":
                _surfaceTypeIDValue = 5.0f;
                break;
            case "Water":
                _surfaceTypeIDValue = 6.0f;
                break;
            case "Tall Grass":
                _surfaceTypeIDValue = 7.0f;
                break;
            default:
                _surfaceTypeIDValue = 0.0f;
                break;
        }

        if (_player.GetComponentInChildren<Animator>().GetCurrentAnimatorStateInfo(2).IsName("Walk"))
        {
            _TypeOfWalkingIDValue = 1.0f;
        }
        else
        {
            _TypeOfWalkingIDValue = 2.0f;
        }
        SetParameterID(_footstepsSFXInstance, _surfaceTypeID, _surfaceTypeIDValue);
        SetParameterID(_footstepsSFXInstance, _TypeOfWalkingID, _TypeOfWalkingIDValue);
        _footstepsSFXInstance.start();
        _footstepsSFXInstance.release();
    }

    private string DetectSurfaceType()
    {
        RaycastHit hit;
        Vector3 offset = new Vector3(0, 0.2f, 0);
        Vector3 rayOrigin = _player.transform.position - offset;
        Vector3 rayDirection = Vector3.down;
        float rayDistance = 5f;

        if (Physics.Raycast(rayOrigin, rayDirection, out hit, rayDistance))
        {
            Terrain terrain = hit.transform.GetComponent<Terrain>();
            if (terrain != null)
            {

                var layerMixes = _checker.GetLayerMixes(hit.point, terrain);

                if (layerMixes.Count > 0)
                {
                    string _dominantLayer = null;
                    float maxMixValue = 0f;

                    foreach (var layer in layerMixes)
                    {
                        if (layer.Value > maxMixValue)
                        {
                            maxMixValue = layer.Value;
                            _dominantLayer = layer.Key;
                        }
                    }

                    return _dominantLayer;
                }
            }
        }
        return "Default"; 
    }

    public void PlayWeaponHitSFX(Component sender, object obj)
    {
        _weaponHitSFXInstance = RuntimeManager.CreateInstance(_weaponHitSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _weaponHitSFXInstance.set3DAttributes(_attributes);
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
        SetGlobalParameterID(_currentWeaponID, 9.0f);
        SetParameterID(_weaponHitSFXInstance, _weaponHitSurfaceID, _weaponHitSurfaceIDValue);
        _weaponHitSFXInstance.start();
        _weaponWhooshSFXInstance.release();
    }

    public void PlayBlockSFX(Component sender, object obj)
    {
        _weaponHitSFXInstance = RuntimeManager.CreateInstance(_weaponHitSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _weaponHitSFXInstance.set3DAttributes(_attributes);
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
        SetGlobalParameterID(_currentWeaponID, 5.0f);
        SetParameterID(_weaponHitSFXInstance, _weaponHitSurfaceID, _weaponHitSurfaceIDValue);
        _weaponHitSFXInstance.start();
        _weaponWhooshSFXInstance.release();
    }

    public void PlayParrySFX(Component sender, object obj)
    {
        _weaponHitSFXInstance = RuntimeManager.CreateInstance(_weaponHitSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _weaponHitSFXInstance.set3DAttributes(_attributes);
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
        SetGlobalParameterID(_currentWeaponID, 4.0f);
        SetParameterID(_weaponHitSFXInstance, _weaponHitSurfaceID, _weaponHitSurfaceIDValue);
        _weaponHitSFXInstance.start();
        _weaponWhooshSFXInstance.release();
    }
    public void PlayWhooshSFX(Component sender, object obj)
    {
        _weaponWhooshSFXInstance = RuntimeManager.CreateInstance(_weaponWhooshSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _weaponWhooshSFXInstance.set3DAttributes(_attributes);
        _weaponWhooshSFXInstance.start();
        _weaponWhooshSFXInstance.release();
    }

    public void PlayComicSwapSFX(Component sender, object obj)
    {
        _comicPanelSwapSFXInstance = RuntimeManager.CreateInstance(_comicPanelSwapSFX);
        _comicPanelSwapSFXInstance.start();
        _comicPanelSwapSFXInstance.release();
    }

    public void PlayShowdownSFX(Component sender, object obj)
    {
        _showdownSFXInstance = RuntimeManager.CreateInstance(_showdownSFX);
        _showdownSFXInstance.start();
        _showdownSFXInstance.release();
    }

    public void PlayShowdownMusic(Component sender, object obj)
    {
        _showdownMusicInstance = RuntimeManager.CreateInstance(_showdownMusic);
        _showdownMusicInstance.start();
        _showdownMusicInstance.release();
    }

    public void PlaySheathSFX(Component sender, object obj)
    {
        _sheathSFXInstance = RuntimeManager.CreateInstance(_sheathSFX);
        _unsheathSFXInstance = RuntimeManager.CreateInstance(_unsheathSFX);
        _attributes = RuntimeUtils.To3DAttributes(sender.transform.position);
        _sheathSFXInstance.set3DAttributes(_attributes);
        _unsheathSFXInstance.set3DAttributes(_attributes);

        if (isSheathing)
        {
            _sheathSFXInstance.start();
            _sheathSFXInstance.release();
            isSheathing = false;
        }
        else
        {
            _unsheathSFXInstance.start();
            _unsheathSFXInstance.release();
            isSheathing = true;
        }
    }

    private void OnDisable()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _ambienceInstance.release();
        _footstepsSFXInstance.release();
        _attackChargeSFXInstance.release();
        _musicInstance.release();
        _showdownSFXInstance.release();
        _weaponWhooshSFXInstance.release();
        _comicPanelSwapSFXInstance.release();
        _weaponHitSFXInstance.release();
    }

    private void OnDestroy()
    {
        _musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _ambienceInstance.release();
        _footstepsSFXInstance.release();
        _attackChargeSFXInstance.release();
        _musicInstance.release();
        _showdownSFXInstance.release();
        _weaponWhooshSFXInstance.release();
        _comicPanelSwapSFXInstance.release();
        _weaponHitSFXInstance.release();
    }
}