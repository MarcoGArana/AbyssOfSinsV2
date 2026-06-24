using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class EndingManager : MonoBehaviour
{
    public GameObject EndingSceneN1_0;
    public GameObject Text1;

    public GameObject EndingSceneN2_0;
    public GameObject Text2;

    public float secondsPerScreen = 7f;
    public string nextSceneName = "Main Menu";

    private int currentScreen = 1;
    private bool finished = false;

    void Start()
    {
        // Mostrar primera imagen y texto
        EndingSceneN1_0.SetActive(true);
        Text1.SetActive(true);

        // Ocultar segunda imagen y texto
        EndingSceneN2_0.SetActive(false);
        Text2.SetActive(false);

        Invoke(nameof(NextScreen), secondsPerScreen);
    }

    void Update()
    {
        if (finished) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SkipEnding();
            }
            else if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                NextScreen();
            }
        }
    }

    void SkipEnding()
    {
        CancelInvoke(nameof(NextScreen));
        finished = true;
        SceneManager.LoadScene(nextSceneName);
    }

    void NextScreen()
    {
        CancelInvoke(nameof(NextScreen));

        if (currentScreen == 1)
        {
            currentScreen = 2;

            // Ocultar pantalla 1
            EndingSceneN1_0.SetActive(false);
            Text1.SetActive(false);

            // Mostrar pantalla 2
            EndingSceneN2_0.SetActive(true);
            Text2.SetActive(true);

            Invoke(nameof(NextScreen), secondsPerScreen);
        }
        else
        {
            finished = true;
            SceneManager.LoadScene(nextSceneName);
        }
    }
}