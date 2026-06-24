using UnityEngine;
using UnityEngine.SceneManagement;

public class GameFlowManager : MonoBehaviour
{
    public static GameFlowManager Instance { get; private set; }

    [Header("Scene Names")]
    [SerializeField] private string menuScene = "MenuScene";
    [SerializeField] private string introScene = "IntroScene";
    [SerializeField] private string gameScene = "p1";
    [SerializeField] private string postCreditsScene = "PostCreditsScene";

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartGame()
    {
        if (!string.IsNullOrEmpty(introScene))
            SceneManager.LoadScene(introScene);
        else
            SceneManager.LoadScene(gameScene);
    }

    public void OnIntroFinished()
    {
        SceneManager.LoadScene(gameScene);
    }

    public void OnGameFinished()
    {
        if (!string.IsNullOrEmpty(postCreditsScene))
            SceneManager.LoadScene(postCreditsScene);
        else
            SceneManager.LoadScene(menuScene);
    }

    public void OnPostCreditsFinished()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(menuScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
