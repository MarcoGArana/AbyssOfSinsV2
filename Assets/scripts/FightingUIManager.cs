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

    [Header("Round UI Elements")]
    [SerializeField] private TextMeshProUGUI announcementText;
    [SerializeField] private TextMeshProUGUI roundScoreText;

    private float timeRemaining = 60f; // 1 minuto (en segundos)
    private bool isMatchActive = false;
    private Vector3 originalTimerScale;

    private int p1RoundWins = 0;
    private int p2RoundWins = 0;
    private int currentRound = 1;
    private bool isSuddenDeath = false;
    private bool isRoundEnding = false;

    private Vector3 p1StartPos;
    private Vector3 p2StartPos;
    private Vector3 p1StartScale;
    private Vector3 p2StartScale;

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
        // Guardar posiciones y escalas iniciales de los luchadores
        if (player1Health != null)
        {
            p1StartPos = player1Health.transform.position;
            p1StartScale = player1Health.transform.localScale;
        }
        if (player2Health != null)
        {
            p2StartPos = player2Health.transform.position;
            p2StartScale = player2Health.transform.localScale;
        }

        // Crear elementos de interfaz dinámicamente si no han sido asignados
        CreateUIDynamically();

        // Inicializar los valores de la UI con los datos iniciales de los personajes
        InitializeHUD();
        UpdateScoreText();

        // Iniciar la secuencia de la primera ronda
        StartCoroutine(StartRoundSequence());
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

        // Desactivar también la IA del enemigo para que no actúe durante intros/pausas
        EnemyAI[] ais = FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (EnemyAI ai in ais) ai.enabled = active;
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
        if (isRoundEnding) return;
        Debug.Log("[FightingUIManager] ¡Jugador 1 ha sido Derrotado!");
        StartCoroutine(EndRoundSequence(false)); // Muere P1 -> Gana P2
    }

    private void HandlePlayer2Death()
    {
        if (isRoundEnding) return;
        Debug.Log("[FightingUIManager] ¡Jugador 2 ha sido Derrotado!");
        StartCoroutine(EndRoundSequence(true)); // Muere P2 -> Gana P1
    }

    // ---------- MÉTODOS AUXILIARES Y CORRUTINAS DE RONDAS ----------

    private void CreateUIDynamically()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) return;

        if (announcementText == null)
        {
            GameObject go = new GameObject("AnnouncementText");
            go.transform.SetParent(canvas.transform, false);
            announcementText = go.AddComponent<TextMeshProUGUI>();
            announcementText.alignment = TextAlignmentOptions.Center;
            announcementText.fontSize = 60f;
            announcementText.color = Color.yellow;
            announcementText.fontStyle = FontStyles.Bold | FontStyles.Italic;
            
            // Borde oscuro para legibilidad (premium)
            announcementText.outlineWidth = 0.2f;
            announcementText.outlineColor = Color.black;

            RectTransform rect = announcementText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            
            announcementText.gameObject.SetActive(false);
        }

        if (roundScoreText == null)
        {
            GameObject go = new GameObject("RoundScoreText");
            go.transform.SetParent(canvas.transform, false);
            roundScoreText = go.AddComponent<TextMeshProUGUI>();
            roundScoreText.alignment = TextAlignmentOptions.Center;
            roundScoreText.fontSize = 32f;
            roundScoreText.color = Color.white;
            roundScoreText.fontStyle = FontStyles.Bold;

            // Sombra
            roundScoreText.outlineWidth = 0.15f;
            roundScoreText.outlineColor = Color.black;

            RectTransform rect = roundScoreText.rectTransform;
            rect.anchorMin = new Vector2(0.5f, 1.0f);
            rect.anchorMax = new Vector2(0.5f, 1.0f);
            rect.pivot = new Vector2(0.5f, 1.0f);
            
            if (timerText != null)
            {
                rect.anchoredPosition = timerText.rectTransform.anchoredPosition - new Vector2(0, 50f);
            }
            else
            {
                rect.anchoredPosition = new Vector2(0, -60f);
            }
        }
    }

    private void UpdateScoreText()
    {
        if (roundScoreText == null) return;

        roundScoreText.text = $"{p1RoundWins}  -  {p2RoundWins}";
    }

    private void UpdateTimerText()
    {
        if (timerText == null) return;
        
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);
        timerText.text = string.Format("{0}:{1:00}", minutes, seconds);
        timerText.color = Color.white;
        timerText.rectTransform.localScale = originalTimerScale;
    }

    private System.Collections.IEnumerator StartRoundSequence()
    {
        // 1. Resetear el estado físico y vida de los luchadores
        ResetPlayers();
        timeRemaining = 60f;
        isMatchActive = false;
        SetFightersControlActive(false);

        // Actualizar UI del temporizador
        UpdateTimerText();

        // 2. Anunciar la ronda
        string roundAnnouncement;
        if (isSuddenDeath)
        {
            roundAnnouncement = "SUDDEN DEATH";
        }
        else if (p1RoundWins == 2 && p2RoundWins == 2)
        {
            roundAnnouncement = "FINAL ROUND";
        }
        else
        {
            roundAnnouncement = "ROUND " + currentRound;
        }

        if (announcementText != null)
        {
            announcementText.text = roundAnnouncement;
            announcementText.gameObject.SetActive(true);
        }
        Debug.Log("[FightingUIManager] " + roundAnnouncement);

        yield return new WaitForSeconds(1.5f);

        // 3. Anunciar ¡FIGHT!
        if (announcementText != null)
        {
            announcementText.text = "FIGHT!";
        }
        Debug.Log("[FightingUIManager] FIGHT!");

        yield return new WaitForSeconds(1.0f);

        if (announcementText != null)
        {
            announcementText.gameObject.SetActive(false);
        }

        // 4. Activar los controles e iniciar la ronda
        SetFightersControlActive(true);
        isMatchActive = true;
    }

    private System.Collections.IEnumerator EndRoundSequence(bool? p1WonNormal)
    {
        isRoundEnding = true;
        isMatchActive = false;
        SetFightersControlActive(false);

        bool isTie = false;
        bool p1Won = false;

        // Determinar desenlace del round
        if (p1WonNormal == null)
        {
            // Fin de tiempo
            if (announcementText != null)
            {
                announcementText.text = "TIME UP";
                announcementText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(2.0f);

            int p1HealthVal = player1Health != null ? player1Health.currentHealth : 0;
            int p2HealthVal = player2Health != null ? player2Health.currentHealth : 0;

            if (p1HealthVal > p2HealthVal)
            {
                p1Won = true;
            }
            else if (p2HealthVal > p1HealthVal)
            {
                p1Won = false;
            }
            else
            {
                isTie = true;
            }
        }
        else
        {
            // KO normal
            bool p1Dead = player1Health != null && player1Health.currentHealth <= 0;
            bool p2Dead = player2Health != null && player2Health.currentHealth <= 0;

            if (announcementText != null)
            {
                announcementText.text = "K.O.";
                announcementText.gameObject.SetActive(true);
            }
            yield return new WaitForSeconds(2.0f);

            if (p1Dead && p2Dead)
            {
                isTie = true;
            }
            else
            {
                p1Won = p1WonNormal.Value;
            }
        }

        // Mostrar ganador de la ronda y sumar puntos
        if (isTie)
        {
            if (announcementText != null)
            {
                announcementText.text = "DRAW ROUND";
            }
            p1RoundWins++;
            p2RoundWins++;
        }
        else
        {
            string roundWinnerName = "Jugador 1";
            if (p1Won)
            {
                p1RoundWins++;
                if (player1Health != null)
                {
                    FighterStats stats = player1Health.GetComponent<FighterStats>();
                    if (stats != null && !string.IsNullOrEmpty(stats.characterName))
                        roundWinnerName = stats.characterName;
                }
            }
            else
            {
                p2RoundWins++;
                if (player2Health != null)
                {
                    FighterStats stats = player2Health.GetComponent<FighterStats>();
                    if (stats != null && !string.IsNullOrEmpty(stats.characterName))
                    {
                        if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
                            roundWinnerName = "CPU";
                        else
                            roundWinnerName = stats.characterName;
                    }
                }
            }

            if (announcementText != null)
            {
                announcementText.text = $"{roundWinnerName.ToUpper()} WINS THE ROUND!";
            }
        }

        UpdateScoreText();
        yield return new WaitForSeconds(2.0f);

        if (announcementText != null)
        {
            announcementText.gameObject.SetActive(false);
        }

        // Comprobar victorias del combate
        bool p1MatchWon = p1RoundWins >= 3;
        bool p2MatchWon = p2RoundWins >= 3;

        if (p1MatchWon && p2MatchWon)
        {
            // Ambos alcanzan 3 victorias a la vez (por empate). Lógica de Muerte Súbita
            isSuddenDeath = true;
            p1RoundWins = 2;
            p2RoundWins = 2;
            UpdateScoreText();
            currentRound++;
            isRoundEnding = false;
            StartCoroutine(StartRoundSequence());
        }
        else if (p1MatchWon)
        {
            FinishMatch(true);
        }
        else if (p2MatchWon)
        {
            FinishMatch(false);
        }
        else
        {
            // Siguiente ronda
            currentRound++;
            isRoundEnding = false;
            StartCoroutine(StartRoundSequence());
        }
    }

    private void FinishMatch(bool p1Won)
    {
        isMatchActive = false;
        string matchWinnerName = "Jugador 1";
        if (p1Won)
        {
            if (player1Health != null)
            {
                FighterStats stats = player1Health.GetComponent<FighterStats>();
                if (stats != null && !string.IsNullOrEmpty(stats.characterName))
                    matchWinnerName = stats.characterName;
            }
        }
        else
        {
            if (player2Health != null)
            {
                FighterStats stats = player2Health.GetComponent<FighterStats>();
                if (stats != null && !string.IsNullOrEmpty(stats.characterName))
                {
                    if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
                        matchWinnerName = "CPU";
                    else
                        matchWinnerName = stats.characterName;
                }
            }
        }

        ShowWinner(matchWinnerName);
        Invoke(nameof(LoadNextSceneAfterMatch), 5f);
    }

    private void LoadNextSceneAfterMatch()
    {
        Time.timeScale = 1f;
        IsPaused = false;

        bool p1Won = p1RoundWins >= 3;

        if (GameManager.Instance != null && GameManager.Instance.IsArcadeMode)
        {
            if (p1Won)
            {
                SceneManager.LoadScene("endingScene");
            }
            else
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayMenuMusic();
                SceneManager.LoadScene("Main Menu");
            }
        }
        else
        {
            SceneManager.LoadScene("endingScene");
        }
    }

    private void ResetPlayers()
    {
        ResetPlayer(player1Health, p1StartPos, p1StartScale);
        ResetPlayer(player2Health, p2StartPos, p2StartScale);
    }

    private void ResetPlayer(Health playerHealth, Vector3 startPos, Vector3 startScale)
    {
        if (playerHealth == null) return;

        // Resetear posición y orientación
        playerHealth.transform.position = startPos;
        playerHealth.transform.localScale = startScale;

        // Resetear Rigidbody2D
        Rigidbody2D rb = playerHealth.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Resetear salud interna
        playerHealth.ResetHealth();

        // Resetear animaciones
        Animator anim = playerHealth.GetComponent<Animator>();
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        // Resetear variables de movimiento
        FighterMovement fm = playerHealth.GetComponent<FighterMovement>();
        if (fm != null)
        {
            fm.isDead = false;
            fm.isHit = false;
            fm.isBlocking = false;
            fm.crouching = false;
        }

        PlayerMovement pm = playerHealth.GetComponent<PlayerMovement>();
        if (pm != null)
        {
            pm.isGrounded = true;
        }

        // Resetear variables de ataque
        PlayerAttack pa = playerHealth.GetComponent<PlayerAttack>();
        if (pa != null)
        {
            pa.ResetAttack();
        }

        // Resetear IA
        EnemyAI ai = playerHealth.GetComponent<EnemyAI>();
        if (ai != null)
        {
            ai.ResetAI();
        }
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
        StartCoroutine(EndRoundSequence(null));
    }
    #endregion
}
