using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Controla el flujo de rondas del modo PvP local: temporizador, conteo de
/// rondas ganadas y avance de una ronda a otra hasta terminar el combate
/// (mejor de 3, configurable).
///
/// IMPORTANTE - Estado actual de la implementación:
/// Por ahora la ronda SOLO termina cuando se acaba el tiempo. No se determina
/// un ganador en ese caso, y tampoco se revisa la vida de los jugadores.
/// Por eso el conteo de rondas ganadas no avanzará todavía: el combate seguirá
/// pasando de ronda en ronda indefinidamente hasta que se complete el TODO
/// de abajo. Esto es intencional para esta etapa.
///
/// TODO: Terminar la ronda anticipadamente cuando la vida de un jugador
///       llegue a 0 (KO). Lo más simple es que el script de vida del jugador
///       llame a RoundManager.Instance.EndRoundByKO(jugadorQuePerdio).
/// TODO: Determinar el ganador de la ronda cuando el tiempo se agota
///       (por ejemplo comparando la vida restante de cada jugador) y
///       llamar a RegisterRoundWin(jugadorGanador) en ese momento.
/// </summary>
public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    public enum RoundEndReason
    {
        TimeUp,
        KO // Reservado para cuando se implemente el TODO de vida
    }

    [Header("Configuración de rondas")]
    [Tooltip("Duración de cada ronda en segundos.")]
    [SerializeField] private float roundDuration = 60f;

    [Tooltip("Rondas que hay que ganar para llevarse el combate (2 = mejor de 3).")]
    [SerializeField] private int roundsToWinMatch = 2;

    [Tooltip("Pausa antes de que empiece a correr el tiempo de la ronda (ej: animación 'Round 1 - Fight!').")]
    [SerializeField] private float delayBeforeRoundStart = 1.5f;

    [Tooltip("Pausa después de terminar la ronda antes de pasar a la siguiente.")]
    [SerializeField] private float delayAfterRoundEnd = 2f;

    [Header("Estado actual (solo lectura, visible en el Inspector para debug)")]
    [SerializeField] private int currentRoundNumber = 1;
    [SerializeField] private int player1RoundsWon = 0;
    [SerializeField] private int player2RoundsWon = 0;
    [SerializeField] private float timeRemaining;
    [SerializeField] private bool isRoundActive = false;

    // --- Eventos para que UIManager u otros scripts reaccionen sin acoplarse directamente ---

    /// <summary>Se dispara al iniciar una ronda. Parámetro: número de ronda.</summary>
    public event Action<int> OnRoundStart;

    /// <summary>Se dispara cada frame mientras la ronda está activa. Parámetro: tiempo restante.</summary>
    public event Action<float> OnTimerTick;

    /// <summary>Se dispara cuando la ronda termina. Parámetro: motivo del fin de ronda.</summary>
    public event Action<RoundEndReason> OnRoundEnd;

    /// <summary>Se dispara cuando el combate completo termina. Parámetros: jugador ganador (1 o 2), rondas jugadas.</summary>
    public event Action<int, int> OnMatchEnd;

    private Coroutine roundRoutine;

    private void Awake()
    {
        // Singleton simple: evita tener dos RoundManager activos a la vez en la escena.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        BeginMatch();
    }

    /// <summary>Reinicia los contadores y arranca el combate desde la ronda 1.</summary>
    public void BeginMatch()
    {
        currentRoundNumber = 1;
        player1RoundsWon = 0;
        player2RoundsWon = 0;
        StartRoundSequence();
    }

    private void StartRoundSequence()
    {
        if (roundRoutine != null)
        {
            StopCoroutine(roundRoutine);
        }
        roundRoutine = StartCoroutine(RoundSequence());
    }

    private IEnumerator RoundSequence()
    {
        // Pausa inicial antes de que corra el tiempo (útil para un cartel "Round 1 - Fight!").
        yield return new WaitForSeconds(delayBeforeRoundStart);

        timeRemaining = roundDuration;
        isRoundActive = true;
        OnRoundStart?.Invoke(currentRoundNumber);

        while (timeRemaining > 0f && isRoundActive)
        {
            // TODO: revisar aquí si algún jugador llegó a 0 de vida y, si es así,
            // llamar a EndRoundByKO(jugadorQuePerdio) para cortar la ronda antes de tiempo.

            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f)
            {
                timeRemaining = 0f;
            }

            OnTimerTick?.Invoke(timeRemaining);
            yield return null;
        }

        if (isRoundActive)
        {
            EndRound(RoundEndReason.TimeUp);
        }
    }

    /// <summary>
    /// Punto de entrada para cuando se implemente el fin de ronda por KO.
    /// De momento no es llamado por nada (queda listo para conectarse).
    /// </summary>
    public void EndRoundByKO(int losingPlayer)
    {
        if (!isRoundActive) return;

        if (roundRoutine != null)
        {
            StopCoroutine(roundRoutine);
        }

        int winningPlayer = losingPlayer == 1 ? 2 : 1;
        RegisterRoundWin(winningPlayer);
        EndRound(RoundEndReason.KO);
    }

    private void EndRound(RoundEndReason reason)
    {
        isRoundActive = false;
        OnRoundEnd?.Invoke(reason);

        // TODO: cuando reason == TimeUp, decidir aquí (o desde afuera, escuchando
        // OnRoundEnd) quién ganó la ronda según la vida restante de cada jugador,
        // y llamar a RegisterRoundWin(jugadorGanador). Por ahora no se asigna nada.

        StartCoroutine(AfterRoundDelay());
    }

    private IEnumerator AfterRoundDelay()
    {
        yield return new WaitForSeconds(delayAfterRoundEnd);
        AdvanceToNextRoundOrEndMatch();
    }

    /// <summary>
    /// Registra que un jugador ganó la ronda actual. Por ahora nada llama esto
    /// automáticamente al acabarse el tiempo (ver TODO en EndRound); úsalo
    /// desde el sistema de vida o desde donde se resuelva ese TODO.
    /// </summary>
    public void RegisterRoundWin(int winningPlayer)
    {
        if (winningPlayer == 1)
        {
            player1RoundsWon++;
        }
        else if (winningPlayer == 2)
        {
            player2RoundsWon++;
        }
    }

    private void AdvanceToNextRoundOrEndMatch()
    {
        if (player1RoundsWon >= roundsToWinMatch)
        {
            OnMatchEnd?.Invoke(1, currentRoundNumber);
            return;
        }

        if (player2RoundsWon >= roundsToWinMatch)
        {
            OnMatchEnd?.Invoke(2, currentRoundNumber);
            return;
        }

        currentRoundNumber++;
        StartRoundSequence();
    }

    // --- Getters públicos para que UIManager pinte el HUD sin acceder a campos privados ---
    public float TimeRemaining => timeRemaining;
    public float RoundDuration => roundDuration;
    public int CurrentRoundNumber => currentRoundNumber;
    public int Player1RoundsWon => player1RoundsWon;
    public int Player2RoundsWon => player2RoundsWon;
    public bool IsRoundActive => isRoundActive;
}
