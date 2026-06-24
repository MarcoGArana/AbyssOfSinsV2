using System;
using UnityEngine;

public class Health : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public int currentHealth;

    private FighterStats stats;

    void Awake()
    {
        stats = GetComponent<FighterStats>();

        currentHealth = stats.maxHealth;
    }


    public void TakeDamage(int damage)
    {
        int finalDamage = Mathf.RoundToInt(
            damage / stats.defense
        );

        currentHealth -= finalDamage;
        if (currentHealth < 0) currentHealth = 0;

        Debug.Log("Daño recibido: " + finalDamage);
        Debug.Log("Vida restante: " + currentHealth);

        OnHealthChanged?.Invoke(currentHealth, stats.maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        Debug.Log("KO");
        OnDeath?.Invoke();
    }
}