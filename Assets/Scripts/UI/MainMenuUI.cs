using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUI : MonoBehaviour
{
    public void GoToMainMenu() => SceneManager.LoadScene(0);

    public void GoToGame() => SceneManager.LoadScene(1);

    public void QuitGame() => Application.Quit();
}
