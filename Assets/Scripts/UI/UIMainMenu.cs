#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

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

    private void Start()
    {
        _masterVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _masterVCAName);
        _masterVCA.getVolume(out _masterVCAVolume);
        
        _sfxVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _sfxVCAName);
        _sfxVCA.getVolume(out _sfxVCAVolume);
        
        _musicVCA = FMODUnity.RuntimeManager.GetVCA("vca:/" + _musicVCAName);
        _musicVCA.getVolume(out _musicVCAVolume);
    }

    public void NewGamePressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }

    public void SettingsPressed()
    {
        _SettingsMenu.SetActive(true);
        _eventSystem.SetSelectedGameObject(_firstSettingsItem);
    }

    public void QuitGamePressed()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }

    public void BackButtonPressed()
    {
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
}