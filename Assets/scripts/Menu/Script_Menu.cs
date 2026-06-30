using System.Collections; // Necesario para usar las Corrutinas (IEnumerator)
using UnityEngine;
using UnityEngine.SceneManagement;

public class Script_Menu : MonoBehaviour
{
    // Creamos una referencia pública para arrastrar el panel desde el Inspector
    [Header("UI Elements")]
    public GameObject panelNextUpdate;

    private void Start()
    {
        // Es buena práctica asegurarse de que el panel inicie oculto al cargar la escena
        if (panelNextUpdate != null)
        {
            panelNextUpdate.SetActive(false);
        }
    }

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
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.Stop();
        }
        SceneManager.LoadScene("creditsScene"); // Asegúrate de que no estás usando la variable 'creditos' si pasas un string fijo
    }

    public void Salir()
    {
        Application.Quit();
        Debug.Log("Has salido exitosamente del juego");
    }
}