using System;
using UnityEngine;

/// <summary>
/// Gestiona la vida del personaje, cálculo de daño y notifica cambios a la UI a través de eventos.
/// </summary>
public class Health : MonoBehaviour
{
    /// <summary>
    /// Evento que se dispara cuando la vida del personaje cambia.
    /// Parámetros: (int currentHealth, int maxHealth)
    /// </summary>
    public event Action<int, int> OnHealthChanged;

    /// <summary>
    /// Evento que se dispara cuando la vida del personaje llega a 0 (KO).
    /// </summary>
    public event Action OnDeath;

    [Header("Health State")]
    public int currentHealth;

    private FighterStats stats;

    void Awake()
    {
        stats = GetComponent<FighterStats>();
        if (stats == null)
        {
            Debug.LogError($"[Health] FighterStats no encontrado en {gameObject.name}.");
            return;
        }

        currentHealth = stats.maxHealth;
    }

    /// <summary>
    /// Aplica daño al personaje reduciendo su defensa y disparando el evento de cambio de vida.
    /// </summary>
    /// <param name="damage">Cantidad de daño base antes de la reducción por defensa.</param>
    public void TakeDamage(int damage)
    {
        // Si el personaje ya está KO, no procesamos más daño
        if (currentHealth <= 0) return;

        // Si la defensa es menor o igual a 0, evitamos división por cero
        float defenseMultiplier = (stats != null && stats.defense > 0) ? stats.defense : 1f;

        int finalDamage = Mathf.RoundToInt(damage / defenseMultiplier);
        currentHealth = Mathf.Max(0, currentHealth - finalDamage);

        Debug.Log($"[Health] {gameObject.name} recibió {finalDamage} de daño. Vida restante: {currentHealth}/{stats.maxHealth}");

        // Invocación del evento (Patrón Observador)
        OnHealthChanged?.Invoke(currentHealth, stats.maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// Maneja el estado de muerte y notifica a los suscriptores.
    /// </summary>
    private void Die()
    {
        Debug.Log($"[Health] {gameObject.name} ha sido noqueado (KO).");
        OnDeath?.Invoke();
    }
}