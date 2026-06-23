using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_Menu : MonoBehaviour
{
    public void EmpezarJuegoSingle(string modo)
    {
        SceneManager.LoadScene(modo);
    }

    public void EmpezarJuegoTwoPlayers(string modo)
    {
        SceneManager.LoadScene(modo);
    }

    public void Creditos(string creditos)
    {
        SceneManager.LoadScene(creditos);
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Has salido exitosamente del juego");
    }
}
