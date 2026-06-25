using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsScroll : MonoBehaviour
{
    public RectTransform creditsText;
    public float scrollSpeed = 40f;
    public float endYPosition = 1200f;
    public string nextSceneName = "Main Menu";
    private bool finished = false;

    void Update()
    {
        if (finished) return;

        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= endYPosition)
        {
            GoToMainMenu();
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            GoToMainMenu();
        }
    }

    /// <summary>
    /// Centraliza la salida hacia el menú principal: restaura la música
    /// del menú antes de cambiar de escena, evitando que el audio se quede
    /// detenido (heredado del .Stop() llamado en la escena de Ending).
    /// </summary>
    private void GoToMainMenu()
    {
        finished = true;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        SceneManager.LoadScene(nextSceneName);
    }
}