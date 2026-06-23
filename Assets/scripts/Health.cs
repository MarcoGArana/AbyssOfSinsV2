using UnityEngine;

public class Health : MonoBehaviour
{
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

        Debug.Log("Daño recibido: " + finalDamage);
        Debug.Log("Vida restante: " + currentHealth);


        if (currentHealth <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        Debug.Log("KO");
    }
}