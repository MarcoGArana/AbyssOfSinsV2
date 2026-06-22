using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;


    void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            int damage = playerAttack.GetAttackDamage();

            health.TakeDamage(damage);
        }
    }
}