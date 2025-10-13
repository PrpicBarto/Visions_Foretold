using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public GameObject optionsPanel;
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptionsMenu()
    {
        optionsPanel.SetActive(true);
    }

    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
