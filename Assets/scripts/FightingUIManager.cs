using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

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

    [Header("Timer UI Elements")]
    [SerializeField] private TextMeshProUGUI timerText;

    [Header("Win UI Elements")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winnerText;

    private float timeRemaining = 60f; // 1 minuto (en segundos)
    private bool isMatchActive = true;
    private Vector3 originalTimerScale;

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
        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
        Time.timeScale = 1f;
        IsPaused = false;

        // Timer
        if (timerText != null)
        {
            originalTimerScale = timerText.rectTransform.localScale;
        }
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

        // Solo corremos el temporizador si la partida está activa y no está en pausa
        if (isMatchActive && !IsPaused)
        {
            ProcessTimer();
        }
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
                if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
                    player2NameText.text = "CPU";
                else
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

    public void ShowWinner(string winnerName)
    {
        isMatchActive = false;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.WinnerName = winnerName;
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (winnerText != null)
        {
            winnerText.text = winnerName + "!";
        }
    }

    private void HandlePlayer1Death()
    {
        Debug.Log("[FightingUIManager] ¡Jugador 1 ha sido Derrotado!");
        string winnerName = "Jugador 2";
        if (player2Health != null)
        {
            FighterStats stats = player2Health.GetComponent<FighterStats>();
            if (stats != null && !string.IsNullOrEmpty(stats.characterName))
            {
                if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
                    winnerName = "CPU";
                else
                    winnerName = stats.characterName;
            }
        }
        ShowWinner(winnerName);
    }

    private void HandlePlayer2Death()
    {
        Debug.Log("[FightingUIManager] ¡Jugador 2 ha sido Derrotado!");
        string winnerName = "Jugador 1";
        if (player1Health != null)
        {
            FighterStats stats = player1Health.GetComponent<FighterStats>();
            if (stats != null && !string.IsNullOrEmpty(stats.characterName))
            {
                if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
                    winnerName = "Jugador";
                else
                    winnerName = stats.characterName;
            }
        }
        ShowWinner(winnerName);
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
        Time.timeScale = 1f;
        IsPaused = false;

        Debug.Log("Menu Principal");

        // Vuelve a la música del menú antes de cambiar de escena.
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayMenuMusic();
        }

        SceneManager.LoadScene("Main Menu");
    }

    /// <summary>
    /// Maneja la cuenta regresiva, el formato de texto y los efectos visuales de tensión.
    /// </summary>
    private void ProcessTimer()
    {
        timeRemaining -= Time.deltaTime;

        // Si el tiempo se acaba
        if (timeRemaining <= 0)
        {
            timeRemaining = 0;
            isMatchActive = false;
            HandleTimeUp();
            return;
        }

        // Formato visual de Minutos:Segundos
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        // Formateamos para que siempre muestre dos dígitos en los segundos (ej: 1:05)
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);

        // Efectos visuales en los últimos 15 segundos
        if (timeRemaining <= 15f)
        {
            // Mathf.PingPong crea un valor que rebota suavemente entre 0 y 1
            // Multiplicamos el tiempo para que el "latido" sea más rápido (ej: * 4f)
            float pingPong = Mathf.PingPong(Time.time * 4f, 1f);

            // Interpola entre Blanco y Rojo usando el valor que rebota
            timerText.color = Color.Lerp(Color.white, Color.red, pingPong);

            // Interpola la escala para que crezca un 20% y regrese a la normalidad
            float scalePulse = Mathf.Lerp(1f, 1.2f, pingPong);
            timerText.rectTransform.localScale = originalTimerScale * scalePulse;
        }
    }

    /// <summary>
    /// Acciones a ejecutar cuando el temporizador llega a cero absoluto.
    /// </summary>
    private void HandleTimeUp()
    {
        timerText.text = "0:00";
        timerText.color = Color.red;
        
        // Crece de tamaño permanentemente a 1.5 veces su tamaño original
        timerText.rectTransform.localScale = originalTimerScale * 1.5f;

        Debug.Log("[Temporizador] ¡Tiempo Agotado! Fin del Round.");
        // Aquí en el futuro puedes disparar el evento OnTimeUp para detener a los jugadores
    }
    #endregion
}
