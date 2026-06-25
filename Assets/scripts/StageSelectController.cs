using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectController : MonoBehaviour
{
    [Header("Nombre de la escena de pelea")]
    [SerializeField] private string fightSceneName = "Fight";

    public void SelectStage1()
    {
        GameManager.Instance.SetStage(GameManager.Stage.Stage1);
        LoadFightScene();
    }

    public void SelectStage2()
    {
        GameManager.Instance.SetStage(GameManager.Stage.Stage2);
        LoadFightScene();
    }

    private void LoadFightScene()
    {
        SceneManager.LoadScene(fightSceneName);
    }
}