using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public string gameSceneName = "GameScene";
    public GameObject optionsPanel;
    public GameObject mainMenuPanel;
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptionsMenu()
    {
        optionsPanel.SetActive(true);
        mainMenuPanel.SetActive(false);
    }

    public void CloseOptionsMenu()
    {
        optionsPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("Game Quit");
        Application.Quit();
    }
}
