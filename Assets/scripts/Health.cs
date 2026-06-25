using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Health : MonoBehaviour
{
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    public int currentHealth;

    private FighterStats stats;
    private Animator anim;
    private FighterMovement movement;

    private bool dead;


    void Awake()
    {
        stats = GetComponent<FighterStats>();
        anim = GetComponent<Animator>();
        movement = GetComponent<FighterMovement>();

        currentHealth = stats.maxHealth;
    }


    public void TakeDamage(
     int damage,
     AttackType attackType
 )
    {
        if (dead)
            return;

        bool blocked = false;

        if (movement.isBlocking)
        {
            switch (attackType)
            {
                case AttackType.High:

                    blocked = !movement.crouching;

                    break;

                case AttackType.Low:

                    blocked = movement.crouching;

                    break;

                case AttackType.Air:

                    blocked = !movement.crouching;

                    break;
            }
        }

        if (blocked)
        {
            damage = Mathf.RoundToInt(
                damage * 0.2f
            );
        }

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
        else if (!blocked)
        {
            movement.isHit = true;
            anim.SetTrigger("Hit");
        }
    }


    public void EndHit()
    {
        movement.isHit = false;
    }


    void Die()
{
    dead = true;

    movement.isDead = true;

    Debug.Log("KO");

    anim.SetTrigger("Death");

    OnDeath?.Invoke();

    Invoke(nameof(LoadEndingScene), 2.5f);
}

void LoadEndingScene()
{
    SceneManager.LoadScene("endingScene");
}
}