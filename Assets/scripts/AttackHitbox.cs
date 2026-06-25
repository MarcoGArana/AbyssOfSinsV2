using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    private PlayerAttack playerAttack;

    private bool hitSomething;

    void Awake()
    {
        playerAttack = GetComponentInParent<PlayerAttack>();
    }

    private void OnEnable()
    {
        hitSomething = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hitSomething)
            return;

        Health health = other.GetComponent<Health>();

        if (health != null)
        {
            hitSomething = true;

            int damage = playerAttack.GetAttackDamage();

            AttackType attackType =
                playerAttack.GetAttackType();

            health.TakeDamage(
                damage,
                attackType
            );
        }
    }
}