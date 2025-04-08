using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public void GameLost(Component sender, object obj)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameLost");
    }

    public void GameWon(Component sender, object obj)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameWon");
    }
}
