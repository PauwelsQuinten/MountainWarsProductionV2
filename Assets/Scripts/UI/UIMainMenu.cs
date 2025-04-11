#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using FMOD.Studio;
using FMODUnity;
public class UIMainMenu : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private GameObject _SettingsMenu;
    [SerializeField] private GameObject _firstSettingsItem;
    [SerializeField] private GameObject _backButton;

    [SerializeField] private GameObject _settingsButton;
    [SerializeField] private EventSystem _eventSystem;

    [Header("VCA")] 
    private FMOD.Studio.VCA _masterVCA;
    private FMOD.Studio.VCA _sfxVCA;
    private FMOD.Studio.VCA _musicVCA;
    [SerializeField] private string _masterVCAName;
    [SerializeField] private string _sfxVCAName;
    [SerializeField] private string _musicVCAName;
    [SerializeField] private float _masterVCAVolume;
    [SerializeField] private float _sfxVCAVolume;
    [SerializeField] private float _musicVCAVolume;

    
    [Header("Audio")]
    [SerializeField] private EventReference _UIConfirmSFX;
    private EventInstance _UIConfirmSFXInstance;
    [SerializeField] private EventReference _UIBackSFX;
    private EventInstance _UIBackSFXInstance;
    [SerializeField] private EventReference _mainMenuMusic;
    private EventInstance _mainMenuMusicInstance;
    private Coroutine _loadScene;
    private void Start()
    {
        _masterVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _masterVCAName);
        _masterVCA.getVolume(out _masterVCAVolume);
        
        _sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _sfxVCAName);
        _sfxVCA.getVolume(out _sfxVCAVolume);
        
        _musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _musicVCAName);
        _musicVCA.getVolume(out _musicVCAVolume);
        
        _UIConfirmSFXInstance = RuntimeManager.CreateInstance(_UIConfirmSFX);
        _UIBackSFXInstance = RuntimeManager.CreateInstance(_UIBackSFX);
        _mainMenuMusicInstance = RuntimeManager.CreateInstance(_mainMenuMusic);
        _mainMenuMusicInstance.start();
    }

    public void NewGamePressed()
    {
        _UIConfirmSFXInstance.start();
        if (_loadScene != null) StopCoroutine(_loadScene);
        _loadScene = StartCoroutine(LoadSceneWithDelay());
        _UIConfirmSFXInstance.release();
        _UIBackSFXInstance.release();
    }

    private IEnumerator LoadSceneWithDelay()
    {
        yield return new WaitForEndOfFrame();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }

    public void SettingsPressed()
    {
        _UIConfirmSFXInstance.start();
        _SettingsMenu.SetActive(true);
        _eventSystem.SetSelectedGameObject(_firstSettingsItem);
    }

    public void QuitGamePressed()
    {
        _UIBackSFXInstance.start();

        #if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
        #endif

        _UIConfirmSFXInstance.release();
        _UIBackSFXInstance.release();
        Application.Quit();
    }

    public void BackButtonPressed()
    {
        _UIBackSFXInstance.start();
        _SettingsMenu.SetActive(false);
        _eventSystem.SetSelectedGameObject(_settingsButton);
    }

    public void MasterVolumeChanged(float value)
    {
        _masterVCA.setVolume(value);
        _masterVCA.getVolume(out _masterVCAVolume);
    }

    public void SFXSliderChanged(float value)
    {
        _sfxVCA.setVolume(value);
        _sfxVCA.getVolume(out _sfxVCAVolume);
    }

    public void MusicSliderChanged(float value)
    {
        _musicVCA.setVolume(value);
        _musicVCA.getVolume(out _sfxVCAVolume);
    }

    private void OnEnable()
    {
        _mainMenuMusicInstance.start();
    }

    private void OnDisable()
    {
        _UIConfirmSFXInstance.release();
        _UIBackSFXInstance.release();
        _mainMenuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _mainMenuMusicInstance.release();
    }

    private void OnDestroy()
    {
        _UIConfirmSFXInstance.release();
        _UIBackSFXInstance.release();
        _mainMenuMusicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        _mainMenuMusicInstance.release();
    }
}
