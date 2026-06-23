using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class IntroManager : MonoBehaviour
{
    bool loading = false;

    void Start()
    {
        Invoke(nameof(LoadGame), 10f);
    }

    void Update()
    {
        if (Keyboard.current.anyKey.wasPressedThisFrame && !loading)
        {
            LoadGame();
        }
    }

    void LoadGame()
    {
        loading = true;
        SceneManager.LoadScene("p1");
    }
}