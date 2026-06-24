using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class CreditsScroll : MonoBehaviour
{
    public RectTransform creditsText;
    public float scrollSpeed = 40f;
    public float endYPosition = 1200f;
    public string nextSceneName = "MainMenu";

    private bool finished = false;

    void Update()
    {
        if (finished) return;

        creditsText.anchoredPosition += Vector2.up * scrollSpeed * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= endYPosition)
        {
            finished = true;
            SceneManager.LoadScene(nextSceneName);
        }

        if (Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(nextSceneName);
        }
    }
}