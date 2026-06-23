using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

/// <summary>
/// Controlador principal de la interfaz de usuario en combate (HUD y Pausa).
/// Sigue el patrón observador para actualizar la UI pasivamente sin usar Update.
/// </summary>
public class FightingUIManager : MonoBehaviour
{
    [Header("Player 1 UI Elements")]
    [SerializeField] private Health player1Health;
    [SerializeField] private Slider player1Slider;
    [SerializeField] private TextMeshProUGUI player1NameText;

    [Header("Player 2 UI Elements")]
    [SerializeField] private Health player2Health;
    [SerializeField] private Slider player2Slider;
    [SerializeField] private TextMeshProUGUI player2NameText;

    [Header("Pause UI Elements")]
    [SerializeField] private GameObject pauseMenuPanel;

    /// <summary>
    /// Propiedad estática para comprobar externamente si el juego está pausado.
    /// </summary>
    public static bool IsPaused { get; private set; } = false;

    private void Awake()
    {
        // Aseguramos que el panel de pausa empiece desactivado y la escala de tiempo activa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;
    }

    private void OnEnable()
    {
        // Suscribirse a los eventos de vida (Patrón Observador)
        if (player1Health != null)
        {
            player1Health.OnHealthChanged += HandlePlayer1HealthChanged;
            player1Health.OnDeath += HandlePlayer1Death;
        }

        if (player2Health != null)
        {
            player2Health.OnHealthChanged += HandlePlayer2HealthChanged;
            player2Health.OnDeath += HandlePlayer2Death;
        }
    }

    private void OnDisable()
    {
        // Cancelar suscripciones para prevenir fugas de memoria (Memory Leaks)
        if (player1Health != null)
        {
            player1Health.OnHealthChanged -= HandlePlayer1HealthChanged;
            player1Health.OnDeath -= HandlePlayer1Death;
        }

        if (player2Health != null)
        {
            player2Health.OnHealthChanged -= HandlePlayer2HealthChanged;
            player2Health.OnDeath -= HandlePlayer2Death;
        }
    }

    private void Start()
    {
        // Inicializar los valores de la UI con los datos iniciales de los personajes
        InitializeHUD();
    }

    private void Update()
    {
        // Detección directa de inputs globales para Pausa (soporta teclado y control)
        CheckPauseInput();
    }

    /// <summary>
    /// Configura los nombres e inicializa las barras de vida basándose en los stats de los jugadores.
    /// </summary>
    private void InitializeHUD()
    {
        if (player1Health != null)
        {
            FighterStats p1Stats = player1Health.GetComponent<FighterStats>();
            if (p1Stats != null && player1NameText != null)
            {
                player1NameText.text = p1Stats.characterName;
            }
            HandlePlayer1HealthChanged(player1Health.currentHealth, p1Stats != null ? p1Stats.maxHealth : 100);
        }

        if (player2Health != null)
        {
            FighterStats p2Stats = player2Health.GetComponent<FighterStats>();
            if (p2Stats != null && player2NameText != null)
            {
                player2NameText.text = p2Stats.characterName;
            }
            HandlePlayer2HealthChanged(player2Health.currentHealth, p2Stats != null ? p2Stats.maxHealth : 100);
        }
    }

    /// <summary>
    /// Detección de teclas y botones de pausa para alternar el estado del juego.
    /// </summary>
    private void CheckPauseInput()
    {
        bool pausePressed = false;

        // Comprobación de Teclado (Escape)
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            pausePressed = true;
        }

        // Comprobación de Control/Gamepad (Start/Options)
        if (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame)
        {
            pausePressed = true;
        }

        if (pausePressed)
        {
            TogglePause();
        }
    }

    /// <summary>
    /// Alterna el estado de pausa del juego, controlando la UI, TimeScale y desactivando/activando los inputs de combate.
    /// </summary>
    public void TogglePause()
    {
        IsPaused = !IsPaused;

        // Detener o reanudar el flujo del tiempo físico y de animaciones
        Time.timeScale = IsPaused ? 0f : 1f;

        // Mostrar u ocultar el panel visual de la pausa
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(IsPaused);
        }

        // Desactivar/Activar los controladores para evitar que los jugadores se muevan o ataquen pausados
        SetFightersControlActive(!IsPaused);
    }

    /// <summary>
    /// Desactiva o activa los componentes de los luchadores para evitar acciones durante la pausa.
    /// </summary>
    private void SetFightersControlActive(bool active)
    {
        // 1. Manejo del InputSystem (desactiva el PlayerInput completo para mayor robustez)
        PlayerInput[] inputs = FindObjectsByType<PlayerInput>(FindObjectsSortMode.None);
        foreach (PlayerInput input in inputs)
        {
            if (active)
                input.ActivateInput();
            else
                input.DeactivateInput();
        }

        // 2. Control de scripts individuales por si hay inputs directos (polling de teclado como en PlayerMovement)
        PlayerController[] controllers = FindObjectsByType<PlayerController>(FindObjectsSortMode.None);
        foreach (PlayerController c in controllers) c.enabled = active;

        FighterMovement[] movements = FindObjectsByType<FighterMovement>(FindObjectsSortMode.None);
        foreach (FighterMovement m in movements) m.enabled = active;

        PlayerAttack[] attacks = FindObjectsByType<PlayerAttack>(FindObjectsSortMode.None);
        foreach (PlayerAttack a in attacks) a.enabled = active;

        PlayerMovement[] playerMovements = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (PlayerMovement pm in playerMovements) pm.enabled = active;
    }

    #region Event Handlers (Observer Pattern)

    private void HandlePlayer1HealthChanged(int currentHealth, int maxHealth)
    {
        if (player1Slider != null)
        {
            // Calculamos el valor porcentual (0.0f a 1.0f)
            player1Slider.value = (float)currentHealth / maxHealth;
        }
    }

    private void HandlePlayer2HealthChanged(int currentHealth, int maxHealth)
    {
        if (player2Slider != null)
        {
            player2Slider.value = (float)currentHealth / maxHealth;
        }
    }

    private void HandlePlayer1Death()
    {
        Debug.Log("[FightingUIManager] ¡Jugador 1 ha sido Derrotado!");
        // Aquí se puede gatillar la UI de Fin del Round / Victoria del P2
    }

    private void HandlePlayer2Death()
    {
        Debug.Log("[FightingUIManager] ¡Jugador 2 ha sido Derrotado!");
        // Aquí se puede gatillar la UI de Fin del Round / Victoria del P1
    }

    /// <summary>
    /// Cierra la aplicación. Funciona al compilar el juego.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
    /// <summary>
    /// Vuelve al menú principal. Funciona al compilar el juego.
    /// </summary>
    public void GoToMenu()
    {
        Debug.Log("Menu Principal");
        // TODO: Implementar carga de escena del menú principal
        // SceneManager.LoadScene("MainMenu");
    }
    #endregion
}
