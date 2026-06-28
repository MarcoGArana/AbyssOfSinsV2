using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameMode { Arcade, Versus }
    public enum Stage { Stage1, Stage2 }

    public GameMode SelectedGameMode { get; private set; } = GameMode.Arcade;
    public Stage SelectedStage { get; private set; } = Stage.Stage1;
    public string WinnerName { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SetGameMode(GameMode mode)
    {
        SelectedGameMode = mode;
    }

    public void SetStage(Stage stage)
    {
        SelectedStage = stage;
    }
}