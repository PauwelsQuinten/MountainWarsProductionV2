#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.EventSystems;

public class UIMainMenu : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField]
    private GameObject _SettingsMenu;
    [SerializeField]
    private GameObject _firstSettingsItem;
    [SerializeField]
    private GameObject _backButton;

    [SerializeField]
    private GameObject _settingsButton;
    [SerializeField]
    private EventSystem _eventSystem;


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

    public void MutePressed(bool isPressed)
    {

    }

    public void SFXSliderChanged(float value)
    {

    }

    public void MusicSliderChanged(float value)
    {

    }
}
