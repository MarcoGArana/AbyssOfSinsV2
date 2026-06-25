using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelectController : MonoBehaviour
{
    [Header("Nombre de la escena de pelea")]
    [SerializeField] private string fightSceneName = "Fight";

    // Llama a esta función desde el botón del Escenario 1
    public void SelectStage1()
    {
        GameManager.Instance.SetStage(GameManager.Stage.Stage1);
        LoadFightScene();
    }

    // Llama a esta función desde el botón del Escenario 2
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