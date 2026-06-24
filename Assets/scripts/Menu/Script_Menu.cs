using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_Menu : MonoBehaviour
{
    public void EmpezarJuegoSingle(string modo)
    {
        Debug.Log("Cargando modo para un jugador...");
        //SceneManager.LoadScene("PVE");
    }

    public void EmpezarJuegoTwoPlayers(string modo)
    {
        Debug.Log("Cargando modo para dos jugadores...");
        SceneManager.LoadScene("P1");
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
