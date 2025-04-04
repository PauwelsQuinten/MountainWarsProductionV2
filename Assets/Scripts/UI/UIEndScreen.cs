using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIEndScreen : MonoBehaviour
{
    public void NewGamePressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Gameplay");
    }

    public void MainMenuPressed()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void QuitGamePressed()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#endif
        Application.Quit();
    }
}
