using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class UIPauseScreen : MonoBehaviour
{
    [SerializeField]
    private GameObject _pauseMenu;
    [SerializeField]
    private EventSystem _eventSystem;
    [SerializeField]
    private GameObject _firstSelected;

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

    }

    public void OpenPauseMenu(Component sender, object obj)
    {
        Time.timeScale = 0f;
        _pauseMenu.SetActive(true);
        _eventSystem.SetSelectedGameObject(_firstSelected);
    }

    public void ClosePauseMenu()
    {
        Time.timeScale = 1f;
        _pauseMenu.SetActive(false);
        _UIBackSFXInstance.start();
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

    public void MainMenuPressed()
    {
        _UIBackSFXInstance.start();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    private void OnDestroy()
    {
        _UIConfirmSFXInstance.release();
        _UIBackSFXInstance.release();
    }
}
