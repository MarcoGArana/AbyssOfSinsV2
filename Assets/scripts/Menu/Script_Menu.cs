using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_Menu : MonoBehaviour
{
    public void EmpezarJuegoSingle(string modo)
    {
        Debug.Log("Cargando modo Arcade...");
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameMode(GameManager.GameMode.Arcade);
        SceneManager.LoadScene(modo);
    }

    public void EmpezarJuegoTwoPlayers(string modo)
    {
        Debug.Log("Cargando modo Versus...");
        if (GameManager.Instance != null)
            GameManager.Instance.SetGameMode(GameManager.GameMode.Versus);
        SceneManager.LoadScene(modo);
    }

    public void Creditos(string creditos)
    {
        Debug.Log("Cargando creditos...");
        //SceneManager.LoadScene("Escena_Creditos");
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Has salido exitosamente del juego");
    }
}
